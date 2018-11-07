using System.Collections.Generic;
using System.Drawing;

namespace SharpGlyph
{
    public class TextSpanModel : List<TextModel>
    {
        public int TagId { get; set; }

        public string FontName { get; set; }

        public RectangleF Box { get; set; }

        public bool IsSideways { get; set; }
    }
}