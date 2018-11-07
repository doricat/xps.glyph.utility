using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SharpGlyph
{
    public class FixedDocument
    {
        internal FixedDocument(string name, Document document)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Document = document ?? throw new ArgumentNullException(nameof(document));
        }

        public string Name { get; }

        public Document Document { get; }

        internal virtual List<Tuple<string, int, int>> ParsePageMetadata()
        {
            var result = new List<Tuple<string, int, int>>();
            var uri = new Uri(Name, UriKind.Relative);
            if (Document.Zip.PartExists(uri))
            {
                var part = Document.Zip.GetPart(uri);
                using (var stream = part.GetStream())
                {
                    var baseUri = Utility.GetDirectoryName(Name);
                    var doc = XDocument.Load(stream);
                    var pageContents = doc.Elements().FirstOrDefault(x => x.Name.LocalName == "FixedDocument")?.Elements().ToList() ?? new List<XElement>();
                    foreach (var xElement in pageContents)
                    {
                        var sourceAttr = xElement.Attribute("Source");
                        var widthAtt = xElement.Attribute("Width");
                        var heightAtt = xElement.Attribute("Height");
                        var width = widthAtt != null ? int.Parse(widthAtt.Value) : 0;
                        var heidth = heightAtt != null ? int.Parse(heightAtt.Value) : 0;
                        if (sourceAttr != null)
                        {
                            var src = Utility.ResolveUrl(baseUri, sourceAttr.Value);
                            result.Add(new Tuple<string, int, int>(src, width, heidth));
                        }
                    }
                }
            }

            return result;
        }
    }
}