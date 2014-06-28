
namespace OSBIDE.Data.DomainObjects
{
    public class VisualizationParams
    {
        public const int DEFAULT_TIMEOUT = 3;

        public TimeScale TimeScale { get; set; }
        public int? Timeout { get; set; }
    }
}
