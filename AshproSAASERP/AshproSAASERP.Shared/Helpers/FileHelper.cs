using Microsoft.AspNetCore.Components.Forms;

namespace AshproSAASERP.Shared.Helpers
{
    public static class FileHelper
    {
        // 1 MB limit (default)
        public const long MaxImageSize = 1 * 1024 * 1024;

        /// <summary>
        /// Validate file size and convert IBrowserFile to byte[].
        /// Returns null if size exceeds the limit.
        /// </summary>
        public static async Task<byte[]?> ValidateAndConvertAsync(IBrowserFile file, long maxSize = MaxImageSize)
        {
            if (file.Size > maxSize)
            {
                return null;
            }

            using var stream = file.OpenReadStream(maxSize);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }
    }
}
