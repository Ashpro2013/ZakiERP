namespace ZakiSAASERP.Shared.Services;

public static class TaskExtensions
{
    /// <summary>
    /// Explicitly dispatches an unawaited asynchronous Task with an error-handling continuation.
    /// Prevents CS4014 warnings while ensuring unhandled exceptions do not crash the process or go unobserved.
    /// </summary>
    public static void SafeFireAndForget(this Task task, Action<Exception>? onError = null)
    {
        _ = task.ContinueWith(t =>
        {
            if (t.IsFaulted && t.Exception != null)
            {
                var ex = t.Exception.GetBaseException();
                onError?.Invoke(ex);
                System.Diagnostics.Debug.WriteLine($"[SafeFireAndForget] Unhandled task exception: {ex.Message}");
            }
        }, TaskScheduler.Default);
    }

    /// <summary>
    /// Explicitly dispatches an unawaited asynchronous Task{T} with an error-handling continuation.
    /// Prevents CS4014 warnings while ensuring unhandled exceptions do not crash the process or go unobserved.
    /// </summary>
    public static void SafeFireAndForget<T>(this Task<T> task, Action<Exception>? onError = null)
    {
        _ = task.ContinueWith(t =>
        {
            if (t.IsFaulted && t.Exception != null)
            {
                var ex = t.Exception.GetBaseException();
                onError?.Invoke(ex);
                System.Diagnostics.Debug.WriteLine($"[SafeFireAndForget] Unhandled task exception: {ex.Message}");
            }
        }, TaskScheduler.Default);
    }
}

