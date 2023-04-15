using System;
using System.Threading;
using System.Threading.Tasks;

namespace XIVDeck.FFXIVPlugin.Utils; 

public static class AsyncUtils {
    public static async Task<IDisposable> UseWaitAsync(this SemaphoreSlim semaphore, CancellationToken ct = default) {
        await semaphore.WaitAsync(ct).ConfigureAwait(false);

        return new DisposableWrapper(() => {
            semaphore.Release();
        });
    }
}