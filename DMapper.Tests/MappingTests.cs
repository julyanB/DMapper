using DMapper.Extensions;
using DMapper.Tests.Models.Attribute_MappingTests.Map_NonNullable_To_NonNullable_ShouldMapCorrectly;
using DMapper.Tests.Models.Attribute_MappingTests.MapTo_FallbackEnumMapping_ShouldRemainNullForNullableEnum_WhenSourceIsNull;
using DMapper.Tests.Models.MappingTests.MapTo_AbsoluteBindTo_ShouldMapFullPathPropertyCorrectly;
using DMapper.Tests.Models.MappingTests.MapTo_ArrayMapping_ShouldMapArrayPropertiesCorrectly;
using DMapper.Tests.Models.MappingTests.MapTo_CollectionMapping_ShouldMapListAndArrayOfComplexObjectsCorrectly;
using DMapper.Tests.Models.MappingTests.MapTo_ComplexBindMapping_ShouldMapAbsoluteKeyCorrectly;
using DMapper.Tests.Models.MappingTests.MapTo_ComplexBindMapping_ShouldMapNestedPropertyUsingComplexBindAttribute;
using DMapper.Tests.Models.MappingTests.MapTo_ComplexObjectBinding_ShouldBindComplexObjectPropertyCorrectly;
using DMapper.Tests.Models.MappingTests.MapTo_ComplexObjectWithBindTo_ShouldBindComplexObjectWithBindToAttributeCorrectly;
using DMapper.Tests.Models.MappingTests.MapTo_CycleDependencyMapping_ShouldHandleCircularReferencesGracefully;
using DMapper.Tests.Models.MappingTests.MapTo_FallbackMapping_ShouldUseFallbackBindToCandidate;
using DMapper.Tests.Models.MappingTests.MapTo_InnerMapping_ShouldMapNestedPropertiesCorrectly;
using DMapper.Tests.Models.MappingTests.MapTo_ListMappingBindTo_ShouldMapListCorrectly;
using DMapper.Tests.Models.MappingTests.MapTo_ListMappingWithPropertyBindTo_ShouldMapPropertiesOnListItemsCorrectly;
using DMapper.Tests.Models.MappingTests.MapTo_MultiComplexBindingMapping_ShouldMapMultipleComplexSourcesCorrectly;
using DMapper.Tests.Models.MappingTests.MapTo_MultiSourceMapping_ShouldMapDifferentSourcesToSameDestination;
using DMapper.Tests.Models.MappingTests.MapTo_NestedCollectionMapping_ShouldMapNestedListsCorrectly;
using DMapper.Tests.Models.MappingTests.MapTo_NestedRelativeBindTo_ShouldMapFlattenedInnerPropertyCorrectly;
using DMapper.Tests.Models.MappingTests.MapTo_NestedRelativeBindTo_ShouldMapRelativePropertyKeyCorrectly;

namespace DMapper.Tests
{
    public class MappingTests
    {
        [Fact]
        public void MapTo_BasicMapping_ShouldMapTopLevelPropertiesCorrectly()
        {
            // Arrange
            var src = new Source1
            {
                Id = 1,
                Name = "Test1",
                Source = "SourceValue"
            };

            // Act
            var dest = src.MapTo<Destination1>();

            // Assert
            Assert.Equal(src.Id, dest.Id);
            Assert.Equal(src.Name, dest.Name);
            Assert.Equal(src.Source, dest.Destination);
            Assert.Equal("SourceName2", dest.Source2?.DestinationName3);
            Assert.Equal("DontChange", dest.DontChange);
        }

        [Fact]
        public void MapTo_InnerMapping_ShouldMapBindToAttributeOnNestedPropertyCorrectly()
        {
            // Arrange
            var src = new SourceTest2();

            // Act
            var dest = src.MapTo<DestinationTest2>();

            // Assert
            Assert.Equal(src.Outer, dest.Outer);
            Assert.Equal(src.Inner.InnerProp, dest.Inner.MyInner);
        }

        [Fact]
        public void MapTo_ComplexBindMapping_ShouldMapAbsoluteKeyCorrectly()
        {
            // Arrange
            var src = new SourceTest3();

            // Act
            var dest = src.MapTo<DestinationTest3>();

            // Assert
            Assert.Equal("DataFromSource", dest.Data);
            Assert.Equal("NestedInfo", dest.NestedDestination?.Info);
            Assert.Equal("NestedInfo_2", dest.NestedDestination?.NestedInfo);
        }

        [Fact]
        public void MapTo_ComplexBindMapping_ShouldMapNestedPropertyUsingComplexBindAttribute()
        {
            // Arrange
            var src = new SourceTest4();

            // Act
            var dest = src.MapTo<DestinationTest4>();

            // Assert
            Assert.Equal("ExtraValue", dest.Sub?.Info);
        }

        [Fact]
        public void MapTo_ArrayMapping_ShouldMapArrayPropertiesCorrectly()
        {
            // Arrange
            var src = new SourceTest5();

            // Act
            var dest = src.MapTo<DestinationTest5>();

            // Assert
            Assert.NotNull(dest.Items);
            Assert.Equal(src.Items, dest.Items);
        }

        [Fact]
        public void MapTo_FallbackMapping_ShouldUseFallbackBindToCandidate()
        {
            // Arrange
            var src = new SourceTest6();

            // Act
            var dest = src.MapTo<DestinationTest6>();

            // Assert
            Assert.Equal("FallbackValue", dest.B);
        }

        [Fact]
        public void MapTo_NestedRelativeBindTo_ShouldMapRelativePropertyKeyCorrectly()
        {
            // Arrange
            var src = new SourceTest7();

            // Act
            var dest = src.MapTo<DestinationTest7>();

            // Assert
            Assert.Equal("Value7", dest.A?.X);
        }

        [Fact]
        public void MapTo_AbsoluteBindTo_ShouldMapFullPathPropertyCorrectly()
        {
            // Arrange
            var src = new SourceTest8();

            // Act
            var dest = src.MapTo<DestinationTest8>();

            // Assert
            Assert.Equal("OValue", dest.X);
        }

        [Fact]
        public void MapTo_NestedRelativeBindTo_ShouldMapFlattenedInnerPropertyCorrectly()
        {
            // Arrange
            var src = new SourceTest9();

            // Act
            var dest = src.MapTo<DestinationTest9>();

            // Assert
            Assert.Equal("Value9", dest.A?.Z);
        }

        [Fact]
        public void MapTo_CollectionMapping_ShouldMapListOfComplexObjectsCorrectly()
        {
            // Arrange
            var src = new Src1();

            // Act
            var dest = src.MapTo<Dest1>();

            // Assert
            Assert.Equal(src.Name, dest.Name);
            Assert.Equal(src.Src2List.Count, dest.Src2List.Count);
            Assert.Equal(src.Src2List[0].Age, dest.Src2List[0].Age);
            Assert.Equal(src.Src2List[0].Name, dest.Src2List[0].Name);
        }

        [Fact]
        public void MapTo_CollectionMapping_ShouldMapListAndArrayOfComplexObjectsCorrectly()
        {
            // Arrange
            var src = new CollectionSource1();

            // Act
            var dest = src.MapTo<CollectionDestination1>();

            // Assert for List Items
            Assert.NotNull(dest.Items);
            Assert.Equal(2, dest.Items.Count);
            Assert.Equal("A", dest.Items[0].Value);
            Assert.Equal(1, dest.Items[0].Number);
            Assert.Equal("B", dest.Items[1].Value);
            Assert.Equal(2, dest.Items[1].Number);

            // Assert for Array Items
            Assert.NotNull(dest.ArrayItems);
            Assert.Equal(2, dest.ArrayItems.Length);
            Assert.Equal("C", dest.ArrayItems[0].Value);
            Assert.Equal(3, dest.ArrayItems[0].Number);
            Assert.Equal("D", dest.ArrayItems[1].Value);
            Assert.Equal(4, dest.ArrayItems[1].Number);
        }

        [Fact]
        public void MapTo_NestedCollectionMapping_ShouldMapNestedListsCorrectly()
        {
            // // Arrange
            // var src = new NestedCollectionSource();
            //
            // // Act
            // var dest = src.MapTo<NestedCollectionDestination>();
            //
            // // Assert
            // Assert.NotNull(dest.NestedItems);
            // Assert.Equal(2, dest.NestedItems.Count);
            //
            // // First inner list
            // var firstInnerList = dest.NestedItems[0];
            // Assert.Equal(2, firstInnerList.Count);
            // Assert.Equal("E", firstInnerList[0].Value);
            // Assert.Equal(5, firstInnerList[0].Number);
            // Assert.Equal("F", firstInnerList[1].Value);
            // Assert.Equal(6, firstInnerList[1].Number);
            //
            // // Second inner list
            // var secondInnerList = dest.NestedItems[1];
            // Assert.Single(secondInnerList);
            // Assert.Equal("G", secondInnerList[0].Value);
            // Assert.Equal(7, secondInnerList[0].Number);
        }

        [Fact]
        public void MapTo_ListMappingWithBindTo_ShouldMapListOfComplexObjectsUsingBindToAttribute()
        {
            // Arrange
            var src = new Src1_13();

            // Act
            var dest = src.MapTo<Dest1_13>();

            // Assert
            Assert.Equal(src.Name, dest.Name);
            Assert.Equal(src.Src2List.Count, dest.Src2List_13.Count);
            Assert.Equal(src.Src2List[0].Age, dest.Src2List_13[0].Age);
            Assert.Equal(src.Src2List[0].Name, dest.Src2List_13[0].Name);
        }

        [Fact]
        public void MapTo_ListMappingWithPropertyBindTo_ShouldMapListOfComplexObjectsAndTheirPropertiesCorrectly()
        {
            // Arrange
            var src = new Src1_14();

            // Act
            var dest = src.MapTo<Dest1_14>();

            // Assert
            Assert.Equal(src.Name, dest.Name);
            Assert.Equal(src.Src2List.Count, dest.Src2List_13.Count);
            Assert.Equal(src.Src2List[0].Age, dest.Src2List_13[0].Age2);
            Assert.Equal(src.Src2List[0].Name, dest.Src2List_13[0].Name);
        }

        [Fact]
        public void MapTo_CycleDependencyMapping_ShouldHandleCircularReferencesGracefully()
        {
            // Arrange
            var source = new SourceTest15 { Name = "Parent" };
            var child = new SourceTest15 { Name = "Child" };
            source.Child = child;
            child.Child = source; // Circular reference

            // Act
            var dest = source.MapTo<DestinationTest15>();

            // Assert
            Assert.Equal("Parent", dest.Name);
            Assert.Null(dest.Child?.Name);
            // Additional assertions can be added if your mapper limits recursion.
        }

        [Fact]
        public void MapTo_MultiSourceMapping_ShouldMapDifferentSourcesToSameDestinationProperty()
        {
            // Arrange
            var src1 = new SourceTest1_16();
            var src2 = new SourceTest2_16();

            // Act
            var dest1 = src1.MapTo<DestinationTest16>();
            var dest2 = src2.MapTo<DestinationTest16>();

            // Assert
            Assert.Equal("Source1", dest1.Name);
            Assert.Equal("Source2", dest2.Name);
        }

        [Fact]
        public void MapTo_MultiComplexBindingMapping_ShouldMapMultipleComplexSourcesToSameDestinationObject()
        {
            // Arrange
            var src1 = new SourceTest1_17();
            var src2 = new SourceTest2_17();

            // Act
            var dest1 = src1.MapTo<DestinationTest1_17>();
            var dest2 = src2.MapTo<DestinationTest1_17>();

            // Assert
            Assert.Equal("Source1", dest1.DestinationTest2_17?.Name);
            Assert.Equal("Source2", dest2.DestinationTest2_17?.Name);
        }

        [Fact]
        public void MapTo_ComplexObjectMapping_ShouldBindComplexObjectPropertyCorrectly()
        {
            // Arrange
            var src = new SourceTest1_18();

            // Act
            var dest = src.MapTo<DestinationTest1_18>();

            // Assert
            Assert.Equal("Source", dest.DestinationTest2_18?.Name);
        }

        [Fact]
        public void MapTo_ComplexObjectWithBindToMapping_ShouldBindComplexObjectWithBindToAttributeCorrectly()
        {
            // Arrange
            var src = new SourceTest1_19();

            // Act
            var dest = src.MapTo<DestinationTest1_19>();

            // Assert
            Assert.Equal("Source", dest.DestinationTest2_19?.Name2);
            Assert.Equal(25, dest.DestinationTest2_19?.Age);
        }

        [Fact]
        public void MapTo_Enum_NonNullable_To_NonNullable_ShouldMapCorrectly()
        {
            // Arrange
            var src = new SourceEnumNonNullable { Enum = TestEnum21_1.Test2 };

            // Act
            var dest = src.MapTo<DestinationEnumNonNullable>();

            // Assert
            Assert.Equal(TestEnum21_2.Test2, dest.Enum);
        }

        [Fact]
        public void MapTo_Enum_NonNullable_To_Nullable_ShouldMapCorrectly()
        {
            // Arrange
            var src = new SourceEnumNonNullable { Enum = TestEnum21_1.Test2 };

            // Act
            var dest = src.MapTo<DestinationEnumNullable>();

            // Assert
            Assert.True(dest.Enum.HasValue);
            Assert.Equal(TestEnum21_2.Test2, dest.Enum.Value);
        }

        [Fact]
        public void MapTo_Enum_Nullable_WithValue_To_NonNullable_ShouldMapCorrectly()
        {
            // Arrange
            var src = new SourceEnumNullable { Enum = TestEnum21_1.Test2 };

            // Act
            var dest = src.MapTo<DestinationEnumNonNullable2>();

            // Assert
            Assert.Equal(TestEnum21_2.Test2, dest.Enum);
        }

        [Fact]
        public void MapTo_Enum_Nullable_WithNull_To_Nullable_ShouldRemainNull()
        {
            // Arrange
            var src = new SourceEnumNullable { Enum = null };

            // Act
            var dest = src.MapTo<DestinationEnumNullable2>();

            // Assert
            Assert.False(dest.Enum.HasValue);
        }
        
        
                // Test 1: For a non‑nullable enum property, if the candidate provided by BindTo is invalid,
        // the mapper should fall back to the property name and convert the enum correctly.
        [Fact]
        public void MapTo_FallbackEnumMapping_ShouldUseFallbackCandidateForNonNullableEnum()
        {
            // Arrange: Source has an "Enum" property of type TestEnumA.
            var src = new SourceEnumFallback { Enum = TestEnumA.Value1 };
            // Act: Destination enum property is decorated with [BindTo("InvalidCandidate")];
            // fallback to "Enum" should occur.
            var dest = src.MapTo<DestinationEnumFallback>();
            // Assert: The destination enum should match the source converted to TestEnumB.
            Assert.Equal(TestEnumB.Value1, dest.Enum);
        }

        // Test 2: For a nullable enum property with a value, if the candidate is invalid the fallback is used.
        [Fact]
        public void MapTo_FallbackEnumMapping_ShouldUseFallbackCandidateForNullableEnum_WithValue()
        {
            // Arrange: Source has a non-null value in the nullable enum property.
            var src = new SourceEnumNullableFallback { Enum = TestEnumA.Value2 };
            // Act: Mapping using an invalid candidate should fall back to the property name.
            var dest = src.MapTo<DestinationEnumNullableFallback>();
            // Assert: The fallback conversion maps correctly.
            Assert.True(dest.Enum.HasValue);
            Assert.Equal(TestEnumB.Value2, dest.Enum.Value);
        }

        // Test 3: For a nullable enum property when the source value is null, the destination remains null.
        [Fact]
        public void MapTo_FallbackEnumMapping_ShouldRemainNullForNullableEnum_WhenSourceIsNull()
        {
            // Arrange: Source enum property is null.
            var src = new SourceEnumNullableFallback { Enum = null };
            // Act
            var dest = src.MapTo<DestinationEnumNullableFallback>();
            // Assert: The destination nullable enum remains null.
            Assert.False(dest.Enum.HasValue);
        }
        
    }
}
