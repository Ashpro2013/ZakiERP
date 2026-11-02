using AshproSAASERP.ZatcaEngine.Enums;

namespace AshproSAASERP.ZatcaEngine.Constants;

public static class ZatcaEndpoints
{
    public static string GetServer(EnvironmentType env) => env switch
    {
        EnvironmentType.Production => "core",
        EnvironmentType.Simulation => "simulation",
        EnvironmentType.Developer or EnvironmentType.Sandbox => "developer-portal",
        _ => "developer-portal"
    };

    public static string GetClearance(EnvironmentType env)
    {
        return $"https://gw-fatoora.zatca.gov.sa/e-invoicing/{GetServer(env)}/invoices/clearance/single";
    }

    public static string GetReporting(EnvironmentType env)
    {
        return $"https://gw-fatoora.zatca.gov.sa/e-invoicing/{GetServer(env)}/invoices/reporting/single";
    }

    public static string GetCompliance(EnvironmentType env)
    {
        return $"https://gw-fatoora.zatca.gov.sa/e-invoicing/{GetServer(env)}/compliance";
    }

    public static string GetProductionCsid(EnvironmentType env)
    {
        return $"https://gw-fatoora.zatca.gov.sa/e-invoicing/{GetServer(env)}/production/csids";
    }

    public static string GetComplianceInvoice(EnvironmentType env)
    {
        return $"https://gw-fatoora.zatca.gov.sa/e-invoicing/{GetServer(env)}/compliance/invoices";
    }
}
