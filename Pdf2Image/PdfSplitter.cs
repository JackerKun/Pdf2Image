﻿using iTextSharp.text;
using iTextSharp.text.pdf;
using PDFiumSharp;
using PDFiumSharp.Enums;
using PDFiumSharp.Types;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Image = System.Drawing.Image;
using PdfDocument = PDFiumSharp.PdfDocument;
using PdfPage = PDFiumSharp.PdfPage;

namespace Pdf2Image
{
  public static class PdfSplitter
  {
    private static PdfReader Reader { get; set; }

    public static List<Image> GetImages(string path, Scale scale, List<int> pagenumbers = null)
    {
      if (!File.Exists(path) || !(Path.GetExtension(path).ToLower() == ".pdf"))
        return new List<Image>();
      pagenumbers = pagenumbers ?? new List<int>();
      if (!File.Exists(path))
        return new List<Image>();
      Reader = new PdfReader(path);
      return ProcessPdfToMemory(scale, pagenumbers);
    }

    public static List<Image> GetImages(byte[] file, Scale scale, List<int> pagenumbers = null)
    {
      pagenumbers = pagenumbers ?? new List<int>();
      if (file == null)
        return new List<Image>();
      Reader = new PdfReader(file);
      return ProcessPdfToMemory(scale, pagenumbers);
    }

    public static void WriteImages(string path, string outputFolder, Scale scale, CompressionLevel compression, List<int> pagenumbers = null)
    {
      if (!File.Exists(path) || !(Path.GetExtension(path).ToLower() == ".pdf") || (!Directory.Exists(outputFolder) || !File.Exists(path)))
        return;
      string withoutExtension = Path.GetFileNameWithoutExtension(path);
      PdfReader.AllowOpenWithFullPermissions = true;
      Reader = new PdfReader(path);
      ProcessPDF2Filesystem(outputFolder, scale, compression, withoutExtension, pagenumbers);
    }

    public static void WriteImages(byte[] file, string outputFolder, Scale scale, CompressionLevel compression, string filename = "pdfpic", List<int> pagenumbers = null)
    {
      if (file == null)
        return;
      PdfReader.AllowOpenWithFullPermissions = true;
      Reader = new PdfReader(file);
      ProcessPDF2Filesystem(outputFolder, scale, compression, filename, pagenumbers);
    }


    private static List<Image> ProcessPdfToMemory(Scale scale, List<int> pagenumbers)
    {
      List<Image> imageList = new List<Image>();
      for (int pagenumber = 1; pagenumber <= Reader.NumberOfPages; ++pagenumber)
      {
        if (pagenumbers.Any<int>() && pagenumbers.Contains(pagenumber) || !pagenumbers.Any<int>())
        {
          Stream pdfPageStream = ExtractPdfPageStream(pagenumber);
          imageList.Add(GetPdfImage(((MemoryStream) pdfPageStream).ToArray(), scale));
        }
      }
      Reader.Close();
      return imageList;
    }

    private static void ProcessPDF2Filesystem(string outputFolder, Scale scale, CompressionLevel compression, string defaultname = "pdfpic", List<int> pagenumbers = null)
    {
      ImageCodecInfo encoder = GetEncoder(ImageFormat.Jpeg);      
      var quality = Encoder.Quality;
      EncoderParameters encoderParameters = new EncoderParameters(1);
      long compression1 = GetCompression(compression);
      EncoderParameter encoderParameter = new EncoderParameter((Encoder) quality, compression1);
      encoderParameters.Param[0] = encoderParameter;
      for (int pagenumber = 1; pagenumber <= Reader.NumberOfPages; ++pagenumber)
      {
        if (pagenumbers == null || pagenumbers.Any<int>() && pagenumbers.Contains(pagenumber))
        {
          using (Image pdfImage = GetPdfImage(((MemoryStream) ExtractPdfPageStream(pagenumber)).ToArray(), scale))
            pdfImage.Save(string.Format("{0}\\{1}_{2}.jpg", (object) outputFolder, (object) defaultname, (object) pagenumber), encoder, encoderParameters);
        }
      }
      Reader.Close();
    }

    private static long GetCompression(CompressionLevel compression)
    {
      switch (compression)
      {
        case CompressionLevel.High:
          return 25;
        case CompressionLevel.Medium:
          return 50;
        case CompressionLevel.Low:
          return 90;
        case CompressionLevel.None:
          return 100;
        default:
          return 100;
      }
    }

    private static ImageCodecInfo GetEncoder(ImageFormat format)
    {
      foreach (ImageCodecInfo imageDecoder in ImageCodecInfo.GetImageDecoders())
      {
        if (imageDecoder.FormatID == format.Guid)
          return imageDecoder;
      }
      return (ImageCodecInfo) null;
    }

    private static Image GetPdfImage(byte[] pdf, Scale resolution)
    {
      PdfDocument pdfDocument = new PdfDocument(pdf, 0, -1, (string) null);
      PdfPage pdfPage = pdfDocument.Pages[0];
      PDFiumBitmap pdFiumBitmap = new PDFiumBitmap((int) pdfPage.Size.Item1 * (int) resolution, (int) pdfPage.Size.Item2 * (int) resolution, false);
      pdFiumBitmap.Fill(new FPDF_COLOR(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
      pdfPage.Render(pdFiumBitmap, (PageOrientations) 0, (RenderingFlags) 0);
      Image image = Image.FromStream(pdFiumBitmap.AsBmpStream(72.0, 72.0));
      pdfDocument.Close();
      return image;
    }

    private static Stream ExtractPdfPageStream(int pagenumber)
    {
      Stream stream = (Stream) new MemoryStream();
      Document document = new Document(Reader.GetPageSizeWithRotation(pagenumber));
      PdfCopy pdfCopy = new PdfCopy(document, stream);
      document.Open();
      PdfImportedPage importedPage = ((PdfWriter) pdfCopy).GetImportedPage(Reader, pagenumber);
      pdfCopy.AddPage(importedPage);
      document.Close();
      return stream;
    }

    public enum Scale
    {
      Low = 1,
      High = 2,
      VeryHigh = 3,
    }

    public enum CompressionLevel : long
    {
      High = 25, // 0x0000000000000019
      Medium = 50, // 0x0000000000000032
      Low = 90, // 0x000000000000005A
      None = 100, // 0x0000000000000064
    }
  }
}
