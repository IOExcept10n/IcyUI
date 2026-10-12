using System.Drawing;
using Icy.UI;
using Icy.UI.Styles;
using Xunit;

namespace Icy.Tests.UI.Styles
{
    public class StyleTests
    {
        private class TestElement : UIElement
        {
            protected override Size MeasureContent() => Size.Empty;

            protected override void ArrangeContent()
            {
                // Test element doesn't need to arrange content
            }
        }

        [Fact]
        public void Apply_SetsPropertyThroughStyleTier()
        {
            var element = new TestElement();
            var style = new Style(typeof(TestElement));
            style.Setters["Width"] = 42f;

            element.Style = style;

            Assert.Equal(42f, element.Width);
        }

        [Fact]
        public void Apply_LocalValueOutranksStyle()
        {
            var element = new TestElement { Width = 10 };
            var style = new Style(typeof(TestElement));
            style.Setters["Width"] = 42f;

            element.Style = style;

            Assert.Equal(10f, element.Width);
        }

        [Fact]
        public void SettingNewStyle_RevertsOldStyleSetters()
        {
            var element = new TestElement();
            var style1 = new Style(typeof(TestElement));
            style1.Setters["Width"] = 42f;
            var style2 = new Style(typeof(TestElement));

            element.Style = style1;
            Assert.Equal(42f, element.Width);

            element.Style = style2;
            Assert.True(float.IsNaN(element.Width));
        }

        [Fact]
        public void BasedOn_AppliesBaseStyleFirstThenOverridesWin()
        {
            var baseStyle = new Style(typeof(TestElement));
            baseStyle.Setters["Width"] = 10f;
            baseStyle.Setters["Height"] = 20f;

            var derivedStyle = new Style(typeof(TestElement)) { BasedOn = baseStyle };
            derivedStyle.Setters["Width"] = 99f;

            var element = new TestElement { Style = derivedStyle };

            Assert.Equal(99f, element.Width);
            Assert.Equal(20f, element.Height);
        }

        [Fact]
        public void FluentStyle_SetsPropertyByExpression()
        {
            var style = new Style<TestElement>().Set(x => x.Width, 55f);
            var element = new TestElement { Style = style };

            Assert.Equal(55f, element.Width);
            Assert.True(style.Setters.ContainsKey("Width"));
        }

        [Fact]
        public void FluentStyle_InheritsFromBaseStyle()
        {
            var baseStyle = new Style<TestElement>().Set(x => x.Height, 20f);
            var derivedStyle = new Style<TestElement>().Set(x => x.Width, 99f).InheritsFrom(baseStyle);

            var element = new TestElement { Style = derivedStyle };

            Assert.Equal(99f, element.Width);
            Assert.Equal(20f, element.Height);
        }

        [Fact]
        public void VisualState_TieBetweenEquallySpecificStates_LastDeclaredWins()
        {
            // A mouse press keeps Hovered on: Pressed must still win, as the later state.
            var group = Group("CommonStates", State(ControlState.Hovered, 0.5f), State(ControlState.Pressed, 0.3f));
            var element = new TestElement();
            element.RegisterStateGroup(group);

            element.ControlState = ControlState.Hovered | ControlState.Pressed;

            Assert.Equal(0.3f, element.Opacity);
        }

        [Fact]
        public void BasedOn_GroupWithSameName_ReplacesBaseGroup()
        {
            var baseStyle = new Style(typeof(TestElement));
            baseStyle.StateGroups.Add(Group("CommonStates", State(ControlState.Hovered, 0.5f), State(ControlState.Pressed, 0.3f)));
            var derived = new Style(typeof(TestElement)) { BasedOn = baseStyle };
            derived.StateGroups.Add(Group("CommonStates", State(ControlState.Hovered, 0.8f)));
            var element = new TestElement { Style = derived };

            element.ControlState = ControlState.Hovered | ControlState.Pressed;
            Assert.Equal(0.8f, element.Opacity);

            element.ControlState = ControlState.Hovered;
            Assert.Equal(0.8f, element.Opacity);
        }

        [Fact]
        public void StateGroups_SettingSameProperty_LeavingHigherGroupRestoresLowerGroupValue()
        {
            var element = new TestElement();
            element.RegisterStateGroup(Group("CommonStates", State(ControlState.Pressed, 0.3f)));
            element.RegisterStateGroup(Group("FocusStates", State(ControlState.Focused, 0.8f)));

            element.ControlState = ControlState.Pressed | ControlState.Focused;
            Assert.Equal(0.8f, element.Opacity);

            element.ControlState = ControlState.Pressed;
            Assert.Equal(0.3f, element.Opacity);
        }

        [Fact]
        public void StateGroups_SettingSameProperty_LowerGroupChangeKeepsHigherGroupValue()
        {
            var element = new TestElement();
            element.RegisterStateGroup(Group("CommonStates", State(ControlState.Hovered, 0.5f), State(ControlState.Pressed, 0.3f)));
            element.RegisterStateGroup(Group("FocusStates", State(ControlState.Focused, 0.8f)));
            element.ControlState = ControlState.Hovered | ControlState.Focused;

            element.ControlState = ControlState.Pressed | ControlState.Focused;
            Assert.Equal(0.8f, element.Opacity);

            element.ControlState = ControlState.Pressed;
            Assert.Equal(0.3f, element.Opacity);
        }

        [Fact]
        public void UnnamedGroups_NeverReplaceEachOther()
        {
            var element = new TestElement();
            element.RegisterStateGroup(Group(null, State(ControlState.Hovered, 0.5f)));
            element.RegisterStateGroup(Group(null, State(ControlState.Focused, 0.8f)));

            element.ControlState = ControlState.Hovered;

            Assert.Equal(0.5f, element.Opacity);
        }

        [Fact]
        public void CodeRegisteredGroup_SurvivesAndOutranksLaterStyleGroupWithSameName()
        {
            var element = new TestElement();
            element.RegisterStateGroup(Group("CommonStates", State(ControlState.Hovered, 0.8f)));
            var style = new Style(typeof(TestElement));
            style.StateGroups.Add(Group("CommonStates", State(ControlState.Hovered, 0.5f), State(ControlState.Pressed, 0.3f)));

            element.Style = style;
            element.ControlState = ControlState.Hovered;
            Assert.Equal(0.8f, element.Opacity);

            element.ControlState = ControlState.Pressed;
            Assert.Equal(0.3f, element.Opacity);
        }

        [Fact]
        public void SettingNewStyle_RevertsBasedOnSettersAndGroups()
        {
            var baseStyle = new Style(typeof(TestElement));
            baseStyle.Setters["Height"] = 20f;
            baseStyle.StateGroups.Add(Group("CommonStates", State(ControlState.Hovered, 0.5f)));
            var derived = new Style(typeof(TestElement)) { BasedOn = baseStyle };
            var element = new TestElement { Style = derived, ControlState = ControlState.Hovered };
            Assert.Equal(0.5f, element.Opacity);

            element.Style = new Style(typeof(TestElement));

            Assert.True(float.IsNaN(element.Height));
            Assert.Equal(1f, element.Opacity);
        }

        [Fact]
        public void VisualState_AppliesBestMatchingState()
        {
            var group = new VisualStateGroup { Name = "CommonStates" };
            var normal = new VisualState(ControlState.Normal) { Name = "Normal" };
            normal.Setters["Opacity"] = 1f;
            var hovered = new VisualState(ControlState.Hovered) { Name = "Hovered" };
            hovered.Setters["Opacity"] = 0.8f;
            group.States.Add(normal);
            group.States.Add(hovered);

            var element = new TestElement();
            element.RegisterStateGroup(group);

            Assert.Equal(1f, element.Opacity);

            element.ControlState = ControlState.Hovered;
            Assert.Equal(0.8f, element.Opacity);

            element.ControlState = ControlState.Normal;
            Assert.Equal(1f, element.Opacity);
        }

        [Fact]
        public void VisualState_PrefersMoreSpecificMatch()
        {
            var group = new VisualStateGroup { Name = "CommonStates" };
            var hovered = new VisualState(ControlState.Hovered) { Name = "Hovered" };
            hovered.Setters["Opacity"] = 0.8f;
            var hoveredFocused = new VisualState(ControlState.Hovered | ControlState.Focused) { Name = "HoveredFocused" };
            hoveredFocused.Setters["Opacity"] = 0.5f;
            group.States.Add(hovered);
            group.States.Add(hoveredFocused);

            var element = new TestElement();
            element.RegisterStateGroup(group);

            element.ControlState = ControlState.Hovered | ControlState.Focused;

            Assert.Equal(0.5f, element.Opacity);
        }

        [Fact]
        public void UnregisterStateGroup_ClearsActiveStateSetters()
        {
            var group = new VisualStateGroup { Name = "CommonStates" };
            var hovered = new VisualState(ControlState.Hovered) { Name = "Hovered" };
            hovered.Setters["Opacity"] = 0.8f;
            group.States.Add(hovered);

            var element = new TestElement { ControlState = ControlState.Hovered };
            element.RegisterStateGroup(group);
            Assert.Equal(0.8f, element.Opacity);

            element.UnregisterStateGroup(group);
            Assert.Equal(1f, element.Opacity);
        }

        [Fact]
        public void LocalValue_OutranksActiveVisualState()
        {
            var group = new VisualStateGroup { Name = "CommonStates" };
            var hovered = new VisualState(ControlState.Hovered) { Name = "Hovered" };
            hovered.Setters["Opacity"] = 0.8f;
            group.States.Add(hovered);

            var element = new TestElement();
            element.RegisterStateGroup(group);
            element.ControlState = ControlState.Hovered;
            Assert.Equal(0.8f, element.Opacity);

            element.Opacity = 0.3f;
            Assert.Equal(0.3f, element.Opacity);

            element.ControlState = ControlState.Normal;
            Assert.Equal(0.3f, element.Opacity);
        }

        [Fact]
        public void FluentVisualState_SetsPropertyByExpression()
        {
            var group = new VisualStateGroup { Name = "CommonStates" };
            var hovered = new VisualState<TestElement>(ControlState.Hovered) { Name = "Hovered" }.Set(x => x.Opacity, 0.8f);
            group.States.Add(hovered);

            var element = new TestElement();
            element.RegisterStateGroup(group);
            element.ControlState = ControlState.Hovered;

            Assert.Equal(0.8f, element.Opacity);
        }

        [Fact]
        public void Style_WithStateGroup_RegistersItOnApply()
        {
            var group = new VisualStateGroup { Name = "CommonStates" };
            var hovered = new VisualState(ControlState.Hovered) { Name = "Hovered" };
            hovered.Setters["Opacity"] = 0.8f;
            group.States.Add(hovered);

            var style = new Style<TestElement>().WithStateGroup(group);
            var element = new TestElement { Style = style, ControlState = ControlState.Hovered };

            Assert.Equal(0.8f, element.Opacity);
        }

        [Fact]
        public void Apply_BasedOnCycle_ThrowsInsteadOfStackOverflow()
        {
            var element = new TestElement();
            var a = new Style(typeof(TestElement));
            var b = new Style(typeof(TestElement));
            a.BasedOn = b;
            b.BasedOn = a;

            Assert.Throws<InvalidOperationException>(() => element.Style = a);
        }

        private static VisualStateGroup Group(string? name, params VisualState[] states)
        {
            var group = new VisualStateGroup { Name = name };
            group.States.AddRange(states);
            return group;
        }

        private static VisualState State(ControlState state, float opacity)
        {
            var visualState = new VisualState(state);
            visualState.Setters["Opacity"] = opacity;
            return visualState;
        }
    }
}
