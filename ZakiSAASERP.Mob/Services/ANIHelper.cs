using System.Net;
using System.Net.NetworkInformation;
using AshproStringExtension;

namespace ZakiSAASERP.Mob.Services;

internal class ANIHelper
{
    public static List<string> GetMaccID()
    {
        List<string> sList = new List<string>();
        NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
        String sMacAddress = string.Empty;
        foreach (NetworkInterface adapter in nics)
        {
            IPInterfaceProperties properties = adapter.GetIPProperties();
            sMacAddress = adapter.GetPhysicalAddress().ToString();
            if ((sMacAddress != string.Empty) && (!sList.Contains(sMacAddress)))
            {
                sList.Add(sMacAddress);
            }
        }
        return sList;
    }
    public static int GetDateDifferences(DateTime fromDate, DateTime twoDate)
    {
        string strDiff = (fromDate.Date - twoDate.Date).ToString();
        int intDiff = strDiff.Length;
        if (intDiff > 9)
        {
            intDiff = intDiff - 9;
            strDiff = strDiff.Remove(intDiff);
        }
        else
        {
            strDiff = "0";
        }
        return strDiff.ToInt32();
    }
    public static bool CheckInternet()
    {
        try
        {
            using (var client = new WebClient())
            using (client.OpenRead("http://google.com/generate_204"))
                return true;
        }
        catch
        {
            return false;
        }
    }
    public static string? GetIpAddress()
    {
        String address = "1234";
        try
        {
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (WebResponse response = request.GetResponse())
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                address = stream.ReadToEnd();
            }
            int first = address.IndexOf("Address: ") + 9;
            int last = address.LastIndexOf("</body>");
            address = address.Substring(first, last - first);
            return address;
        }
        catch (Exception) { return address; }
    }
}
