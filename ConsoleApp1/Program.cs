using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string file = "c:\\tmp\\Comprovativo.pdf";
            var t = Pdf2Image.PdfSplitter.GetImages(file, Pdf2Image.PdfSplitter.Scale.High);

            Pdf2Image.PdfSplitter.WriteImages(file, "c:\\tmp", Pdf2Image.PdfSplitter.Scale.High, Pdf2Image.PdfSplitter.CompressionLevel.Medium);
        }
    }
}
