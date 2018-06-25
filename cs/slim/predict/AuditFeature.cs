using System.Diagnostics;

namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// A feature augmented with audit information.
    /// </summary>
    [DebuggerDisplay("Feature({Name}:{X})")]
    public class AuditFeature : Feature
    {
        public string Namespace;

        public string Name;
    }
}