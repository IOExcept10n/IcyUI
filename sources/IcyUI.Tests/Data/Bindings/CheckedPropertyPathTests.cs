using Icy.Configuration;
using Icy.Data.Bindings;
using Icy.Tests.Logging;

namespace Icy.Tests.Data.Bindings
{
    public class CheckedPropertyPathTests
    {
        public interface INamed
        {
            string Name { get; }
        }

        public class Hero : INamed
        {
            public string Name { get; set; } = "Ann";
        }

        public sealed class Paladin : Hero
        {
        }

        public sealed class Villain
        {
            public string Name { get; set; } = "Zed";
        }

        private static (CheckedPropertyPath Path, CapturingLoggerFactory Log) Create(Type declared, bool compiled)
        {
            var log = new CapturingLoggerFactory();
            PropertyPath typed = new ReflectionConfiguration().GetTypedPath(declared, "Name", compiled);
            return (new CheckedPropertyPath(typed, declared, compiled, "Name", log.CreateLogger("test")), log);
        }

        [Fact]
        public void GetTypedPath_IsSharedPerTypePathAndKind()
        {
            var types = new ReflectionConfiguration();

            Assert.Same(types.GetTypedPath(typeof(Hero), "Name", compiled: true), types.GetTypedPath(typeof(Hero), "Name", compiled: true));
            Assert.NotSame(types.GetTypedPath(typeof(Hero), "Name", compiled: true), types.GetTypedPath(typeof(Hero), "Name", compiled: false));
            Assert.IsType<CompiledPropertyPath>(types.GetTypedPath(typeof(Hero), "Name", compiled: true));
            Assert.IsNotType<CompiledPropertyPath>(types.GetTypedPath(typeof(Hero), "Name", compiled: false));
        }

        [Fact]
        public void GetTypedPath_MissingMember_Throws()
        {
            Assert.Throws<ArgumentException>(() => new ReflectionConfiguration().GetTypedPath(typeof(Hero), "Nmae", compiled: false));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void SubclassAndInterface_UseTheTypedPath_WithoutWarning(bool compiled)
        {
            var (path, log) = Create(typeof(INamed), compiled);

            Assert.Equal("Ann", path.GetValue(new Paladin()));
            Assert.Empty(log.Entries);
        }

        [Fact]
        public void Static_WrongType_WarnsOnceAndReadsDynamically()
        {
            var (path, log) = Create(typeof(Hero), compiled: false);

            Assert.Equal("Zed", path.GetValue(new Villain()));
            Assert.Equal("Zed", path.GetValue(new Villain()));
            Assert.Equal("Ann", path.GetValue(new Hero()));

            CapturedLog entry = Assert.Single(log.Entries, e => e.EventId.Id == 2002);
            Assert.Contains(nameof(Hero), entry.Message);
            Assert.Contains(nameof(Villain), entry.Message);
        }

        [Fact]
        public void Compiled_WrongType_Throws()
        {
            var (path, _) = Create(typeof(Hero), compiled: true);

            var ex = Assert.Throws<BindingException>(() => path.GetValue(new Villain()));
            Assert.Contains(nameof(Hero), ex.Message);
            Assert.Contains(nameof(Villain), ex.Message);
        }

        [Fact]
        public void SetValue_GoesThroughTheTypedPath()
        {
            var (path, _) = Create(typeof(Hero), compiled: true);
            var hero = new Hero();

            path.SetValue(hero, "Bea");

            Assert.Equal("Bea", hero.Name);
            Assert.Equal(typeof(string), path.GetPropertyType(hero));
        }
    }
}
