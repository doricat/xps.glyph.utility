using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using SharpFont;
using SharpFont.TrueType;

namespace SharpGlyph
{
    public class Font
    {
        protected byte[] Data { get; set; }

        protected Face FtFace { get; set; }

        public string Uri { get; }

        public string Name => FtFace.FamilyName;

        public Rect BBox { get; protected set; }

        public int GlyphCount => FtFace.GlyphCount;

        public Rect[] BBoxTable { get; protected set; }

        public void Init(byte[] buf)
        {
            var extension = Path.GetExtension(Uri);
            if (extension != null && extension.ToLower() == ".odttf")
                DeobfuscateFontResource(buf);
            else
                Data = buf;
            FtFace = new Face(new Library(), Data, 0);
            SetFontBBox();
            SetBBoxTable();
        }

        public uint EncodeFontChar(uint code)
        {
            var gid = FtFace.GetCharIndex(code);
            if (gid == 0 && FtFace.CharMap != null && FtFace.CharMap.PlatformId == PlatformId.Microsoft && FtFace.CharMap.EncodingId == 0)
                gid = FtFace.GetCharIndex(0xF000 | code);
            return gid;
        }

        public Tuple<double, double, double> MeasureFontGlyph(uint gid)
        {
            var hadv = FtFace.GetAdvance(gid, LoadFlags.NoScale | LoadFlags.IgnoreTransform);
            var vadv = FtFace.GetAdvance(gid, LoadFlags.NoScale | LoadFlags.IgnoreTransform | LoadFlags.VerticalLayout);
            return new Tuple<double, double, double>(hadv.Value / (float) FtFace.UnitsPerEM, vadv.Value / (float) FtFace.UnitsPerEM, FtFace.Ascender / (float) FtFace.UnitsPerEM);
        }

        protected virtual void DeobfuscateFontResource(byte[] buf)
        {
            if (buf.Length < 32)
                throw new InvalidOperationException("insufficient data for font deobfuscation");
            if (!Guid.TryParse(Regex.Match(Uri, "[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}", RegexOptions.IgnoreCase).Value, out var name))
                throw new InvalidOperationException("cannot extract GUID from obfuscated font part name");
            var key = name.ToByteArray();
            for (var i = 0; i < key.Length; i++)
            {
                buf[i] ^= key[15 - i];
                buf[i + 16] ^= key[15 - i];
            }

            Data = buf;
        }

        protected void SetFontBBox()
        {
            var x0 = FtFace.BBox.Left / (float) FtFace.UnitsPerEM;
            var y0 = FtFace.BBox.Bottom / (float) FtFace.UnitsPerEM;
            var x1 = FtFace.BBox.Right / (float) FtFace.UnitsPerEM;
            var y1 = FtFace.BBox.Top / (float) FtFace.UnitsPerEM;
            if (x0 >= x1 || y0 >= y1)
                BBox = new Rect(new Point(-1, -1), new Point(2, 2));
            else
                BBox = new Rect(new Point(x0, y0), new Point(x1, y1));
        }

        protected void SetBBoxTable()
        {
            if (FtFace.GlyphCount <= 4096)
            {
                BBoxTable = new Rect[FtFace.GlyphCount];
                for (var i = 0; i < FtFace.GlyphCount; i++)
                    BBoxTable[i] = new Rect(new Point(1, 1), new Point(-1, -1));
            }
            else
            {
                BBoxTable = null;
            }
        }

        protected virtual Matrix AdjustGlyphWidth(uint glyphIndex)
        {
            return Matrix.Identity;
        }

        internal virtual Rect BoundGlyph(uint glyphIndex, StyleSimulations style)
        {
            var recip = 1 / (float) FtFace.UnitsPerEM;
            //var strength = 0.02f;
            var trm = AdjustGlyphWidth(glyphIndex);
            var m = new FTMatrix((int) trm.M11 * 65536, (int) trm.M21 * 65536, (int) trm.M12 * 65536, (int) trm.M22 * 65536);
            var v = new FTVector(Fixed16Dot16.FromDouble(trm.OffsetX * 65536), Fixed16Dot16.FromDouble(trm.OffsetY * 65536));
            FtFace.SetCharSize(Fixed26Dot6.FromInt32(FtFace.UnitsPerEM), Fixed26Dot6.FromInt32(FtFace.UnitsPerEM), 72, 72);
            FtFace.SetTransform(m, v);
            FtFace.LoadGlyph(glyphIndex, LoadFlags.NoBitmap | LoadFlags.NoHinting, LoadTarget.Normal);
            /*if (style == StyleSimulations.BoldSimulation)
            {
                FtFace.Glyph.Outline.Embolden(Fixed26Dot6.FromDouble(strength * FtFace.UnitsPerEM));
                FtFace.Glyph.Outline.Translate((int) (-strength * 0.5 * FtFace.UnitsPerEM), (int) (-strength * 0.5 * FtFace.UnitsPerEM));
            }*/
            var box = FtFace.Glyph.GetGlyph().GetCBox(GlyphBBoxMode.Pixels);
            var rect = new Rect(new Point(box.Left * recip, box.Top * recip), new Point(box.Right * recip, box.Bottom * recip));
            if (rect.IsEmpty)
                rect = new Rect(new Point(trm.OffsetX, trm.OffsetY), new Point(trm.OffsetX, trm.OffsetY));

            return rect;
        }
    }
}