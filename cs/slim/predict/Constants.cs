using System;

namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// Some magic constants used through out the code base. See constant.h.
    /// </summary>
    internal class Constants
    {
        /// <summary>
        /// The feature group of the constant feature.
        /// </summary>
        internal const ushort constant_namespace = 128;

        /// <summary>
        /// The feature index of the constant feature.
        /// </summary>
        internal const Int64 constant = 11650396;

        /// <summary>
        /// Used by hashing when constructing interaction terms.
        /// </summary>
        internal const UInt32 FNV_prime = 16777619;
    }
}
