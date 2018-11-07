using System;
using System.Text.RegularExpressions;

namespace SharpGlyph
{
    internal sealed class Utility
    {
        public static string SkipScheme(string path)
        {
            return !Regex.IsMatch(path.Substring(0, 1), @"[a-zA-Z]") ? path : path.Substring(path.IndexOf(':') + 1);
        }

        public static string SkipAuthority(string path)
        {
            return !path.StartsWith("//") ? path : path.Substring(2).Substring(path.IndexOfAny(new[] { '/', '?' }) + 1);
        }

        public static string ResolveUrl(string baseUri, string path)
        {
            var s = SkipAuthority(SkipScheme(path));
            if (s != path || s.StartsWith("/"))
                return path;

            var result = baseUri;
            if (result.Length == 0 || !result.EndsWith("/"))
                result += "/";
            result += path;
            return result;
        }

        public static string GetDirectoryName(string name)
        {
            var path = name.Substring(name.LastIndexOf('/'));
            if (!string.IsNullOrWhiteSpace(path))
                path = name.Replace(path, "");
            if (path.IndexOf("/_rels", StringComparison.OrdinalIgnoreCase) >= 0)
                path = path.Replace("/_rels", "");
            return path;
        }
    }
}