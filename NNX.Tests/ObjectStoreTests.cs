using System;
using FluentAssertions;
using Xunit;

namespace NNX.Tests
{
    public class ObjectStoreTests : ObjectStoreTestBase
    {
        [Fact]
        public void WhenItemAdded_ShouldReturnItem()
        {
            var item = "test";
            ObjectStore.Add("item", item);
            var result = ObjectStore.Get<string>("item");
            result.Should().Be(item);
        }

        [Fact]
        public void WhenTwoItemsAdded_ShouldReturnCorrectItem()
        {
            var item = "test";
            ObjectStore.Add("item", "test");
            ObjectStore.Add("item2", "test2");
            var result = ObjectStore.Get<string>("item");

            Assert.Equal(item, result);

        }

        [Fact]
        public void WhenGettingMissingItem_Throw()
        {
            Action action = () => ObjectStore.Get<string>("noSuch");
            action.ShouldThrow<NNXException>()
                .WithMessage("No such object: 'noSuch'");
        }

        [Fact]
        public void WhenGettingItemWithWrongType_Throw()
        {
            var obj = new object();
            ObjectStore.Add("badType", obj);
            Action action = () => ObjectStore.Get<string>("badType");
            action.ShouldThrow<NNXException>()
                .WithMessage("Object 'badType' was expected to be*");
        }

        [Fact]
        public void WhenItemOverwritten_ShouldGetNewItem()
        {
            var item = "test";
            ObjectStore.Add("item", 2);
            ObjectStore.Add("item", item);
            var result = ObjectStore.Get<string>("item");
            Assert.Equal(item, result);
        }

        [Fact]
        public void WhenClear_ShouldRemoveObjects()
        {
            ObjectStore.Add("item", "test");
            ObjectStore.Add("item2", "test2");

            ObjectStore.Clear();

            Assert.Throws<NNXException>(() => ObjectStore.Get<string>("item"));
            Assert.Throws<NNXException>(() => ObjectStore.Get<string>("item2"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData((string)null)]
        public void IfNameIsNullOrWhitespace_Throw(string badName)
        {
            Assert.Throws<NNXException>(() => ObjectStore.Add(badName, 2));
        }
    }
}
