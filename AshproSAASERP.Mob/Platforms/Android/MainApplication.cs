using Android.App;
using Android.Runtime;

namespace AshproSAASERP.Mob.Platforms.Android;

[global::Android.App.Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
