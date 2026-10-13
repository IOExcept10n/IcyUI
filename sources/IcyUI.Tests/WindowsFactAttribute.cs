namespace Icy.Tests
{
    /// <summary>A fact that needs Windows (for example the Windows fonts folder); skipped on other systems.</summary>
    public sealed class WindowsFactAttribute : FactAttribute
    {
        public WindowsFactAttribute()
        {
            if (!OperatingSystem.IsWindows())
                Skip = "Needs Windows.";
        }
    }
}
