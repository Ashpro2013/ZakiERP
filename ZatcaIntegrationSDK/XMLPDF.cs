using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using iTextSharp.text.pdf;
using iTextSharp.text;
using ZatcaIntegrationSDK.GeneralLogic;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ZatcaIntegrationSDK
{
    public class XMLPDF
    {
        public XMLPDF()
        { 
        
        }
        //this is code with FreeSpireDll
        //public PDFA3Result ConvertToPDFA3(string XMLFileName, string PDFFileName)
        //{
        //    PDFA3Result pdfresult = new PDFA3Result();
        //    pdfresult.IsValid = false;
        //    try
        //    {


        //        XmlDocument doc = new XmlDocument();
        //        doc.PreserveWhitespace = true;
        //        try
        //        {
        //            doc.Load(XMLFileName);
        //        }
        //        catch
        //        {
        //            pdfresult.ErrorMessage = "Can not load XML file";
        //            return pdfresult;
        //        }
        //        PdfDocument pdfdoc = new PdfDocument();
        //        try
        //        {
        //            using (MemoryStream stream = new MemoryStream())
        //            {
        //                using (FileStream file = new FileStream(PDFFileName, FileMode.Open, FileAccess.Read))
        //                {
        //                    byte[] pdfbytes = new byte[file.Length];
        //                    file.Read(pdfbytes, 0, (int)file.Length);
        //                    stream.Write(pdfbytes, 0, (int)file.Length);
        //                }
        //                pdfdoc.LoadFromStream(stream);
        //            }
        //        }
        //        catch
        //        {
        //            pdfresult.ErrorMessage = "this pdf file doesn't exist !";
        //            return pdfresult;
        //        }


        //        string invoice_id = Utility.GetNodeInnerText(doc, SettingsParams.INVOICE_ID_XPATH);

        //        PdfAttachment attachment = new PdfAttachment(XMLFileName);
        //        pdfdoc.Attachments.Add(attachment);
        //        //  var outputNormalFile = ConfigurationManager.AppSettings["OutputPDFA3Path"] + request.InvoiceNumber + "NormalPDF" + ".pdf";
        //        string outputDirectory = Path.GetDirectoryName(PDFFileName);
        //        //string outputPDFA3Name = outputDirectory+"\\"+ invoice_id + "_Normal.pdf";
        //        string outputPDFA3Name = outputDirectory + "\\" + invoice_id + "_Normal_PDFA3.pdf";
        //        var outputNormalfileExists = File.Exists(outputPDFA3Name);
        //        if (outputNormalfileExists)
        //            File.Delete(outputPDFA3Name);
        //        pdfdoc.SaveToFile(Path.GetFullPath(outputPDFA3Name));
        //        string outputPDF = outputDirectory + "\\" + invoice_id + "_PDFA3.pdf";
        //        var outputfileExists = File.Exists(outputPDF);
        //        if (outputfileExists)
        //            File.Delete(outputPDF);
        //        PdfStandardsConverter converter = new PdfStandardsConverter(Path.GetFullPath(outputPDFA3Name));
        //        converter.ToPdfA3B(outputPDF);

        //        File.Delete(outputPDFA3Name);
        //        pdfresult.IsValid = true;
        //        pdfresult.PDFA3FileNameFullPath = outputPDF;
        //        return pdfresult;
        //    }

        //    catch (Exception ex)
        //    {
        //        pdfresult.ErrorMessage = ex.Message;
        //        return pdfresult;
        //    }
        //}

        public PDFA3Result ConvertToPDFA3(string XMLFileName, string PDFFileName)
        {

            PDFA3Result pdfresult = new PDFA3Result();
            pdfresult.IsValid = false;
            try
            {


                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                try
                {
                    doc.Load(XMLFileName);
                }
                catch
                {
                    pdfresult.ErrorMessage = "Can not load XML file";
                    return pdfresult;
                }
                //check if pdf file exist 
                if (!File.Exists(PDFFileName))
                {
                    pdfresult.ErrorMessage = "PDF file Doesn't Exist";
                    return pdfresult;
                }
                string invoice_id = Utility.GetNodeInnerText(doc, SettingsParams.INVOICE_ID_XPATH);

                string outputDirectory = Path.GetDirectoryName(PDFFileName);
                


                string outputPDFA3Name = outputDirectory + "\\" + invoice_id + "_PDFA3.pdf";
                var outputfileExists = File.Exists(outputPDFA3Name);
                if (outputfileExists)
                    File.Delete(outputPDFA3Name);

                var document = new Document();
                PdfReader reader = null;
               
                // Create a PdfAWriter instance to write the converted file.
                using (var writer = PdfAWriter.GetInstance(document, new System.IO.FileStream(outputPDFA3Name, System.IO.FileMode.Create), PdfAConformanceLevel.PDF_A_3A))
                {
                    writer.CreateXmpMetadata();
                    // we open the document
                    document.Open();

                    reader = new PdfReader(PDFFileName);
                    // Iterate through each page of the source PDF file.
                    for (int pageNumber = 1; pageNumber <= reader.NumberOfPages; pageNumber++)
                    {
                        document.SetPageSize(reader.GetPageSizeWithRotation(pageNumber));
                        document.NewPage();
                        var page = writer.GetImportedPage(reader, pageNumber);
                        writer.DirectContent.AddTemplate(page, 0, 0);
                    }

                    string attatchmentname = "signedxml.xml";
                    try
                    {
                        int pos = XMLFileName.LastIndexOf('\\') + 1;
                        attatchmentname = XMLFileName.Substring(pos);
                    }
                    catch
                    {

                    }

                    PdfFileSpecification fileSpec = PdfFileSpecification.FileEmbedded(writer, XMLFileName,attatchmentname, null, "application/xml", null, 0);
                    fileSpec.Put(new PdfName("AFRelationship"), new PdfName("Data"));
                    writer.AddFileAttachment(fileSpec);

                    PdfDictionary extraCatalog = writer.ExtraCatalog;
                    PdfDictionary markInfoDict = new PdfDictionary();
                    markInfoDict.Put(PdfName.MARKED, new PdfBoolean(true));
                    extraCatalog.Put(PdfName.MARKINFO, markInfoDict);

                    document.Close();
                    reader.Close();
                }


                pdfresult.IsValid = true;
                pdfresult.PDFA3FileNameFullPath = outputPDFA3Name;
                pdfresult.PDFA3FileName= "\\" + invoice_id + "_PDFA3.pdf";

                return pdfresult;
            }

            catch (Exception ex)
            {
                pdfresult.ErrorMessage = ex.Message;
                return pdfresult;
            }

        }
        public PDFA3Result ConvertXMLToPDFA3ByteArray(string XMLFileName, string PDFFileName)
        {

            PDFA3Result pdfresult = new PDFA3Result();
            pdfresult.IsValid = false;
            try
            {


                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                try
                {
                    doc.Load(XMLFileName);
                }
                catch
                {
                    pdfresult.ErrorMessage = "Can not load XML file";
                    return pdfresult;
                }
                //check if pdf file exist 
                if (!File.Exists(PDFFileName))
                {
                    pdfresult.ErrorMessage = "PDF file Doesn't Exist";
                    return pdfresult;
                }
                string invoice_id = Utility.GetNodeInnerText(doc, SettingsParams.INVOICE_ID_XPATH);

                
                var document = new Document();
                PdfReader reader = null;
                byte[] filecontent = null;

                using (MemoryStream myMemoryStream = new MemoryStream())
                {
                    // Create a PdfAWriter instance to write the converted file.
                    using (var writer = PdfAWriter.GetInstance(document, myMemoryStream, PdfAConformanceLevel.PDF_A_3A))
                    {
                        writer.CreateXmpMetadata();
                        // we open the document
                        document.Open();
                        //document.Add(new Chunk(""));

                        
                        reader = new PdfReader(PDFFileName);
                        // Iterate through each page of the source PDF file.
                        for (int pageNumber = 1; pageNumber <= reader.NumberOfPages; pageNumber++)
                        {
                            document.SetPageSize(reader.GetPageSizeWithRotation(pageNumber));
                            document.NewPage();
                            var page = writer.GetImportedPage(reader, pageNumber);
                            writer.DirectContent.AddTemplate(page, 0, 0);
                        }

                        string attatchmentname = RemoveNonAlphanumeric(invoice_id) + ".xml";

                        PdfFileSpecification fileSpec = PdfFileSpecification.FileEmbedded(writer, XMLFileName, attatchmentname, null, "application/xml", null, 0);
                        fileSpec.Put(new PdfName("AFRelationship"), new PdfName("Data"));
                        writer.AddFileAttachment(fileSpec);

                        PdfDictionary extraCatalog = writer.ExtraCatalog;
                        PdfDictionary markInfoDict = new PdfDictionary();
                        markInfoDict.Put(PdfName.MARKED, new PdfBoolean(true));
                        extraCatalog.Put(PdfName.MARKINFO, markInfoDict);

                        document.Close();
                        reader.Close();
                    }
                    filecontent = myMemoryStream.ToArray();
                }


                pdfresult.IsValid = true;
                pdfresult.PDFA3FileNameFullPath = "";
                pdfresult.PDFA3FileName = "";
                pdfresult.PDFA3ContentFile = filecontent;
                return pdfresult;
            }

            catch (Exception ex)
            {
                pdfresult.ErrorMessage = ex.Message;
                return pdfresult;
            }

        }
        //public PDFA3Result ConvertEncodedXMLToPDFA3ByteArray(string EncodedXML, byte[] PDFContent,string TempDirectory = "")
        //{

        //    PDFA3Result pdfresult = new PDFA3Result();
        //    pdfresult.IsValid = false;
        //    try
        //    {
        //        if (string.IsNullOrEmpty(TempDirectory))
        //        {
        //            TempDirectory = Directory.GetCurrentDirectory();
        //        }

        //        XmlDocument doc = new XmlDocument();
        //        doc.PreserveWhitespace = true;
        //        string invoice_id = "";
        //        string uuid = "";
        //        string xmlfilefullname = "";
                
        //        string pdffilename = "";
        //        try
        //        {
        //            string xmltext=Utility.Base64Dencode(EncodedXML);
        //            doc.LoadXml(xmltext);
        //            invoice_id = Utility.GetNodeInnerText(doc, SettingsParams.INVOICE_ID_XPATH);
        //            uuid = Utility.GetNodeInnerText(doc, SettingsParams.UUID_XPATH);
                    
        //            xmlfilefullname = TempDirectory + "\\" + RemoveNonAlphanumeric(invoice_id) + "_" + uuid + ".xml";

        //            using (var fs = new FileStream(xmlfilefullname, FileMode.Create))
        //                {
        //                    doc.Save(fs);
        //                }
        //        }
        //        catch
        //        {
        //            pdfresult.ErrorMessage = "Can not load XML file";
        //            return pdfresult;
        //        }

        //        //check if pdf file exist 
        //        if (PDFContent != null && PDFContent.Length > 0)
        //        {
                    
        //        }
        //        else
        //        {
        //            pdfresult.ErrorMessage = "PDF file Doesn't Exist";
        //            return pdfresult;
        //        }
              
        //        var document = new Document();
        //        PdfReader reader = null;
        //        byte[] filecontent = null;
        //        using (MemoryStream myMemoryStream = new MemoryStream())
        //        {
        //            // Create a PdfAWriter instance to write the converted file.
        //            using (var writer = PdfAWriter.GetInstance(document, myMemoryStream, PdfAConformanceLevel.PDF_A_3A))
        //            {
        //                writer.CreateXmpMetadata();
        //                // we open the document
        //                document.Open();

        //                reader = new PdfReader(PDFContent);
        //                // Iterate through each page of the source PDF file.
        //                for (int pageNumber = 1; pageNumber <= reader.NumberOfPages; pageNumber++)
        //                {
        //                    document.SetPageSize(reader.GetPageSizeWithRotation(pageNumber));
        //                    document.NewPage();
        //                    var page = writer.GetImportedPage(reader, pageNumber);
        //                    writer.DirectContent.AddTemplate(page, 0, 0);
        //                }

        //                string attatchmentname = RemoveNonAlphanumeric(invoice_id) + ".xml";
                        

        //                PdfFileSpecification fileSpec = PdfFileSpecification.FileEmbedded(writer, xmlfilefullname, attatchmentname, null, "application/xml", null, 0);
        //                fileSpec.Put(new PdfName("AFRelationship"), new PdfName("Data"));
        //                writer.AddFileAttachment(fileSpec);

        //                PdfDictionary extraCatalog = writer.ExtraCatalog;
        //                PdfDictionary markInfoDict = new PdfDictionary();
        //                markInfoDict.Put(PdfName.MARKED, new PdfBoolean(true));
        //                extraCatalog.Put(PdfName.MARKINFO, markInfoDict);

        //                document.Close();
        //            }
        //            filecontent = myMemoryStream.ToArray();
        //        }
        //        //remove xmlfile
        //        if (File.Exists(xmlfilefullname))
        //            File.Delete(xmlfilefullname);
              

        //        pdfresult.IsValid = true;
        //        pdfresult.PDFA3FileNameFullPath = "";
        //        pdfresult.PDFA3FileName = "";
        //        pdfresult.PDFA3ContentFile = filecontent;
        //        return pdfresult;
        //    }

        //    catch (Exception ex)
        //    {
        //        pdfresult.ErrorMessage = ex.Message;
        //        return pdfresult;
        //    }

        //}
        public PDFA3Result ConvertXMLToPDFA3ByteArray(string XMLFileName, byte[] PDFContent)
        {

            PDFA3Result pdfresult = new PDFA3Result();
            pdfresult.IsValid = false;
            try
            {

                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                string invoice_id = "";
                try
                {
                    doc.Load(XMLFileName);
                    invoice_id = Utility.GetNodeInnerText(doc, SettingsParams.INVOICE_ID_XPATH);
                }
                catch
                {
                    pdfresult.ErrorMessage = "Can not load XML file";
                    return pdfresult;
                }
                
               
                //check if pdf file exist 
                if (PDFContent != null && PDFContent.Length > 0)
                {

                }
                else
                {
                    pdfresult.ErrorMessage = "PDF file Doesn't Exist";
                    return pdfresult;
                }

                var document = new Document();
                PdfReader reader = null;
                byte[] filecontent = null;
                using (MemoryStream myMemoryStream = new MemoryStream())
                {
                    // Create a PdfAWriter instance to write the converted file.
                    using (var writer = PdfAWriter.GetInstance(document, myMemoryStream, PdfAConformanceLevel.PDF_A_3A))
                    {
                        writer.CreateXmpMetadata();
                        // we open the document
                        document.Open();

                        reader = new PdfReader(PDFContent);
                        // Iterate through each page of the source PDF file.
                        for (int pageNumber = 1; pageNumber <= reader.NumberOfPages; pageNumber++)
                        {
                            document.SetPageSize(reader.GetPageSizeWithRotation(pageNumber));
                            document.NewPage();
                            var page = writer.GetImportedPage(reader, pageNumber);
                            writer.DirectContent.AddTemplate(page, 0, 0);
                        }

                        string attatchmentname = RemoveNonAlphanumeric(invoice_id) + ".xml";


                        PdfFileSpecification fileSpec = PdfFileSpecification.FileEmbedded(writer, XMLFileName, attatchmentname, null, "application/xml", null, 0);
                        fileSpec.Put(new PdfName("AFRelationship"), new PdfName("Data"));
                        writer.AddFileAttachment(fileSpec);

                        PdfDictionary extraCatalog = writer.ExtraCatalog;
                        PdfDictionary markInfoDict = new PdfDictionary();
                        markInfoDict.Put(PdfName.MARKED, new PdfBoolean(true));
                        extraCatalog.Put(PdfName.MARKINFO, markInfoDict);

                        document.Close();
                        reader.Close();
                    }
                    filecontent = myMemoryStream.ToArray();
                }
              
                pdfresult.IsValid = true;
                pdfresult.PDFA3FileNameFullPath = "";
                pdfresult.PDFA3FileName = "";
                pdfresult.PDFA3ContentFile = filecontent;
                return pdfresult;
            }

            catch (Exception ex)
            {
                pdfresult.ErrorMessage = ex.Message;
                return pdfresult;
            }

        }
        public PDFA3Result ConvertEncodedXMLToPDFA3ByteArray(string EncodedXML, byte[] PDFContent, string TempDirectory = "",bool SavePDF=false)
        {

            PDFA3Result pdfresult = new PDFA3Result();
            pdfresult.IsValid = false;
            try
            {
                if (string.IsNullOrEmpty(TempDirectory))
                {
                    TempDirectory = Directory.GetCurrentDirectory();
                }

                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                string invoice_id = "";
                string uuid = "";
                string VAT_REGISTERATION = "";
                string ISSUE_TIME = "";
                string xmlfilefullname = "";

                string pdfa3filename = "";
                string ISSUE_DATE = "";
                string pdfa3filefullpath = "";
                try
                {
                    string xmltext = Utility.Base64Dencode(EncodedXML);
                    doc.LoadXml(xmltext);
                    invoice_id = Utility.GetNodeInnerText(doc, SettingsParams.INVOICE_ID_XPATH);
                    uuid = Utility.GetNodeInnerText(doc, SettingsParams.UUID_XPATH);
                    VAT_REGISTERATION = Utility.GetNodeInnerText(doc, SettingsParams.VAT_REGISTERATION_XPATH);
                    ISSUE_DATE = Utility.GetNodeInnerText(doc, SettingsParams.ISSUE_DATE_XPATH);
                    ISSUE_TIME = Utility.GetNodeInnerText(doc, SettingsParams.ISSUE_TIME_XPATH);

                    pdfa3filename = VAT_REGISTERATION + "_" + ISSUE_DATE.Replace("-", "") + "T" + ISSUE_TIME.Replace(":", "") + "_" + Utility.RemoveNonAlphanumeric(invoice_id) + ".pdf";
                    xmlfilefullname = TempDirectory + "\\" + RemoveNonAlphanumeric(invoice_id) + "_" + uuid + ".xml";
                    using (var fs = new FileStream(xmlfilefullname, FileMode.Create))
                    {
                        doc.Save(fs);
                    }
                }
                catch
                {
                    pdfresult.ErrorMessage = "Can not load XML file";
                    return pdfresult;
                }

                //check if pdf file exist 
                if (PDFContent != null && PDFContent.Length > 0)
                {

                }
                else
                {
                    pdfresult.ErrorMessage = "PDF file Doesn't Exist";
                    return pdfresult;
                }

                var document = new Document();
                PdfReader reader = null;
                byte[] filecontent = null;
                using (MemoryStream myMemoryStream = new MemoryStream())
                {
                    // Create a PdfAWriter instance to write the converted file.
                    using (var writer = PdfAWriter.GetInstance(document, myMemoryStream, PdfAConformanceLevel.PDF_A_3A))
                    {
                        writer.CreateXmpMetadata();
                        // we open the document
                        document.Open();

                        reader = new PdfReader(PDFContent);
                        // Iterate through each page of the source PDF file.
                        for (int pageNumber = 1; pageNumber <= reader.NumberOfPages; pageNumber++)
                        {
                            document.SetPageSize(reader.GetPageSizeWithRotation(pageNumber));
                            document.NewPage();
                            var page = writer.GetImportedPage(reader, pageNumber);
                            writer.DirectContent.AddTemplate(page, 0, 0);
                        }

                        string attatchmentname = RemoveNonAlphanumeric(invoice_id) + ".xml";


                        PdfFileSpecification fileSpec = PdfFileSpecification.FileEmbedded(writer, xmlfilefullname, attatchmentname, null, "application/xml", null, 0);
                        fileSpec.Put(new PdfName("AFRelationship"), new PdfName("Data"));
                        writer.AddFileAttachment(fileSpec);

                        PdfDictionary extraCatalog = writer.ExtraCatalog;
                        PdfDictionary markInfoDict = new PdfDictionary();
                        markInfoDict.Put(PdfName.MARKED, new PdfBoolean(true));
                        extraCatalog.Put(PdfName.MARKINFO, markInfoDict);

                        document.Close();
                        reader.Close();
                    }
                    filecontent = myMemoryStream.ToArray();
                }
                //remove xmlfile
                if (File.Exists(xmlfilefullname))
                    File.Delete(xmlfilefullname);
                if (SavePDF && filecontent != null && filecontent.Length > 0)
                {
                    DateTime issueDateTime = default(DateTime);

                    DateTime.TryParseExact(ISSUE_DATE, SettingsParams.allDatesFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out issueDateTime);
                    string issueDateTimeStr = issueDateTime.ToString("ddMMyyyy", System.Globalization.CultureInfo.GetCultureInfo("en-us", "en"));
                    if (!Directory.Exists(TempDirectory + "\\" + issueDateTimeStr))
                        Directory.CreateDirectory(TempDirectory + "\\" + issueDateTimeStr);
                    // save pdf a3 file
                    pdfa3filefullpath = TempDirectory + "\\" + issueDateTimeStr + "\\" + pdfa3filename;
                    SaveByteArrayToFileWithFileStream(filecontent, pdfa3filefullpath);


                }
                pdfresult.IsValid = true;
                pdfresult.PDFA3FileNameFullPath = pdfa3filefullpath;
                pdfresult.PDFA3FileName = pdfa3filename;
                pdfresult.PDFA3ContentFile = filecontent;
                return pdfresult;
            }

            catch (Exception ex)
            {
                pdfresult.ErrorMessage = ex.Message;
                return pdfresult;
            }

        }
        public PDFA3Result ConvertEncodedXMLToPDFA3ByteArray(string EncodedXML, string PDFFileName, string TempDirectory = "")
        {

            PDFA3Result pdfresult = new PDFA3Result();
            pdfresult.IsValid = false;
            try
            {
                if (string.IsNullOrEmpty(TempDirectory))
                {
                    TempDirectory = Directory.GetCurrentDirectory();
                }

                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                string invoice_id = "";
                string uuid = "";
                string xmlfilefullname = "";


                try
                {
                    string xmltext = Utility.Base64Dencode(EncodedXML);
                    doc.LoadXml(xmltext);
                    invoice_id = Utility.GetNodeInnerText(doc, SettingsParams.INVOICE_ID_XPATH);
                    uuid = Utility.GetNodeInnerText(doc, SettingsParams.UUID_XPATH);

                    xmlfilefullname = TempDirectory + "\\" + RemoveNonAlphanumeric(invoice_id) + "_" + uuid + ".xml";

                    using (var fs = new FileStream(xmlfilefullname, FileMode.Create))
                    {
                        doc.Save(fs);
                    }
                }
                catch
                {
                    pdfresult.ErrorMessage = "Can not load XML file";
                    return pdfresult;
                }
                //check if pdf file exist 
                if (!File.Exists(PDFFileName))
                {
                    pdfresult.ErrorMessage = "PDF file Doesn't Exist";
                    return pdfresult;
                }
                string outputDirectory = Path.GetDirectoryName(PDFFileName);
                string outputPDFA3Name = outputDirectory + "\\" + invoice_id + "_PDFA3.pdf";
                var outputfileExists = File.Exists(outputPDFA3Name);
                if (outputfileExists)
                    File.Delete(outputPDFA3Name);
                var document = new Document();
                PdfReader reader = null;
                byte[] filecontent = null;
                using (MemoryStream myMemoryStream = new MemoryStream())
                {
                    // Create a PdfAWriter instance to write the converted file.
                    using (var writer = PdfAWriter.GetInstance(document, myMemoryStream, PdfAConformanceLevel.PDF_A_3A))
                    {
                        writer.CreateXmpMetadata();
                        // we open the document
                        document.Open();

                        reader = new PdfReader(PDFFileName);
                        // Iterate through each page of the source PDF file.
                        for (int pageNumber = 1; pageNumber <= reader.NumberOfPages; pageNumber++)
                        {
                            document.SetPageSize(reader.GetPageSizeWithRotation(pageNumber));
                            document.NewPage();
                            var page = writer.GetImportedPage(reader, pageNumber);
                            writer.DirectContent.AddTemplate(page, 0, 0);
                        }

                        string attatchmentname = RemoveNonAlphanumeric(invoice_id) + ".xml";


                        PdfFileSpecification fileSpec = PdfFileSpecification.FileEmbedded(writer, xmlfilefullname, attatchmentname, null, "application/xml", null, 0);
                        fileSpec.Put(new PdfName("AFRelationship"), new PdfName("Data"));
                        writer.AddFileAttachment(fileSpec);

                        PdfDictionary extraCatalog = writer.ExtraCatalog;
                        PdfDictionary markInfoDict = new PdfDictionary();
                        markInfoDict.Put(PdfName.MARKED, new PdfBoolean(true));
                        extraCatalog.Put(PdfName.MARKINFO, markInfoDict);

                        document.Close();
                        reader.Close();
                    }
                    filecontent = myMemoryStream.ToArray();
                    if (filecontent != null && filecontent.Length > 0)
                    {

                        SaveByteArrayToFileWithFileStream(filecontent, outputPDFA3Name);

                    }
                }
                //remove xmlfile
                if (File.Exists(xmlfilefullname))
                    File.Delete(xmlfilefullname);


                pdfresult.IsValid = true;
                pdfresult.PDFA3FileNameFullPath = outputPDFA3Name;
                pdfresult.PDFA3FileName = invoice_id + "_PDFA3.pdf";
                pdfresult.PDFA3ContentFile = filecontent;
                return pdfresult;
            }

            catch (Exception ex)
            {
                pdfresult.ErrorMessage = ex.Message;
                return pdfresult;
            }

        }

        private string RemoveNonAlphanumeric(string str)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            str = rgx.Replace(str, "");
            return str;
        }
        private void SaveByteArrayToFileWithFileStream(byte[] data, string filePath)
        {
            using (var stream = File.Create(filePath))
            {
                stream.Write(data, 0, data.Length);
            }

        }
    }
}
