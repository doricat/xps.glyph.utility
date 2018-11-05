using System.Xml.Linq;

namespace SharpGlyph
{
    public abstract class VisualElement
    {
        protected VisualElement(Page page, XElement xmlNode, VisualElement parent)
        {
            Page = page;
            XmlNode = xmlNode;
            Parent = parent;
        }

        public Page Page { get; }

        public XElement XmlNode { get; }

        public VisualElement Parent { get; }

        protected abstract void ParseMetadata();
    }
}