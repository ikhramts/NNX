using System;

namespace NNX.AddIn.Tests
{
    public abstract class ObjectStoreTestBase : IDisposable
    {
        public virtual void Dispose()
        {
            ObjectStore.Clear();
        }
    }
}
