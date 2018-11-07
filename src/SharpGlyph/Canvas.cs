using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Xml.Linq;

namespace SharpGlyph
{
    public class Canvas : VisualElement
    {
        private readonly List<TextSpan> _spans = new List<TextSpan>();

        internal Canvas(Page page, XElement xmlNode, VisualElement parent) : base(page, xmlNode, parent)
        {
        }

        public static Canvas Parse(Page page, XElement xmlNode, VisualElement parent)
        {
            var canvas = new Canvas(page, xmlNode, parent);
            canvas.Init();
            return canvas;
        }

        public void Init()
        {
            ParseMetadata();
        }

        public List<TextSpan> GetContent()
        {
            return _spans;
        }

        protected override void ParseMetadata()
        {
            Opacity = double.Parse(XmlNode.Attribute("Opacity")?.Value ?? "1.0");
            var transformString = XmlNode.Attribute("RenderTransform")?.Value;
            //var clipString = XmlNode.Attribute("Clip")?.Value;
            //var opacityMaskString = XmlNode.Attribute("OpacityMask")?.Value;
            foreach (var xElement in XmlNode.Elements())
            {
                switch (xElement.Name.LocalName)
                {
                    case "Canvas.Resources":
                    case "Canvas.Clip":
                    case "Canvas.OpacityMask":
                        break;
                    case "Canvas.RenderTransform":
                        transformString = xElement.Elements().FirstOrDefault()?.Attribute("Matrix")?.Value;
                        break;
                }
            }

            RenderTransform = transformString == null ? Matrix.Identity : Matrix.Parse(transformString);
            foreach (var xElement in XmlNode.Elements())
            {
                switch (xElement.Name.LocalName)
                {
                    case "Path":
                        break;
                    case "Glyphs":
                        var glyphs = new Glyphs(Page, xElement, this);
                        var span = glyphs.GetContent();
                        if (span != null)
                            _spans.Add(span);
                        break;
                    case "Canvas":
                        var canvas = new Canvas(Page, xElement, this);
                        _spans.AddRange(canvas.GetContent());
                        break;
                }
            }
        }
    }
}