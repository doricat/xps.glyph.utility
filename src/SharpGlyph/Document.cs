using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Xml.Linq;

namespace SharpGlyph
{
    public class Document : DisposableBase
    {
        private readonly HashSet<string> _fixedDocumentNameSet = new HashSet<string>();
        private readonly HashSet<string> _fixedPageNameSet = new HashSet<string>();
        private readonly Dictionary<string, Font> _fontCaches = new Dictionary<string, Font>();

        public Document(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException();

            FileName = fileName;
        }

        public Document(Stream fileStream)
        {
            if (fileStream == null)
                throw new ArgumentNullException();

            fileStream.CopyTo(FileStream);
        }

        public string FileName { get; }

        public Stream FileStream { get; } = new MemoryStream();

        internal Package Zip { get; private set; }

        public int PageCount { get; private set; }

        protected Dictionary<int, FixedPage> FixedPages { get; } = new Dictionary<int, FixedPage>();

        protected LinkedList<FixedDocument> FixedDocuments { get; } = new LinkedList<FixedDocument>();

        public static Document Open(string fileName)
        {
            var doc = new Document(fileName);
            doc.Open();
            return doc;
        }

        public void Open()
        {
            if (!string.IsNullOrWhiteSpace(FileName))
                using (var stream = File.Open(FileName, FileMode.Open))
                    stream.CopyTo(FileStream);

            OpenWithStream();
        }

        public void Close()
        {
            Zip?.Close();
            FileStream?.Dispose();
            foreach (var item in _fontCaches)
                item.Value.Dispose();
        }

        public Page GetPage(int number)
        {
            return FixedPages.TryGetValue(number, out var page) ? page.Load() : null;
        }

        public IEnumerable<Page> GetPages()
        {
            for (var i = 1; i <= PageCount; i++)
            {
                if (FixedPages.TryGetValue(i, out var page))
                    yield return page.Load();
                else
                    yield break;
            }
        }

        protected void OpenWithStream()
        {
            try
            {
                Zip = Package.Open(FileStream);
            }
            catch (IOException e)
            {
                if (!string.IsNullOrWhiteSpace(FileName))
                    e.Data.Add("FileName", FileName);
                throw;
            }

            ReadPageList();
        }

        protected void ReadPageList()
        {
            var startPart = ReadStartPart();
            if (string.IsNullOrWhiteSpace(startPart))
                throw new Exception("Cannot find fixed document sequence start part.");

            ParseFixedDocumentSequence(startPart);
            foreach (var fixdoc in FixedDocuments)
            {
                var tuples = fixdoc.ParsePageMetadata();
                foreach (var tuple in tuples)
                    AddFixedPage(tuple.Item1, tuple.Item2, tuple.Item3);
            }
        }

        protected string ReadStartPart()
        {
            const string name = "/_rels/.rels";
            var uri = new Uri(name, UriKind.Relative);
            if (!Zip.PartExists(uri))
                return null;

            var part = Zip.GetPart(uri);
            var baseUri = Utility.GetDirectoryName(name);
            using (var stream = part.GetStream())
            {
                var doc = XDocument.Load(stream);
                var relationships = doc.Elements().FirstOrDefault(x => x.Name.LocalName == "Relationships")?.Elements().ToList() ?? new List<XElement>(0);
                foreach (var xElement in relationships)
                {
                    var targetAttr = xElement.Attribute("Target");
                    var typeAttr = xElement.Attribute("Type");
                    if (targetAttr != null && typeAttr != null)
                    {
                        var target = Utility.ResolveUrl(baseUri, targetAttr.Value);
                        if (string.Compare(typeAttr.Value, Schemas.RelStartPart, StringComparison.OrdinalIgnoreCase) == 0 ||
                            string.Compare(typeAttr.Value, Schemas.RelStartPartOxps, StringComparison.OrdinalIgnoreCase) == 0)
                            return target;
                    }
                }
            }

            return null;
        }

        protected void ParseFixedDocumentSequence(string name)
        {
            var uri = new Uri(name, UriKind.Relative);
            if (!Zip.PartExists(uri))
                return;
            var part = Zip.GetPart(uri);
            var baseUri = Utility.GetDirectoryName(name);
            using (var stream = part.GetStream())
            {
                var doc = XDocument.Load(stream);
                var documentReferences = doc.Elements().FirstOrDefault(x => x.Name.LocalName == "FixedDocumentSequence")?.Elements().ToList() ?? new List<XElement>();
                foreach (var xElement in documentReferences)
                {
                    var sourceAttr = xElement.Attribute("Source");
                    if (sourceAttr != null)
                    {
                        var source = Utility.ResolveUrl(baseUri, sourceAttr.Value);
                        AddFixedDocument(source);
                    }
                }
            }
        }

        protected void AddFixedPage(string name, int width, int height)
        {
            if (_fixedPageNameSet.Contains(name))
                return;
            ++PageCount;
            var page = new FixedPage(name, PageCount, width, height, this);
            FixedPages.Add(PageCount, page);
            _fixedPageNameSet.Add(name);
        }

        protected void AddFixedDocument(string name)
        {
            if (_fixedDocumentNameSet.Contains(name))
                return;
            var document = new FixedDocument(name, this);
            FixedDocuments.AddLast(document);
            _fixedDocumentNameSet.Add(name);
        }

        protected override void DisposeCore()
        {
            Close();
        }

        internal virtual Font LoadFont(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentNullException(nameof(uri));
            if (_fontCaches.ContainsKey(uri))
                return _fontCaches[uri];

            var nUri = new Uri(uri, UriKind.Relative);
            if (Zip.PartExists(nUri))
            {
                var part = Zip.GetPart(nUri);
                using (var stream = part.GetStream())
                {
                    var font = new Font(stream, uri);
                    _fontCaches.Add(uri, font);
                    return font;
                }
            }

            return null;
        }
    }
}