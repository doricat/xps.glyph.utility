using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace SharpGlyph
{
    public class Glyphs : VisualElement
    {
        public Glyphs(Page page, XElement xmlNode, VisualElement parent) : base(page, xmlNode, parent)
        {
        }

        public int BidiLevel { get; protected set; }

        public string Fill { get; protected set; }

        public double FontRenderingEmSize { get; protected set; }

        public string FontUri { get; protected set; }

        public double OriginX { get; protected set; }

        public double OriginY { get; protected set; }

        public bool IsSideways { get; protected set; }

        public string Indices { get; protected set; }

        public string UnicodeString { get; protected set; }

        public StyleSimulations Style { get; protected set; }

        public bool IsEffective { get; protected set; }

        public Font Font { get; protected set; }

        public TextSpan TextSpan { get; protected set; }

        public static Glyphs Parse(Page page, XElement xmlNode, VisualElement parent)
        {
            var glyphs = new Glyphs(page, xmlNode, parent);
            glyphs.Init();
            return glyphs;
        }

        public void Init()
        {
            ParseMetadata();
        }

        public virtual TextSpan GetContent()
        {
            return !IsEffective ? null : TextSpan;
        }

        protected override void ParseMetadata()
        {
            var originXString = XmlNode.Attribute("OriginX")?.Value;
            var originYString = XmlNode.Attribute("OriginY")?.Value;
            var fontSizeString = XmlNode.Attribute("FontRenderingEmSize")?.Value;
            FontUri = XmlNode.Attribute("FontUri")?.Value;
            if (originXString == null || originYString == null || fontSizeString == null || FontUri == null)
                throw new Exception("Missing attributes in glyphs element.");
            OriginX = double.Parse(originXString);
            OriginY = double.Parse(originYString);
            FontRenderingEmSize = double.Parse(fontSizeString);
            UnicodeString = XmlNode.Attribute("UnicodeString")?.Value;
            Indices = XmlNode.Attribute("Indices")?.Value;
            if (string.IsNullOrEmpty(UnicodeString) && string.IsNullOrEmpty(Indices))
            {
                IsEffective = false;
                return;
            }
            IsSideways = bool.Parse(XmlNode.Attribute("IsSideways")?.Value ?? "false");
            Opacity = double.Parse(XmlNode.Attribute("Opacity")?.Value ?? "1.0");
            if (Math.Abs(Opacity) <= 0)
            {
                IsEffective = false;
                return;
            }
            BidiLevel = int.Parse(XmlNode.Attribute("BidiLevel")?.Value ?? "0");
            Style = (StyleSimulations)Enum.Parse(typeof(StyleSimulations), XmlNode.Attribute("StyleSimulations")?.Value ?? "None", false);
            //var fillString = XmlNode.Attribute("Fill")?.Value;
            var transformString = XmlNode.Attribute("RenderTransform")?.Value;
            //var clipString = XmlNode.Attribute("Clip")?.Value;
            //var opacityMaskString = XmlNode.Attribute("OpacityMask")?.Value;
            foreach (var xElement in XmlNode.Elements())
                switch (xElement.Name.LocalName)
                {
                    case "Glyphs.RenderTransform":
                        transformString = xElement.Elements().FirstOrDefault()?.Attribute("Matrix")?.Value;
                        break;
                    case "Glyphs.OpacityMask":
                        break;
                    case "Glyphs.Clip":
                        break;
                    case "Glyphs.Fill":
                        break;
                }
            RenderTransform = transformString == null ? Matrix.Identity : Matrix.Parse(transformString);
            IsEffective = true;
            var font = LoadFont();
            Font = font ?? throw new InvalidOperationException();
            TextSpan = ParseText();
            ParseRenderArea();
        }

        protected virtual TextSpan ParseText()
        {
            var x = OriginX;
            var tm = Matrix.Identity;
            var unicode = Regex.Replace(UnicodeString, "^{}", "");
            var indices = Indices;
            if (IsSideways)
            {
                tm.Rotate(90);
                tm.Scale(-FontRenderingEmSize, FontRenderingEmSize);
            }
            else
            {
                tm.Scale(FontRenderingEmSize, -FontRenderingEmSize);
            }
            var span = new TextSpan(this, tm, IsSideways, BidiLevel, BidiLevel);
            while (!string.IsNullOrEmpty(unicode) || !string.IsNullOrWhiteSpace(indices))
            {
                uint charCode = '?';
                var codeUnitCount = 1;
                var glyphCount = 1;
                if (!string.IsNullOrWhiteSpace(indices))
                {
                    var tuple = ParseClusterMapping(ref indices);
                    codeUnitCount = tuple.Item1;
                    glyphCount = tuple.Item2;
                }
                while (codeUnitCount-- > 0)
                    if (!string.IsNullOrEmpty(unicode))
                    {
                        charCode = unicode[0];
                        unicode = unicode.Substring(1);
                    }
                while (glyphCount-- > 0)
                {
                    double advance;
                    double uOffset = 0;
                    double vOffset = 0;
                    uint? glyphIndex = null;
                    if (!string.IsNullOrWhiteSpace(indices))
                        glyphIndex = ParseGlyphIndex(ref indices);
                    if (glyphIndex == null)
                        glyphIndex = Font.EncodeFontChar(charCode);
                    var mtx = Font.MeasureFontGlyph(glyphIndex.Value);
                    if (IsSideways)
                        advance = mtx.Item2 * 100;
                    else if (BidiLevel == 1)
                        advance = -mtx.Item1 * 100;
                    else
                        advance = mtx.Item1 * 100;
                    if (Style == StyleSimulations.BoldSimulation)
                        advance *= 1.02f;
                    var advance2 = advance;
                    if (!string.IsNullOrWhiteSpace(indices))
                    {
                        var tuple = ParseGlyphMetrics(ref indices, advance);
                        advance = tuple.Item1;
                        uOffset = tuple.Item2;
                        vOffset = tuple.Item3;
                        if (advance <= 110)
                            advance2 = advance;
                        if (indices.StartsWith(";"))
                            indices = indices.Substring(1);
                    }
                    if (BidiLevel == 1)
                        uOffset = -mtx.Item1 * 100 - uOffset;
                    uOffset = uOffset * 0.01f * FontRenderingEmSize;
                    vOffset = vOffset * 0.01f * FontRenderingEmSize;
                    if (IsSideways)
                    {
                        tm.OffsetX = x + uOffset + mtx.Item3 * FontRenderingEmSize;
                        tm.OffsetY = OriginY - vOffset + mtx.Item1 * 0.5f * FontRenderingEmSize;
                    }
                    else
                    {
                        tm.OffsetX = x + uOffset;
                        tm.OffsetY = OriginY + vOffset;
                    }
                    span.Add(new Text(span, tm.OffsetX, tm.OffsetY, glyphIndex.Value, charCode, (char)charCode, advance2));
                    x += advance * 0.01f * FontRenderingEmSize;
                }
            }
            return span;
        }

        protected Rect ParseRenderArea()
        {
            return TextSpan.BoundText(ParseTransform());
        }

        protected Font LoadFont()
        {
            return Page.Document.LoadFont(FontUri);
        }

        protected Tuple<int, int> ParseClusterMapping(ref string s)
        {
            var codeUnitCount = 1;
            var glyphCount = 1;
            var match = Regex.Match(s, @"^\(\d+:\d+\)");
            if (match.Success)
            {
                var matches = Regex.Matches(match.Value, @"\d+");
                codeUnitCount = int.Parse(matches[0].Value);
                glyphCount = int.Parse(matches[1].Value);
                s = Regex.Replace(s, @"^\(\d+:\d+\)", "");
            }
            return new Tuple<int, int>(codeUnitCount, glyphCount);
        }

        protected uint? ParseGlyphIndex(ref string s)
        {
            var match = Regex.Match(s, @"^\d+");
            uint? result = null;
            if (match.Success)
            {
                result = uint.Parse(match.Value);
                s = Regex.Replace(s, @"^\d+", "");
            }
            return result;
        }

        protected Tuple<double, double, double> ParseGlyphMetrics(ref string s, double advance)
        {
            const string pattern = @"^[-+]?[0-9]*\.?[0-9]+";
            Match match;
            var nAdvance = advance;
            double uOffset = 0;
            double vOffset = 0;
            if (s.FirstOrDefault() == ',')
            {
                s = s.Substring(1);
                match = Regex.Match(s, pattern);
                if (match.Success)
                {
                    nAdvance = double.Parse(match.Value);
                    s = s.Substring(match.Length);
                }
            }
            if (s.FirstOrDefault() == ',')
            {
                s = s.Substring(1);
                match = Regex.Match(s, pattern);
                if (match.Success)
                {
                    uOffset = double.Parse(match.Value);
                    s = s.Substring(match.Length);
                }
            }
            if (s.FirstOrDefault() == ',')
            {
                s = s.Substring(1);
                match = Regex.Match(s, pattern);
                if (match.Success)
                {
                    vOffset = double.Parse(match.Value);
                    s = s.Substring(match.Length);
                }
            }

            return new Tuple<double, double, double>(nAdvance, uOffset, vOffset);
        }
    }
}