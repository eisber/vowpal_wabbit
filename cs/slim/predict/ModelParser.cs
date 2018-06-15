using System;
using System.IO;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace VowpalWabbit.Prediction
{
    public class ModelParserException : Exception
    {
        public ModelParserException(string fieldName, string message) : base(message)
        {
            this.FieldName = fieldName;
        }

        public ModelParserException(string fieldName, string message, Exception innerException) : base(message, innerException)
        {
            this.FieldName = fieldName;
        }

        public string FieldName { get; private set; }
    }

    public class ModelParser : IDisposable
    {
        public static Model Parse(Stream s)
        {
            return new ModelParser(s).Parse();
        }

        private BinaryReader input;

        private UInt32 checksum;

        private ModelParser(Stream input)
        {
            this.input = new BinaryReader(input, Encoding.UTF8, leaveOpen: true);
        }

        internal Model Parse()
        {
            var model = new Model();

            // note: checksum excludes this field
            this.ReadString("version");

            model.Id = this.ReadStringUpdateChecksum("id");
            this.SkipUpdateChecksum("model character", sizeof(byte));
            this.SkipUpdateChecksum("min_label", sizeof(float));
            this.SkipUpdateChecksum("max_label", sizeof(float));

            model.NumBits = (int)this.ReadUInt32UpdateChecksum("num_bits");

            this.SkipUpdateChecksum("lda", sizeof(UInt32));

            UInt32 ngramLen = this.ReadUInt32UpdateChecksum("ngram_len");
            this.SkipUpdateChecksum("ngrams", (int)(3 * ngramLen));

            UInt32 skipsLen = this.ReadUInt32UpdateChecksum("skips_len");
            this.SkipUpdateChecksum("skips", (int)(3 * skipsLen));

            string commandlineArgs = this.ReadStringUpdateChecksum("file_options");

            bool no_constant = commandlineArgs.Contains("--noconstant");

            int[] hashSeeds = OptionParser.FindOptionsInt(commandlineArgs, "--hash_seed").ToArray();
            if (hashSeeds.Length > 1 && hashSeeds[0] != 0)
                throw new ModelParserException("hash_seed", "Only 0-valued hash_seed is supported");

            model.Interactions = new List<string>();
            foreach (var arg in new[] { "-q", "--quadratic", "--cubic", "--interactions" })
                model.Interactions.AddRange(OptionParser.FindOptions(commandlineArgs, arg));

            UInt64 numWeights = 0;

            if (commandlineArgs.Contains("--cb_explore_adf"))
            {
                model.BagSize = OptionParser.FindOptionsInt(commandlineArgs, "--bag").FirstOrDefault();
                if (model.BagSize > 0)
                {
                    model.Exploration = Exploration.Bag;
                    numWeights = (UInt64)model.BagSize;

                    // check for additional minimum epsilon greedy
                    model.BagMinimumEpsilon = OptionParser.FindOptionsFloat(commandlineArgs, "--epsilon").FirstOrDefault();
                }
                else if (commandlineArgs.Contains("--softmax"))
                {
                    model.Exploration = Exploration.Softmax;

                    model.Lambda = OptionParser.FindOptionsFloat(commandlineArgs, "--lambda").FirstOrDefault();
                }
                else
                {
                    model.Exploration = Exploration.EpsilonGreedy;
                    model.Epsilon = OptionParser.FindOptionsFloat(commandlineArgs, "--epsilon").FirstOrDefault();
                }
            }

            // Console.Out.WriteLine("numBits: " + numBits);
            // Console.Out.WriteLine("args: '" + commandlineArgs + "'");
            // Console.Out.WriteLine("interactions: '" + string.Join(",", interactions) + "'");

            UInt32 checksumLen = this.ReadUInt32("check_sum_len");
            if (checksumLen != sizeof(UInt32))
                throw new ModelParserException("check_sum_len", "Check sum length must be 4");

            UInt32 storedChecksum = this.ReadUInt32("check_sum");
            if (storedChecksum != this.checksum)
                throw new ModelParserException("check_sum", "Invalid check sum " + storedChecksum + " vs " + this.checksum);

            bool gdResume = this.input.ReadByte() != 0; 
            if (gdResume)
                throw new ModelParserException("gd_resume", "GD Resume is not supported");

            model.StrideShift = (int)CeilLog2(numWeights);

            model.Weights = ReadWeights(model.NumBits);

            return model;
        }

        private IModelWeights ReadWeights(int numBits)
        {
            var weights = new SparseModelWeights();

            // TODO: double check the original code
            if (numBits < 31)
                this.ReadWeights(weights, reader => (UInt64)reader.ReadUInt32());
            else
                this.ReadWeights(weights, reader => reader.ReadUInt64());

            return weights;
        }

        private void ReadWeights(IModelWeights weights, Func<BinaryReader, UInt64> indexReader)
        {
            // read until we reach end-of-file
            while (this.input.BaseStream.Position != this.input.BaseStream.Length)
            {
                // 32 vs 64bit indices
                UInt64 idx = indexReader(this.input);

                weights[idx] = this.input.ReadSingle();
            }
        }

        private UInt64 CeilLog2(UInt64 v)
        {
            if (v == 0)
                return 0;
            else
                return 1 + CeilLog2(v >> 1);
        }
        
        private void SkipUpdateChecksum(string fieldName, int len)
        {
            try
            {
                if (len > 0)
                {
                    byte[] data = this.input.ReadBytes(len);
                    
                    this.checksum = (UInt32)MurMurHash3.ComputeIdHash(data, this.checksum);
                }
            }
            catch (Exception e)
            {
                throw new ModelParserException(fieldName, "Failed to skip " + len + " bytes", e);
            }
        }

        private UInt32 ReadUInt32(string fieldName)
        {
            try
            {
                return this.input.ReadUInt32();
            }
            catch (Exception e)
            {
                throw new ModelParserException(fieldName, "Failed to read UInt32", e);
            }
        }
        private UInt32 ReadUInt32UpdateChecksum(string fieldName)
        {
            try
            {
                UInt32 value = this.input.ReadUInt32();

                this.checksum = (UInt32)MurMurHash3.ComputeIdHash(BitConverter.GetBytes(value), this.checksum);

                return value;
            }
            catch (Exception e)
            {
                throw new ModelParserException(fieldName, "Failed to read UInt32", e);
            }
        }

        private string ReadString(string fieldName)
        {
            try
            {
                UInt32 len = this.input.ReadUInt32();

                // TODO: maybe length-1
                return new String(this.input.ReadChars((int)len));
            }
            catch (Exception e)
            {
                throw new ModelParserException(fieldName, "Failed to read string", e);
            }
        }

        private string ReadStringUpdateChecksum(string fieldName)
        {
            UInt32 len = this.ReadUInt32UpdateChecksum(fieldName);

            try
            {
                if (len == 0)
                    throw new ModelParserException(fieldName, "string fields must have a least 1 byte");

                byte[] chars = this.input.ReadBytes((int)len);
                
                this.checksum = (UInt32)MurMurHash3.ComputeIdHash(chars, this.checksum);

                return Encoding.UTF8.GetString(chars, 0,  (int)len - 1);
            }
            catch (Exception e)
            {
                throw new ModelParserException(fieldName, "Failed to read string", e);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.input.Dispose();
                    this.input = null;
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}