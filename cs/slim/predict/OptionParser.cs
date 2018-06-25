using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// Extract command line options from Vowpal Wabbit command line arguments.
    /// </summary>
    public static class OptionParser
    {
        public static IEnumerable<string> FindOptions(string commandlineArgs, string argName)
        {
            argName += ' ';

            for (int start = 0; start < commandlineArgs.Length;)
            {
                int idx = commandlineArgs.IndexOf(argName, start);
                if (idx == -1)
                    yield break;

                int idxAfterArg = idx + argName.Length;
                for (; idxAfterArg < commandlineArgs.Length && char.IsWhiteSpace(commandlineArgs[idxAfterArg]); ++idxAfterArg);

                if (idxAfterArg == commandlineArgs.Length)
                    yield break;

                if (commandlineArgs[idxAfterArg] == '-' && 
                    // make sure we allow -5.2 (negative numbers) 
                    !(idxAfterArg + 1 < commandlineArgs.Length && (commandlineArgs[idxAfterArg + 1] >= '0' && commandlineArgs[idxAfterArg + 1] <= '9')))
                {
                    start = idxAfterArg;
                    continue; // next option found
                }

                // find next non-white space character
                int idxAfterValue = idxAfterArg;
                for (; idxAfterValue < commandlineArgs.Length  && !char.IsWhiteSpace(commandlineArgs[idxAfterValue]);
                    ++idxAfterValue);

                int valueSize = idxAfterValue - idxAfterArg;
                if (valueSize > 0)
                    yield return commandlineArgs.Substring(idxAfterArg, valueSize);

                start = idxAfterArg + 1;
            }
        }

        public static IEnumerable<int> FindOptionsInt(string commandlineArgs, string argName) 
            => FindOptions(commandlineArgs, argName).Select(opt => int.Parse(opt, CultureInfo.InvariantCulture));

        public static IEnumerable<float> FindOptionsFloat(string commandlineArgs, string argName) 
            => FindOptions(commandlineArgs, argName).Select(opt => float.Parse(opt, CultureInfo.InvariantCulture));
    }
}