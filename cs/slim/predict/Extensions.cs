using System;
using System.Collections.Generic;

namespace VowpalWabbit.Prediction
{
    public static class Extensions
    {
        /// <summary>
        /// Finds the index of the minimum element.
        /// </summary>
        /// <typeparam name="T">Type of the sequence elements.</typeparam>
        /// <param name="source">The source enumerable.</param>
        /// <returns></returns>
        public static int MinIndex<T>(this IEnumerable<T> source)
        {
            IComparer<T> comparer = Comparer<T>.Default;
            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                    throw new InvalidOperationException("Empty sequence");

                int minIndex = 0;
                T minElement = iterator.Current;
                int index = 0;
                while (iterator.MoveNext())
                {
                    index++;
                    T element = iterator.Current;
                    if (comparer.Compare(element, minElement) < 0)
                    {
                        minElement = element;
                        minIndex = index;
                    }
                }

                return minIndex;
            }
        }
    }
}
