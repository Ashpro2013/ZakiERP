using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.Xsl;

namespace ZatcaIntegrationSDK
{

    public static class Utility
    {
        public static string GetNodeInnerText(XmlDocument doc, string xPath)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            nsmgr.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            nsmgr.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            XmlNode titleNode = doc.SelectSingleNode(xPath, nsmgr);
            if (titleNode != null)
            {
                return titleNode.InnerText;
            }
            return "";
        }

        public static string GetNodeInnerXML(XmlDocument doc, string xPath)
        {
            XmlNode titleNode = doc.SelectSingleNode(xPath);
            if (titleNode != null)
            {
                string canonXml = "";
                using (MemoryStream msIn = new MemoryStream(Encoding.UTF8.GetBytes(titleNode.OuterXml)))
                {
                    XmlDsigC14NTransform t = new XmlDsigC14NTransform(includeComments: false);
                    t.LoadInput(msIn);
                    MemoryStream o = t.GetOutput() as MemoryStream;
                    canonXml = Encoding.UTF8.GetString(o.ToArray());
                }
                return canonXml.Replace("></ds:DigestMethod>", "/>");
            }
            return "";
        }

        public static XmlDocument RemoveXmlns(string xml)
        {
            XDocument d = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
            (from x in d.Root.Descendants().Attributes()
             where x.IsNamespaceDeclaration
             select x).Remove();
            foreach (XElement elem in d.Descendants())
            {
                elem.Name = elem.Name.LocalName;
            }
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.LoadXml(d.ToString());
            return xmlDocument;
        }

        public static void SetNodeValue(XmlDocument doc, string xPath, string value)
        {
            XmlNode titleNode = doc.SelectSingleNode(xPath);
            if (titleNode != null)
            {
                titleNode.InnerText = value;
            }
        }

        public static string GetNodeAttributeValue(XmlDocument doc, string xPath, string attributeName)
        {
            XmlNode titleNode = doc.SelectSingleNode(xPath);
            if (titleNode != null)
            {
                return titleNode.Attributes[attributeName].Value;
            }
            return "";
        }
        public static string ApplyXSLT(XmlDocument xmlDoc, string xsltFilePath)
        {
            //XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.PreserveWhitespace = true;
            //try
            //{
            //    using (var fs = new FileStream(xmlFilePath, FileMode.Open))
            //    {
            //        xmlDoc.Load(fs);
            //    }

            //}
            //catch
            //{

            //}
            StringBuilder output = new StringBuilder();
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.OmitXmlDeclaration = true;
            writerSettings.Encoding = Encoding.UTF8;
            writerSettings.Indent = false;
            writerSettings.ConformanceLevel = ConformanceLevel.Auto;
            using (XmlWriter transformedData = XmlWriter.Create(output, writerSettings))
            {
                XmlReader xslReader = XmlReader.Create(ReadInternalEmbededResourceStream(xsltFilePath));
                XslCompiledTransform xsltTransform = new XslCompiledTransform();
                xsltTransform.Load(xslReader);

                XmlReader xmlReader = XmlReader.Create(new StringReader(xmlDoc.OuterXml));
                xmlReader.Read();
                xsltTransform.Transform(xmlReader, transformedData);
            }

            return output.ToString();
        }
        public static string ApplyXSLTPassingXML(string xml, string xsltFilePath)
        {
            StringBuilder output = new StringBuilder();
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.OmitXmlDeclaration = true;
            writerSettings.Encoding = Encoding.UTF8;
            writerSettings.Indent = true;
            writerSettings.ConformanceLevel = ConformanceLevel.Auto;
            using (XmlWriter transformedData = XmlWriter.Create(output, writerSettings))
            {
                XmlReader xslReader = XmlReader.Create(ReadInternalEmbededResourceStream(xsltFilePath));
                XmlReader xmlReader = XmlReader.Create(new StringReader(xml));
                xmlReader.Read();
                XslCompiledTransform xsltTransform = new XslCompiledTransform();
                xsltTransform.Load(xslReader);
                xsltTransform.Transform(xmlReader, transformedData);
            }
            return output.ToString();
        }

        public static byte[] Sha256_hashAsBytes(string value)
        {
            SHA256 hash = SHA256.Create();
            StringBuilder Sb = new StringBuilder();
            Encoding enc = Encoding.UTF8;
            return hash.ComputeHash(enc.GetBytes(value));
        }

        public static string Sha256_hashAsString(string rawData)
        {
            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                byte[] array = result;
                foreach (byte b in array)
                {
                    Sb.Append(b.ToString("x2"));
                }
            }
            return Sb.ToString();
        }

        public static string ToBase64Encode(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
            return Convert.ToBase64String(toEncodeAsBytes);
        }
        public static string Base64Dencode(string Encodedtext)
        {
            byte[] data = Convert.FromBase64String(Encodedtext);
            string decodedString = Encoding.UTF8.GetString(data);
            return decodedString;
        }

        public static string Sha256_hashAsBytesThenHexa(string value)
        {
            SHA256 hash = SHA256.Create();
            StringBuilder Sb = new StringBuilder();
            Encoding enc = Encoding.UTF8;
            sbyte[] result = (from x in hash.ComputeHash(enc.GetBytes(value))
                              select (sbyte)x).ToArray();
            return BytesToHex((byte[])(object)result);
        }

        public static string ToBase64Encode(byte[] value)
        {
            if (value == null)
            {
                return null;
            }
            return Convert.ToBase64String(value);
        }

        public static byte[] ToBase64DecodeAsBinary(string base64EncodedText)
        {
            if (string.IsNullOrEmpty(base64EncodedText))
            {
                return null;
            }
            return Convert.FromBase64String(base64EncodedText);
        }

        public static sbyte[] GetTlvVAlue(string tagnums, string tagvalue)
        {
            string[] tagnums_array = new string[1] { tagnums };
            sbyte[] tagnum = tagnums_array.Select((string s) => sbyte.Parse(s)).ToArray();
            byte[] tagvalueb = Encoding.UTF8.GetBytes(tagvalue);
            sbyte[] stagvalueb = (sbyte[])(object)tagvalueb;
            sbyte[] tagvaluelengths = (sbyte[])(object)Encoding.UTF8.GetBytes(tagvalueb.Length.ToString());
            return tagnum.Concat(tagvaluelengths).Concat(stagvalueb).ToArray();
        }

        public static sbyte[] GetTlvVAlue(string tagnums, sbyte[] tagvalueb)
        {
            string[] tagnums_array = new string[1] { tagnums };
            sbyte[] tagnum = tagnums_array.Select((string s) => sbyte.Parse(s)).ToArray();
            sbyte[] tagvaluelengths = (sbyte[])(object)Encoding.UTF8.GetBytes(tagvalueb.Length.ToString());
            return tagnum.Concat(tagvaluelengths).Concat(tagvalueb).ToArray();
        }

        public static string GetInvoiceType(XmlDocument xmlDoc)
        {
            string typeCode = GetNodeAttributeValue(xmlDoc, SettingsParams.Invoice_Type_XPATH, "name");
            if (typeCode.StartsWith("01"))
            {
                return "Standard";
            }
            return "Simplified";
        }

        public static string HexToDecimal(string hex)
        {
            List<int> dec = new List<int> { 0 };
            for (int j = 0; j < hex.Length; j++)
            {
                int carry = Convert.ToInt32(hex[j].ToString(), 16);
                for (int i = 0; i < dec.Count; i++)
                {
                    int val = dec[i] * 16 + carry;
                    dec[i] = val % 10;
                    carry = val / 10;
                }
                while (carry > 0)
                {
                    dec.Add(carry % 10);
                    carry /= 10;
                }
            }
            IEnumerable<char> chars = dec.Select((int d) => (char)(48 + d));
            char[] cArr = chars.Reverse().ToArray();
            return new string(cArr);
        }

        private static string BytesToHex(byte[] hash)
        {
            StringBuilder hexString = new StringBuilder(2 * hash.Length);
            for (int i = 0; i < hash.Length; i++)
            {
                string hex = hash[i].ToString("X");
                if (hex.Length == 1)
                {
                    hexString.Append('0');
                }
                hexString.Append(hex);
            }
            return hexString.ToString();
        }

        public static Stream ReadInternalEmbededResourceStream(string resource)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
        }

        public static void WriteTag(Stream stream, uint tag)
        {
            bool firstByte = true;
            for (int i = 3; i >= 0; i--)
            {
                byte thisByte = (byte)(tag >> 8 * i);
                if (!(thisByte == 0 && firstByte) || i <= 0)
                {
                    if (firstByte)
                    {
                        if (i == 0)
                        {
                            if ((thisByte & 0x1F) == 31)
                            {
                                throw new Exception("Invalid tag value: first octet indicates subsequent octets, but no subsequent octets found");
                            }
                        }
                        else if ((thisByte & 0x1F) != 31)
                        {
                            throw new Exception("Invalid tag value: first octet indicates no subsequent octets, but subsequent octets found");
                        }
                    }
                    else if (i == 0)
                    {
                        if ((thisByte & 0x80) == 128)
                        {
                            throw new Exception("Invalid tag value: last octet indicates subsequent octets");
                        }
                    }
                    else if ((thisByte & 0x80) != 128)
                    {
                        throw new Exception("Invalid tag value: non-last octet indicates no subsequent octets");
                    }
                    stream.WriteByte(thisByte);
                    firstByte = false;
                }
            }
        }

        public static void WriteLength(Stream stream, int? length)
        {
            if (!length.HasValue)
            {
                stream.WriteByte(128);
                return;
            }
            if (length < 0 || length > uint.MaxValue)
            {
                throw new Exception($"Invalid length value: {length}");
            }
            if (length <= 127)
            {
                stream.WriteByte(checked((byte)length.Value));
                return;
            }
            byte lengthBytes;
            if (length <= 255)
            {
                lengthBytes = 1;
            }
            else if (length <= 65535)
            {
                lengthBytes = 2;
            }
            else if (length <= 16777215)
            {
                lengthBytes = 3;
            }
            else
            {
                if (!(length <= uint.MaxValue))
                {
                    throw new Exception($"Length value too big: {length}");
                }
                lengthBytes = 4;
            }
            stream.WriteByte((byte)(lengthBytes | 0x80u));
            for (int i = lengthBytes - 1; i >= 0; i--)
            {
                byte data = (byte)(length >> 8 * i).Value;
                stream.WriteByte(data);
            }
        }

        public static MemoryStream WriteTlv(uint tag, byte[] value)
        {
            MemoryStream stream = new MemoryStream();
            WriteTag(stream, tag);
            int length = ((value != null) ? value.Length : 0);
            WriteLength(stream, length);
            if (value == null)
            {
                throw new Exception("Please provide a value!");
            }
            stream.Write(value, 0, length);
            return stream;
        }

        public static byte[] StringToByteArray(string hex)
        {
            return (from x in Enumerable.Range(0, hex.Length)
                    where x % 2 == 0
                    select Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
        }

        public static T DeserializeToObject<T>(string filepath) where T : class
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            StreamReader sr = new StreamReader(filepath);
            return (T)ser.Deserialize(sr);
        }

        public static void SerializeToXml<T>(T anyobject, string xmlFilePath)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(anyobject.GetType());
            StreamWriter writer = new StreamWriter(xmlFilePath);
            xmlSerializer.Serialize(writer, anyobject);
        }

        public static string[] getAllResources()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames();
        }

        public static string ReplaceXMLSpecialCharacters(string str)
        {
            return str.Trim().Replace("&", "&#38;").Replace("<", "&#60;").Replace(">", "&#62;").Replace("'", "&#39;").Replace("\"", "&#34;");
        }
        public static string RemoveAllNamespaces(string xmlDocument)
        {
            XElement xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xmlDocument));

            return xmlDocumentWithoutNs.ToString();
        }

        private static XElement RemoveAllNamespaces(XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;

                foreach (XAttribute attribute in xmlDocument.Attributes())
                    xElement.Add(attribute);

                return xElement;
            }
            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
        }
        public static string RemoveNonAlphanumeric(string str)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            str = rgx.Replace(str, "");
            return str;
        }
        public static void CreateInvoicesFolder(string Directorypath)
        {
            try
            {
                string path = Directorypath;
                if (!Directory.Exists(path + SettingsParams.NormalSimplifiedInvoicePath))
                    Directory.CreateDirectory(path + SettingsParams.NormalSimplifiedInvoicePath);
                if (!Directory.Exists(path + SettingsParams.NormalSimplifiedDebitPath))
                    Directory.CreateDirectory(path + SettingsParams.NormalSimplifiedDebitPath);
                if (!Directory.Exists(path + SettingsParams.NormalSimplifiedCreditPath))
                    Directory.CreateDirectory(path + SettingsParams.NormalSimplifiedCreditPath);
                if (!Directory.Exists(path + SettingsParams.NormalStandardInvoicePath))
                    Directory.CreateDirectory(path + SettingsParams.NormalStandardInvoicePath);
                if (!Directory.Exists(path + SettingsParams.NormalStandardDebitPath))
                    Directory.CreateDirectory(path + SettingsParams.NormalStandardDebitPath);
                if (!Directory.Exists(path + SettingsParams.NormalStandardCreditPath))
                    Directory.CreateDirectory(path + SettingsParams.NormalStandardCreditPath);

                if (!Directory.Exists(path + SettingsParams.StandardInvoicePath))
                    Directory.CreateDirectory(path + SettingsParams.StandardInvoicePath);
                if (!Directory.Exists(path + SettingsParams.StandardDebitPath))
                    Directory.CreateDirectory(path + SettingsParams.StandardDebitPath);
                if (!Directory.Exists(path + SettingsParams.StandardCreditPath))
                    Directory.CreateDirectory(path + SettingsParams.StandardCreditPath);
                if (!Directory.Exists(path + SettingsParams.SimplifiedInvoicePath))
                    Directory.CreateDirectory(path + SettingsParams.SimplifiedInvoicePath);
                if (!Directory.Exists(path + SettingsParams.SimplifiedDebitPath))
                    Directory.CreateDirectory(path + SettingsParams.SimplifiedDebitPath);
                if (!Directory.Exists(path + SettingsParams.SimplifiedCreditPath))
                    Directory.CreateDirectory(path + SettingsParams.SimplifiedCreditPath);
            }
            catch
            {

            }

        }
        public static void CreateClearedInvoicesFolder(string Directorypath)
        {
            try
            {
                string path = Directorypath;
                //for Cleared and REported folders
                if (!Directory.Exists(path + SettingsParams.StandardInvoiceFromZatcaPath))
                    Directory.CreateDirectory(path + SettingsParams.StandardInvoiceFromZatcaPath);
                if (!Directory.Exists(path + SettingsParams.StandardDebitFromZatcaPath))
                    Directory.CreateDirectory(path + SettingsParams.StandardDebitFromZatcaPath);
                if (!Directory.Exists(path + SettingsParams.StandardCreditFromZatcaPath))
                    Directory.CreateDirectory(path + SettingsParams.StandardCreditFromZatcaPath);
            }
            catch
            {

            }

        }
    }


}