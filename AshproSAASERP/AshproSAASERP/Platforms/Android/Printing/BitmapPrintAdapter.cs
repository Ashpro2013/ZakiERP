using Android.Print;
using Android.OS;
using Android.Graphics;
using Java.IO;
using SkiaSharp;

public class BitmapPrintAdapter : PrintDocumentAdapter
{
    private readonly SKBitmap _skBitmap;

    public BitmapPrintAdapter(SKBitmap skBitmap)
    {
        _skBitmap = skBitmap;
    }

    public override void OnLayout(
        PrintAttributes oldAttributes,
        PrintAttributes newAttributes,
        CancellationSignal cancellationSignal,
        LayoutResultCallback callback,
        Bundle extras)
    {
        var info = new PrintDocumentInfo
            .Builder("Invoice")
            .SetContentType(PrintContentType.Document)
            .SetPageCount(1)
            .Build();

        callback.OnLayoutFinished(info, true);
    }

    public override void OnWrite(
        PageRange[] pages,
        ParcelFileDescriptor destination,
        CancellationSignal cancellationSignal,
        WriteResultCallback callback)
    {
        // Java output stream
        OutputStream javaStream = new ParcelFileDescriptor.AutoCloseOutputStream(destination);

        // Convert Java stream → .NET stream
        using Stream dotNetStream = new Android.Runtime.OutputStreamInvoker(javaStream);

        // Create Skia PDF directly into Android print spool
        using var skStream = new SKManagedWStream(dotNetStream);
        using var document = SKDocument.CreatePdf(skStream);

        using (var canvas = document.BeginPage(_skBitmap.Width, _skBitmap.Height))
        {
            using var image = SKImage.FromBitmap(_skBitmap);
            canvas.DrawImage(image, 0, 0);
        }

        document.EndPage();
        document.Close();

        callback.OnWriteFinished(new[] { PageRange.AllPages });
    }
}
public static class SkiaAndroidExtensions
{
    public static Bitmap ToAndroidBitmap(this SKBitmap skBitmap)
    {
        var bmp = Bitmap.CreateBitmap(
            skBitmap.Width,
            skBitmap.Height,
            Bitmap.Config.Argb8888);

        var pixels = skBitmap.Bytes;
        var buffer = Java.Nio.ByteBuffer.Wrap(pixels);

        bmp.CopyPixelsFromBuffer(buffer);
        return bmp;
    }
}
