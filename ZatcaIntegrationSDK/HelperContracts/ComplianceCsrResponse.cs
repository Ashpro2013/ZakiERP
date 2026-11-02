namespace ZatcaIntegrationSDK.HelperContracts
{

    public class ComplianceCsrResponse
    {
        public string RequestId { get; set; }
        public string DispositionMessage { get; set; }
        public string BinarySecurityToken { get; set; }
        public string Secret { get; set; }
        public string[] Errors { get; set; }
        public string ErrorMessage { get; set; }
        public int StatusCode { get; set; }
    }
}
