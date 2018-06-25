using System;

namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// Exception raised during model parsing.
    /// </summary>
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

        /// <summary>
        /// The field name at which the parsing error happenend.
        /// </summary>
        public string FieldName { get; private set; }
    }
}