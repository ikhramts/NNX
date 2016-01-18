using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace NNX.Tests.ExcelFuncionsTests
{
    public class MakeObjectTests : IDisposable
    {
        private const string Name = "Thing";
        private const string ObjectType = "TestType";

        private readonly object[,] _properties = {{"Name", "Value"}};

        public MakeObjectTests()
        {
            ExcelFunctions.SupportedObjects = new Dictionary<string, Type>
            {
                {ObjectType, typeof(MakeObjectTestType) }
            };
        }

        public void Dispose()
        {
            ObjectStore.Clear();
            ExcelFunctions.SupportedObjects = ExcelFunctions.DefaultSupportedObjects;
        }

        [Fact]
        public void WhenTypeIsSupported_MakeObjectOfRequestedType()
        {
            ExcelFunctions.MakeObject(Name, ObjectType, new object[0, 0]);
            var result = ObjectStore.Get<MakeObjectTestType>(Name);
            result.Should().NotBeNull();
        }

        [Fact]
        public void IfObjectCreatedSuccessfully_ReturnObjectName()
        {
            var name = ExcelFunctions.MakeObject(Name, ObjectType, new object[0, 0]);
            name.Should().Be(Name);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void WhenTypeIsNullOrEmpty_Throw(string typeName)
        {
            Action action = () => ExcelFunctions.MakeObject(Name, typeName, new object[0, 0]);
            action.ShouldThrow<NNXException>()
                .WithMessage("*Argument TypeName should not be null or empty.*");
        }

        [Fact]
        public void WhenTypeIsNotSupported_Throw()
        {
            Action action = () => ExcelFunctions.MakeObject(Name, "nosuch", new object[0, 0]);
            action.ShouldThrow<NNXException>()
                .WithMessage("*Unrecognized object type: 'nosuch'.*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        public void WhenPropertyArrayIsEmpty_CreateObject(int arrayWidth)
        {
            ExcelFunctions.MakeObject(Name, ObjectType, new object[0, arrayWidth]);
            var result = ObjectStore.Get<MakeObjectTestType>(Name);
            result.Should().NotBeNull();
        }

        [Fact]
        public void WhenPropertyArrayIsNull_CreateObject()
        {
            ExcelFunctions.MakeObject(Name, ObjectType, null);
            var result = ObjectStore.Get<MakeObjectTestType>(Name);
            result.Should().NotBeNull();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void WhenPropertyArrayIsWrongWidth_Throw(int badWidth)
        {
            var properties = new object[1, badWidth];

            for (var i = 0; i < badWidth; i++)
                properties[0, i] = "StringProperty";

            Action action = () => ExcelFunctions.MakeObject(Name, ObjectType, properties);
            action.ShouldThrow<NNXException>()
                .WithMessage($"*Argument Properties must have width 2; was: {badWidth}.*");
        }

        [Fact]
        public void WhenPropertyDoesNotExist_Throw()
        {
            _properties[0, 0] = "NoSuch";
            Action action = () => ExcelFunctions.MakeObject(Name, ObjectType, _properties);
            action.ShouldThrow<NNXException>()
                .WithMessage($"*Object type {ObjectType} does not have property NoSuch.*");
        }

        [Theory]
        [InlineData(12)]
        [InlineData(12.0)]
        public void WhenIntPropertyIsAssigned_ShouldSetValue(object value)
        {
            _properties[0, 0] = "IntProperty";
            _properties[0, 1] = value;
            ExcelFunctions.MakeObject(Name, ObjectType, _properties);
            var result = ObjectStore.Get<MakeObjectTestType>(Name);
            result.IntProperty.Should().Be(12);
        }

        [Fact]
        public void WhenIntPropertyIsAssigned_IfDoubleValueHasFraction_Throw()
        {
            _properties[0, 0] = "IntProperty";
            _properties[0, 1] = 12.5;
            Action action = () => ExcelFunctions.MakeObject(Name, ObjectType, _properties);
            action.ShouldThrow<NNXException>()
                .WithMessage($"*Property IntProperty of object type {ObjectType} must be an integer; was {12.5}.*");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("bad")]
        public void WhenIntPropertyIsAssigned_IfValueIsNotIntOrDouble_Throw(object bad)
        {
            _properties[0, 0] = "IntProperty";
            _properties[0, 1] = bad;
            Action action = () => ExcelFunctions.MakeObject(Name, ObjectType, _properties);
            action.ShouldThrow<NNXException>()
                .WithMessage($"*Property IntProperty of object type {ObjectType} must be an integer; was {bad}.*");
        }

        [Theory]
        [InlineData(12.5)]
        [InlineData(12)]
        public void WhenDoublePropertyIsAssigned_ShouldSetValue(object value)
        {
            _properties[0, 0] = "DoubleProperty";
            _properties[0, 1] = value;
            ExcelFunctions.MakeObject(Name, ObjectType, _properties);
            var result = ObjectStore.Get<MakeObjectTestType>(Name);
            result.DoubleProperty.Should().Be(double.Parse(value.ToString()));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("bad")]
        public void WhenDoublePropertyIsAssigned_IfValueIsNotDoubleOrInteger_Throw(object bad)
        {
            _properties[0, 0] = "DoubleProperty";
            _properties[0, 1] = bad;
            Action action = () => ExcelFunctions.MakeObject(Name, ObjectType, _properties);
            action.ShouldThrow<NNXException>()
                .WithMessage($"*Property DoubleProperty of object type {ObjectType} must be a number; was {bad}.*");
        }

        [Theory]
        [InlineData("")]
        [InlineData("thing")]
        [InlineData(12.5)]
        [InlineData(12)]
        [InlineData(null)]
        public void WhenStringPropertyIsAssigned_ShouldSetValue(object value)
        {
            _properties[0, 0] = "StringProperty";
            _properties[0, 1] = value;
            ExcelFunctions.MakeObject(Name, ObjectType, _properties);
            var result = ObjectStore.Get<MakeObjectTestType>(Name);
            result.StringProperty.Should().Be(value?.ToString() ?? "");
        }

        [Fact]
        public void WhenStringEnumerableIsAssigned_IfValueRefersToStringArray_ShouldSetValue()
        {
            var expected = new[] {"one", "two"};
            ExcelFunctions.MakeArray("array", expected.Cast<object>().ToArray());
            _properties[0, 0] = "StringEnumerableProperty";
            _properties[0, 1] = "array";
            ExcelFunctions.MakeObject(Name, ObjectType, _properties);
            var result = ObjectStore.Get<MakeObjectTestType>(Name);

            result.StringEnumerableProperty.Should().Equal(expected);
        }

        [Fact]
        public void WhenIntEnumerableIsAssigned_IfValueRefersToWholeDoubleArray_ShouldSetValue()
        {
            var array = new[] {1.0, 2.0};
            var expected = new[] {1, 2};

            ExcelFunctions.MakeArray("array", array.Cast<object>().ToArray());
            _properties[0, 0] = "IntEnumerableProperty";
            _properties[0, 1] = "array";
            ExcelFunctions.MakeObject(Name, ObjectType, _properties);
            var result = ObjectStore.Get<MakeObjectTestType>(Name);

            result.IntEnumerableProperty.Should().Equal(expected);
        }

        [Fact]
        public void WhenDoubleEnumerableIsAssigned_IfValueRefersToDoubleArray_ShouldSetValue()
        {
            var expected = new[] { 1.1, 1.2 };
            ExcelFunctions.MakeArray("array", expected.Cast<object>().ToArray());
            _properties[0, 0] = "DoubleEnumerableProperty";
            _properties[0, 1] = "array";
            ExcelFunctions.MakeObject(Name, ObjectType, _properties);
            var result = ObjectStore.Get<MakeObjectTestType>(Name);

            result.DoubleEnumerableProperty.Should().Equal(expected);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(1.1)]
        public void WhenEnumerableValueIsAssigned_IfValueIsNotString_Throw(object bad)
        {
            _properties[0, 0] = "StringEnumerableProperty";
            _properties[0, 1] = bad;
            Action action = () => ExcelFunctions.MakeObject(Name, ObjectType, _properties);
            action.ShouldThrow<NNXException>()
                .WithMessage($"*Property StringEnumerableProperty of type {ObjectType} must refer to" +
                             " array containing elements of type string.*");
        }

        [Fact]
        public void WhenEnumerableValueIsAssigned_IfStringDoesNotPointToArray_Throw()
        {
            _properties[0, 0] = "StringEnumerableProperty";
            _properties[0, 1] = "array";
            Action action = () => ExcelFunctions.MakeObject(Name, ObjectType, _properties);
            action.ShouldThrow<NNXException>()
                .WithMessage($"*Property StringEnumerableProperty of type {ObjectType} must refer to" +
                             " array containing elements of type string.*");
        }

        [Theory]
        [MemberData("WhenEnumerableValueIsAssigned_IfStringPointsToWrongTypeArray_Throw_Cases")]
        public void WhenEnumerableValueIsAssigned_IfStringPointsToWrongTypeArray_Throw(
            string propertyName,
            object[] array,
            string expectedValueType,
            string badValueType)
        {
            ExcelFunctions.MakeArray("array", array);
            _properties[0, 0] = propertyName;
            _properties[0, 1] = "array";
            Action action = () => ExcelFunctions.MakeObject(Name, ObjectType, _properties);
            action.ShouldThrow<NNXException>()
                .WithMessage($"*Property {propertyName} of type {ObjectType} must refer to array containing " +
                             $"elements of type {expectedValueType}.*");
        }

        public static IEnumerable<object[]> WhenEnumerableValueIsAssigned_IfStringPointsToWrongTypeArray_Throw_Cases()
        {
            yield return new object[]
            {
                "StringEnumerableProperty",
                new object[] {1.0, 1.2},
                "string",
                "number"
            };
            yield return new object[]
            {
                "IntEnumerableProperty",
                new object[] {1.0, 1.2},
                "integer",
                "number"
            };
            yield return new object[]
            {
                "IntEnumerableProperty",
                new object[] {"one", "two"},
                "integer",
                "string"
            };
            yield return new object[]
            {
                "DoubleEnumerableProperty",
                new object[] {"one", "two"},
                "number",
                "string"
            };
        }

        [Fact]
        public void WhenDoubleArrayOfArraysValueIsAssigned_SetPropertyValue()
        {
            var array1 = new[] {1.1, 1.2};
            var array2 = new[] {2.1, 2.2, 2.3};
            ExcelFunctions.MakeArray("array1", array1.Cast<object>().ToArray());
            ExcelFunctions.MakeArray("array2", array2.Cast<object>().ToArray());

            ExcelFunctions.MakeWeights("array", new [] { "array1", "array2" });

            _properties[0, 0] = "DoubleArrayOfArrays";
            _properties[0, 1] = "array";
            ExcelFunctions.MakeObject(Name, ObjectType, _properties);

            var result = ObjectStore.Get<MakeObjectTestType>(Name);
            result.DoubleArrayOfArrays.Should().NotBeNullOrEmpty();
            result.DoubleArrayOfArrays.Should().HaveCount(2);
            result.DoubleArrayOfArrays[0].Should().Equal(array1);
            result.DoubleArrayOfArrays[1].Should().Equal(array2);
        }

        [Theory]
        [MemberData("WhenDoubleArrayOfArraysValueIsAssigned_IfTargetIsNotWeights_Throw_Cases")]
        public void WhenDoubleArrayOfArraysValueIsAssigned_IfTargetIsNotWeights_Throw(object bad)
        {
            ObjectStore.Add("bad", bad);
            _properties[0, 0] = "DoubleArrayOfArrays";
            _properties[0, 1] = "bad";
            Action action = () => ExcelFunctions.MakeObject(Name, ObjectType, _properties);

            action.ShouldThrow<NNXException>()
                .WithMessage($"*Property DoubleArrayOfArrays of type {ObjectType} must be of type " +
                             "Weights created using nnMakeWeights() function.*");
        }

        public static IEnumerable<object[]> WhenDoubleArrayOfArraysValueIsAssigned_IfTargetIsNotWeights_Throw_Cases()
        {
            return new[]
            {
                new object[] {"Bob"},
                new object[] {new object()},
                new object[] {new[] {"x", "y"}},
                new object[] {new[] {1.1, 1.2}},
            };
        }

        [Fact]
        public void WhenTwoPropertiesAssigned_ShouldSetBothProperties()
        {
            var properties = new object[2, 2]
            {
                {"StringProperty", "Bob" },
                {"DoubleProperty", 2.2 }
            };

            ExcelFunctions.MakeObject(Name, ObjectType, properties);
            var result = ObjectStore.Get<MakeObjectTestType>(Name);
            result.StringProperty.Should().Be("Bob");
            result.DoubleProperty.Should().Be(2.2);
        }

        //========================================================
        private class MakeObjectTestType
        {
            public string StringProperty { get; set; }
            public int IntProperty { get; set; }
            public double DoubleProperty { get; set; }
            public IEnumerable<int> IntEnumerableProperty { get; set; } 
            public IEnumerable<string> StringEnumerableProperty { get; set; } 
            public IEnumerable<double> DoubleEnumerableProperty { get; set; }
            public double[][] DoubleArrayOfArrays { get; set; }
        }
    }
}
