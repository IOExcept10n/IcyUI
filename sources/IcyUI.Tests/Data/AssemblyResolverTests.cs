using System.Reflection;
using Icy.Data;
using Icy.Tests.Logging;
using Icy.UI.Controls;

namespace Icy.Tests.Data
{
    public class AssemblyResolverTests
    {
        private static AssemblyResolver ResolverWithABrokenAssembly(CapturingLoggerFactory log)
        {
            Assembly broken = typeof(AssemblyResolverTests).Assembly;
            Assembly icy = typeof(Button).Assembly;
            return new AssemblyResolver
            {
                DefaultAssembly = broken,
                Logger = log.CreateLogger("Icy.Data.AssemblyResolver"),
                AssembliesProvider = () => [broken, icy],
                TypesProvider = a => a == broken
                    ? throw new ReflectionTypeLoadException([null], [new TypeLoadException("A dependency is missing.")])
                    : a.GetTypes(),
            };
        }

        [Fact]
        public void FindType_KeepsSearchingPastAnAssemblyThatFailsToLoadTypes()
        {
            AssemblyResolver resolver = ResolverWithABrokenAssembly(new CapturingLoggerFactory());

            Assert.Equal(typeof(Button), resolver.FindType("Button"));
        }

        [Fact]
        public void FindType_WarnsOncePerBrokenAssembly()
        {
            var log = new CapturingLoggerFactory();
            AssemblyResolver resolver = ResolverWithABrokenAssembly(log);

            resolver.FindType("Button");
            resolver.FindType("TextBlock");

            CapturedLog entry = Assert.Single(log.Entries, e => e.EventId.Id == 3001);
            Assert.Contains("IcyUI.Tests", entry.Message);
        }
    }
}
