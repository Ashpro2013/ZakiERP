using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;


namespace ZatcaIntegrationSDK.BLL
{
    public class HashingValidator
    {
        public Result GenerateEInvoiceHashing(XmlDocument xmlDoc)
        {
            Result objResult = new Result();
            objResult.Operation = "Generate Invoice Hashing";
            objResult.IsValid = false;
            try
            {
                if (string.IsNullOrEmpty(xmlDoc.OuterXml))
                {
                    objResult.ErrorMessage = "Invalid invoice XML content";
                    return objResult;
                }
                string result = "";
                try
                {
                    result = Utility.ApplyXSLT(xmlDoc, SettingsParams.Embeded_InvoiceXSLFileForHashing);
                }
                catch
                {
                    objResult.ErrorMessage = "Can not apply XSL file";
                    return objResult;
                }
                if (string.IsNullOrEmpty(result))
                {
                    objResult.ErrorMessage = "Error In applying XSL file";
                    return objResult;
                }
                using (MemoryStream msIn = new MemoryStream(Encoding.UTF8.GetBytes(result)))
                {
                    XmlDsigC14NTransform t = new XmlDsigC14NTransform(includeComments: false);
                    t.LoadInput(msIn);
                    MemoryStream o = t.GetOutput() as MemoryStream;
                    string canonXml = Encoding.UTF8.GetString(o.ToArray());
                    sbyte[] data = (from x in Utility.Sha256_hashAsBytes(canonXml)
                                    select (sbyte)x).ToArray();
                    objResult.ResultedValue = Utility.ToBase64Encode((byte[])(object)data);
                    objResult.IsValid = true;
                }
                return objResult;
            }
            catch (Exception ex)
            {
                objResult.ErrorMessage = ex.Message;
                return objResult;
            }
        }
        public Result ValidateEInvoiceHashing(XmlDocument xmlDoc)
        {
            Result objResult = new Result();
            objResult.Operation = "Validating Invoice Hashing";
            objResult.IsValid = false;
           
            string existHashingNodeValue = Utility.GetNodeInnerText(xmlDoc, SettingsParams.Hash_XPATH);
            if (string.IsNullOrEmpty(existHashingNodeValue))
            {
                objResult.ErrorMessage = "There is no Hashing node value in this XML file";
                return objResult;
            }
            Result objHashingResult = GenerateEInvoiceHashing(xmlDoc);
            if (!objHashingResult.IsValid)
            {
                objResult.ErrorMessage = objHashingResult.ErrorMessage;
                return objResult;
            }
            if (existHashingNodeValue != objHashingResult.ResultedValue)
            {
                objResult.ErrorMessage = "The generated Hashing is different of the one exists in the XML file.";
                return objResult;
            }
            objResult.IsValid = true;
            return objResult;
        }
    }

}