using System;
using System.Runtime.InteropServices;

namespace VowpalWabbit.Native.Exploration
{
    public static class ExplorationStrategies
    {
        private const string DllName = "cs_native_explore.dll";

        [DllImport(DllName)]
        private static extern int generate_epsilon_greedy(float epsilon, UInt32 topAction, [In, Out] float[] pdf, UInt32 size);

        public static float[] GenerateEpsilonGreedy(float epsilon, UInt32 topAction, UInt32 numActions)
        {
            var pdf = new float[numActions];
            var errorCode = generate_epsilon_greedy(epsilon, topAction, pdf, numActions);
            if (errorCode != 0) // TODO: map error codes
                throw new Exception("generate_epsilon_greedy failed: " + errorCode);

            return pdf;
        }

        [DllImport(DllName)]
        private static extern int generate_softmax(float lambda, [In, Out] float[] scores, UInt32 scores_size, [In, Out] float[] pdf, UInt32 pdf_size);

        public static float[] GenerateSoftmax(float lambda, float[] scores)
        {
            var pdf = new float[scores.Length];
            var errorCode = generate_softmax(lambda, scores, (UInt32)scores.Length, pdf, (UInt32)pdf.Length);
            if (errorCode != 0) // TODO: map error codes
                throw new Exception("generate_softmax failed: " + errorCode);

            return pdf;
        }

        [DllImport(DllName)]
        private static extern int generate_bag([In, Out] int[] topActions, UInt32 topActions_size, [In, Out] float[] pdf, UInt32 pdf_size);

        public static float[] GenerateBag(int[] topActions)
        {
            var pdf = new float[topActions.Length];
            var errorCode = generate_bag(topActions, (UInt32)topActions.Length, pdf, (UInt32)pdf.Length);
            if (errorCode != 0) // TODO: map error codes
                throw new Exception("generate_bag failed: " + errorCode);

            return pdf;
        }
    }
}