namespace ZakiSAASERP.Models.Enums;

/// <summary>
/// Defines where images are stored for multi-tenant ERP system
/// </summary>
public enum ImageStorageType
{
    /// <summary>
    /// Images stored in local file system or network share
    /// Path configured in PermanentSettings.ImageLocalBasePath
    /// </summary>
    Local = 0,

    /// <summary>
    /// Images stored in AWS S3 bucket
    /// Bucket and credentials configured in PermanentSettings
    /// </summary>
    AWS = 1
}
