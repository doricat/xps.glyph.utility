using System.Windows.Media;
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

            TagId = page.GenerateTagId();
        }

        public Page Page { get; }

        public XElement XmlNode { get; }

        public VisualElement Parent { get; }

        public int TagId { get; protected set; }

        public Matrix RenderTransform { get; protected set; }

        public double Opacity { get; protected set; } = 1.0;

        protected virtual Matrix ParseTransform()
        {
            return Matrix.Multiply(RenderTransform, Parent?.ParseTransform() ?? Page.TransformMatrix);
        }

        protected abstract void ParseMetadata();
    }
}