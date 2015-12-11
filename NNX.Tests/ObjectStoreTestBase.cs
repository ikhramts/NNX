using System;

namespace NNX.Tests
{
    public abstract class ObjectStoreTestBase : IDisposable
    {
        public void Dispose()
        {
            ObjectStore.Clear();
        }
    }
}
