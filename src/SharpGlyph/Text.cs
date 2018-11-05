using System;
using System.Windows;
using System.Windows.Media;

namespace SharpGlyph
{
    public class Text
    {
        public Text(TextSpan span, double x, double y, uint glyphIndex, uint unicodeCharCode, char c, double absoluteWidth)
        {
            Span = span ?? throw new ArgumentNullException(nameof(span));
            X = x;
            Y = y;
            GlyphIndex = glyphIndex;
            UnicodeCharCode = unicodeCharCode;
            Char = c;
            AbsoluteWidth = absoluteWidth;
        }

        public TextSpan Span { get; }

        public double X { get; }

        public double Y { get; }

        public uint GlyphIndex { get; }

        public uint UnicodeCharCode { get; }

        public char Char { get; }

        public double AbsoluteWidth { get; }

        public Rect BoundGlyph(Matrix transformMatrix, Glyphs glyphs)
        {
            var font = Span.Glyphs.Font;
            Rect rect;
            if (font.BBoxTable != null && GlyphIndex < font.GlyphCount)
            {
                font.BBoxTable[GlyphIndex] = font.BoundGlyph(GlyphIndex, glyphs.Style);
                rect = font.BBoxTable[GlyphIndex];
                if (rect.IsEmpty)
                    rect = font.BBox;
            }
            else
            {
                rect = font.BBox;
            }

            rect.Transform(transformMatrix);

            return new Rect(rect.Location, new Size(font.BBoxTable != null && GlyphIndex < font.GlyphCount ? rect.Width : rect.Width * AbsoluteWidth / 100, rect.Height));
        }
    }
}