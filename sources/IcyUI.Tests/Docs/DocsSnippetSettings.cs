namespace Icy.Tests.Docs
{
    /// <summary>Graphics settings shown by the editors snippet's PropertyGrid.</summary>
    public sealed class DocsSnippetSettings
    {
        public enum QualityLevel
        {
            Low,
            Medium,
            High,
        }

        public bool VSync { get; set; } = true;

        public int FrameRateLimit { get; set; } = 144;

        public QualityLevel Quality { get; set; } = QualityLevel.High;
    }
}
