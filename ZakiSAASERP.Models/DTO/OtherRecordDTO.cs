using ZakiSAASERP.Models.Masters;
using ZakiSAASERP.Models.Others;
using ZakiSAASERP.Models.User;
using ZakiSAASERP.Models.Zatca;


namespace ZakiSAASERP.Models.DTO;

public record TransactionDialogDTO(
    string FormTitle,
    string FormTitle2,
    string IdHead,
    string NameHead,
    string DescriptionHead,
    string CodeHead,
    string AmountHead,
    string SearchText
);
public record PrintDesignDTO(PrintDesignHdr hdr, List<PrintDesignDtls> dtls);
public record BarcodeDesignDTO(BarcodeDesignHdr hdr, List<BarcodeDesignDtls> dtls);
public record PrintParameterDTO(PrintDesignHdr hdr, List<PrintDesignDtls> dtls, TransactionHdr tranHdr, List<TransactionDtls> tranDtls, Party party, CompanyDetails Company, List<FooterDetails>? fDtls);
public record PendingIcvData(int CountOne, int CountTwo, int CountThree);
public record IcvPendingPDTO(List<ExInvoiceICV> PendingInvoices, int TotalCount);
public record ICVReqDTO(DateTime? FrmDate, DateTime? ToDate, int BranchId, int Type);
public record IcvPendReqDTO(PaginationModel PaginationDTO, ICVReqDTO reqParameter);
public record IcvReupload(ExInvoiceICV icv, CompanyDetails Company = null);
public record IcvReuploadAll(List<ExInvoiceICV> icvs, CompanyDetails Company = null);
public record ZatcaUploadResponse(bool IsUploaded, string? ErrorMessage, string? QrCodeString);
public record PrintDesignReq(int? id, string VoucherType,bool isThermal = false);
public record UserPrivilegeDTO(UserPrivilegeHdr entUserPrivilegeHdr, List<ExUserPrivilege> entUserPrivilegeDtls, List<ExFuncDetails> entFunctionalPrivilege);
public record PrivilegeDTO(int id);
public record CommonModelDTO(List<CommonModel> commonModels);
public record ZatcaInvoiceDTO(List<ZatcaInvoices> ZatcaInvoices, int count);
public record PaymentResult(decimal Tendered, decimal Changes);


