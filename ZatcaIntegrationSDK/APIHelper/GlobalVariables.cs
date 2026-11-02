namespace ZatcaIntegrationSDK.APIHelper
{
    public enum Mode
    {
        developer,
        Simulation,
        Production
    }
    public static class GlobalVariables
    {
        // public static string BaseUrl = "https://gw-apic-gov.gazt.gov.sa/";
        //baseurl for simulation and production
        public static string BaseUrl = "https://gw-fatoora.zatca.gov.sa/";
        //developer for compliance
        //public static string mode = "developer-portal";

        //simulation for simulation
        //public static string mode = "simulation";

        //core for production
        //public static string mode = "core";
        //e-invoicing/core/compliance
        private static string GetServer(Mode mode)
        {
            if (mode == Mode.Production)
                return "core";
            else if (mode == Mode.Simulation)
                return "simulation";
            else
                return "developer-portal";
        }
        public static string ComplianceCsidEndpoint(Mode mode)
        {
            return "e-invoicing/" + GetServer(mode) + "/compliance";
        }
        public static string ProdCsidEndpoint(Mode mode)
        {
            if (mode == Mode.Simulation)
            {
                return "e-invoicing/" + GetServer(mode) + "/core/csids";
            }
            else
                return "e-invoicing/" + GetServer(mode) + "/production/csids";
        }
        public static string ComplianceInvoiceEndPoint(Mode mode)
        {
            return "e-invoicing/" + GetServer(mode) + "/compliance/invoices";
        }
        public static string InvoiceClearanceEndPoint(Mode mode)
        {
            return "e-invoicing/" + GetServer(mode) + "/invoices/clearance/single";
        }
        public static string InvoiceReportingEndPoint(Mode mode)
        {
            return "e-invoicing/" + GetServer(mode) + "/invoices/reporting/single";
        }

    }
}

