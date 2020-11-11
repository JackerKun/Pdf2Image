using iTextSharp.text;
using Pdf2Image;
using System;
using System.Collections.Generic;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string file = "c:\\tmp\\test.pdf";
            List<System.Drawing.Image> images = PdfSplitter.GetImages(file, PdfSplitter.Scale.High);

            PdfSplitter.WriteImages(file, "c:\\tmp", PdfSplitter.Scale.High, PdfSplitter.CompressionLevel.Medium);
        }
    }
}
