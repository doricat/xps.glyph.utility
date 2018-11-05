using System;
using System.Xml.Linq;

namespace SharpGlyph
{
    public class Glyphs : VisualElement
    {
        public Glyphs(Page page, XElement xmlNode, VisualElement parent) : base(page, xmlNode, parent)
        {
        }

        public int BidiLevel { get; protected set; }

        public string Fill { get; protected set; }

        public double FontRenderingEmSize { get; protected set; }

        public string FontUri { get; protected set; }

        public double OriginX { get; protected set; }

        public double OriginY { get; protected set; }

        public bool IsSideways { get; protected set; }

        public string Indices { get; protected set; }

        public string UnicodeString { get; protected set; }

        public StyleSimulations Style { get; protected set; }

        public bool IsEffective { get; protected set; }

        public Font Font { get; protected set; }

        public TextSpan TextSpan { get; protected set; }

        protected override void ParseMetadata()
        {
            throw new NotImplementedException();
        }
    }
}