using System;
using System.Xml.Linq;

namespace SharpGlyph
{
    public class Canvas : VisualElement
    {
        public Canvas(Page page, XElement xmlNode, VisualElement parent) : base(page, xmlNode, parent)
        {
        }

        protected override void ParseMetadata()
        {
            throw new NotImplementedException();
        }
    }
}