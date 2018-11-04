using System;
using System.IO;
using System.IO.Packaging;

namespace SharpGlyph
{
    public class Document
    {
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

        public void Open()
        {
            if (!string.IsNullOrWhiteSpace(FileName))
                using (var stream = File.Open(FileName, FileMode.Open))
                    stream.CopyTo(FileStream);

            OpenWithStream();
        }

        public void Close()
        {
            FileStream?.Dispose();
        }

        public object GetPage(int number)
        {
            throw new NotImplementedException();
        }

        public object GetPages()
        {
            throw new NotImplementedException();
        }

        protected void OpenWithStream()
        {
            try
            {
                var zip = Package.Open(FileStream);
            }
            catch (Exception e)
            {
                Console.WriteLine(e); // TODO
                throw;
            }

            ReadPageList();
        }

        protected void ReadPageList()
        {

        }
    }
}