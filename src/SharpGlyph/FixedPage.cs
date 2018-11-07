using System;
using System.Xml.Linq;

namespace SharpGlyph
{
    public class FixedPage
    {
        internal FixedPage(string name, int number, int width, int height, Document document)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Number = number;
            Width = width;
            Height = height;
            Document = document ?? throw new ArgumentNullException(nameof(document));
        }

        public string Name { get; }

        public int Number { get; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public Document Document { get; }

        public Page Load()
        {
            var doc = LoadFixedPage();
            return new Page(doc.Root, Document, this);
        }

        protected XDocument LoadFixedPage()
        {
            XDocument doc = null;
            var uri = new Uri(Name, UriKind.Relative);
            if (Document.Zip.PartExists(uri))
            {
                var part = Document.Zip.GetPart(uri);
                using (var stream = part.GetStream())
                {
                    doc = XDocument.Load(stream);
                    if (doc.Root == null)
                        throw new InvalidOperationException("FixedPage missing root element.");
                    var root = doc.Root;
                    if (root.Name.LocalName != "FixedPage")
                        throw new InvalidOperationException("Expected FixedPage element.");
                    var widthAttr = root.Attribute("Width");
                    if (widthAttr == null)
                        throw new InvalidOperationException("FixedPage missing required attribute: Width.");
                    var heightAttr = root.Attribute("Height");
                    if (heightAttr == null)
                        throw new InvalidOperationException("FixedPage missing required attribute: Height.");
                    Width = (int) double.Parse(widthAttr.Value);
                    Height = (int) double.Parse(heightAttr.Value);
                }
            }

            return doc;
        }
    }
}