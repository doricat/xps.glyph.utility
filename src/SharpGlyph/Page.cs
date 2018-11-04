using System;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace SharpGlyph
{
    public class Page
    {
        public XElement XmlNode { get; }

        public Document Document { get; }

        protected Rect BoundSize()
        {
            throw new NotImplementedException();
        }

        protected void ParseMetadata()
        {

        }

        internal int GenerateTagId()
        {
            throw new NotImplementedException();
        }
    }

    public class Canvas
    {

    }

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


    }

    public class Text
    {

    }

    public class TextSpan
    {

    }

    public class Font
    {

    }

    public class Glyph
    {

    }
}