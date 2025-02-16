using Xunit;
using DMapper.Extensions;
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
using DMapper.Tests.Models.MappingTests.MapTo_NestedRelativeBindTo_ShouldMapRelativePropertyKeyCorrectly; // Contains the MapTo extension method.
// Ensure the appropriate namespaces for your source/destination classes are referenced.

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
    }
}
