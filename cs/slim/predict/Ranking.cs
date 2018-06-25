namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// A ranking decision made by the VowpalWabbit predictor.
    /// </summary>
    public class Ranking
    {
        /// <summary>
        /// Action indicies ordered by underlying scores.
        /// </summary>
        public int[] Indicies { get; set; }

        /// <summary>
        /// Order by action index (Probabilities[0] <-> Action 0).
        /// </summary>
        public float[] Probabilities { get; set; }
    }
}
