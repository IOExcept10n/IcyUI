// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Text;
using Icy.Data.Markup.Attributes;
using Icy.Markup;
using Icy.Rendering;
using Icy.Rendering.Fonts;

namespace Icy.UI.Controls
{
    /// <summary>
    /// Displays a single run of text using a font resolved from <see cref="UIElement.Configuration"/>'s
    /// <see cref="Icy.Configuration.IcyConfiguration.Fonts"/> service.
    /// </summary>
    /// <remarks>
    /// <para>
    /// One style for the whole text. <see cref="TextWrapping"/> breaks it into lines by the width it has, and
    /// <see cref="TextAlignment"/> places each line within that width; explicit <c>\n</c> line breaks always break.
    /// </para>
    /// Renders nothing (and measures as empty) until
    /// <see cref="Text"/> is set and a font resolves; <see cref="FontFamily"/> is optional, since resolution falls
    /// back to <see cref="Icy.Rendering.Fonts.FontSystem.DefaultFontFamily"/> and then
    /// <see cref="Icy.Rendering.Fonts.FontSystem.FallbackFont"/>.
    /// </remarks>
    [ContentProperty(nameof(Text))]
    public class TextBlock : UIElement
    {
        private string fontFamily = string.Empty;
        private float fontSize = 16;
        private FontStyle fontStyle = FontStyle.Regular;
        private string text = string.Empty;
        private TextAlignment textAlignment = TextAlignment.Left;
        private TextWrapping textWrapping = TextWrapping.NoWrap;
        private TextLayout? layout;

        /// <summary>
        /// Gets or sets the font family to render <see cref="Text"/> with.
        /// </summary>
        /// <remarks>
        /// Resolved via <see cref="Icy.Rendering.Fonts.FontSystem.GetOrLoad(FontInfo)"/> - the family must already
        /// be importable/loaded by the configured <see cref="Icy.Rendering.Fonts.FontSystem"/> (e.g. via
        /// <see cref="Icy.Rendering.Fonts.FontSystem.ImportFont{TContext}"/> or
        /// <see cref="Icy.Rendering.Fonts.FontSystem.EnableSystemFonts"/>) for text to actually render.
        /// </remarks>
        [Category("Appearance")]
        [DefaultValue("")]
        [RegisterReference]
        [AffectsMeasure]
        public string FontFamily
        {
            get => fontFamily;
            set
            {
                if (SetProperty(ref fontFamily, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Gets or sets the font size, in pixels.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(16f)]
        [RegisterReference]
        [AffectsMeasure]
        public float FontSize
        {
            get => fontSize;
            set
            {
                if (SetProperty(ref fontSize, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Gets or sets the font style (bold/italic/etc.) to render <see cref="Text"/> with.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(FontStyle.Regular)]
        [RegisterReference]
        [AffectsMeasure]
        public FontStyle FontStyle
        {
            get => fontStyle;
            set
            {
                if (SetProperty(ref fontStyle, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Gets or sets the text to display.
        /// </summary>
        [Category("Content")]
        [DefaultValue("")]
        [RegisterReference]
        [AffectsMeasure]
        public string Text
        {
            get => text;
            set
            {
                if (SetProperty(ref text, value ?? string.Empty))
                {
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the text breaks into lines to fit the width it has.
        /// </summary>
        /// <remarks>
        /// <para>The width a wrapping text block breaks its lines at is, in order:</para>
        /// <list type="number">
        /// <item><description>its own <see cref="UIElement.Width"/>, when set;</description></item>
        /// <item><description>
        /// otherwise, the width of the slot its parent arranges it in, capped by <see cref="UIElement.MaxWidth"/>. Its
        /// height follows the lines within the same frame.
        /// </description></item>
        /// </list>
        /// <para>
        /// In a slot that sizes to its content, such as an <c>Auto</c> grid column, give it a <see cref="UIElement.Width"/>
        /// or <see cref="UIElement.MaxWidth"/>: that slot is only as wide as the text already is.
        /// </para>
        /// </remarks>
        [Category("Layout")]
        [DefaultValue(TextWrapping.NoWrap)]
        [RegisterReference]
        [AffectsMeasure]
        public TextWrapping TextWrapping
        {
            get => textWrapping;
            set
            {
                if (SetProperty(ref textWrapping, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Gets or sets where each line sits within the text block's width.
        /// </summary>
        /// <remarks>
        /// Lines are placed within the width the text block is arranged with, so a text block that sizes to its text has
        /// no room to move them: give it a <see cref="UIElement.Width"/>, or stretch it.
        /// </remarks>
        [Category("Layout")]
        [DefaultValue(TextAlignment.Left)]
        [RegisterReference]
        public TextAlignment TextAlignment
        {
            get => textAlignment;
            set
            {
                if (SetProperty(ref textAlignment, value))
                {
                    InvalidateVisual();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the text shows as it is: no wrapping and left-aligned, so no line layout is needed.
        /// </summary>
        private bool IsPlain => TextWrapping == TextWrapping.NoWrap && TextAlignment == TextAlignment.Left;

        /// <inheritdoc/>
        protected override void ArrangeContent()
        {
            // A leaf element, with nothing to arrange. A wrapping text block measured before its parent gave it a slot (or
            // for a slot of another width) has lines for the wrong width: ask for another pass, which measures it again.
            if (layout != null && TextWrapping == TextWrapping.Wrap && float.IsNaN(Width) && WrapWidth() != layout.WrapWidth)
                Canvas?.RequestRelayout(this);
        }

        /// <inheritdoc/>
        protected override Size MeasureContent()
        {
            IFont? font = ResolveFont();
            if (font == null || Text.Length == 0)
                return Size.Empty;

            // Plain left-aligned text (the default, and what HUD counters that change every frame use) needs no line layout.
            bool plain = IsPlain;
            string shown = plain ? Text : GetLayout(font).Text;
            Vector2 measured = font.MeasureString(shown, DefaultRenderingOptions());

            // The height comes from the font's lines, not the ink: the baseline sits at the ascent below the top and a
            // descender reaches the descent below it, while MeasureString's ink extent leaves out the space above the
            // tallest glyph (cutting descenders off) and changes from word to word.
            FontMetrics metrics = font.Metrics;
            int count = plain ? Text.Count(c => c == '\n') + 1 : GetLayout(font).Lines.Length;
            float height = (metrics.Ascent - metrics.Descent) + ((count - 1) * (metrics.Ascent - metrics.Descent + metrics.LineGap));
            return new Size((int)MathF.Ceiling(measured.X), (int)MathF.Ceiling(MathF.Max(height, measured.Y)));
        }

        /// <inheritdoc/>
        protected override void OnRender(IRenderContext context)
        {
            IFont? font = ResolveFont();
            if (font == null || Text.Length == 0)
                return;

            FontRenderingOptions options = DefaultRenderingOptions();
            if (TextAlignment == TextAlignment.Left)
            {
                font.DrawString(context, IsPlain ? Text : GetLayout(font).Text, options);
                return;
            }

            TextLayout lines = GetLayout(font);

            // Each line on its own, shifted within the width, one SpriteFont line step apart (the step it uses for '\n').
            float factor = TextAlignment == TextAlignment.Center ? 0.5f : 1f;
            float area = ActualBounds.Width - Padding.Width;
            FontMetrics metrics = font.Metrics;
            int lineStep = (int)(metrics.Ascent - metrics.Descent + metrics.LineGap);
            for (int i = 0; i < lines.Lines.Length; i++)
            {
                float offset = MathF.Max(0, factor * (area - lines.Widths[i]));
                font.DrawString(context, lines.Lines[i], options with { Position = options.Position + new Vector2(offset, i * lineStep) });
            }
        }

        /// <summary>
        /// Breaks <paramref name="paragraph"/>, a line without explicit breaks, into lines no wider than
        /// <paramref name="maxWidth"/>: at spaces where it can, and inside a word wider than a whole line.
        /// </summary>
        private static void BreakParagraph(IFont font, in FontRenderingOptions options, string paragraph, float maxWidth, List<string> lines)
        {
            float spaceWidth = font.MeasureAdvance(" ", options).X;
            var line = new StringBuilder();
            float lineWidth = 0;
            foreach (string word in paragraph.Split(' '))
            {
                float wordWidth = font.MeasureAdvance(word, options).X;
                float needed = line.Length == 0 ? wordWidth : lineWidth + spaceWidth + wordWidth;
                if (needed <= maxWidth)
                {
                    if (line.Length > 0)
                        line.Append(' ');
                    line.Append(word);
                    lineWidth = needed;
                    continue;
                }

                if (line.Length > 0)
                {
                    lines.Add(line.ToString());
                    line.Clear();
                    lineWidth = 0;
                }

                if (wordWidth <= maxWidth)
                {
                    line.Append(word);
                    lineWidth = wordWidth;
                    continue;
                }

                // A word wider than a whole line: as many whole text elements as fit, at least one per line.
                int start = 0;
                while (start < word.Length)
                {
                    int end = start + StringInfo.GetNextTextElementLength(word, start);
                    while (end < word.Length)
                    {
                        int next = end + StringInfo.GetNextTextElementLength(word, end);
                        if (font.MeasureAdvance(word[start..next], options).X > maxWidth)
                            break;
                        end = next;
                    }

                    if (end < word.Length)
                    {
                        lines.Add(word[start..end]);
                    }
                    else
                    {
                        line.Append(word, start, end - start);
                        lineWidth = font.MeasureAdvance(word[start..], options).X;
                    }

                    start = end;
                }
            }

            lines.Add(line.ToString());
        }

        private FontRenderingOptions DefaultRenderingOptions() => new(
            Position: new Vector2(Padding.Left, Padding.Top),
            Scale: null,
            Rotation: 0,
            Origin: Vector2.Zero,
            CharacterSpacing: 0,
            LineSpacing: 0,
            Color: Foreground,
            Depth: ZIndex,
            Effect: null);

        /// <summary>
        /// Gets the width lines break at: <see cref="float.PositiveInfinity"/> without wrapping.
        /// </summary>
        private float WrapWidth()
        {
            if (TextWrapping == TextWrapping.NoWrap)
                return float.PositiveInfinity;
            if (!float.IsNaN(Width))
                return Width;

            float slot = LastContainerBounds is { } bounds ? MathF.Max(0, bounds.Width - Margin.Width - Padding.Width) : float.PositiveInfinity;
            return float.IsNaN(MaxWidth) ? slot : MathF.Min(MaxWidth, slot);
        }

        /// <summary>
        /// Gets the lines <see cref="Text"/> shows in, breaking them again only when the text, the font or the width changed.
        /// </summary>
        private TextLayout GetLayout(IFont font)
        {
            float wrapWidth = WrapWidth();
            if (layout is { } cached && ReferenceEquals(cached.Source, Text) && cached.Font == font && cached.WrapWidth == wrapWidth)
                return cached;

            FontRenderingOptions options = DefaultRenderingOptions();
            var lines = new List<string>();
            foreach (string paragraph in Text.Split('\n'))
            {
                if (float.IsPositiveInfinity(wrapWidth))
                    lines.Add(paragraph);
                else
                    BreakParagraph(font, options, paragraph, wrapWidth, lines);
            }

            float[] widths = [.. lines.Select(line => font.MeasureAdvance(line, options).X)];
            return layout = new TextLayout(Text, font, wrapWidth, [.. lines], widths);
        }

        /// <summary>
        /// Resolves the font to measure and draw <see cref="Text"/> with.
        /// </summary>
        /// <returns>The resolved font, or <see langword="null"/> when nothing at all could be resolved.</returns>
        /// <remarks>
        /// Falls back in three steps: this element's own <see cref="FontFamily"/>, then
        /// <see cref="Icy.Rendering.Fonts.FontSystem.DefaultFontFamily"/>, then
        /// <see cref="Icy.Rendering.Fonts.FontSystem.FallbackFont"/>. An element that never had a family assigned
        /// used to short-circuit to <see langword="null"/> here and render nothing even when the configuration had
        /// a perfectly good fallback - which markup's bare-text sugar (<c>&lt;Button&gt;Click Me&lt;/Button&gt;</c>)
        /// would have hit on every element it creates.
        /// </remarks>
        private IFont? ResolveFont()
        {
            return Configuration?.Fonts?.ResolveTextFont(FontFamily, FontSize, FontStyle);
        }

        /// <summary>
        /// The lines <see cref="Text"/> was broken into for one font and width.
        /// </summary>
        /// <param name="Source">The text the lines come from.</param>
        /// <param name="Font">The font they were measured with.</param>
        /// <param name="WrapWidth">The width they were broken at.</param>
        /// <param name="Lines">The lines.</param>
        /// <param name="Widths">Each line's advance width.</param>
        private sealed record TextLayout(string Source, IFont Font, float WrapWidth, string[] Lines, float[] Widths)
        {
            /// <summary>
            /// Gets the lines joined with <c>\n</c>, as the font measures and draws them.
            /// </summary>
            public string Text { get; } = string.Join('\n', Lines);
        }
    }
}
