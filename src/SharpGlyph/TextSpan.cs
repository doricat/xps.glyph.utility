using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace SharpGlyph
{
    public class TextSpan : List<Text>
    {
        internal TextSpan(Glyphs glyphs, Matrix transformMatrix, bool isSideways, int bidiLevel, int markupDir)
        {
            Glyphs = glyphs;
            TransformMatrix = transformMatrix;
            IsSideways = isSideways;
            BidiLevel = bidiLevel;
            MarkupDir = markupDir;
        }

        public Glyphs Glyphs { get; }

        public Matrix TransformMatrix { get; }

        public bool IsSideways { get; }

        public int BidiLevel { get; }

        public int MarkupDir { get; }

        internal Rect SpanBox { get; set; }

        public Rect BoundText(Matrix transformMatrix)
        {
            var tm = TransformMatrix;
            var result = Rect.Empty;
            foreach (var text in this)
            {
                if (text.GlyphIndex > 0)
                {
                    tm.OffsetX = text.X;
                    tm.OffsetY = text.Y;
                    var trm = Matrix.Multiply(tm, transformMatrix);
                    var rect = text.BoundGlyph(trm, Glyphs);
                    result = Rect.Union(result, rect);
                }
            }

            if (!result.IsEmpty)
                result.Inflate(new Size(1, 1));
            SpanBox = result;

            return result;
        }
    }
}