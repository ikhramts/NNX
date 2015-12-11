using Xunit;

namespace NNX.Tests.ExcelFuncionsTests
{
    public class ClearAllObjectsTests : ObjectStoreTestBase
    {
        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        public void ShouldClearAllObjects(int numObjects)
        {
            ObjectStore.Add("obj1", "x");
            ObjectStore.Add("obj2", "x");

            ExcelFunctions.ClearAllObjects();

            Assert.Throws<NNXException>(() => ObjectStore.Get<string>("obj1"));
            Assert.Throws<NNXException>(() => ObjectStore.Get<string>("obj2"));
        }
    }
}
