using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace SharpGlyph
{
    public class Page
    {
        private int _tagId;
        private readonly List<TextSpan> _spans = new List<TextSpan>();
        private Matrix? _transformMatrix;

        internal Page(XElement xmlNode, Document document, FixedPage fixPage)
        {
            XmlNode = xmlNode ?? throw new ArgumentNullException(nameof(xmlNode));
            Document = document ?? throw new ArgumentNullException(nameof(document));
            FixPage = fixPage ?? throw new ArgumentNullException(nameof(fixPage));
        }

        public XElement XmlNode { get; }

        public Document Document { get; }

        public FixedPage FixPage { get; }

        public Matrix TransformMatrix
        {
            get
            {
                if (_transformMatrix == null)
                {
                    var matrix = Matrix.Identity;
                    matrix.Scale(72.0f / 96.0f, 72.0f / 96.0f);
                    _transformMatrix = matrix;
                }

                return _transformMatrix.Value;
            }
        }

        public void Init()
        {
            ParseMetadata();
        }

        public Rect BoundSize()
        {
            var matrix = new Matrix(FixPage.Width, 0, 0, FixPage.Height, 0, 0);
            var rect = new Rect(0, 0, 1, 1);
            rect.Transform(matrix);
            return rect;
        }

        public TextSpans GetPageContent()
        {
            var result = new TextSpans
            {
                PageSize = new Size(FixPage.Width, FixPage.Height)
            };
            foreach (var item in _spans)
            {
                var span = new TextSpanModel
                {
                    TagId = item.Glyphs.TagId,
                    FontName = item.Glyphs.Font.Name,
                    Box = item.SpanBox
                };
                span.AddRange(item.Select(x => new TextModel
                {
                    Box = x.GlyphBox,
                    Char = x.Char
                }));
                result.Add(span);
            }

            return result;
        }

        protected void ParseMetadata()
        {
            foreach (var element in XmlNode.Elements())
            {
                switch (element.Name.LocalName)
                {
                    case "Canvas":
                        var canvas = Canvas.Parse(this, element, null);
                        _spans.AddRange(canvas.GetContent());
                        break;
                    case "Glyphs":
                        var glyphs = Glyphs.Parse(this, element, null);
                        var span = glyphs.GetContent();
                        if (span != null)
                            _spans.Add(span);
                        break;
                }
            }
        }

        internal int GenerateTagId()
        {
            return _tagId++;
        }
    }
}