using System.Collections.Generic;
using System.Windows;

namespace SharpGlyph
{
    public class TextSpans : List<TextSpanModel>
    {
        public Size PageSize { get; set; }
    }
}