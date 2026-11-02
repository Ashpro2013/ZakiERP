using System.Xml.Linq;

namespace ZakiSAASERP.ZatcaEngine.Ubl;

public static class UblNamespaces
{
    public static readonly XNamespace invoice =
        "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";

    public static readonly XNamespace cac =
        "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

    public static readonly XNamespace cbc =
        "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

    public static readonly XNamespace ext =
        "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
}
