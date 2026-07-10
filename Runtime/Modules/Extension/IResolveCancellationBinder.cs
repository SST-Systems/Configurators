using System.Threading;

namespace SST.Configurators
{
    /// <summary>Internal contract for data objects that accept the resolve-scoped cancellation token during resolve.</summary>
    internal interface IResolveCancellationBinder
    {
        void SetResolveCancellation(CancellationToken token);
    }
}