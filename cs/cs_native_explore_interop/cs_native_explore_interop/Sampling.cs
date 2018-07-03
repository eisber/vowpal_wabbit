using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace VowpalWabbit.Native.Exploration
{
    public static class Sampling
    {
        [DllImport(@"cs_native_explore.dll", CharSet = CharSet.Ansi)]

        private static extern int sample_after_normalizing([MarshalAs(UnmanagedType.LPStr)] string seed, [In, Out] float[] pdf, UInt32 pdf_size, ref UInt32 chosen_index);

        public static UInt32 SampleAfterNormalizing(string seed, float[] pdf)
        {
            UInt32 chosen_index = 0;
            var errorCode = sample_after_normalizing(seed, pdf, (UInt32)pdf.Length, ref chosen_index);
            if (errorCode != 0)
                throw new Exception("sample_after_normalizing failed: " + errorCode);

            return chosen_index;
        }
    }
}
