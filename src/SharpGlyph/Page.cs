using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace SharpGlyph
{
    public class Page
    {
        private int _tagId;
        private readonly IList<TextSpan> _spans = new List<TextSpan>();

        internal Page(XElement xmlNode, Document document, FixPage fixPage)
        {
            XmlNode = xmlNode ?? throw new ArgumentNullException(nameof(xmlNode));
            Document = document ?? throw new ArgumentNullException(nameof(document));
            FixPage = fixPage ?? throw new ArgumentNullException(nameof(fixPage));
        }

        public XElement XmlNode { get; }

        public Document Document { get; }

        public FixPage FixPage { get; }

        public Rect BoundSize()
        {
            var matrix = new Matrix(FixPage.Width, 0, 0, FixPage.Height, 0, 0);
            var rect = new Rect(0, 0, 1, 1);
            rect.Transform(matrix);
            return rect;
        }

        protected void ParseMetadata()
        {
            foreach (var element in XmlNode.Elements())
            {
                switch (element.Name.LocalName)
                {
                    case "Canvas":
                        var canvas = new Canvas(this, element, null);
                        //_spans.AddRange(canvas.GetContent());
                        break;
                    case "Glyphs":
                        var glyphs = new Glyphs(this, element, null);
//                        var span = glyphs.GetContent();
//                        if (span != null)
//                            _spans.Add(span);
                        break;
                }
            }
        }

        internal int GenerateTagId()
        {
            return _tagId++;
        }
    }

    public class FixDocument
    {
    }
}