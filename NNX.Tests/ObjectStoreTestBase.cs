using System;

namespace NNX.Tests
{
    public abstract class ObjectStoreTestBase : IDisposable
    {
        public virtual void Dispose()
        {
            ObjectStore.Clear();
        }
    }
}
