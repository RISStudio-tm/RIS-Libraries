using System;

namespace RIS.Pools
{
    public sealed class ReusableResource<TResource> : IDisposable
    {
        private readonly Action _disposeAction;
        public TResource Resource { get; }

        public ReusableResource(TResource resource, Action disposeAction)
        {
            Resource = resource;
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            _disposeAction();
        }
    }
}
