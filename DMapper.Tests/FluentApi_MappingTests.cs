using DMapper.Extensions;
using DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_AbsoluteBindTo_ShouldMapFullPathPropertyCorrectly;
using DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_ArrayMapping_ShouldMapArrayPropertiesCorrectly;
using DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_BasicMapping_ShouldMapTopLevelPropertiesCorrectly;
using DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_CollectionMapping_ShouldMapListOfComplexObjectsCorrectly;
using DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_ComplexBindMapping_ShouldMapAbsoluteKeyCorrectly;
using DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_ComplexBindMapping_ShouldMapNestedPropertyUsingFluentConfig;
using DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_ComplexObjectBinding_ShouldBindComplexObjectPropertyCorrectly;
using DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_ComplexObjectWithBindTo_ShouldBindComplexObjectWithFluentOverrideCorrectly;
using DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_CycleDependencyMapping_ShouldHandleCircularReferencesGracefully;
using DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_FallbackMapping_ShouldUseFallbackBindToCandidate;
using DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_InnerMapping_ShouldMapNestedPropertiesCorrectly;
using DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_ListMappingWithBindTo_ShouldMapListCorrectly;
using DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_ListMappingWithPropertyBindTo_ShouldMapPropertiesOnListItemsCorrectly;
using DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_MultiComplexBindingMapping_ShouldMapMultipleComplexSourcesCorrectly;
using DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_MultiSourceMapping_ShouldMapDifferentSourcesToSameDestination;
using DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_NestedCollectionMapping_ShouldMapCollectionsCorrectly;
using DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_NestedRelativeBindTo_ShouldMapFlattenedInnerPropertyCorrectly;
using DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_RelativeBindTo_ShouldMapNestedRelativePropertyCorrectly;

namespace DMapper.Tests;

public class FluentApi_MappingTests
{
    [Fact]
    public void FluentMapping_AbsoluteBindTo_ShouldMapFullPathPropertyCorrectly()
    {
        // Arrange
        var src = new SourceFluentTest8();
        // Act
        var dest = src.MapTo<DestinationFluentTest8>();
        // Assert
        Assert.Equal("OValue", dest.X);
    }

    [Fact]
    public void FluentMapping_ArrayMapping_ShouldMapArrayPropertiesCorrectly()
    {
        var src = new SourceFluentTest5();
        var dest = src.MapTo<DestinationFluentTest5>();
        Assert.Equal(src.Items, dest.Items);
    }

    [Fact]
    public void FluentMapping_BasicMapping_ShouldMapTopLevelPropertiesCorrectly()
    {
        var src = new SourceFluentTest1
        {
            Id = 1,
            Name = "Test1",
            Source = "SourceValue"
        };
        var dest = src.MapTo<DestinationFluentTest1>();
        Assert.Equal(src.Id, dest.Id);
        Assert.Equal(src.Name, dest.Name);
        Assert.Equal(src.Source, dest.Destination);
        Assert.Equal("SourceName2", dest.Source2?.Destination3_Fluent.DestinationName3);
        Assert.Equal("DontChange", dest.DontChange);
    }

    [Fact]
    public void FluentMapping_CollectionMapping_ShouldMapListOfComplexObjectsCorrectly()
    {
        var src = new Src1_Fluent();
        var dest = src.MapTo<Dest1_Fluent>();
        Assert.Equal(src.Name, dest.Name);
        Assert.Equal(src.Src2List.Count, dest.Src2List.Count);
        Assert.Equal(src.Src2List[0].Age, dest.Src2List[0].Age);
        Assert.Equal(src.Src2List[0].Name, dest.Src2List[0].Name);
    }

    [Fact]
    public void FluentMapping_ComplexBindMapping_ShouldMapAbsoluteKeyCorrectly()
    {
        var src = new SourceFluentTest3();
        var dest = src.MapTo<DestinationFluentTest3>();
        Assert.Equal("DataFromSource", dest.Data);
        Assert.Equal("NestedInfo", dest.NestedDestination?.Info);
    }

    [Fact]
    public void FluentMapping_ComplexBindMapping_ShouldMapNestedPropertyUsingFluentConfig()
    {
        var src = new SourceFluentTest4();
        var dest = src.MapTo<DestinationFluentTest4>();
        Assert.Equal("ExtraValue", dest.Sub?.Info);
    }

    [Fact]
    public void FluentMapping_ComplexObjectBinding_ShouldBindComplexObjectPropertyCorrectly()
    {
        var src = new SourceFluentTest1_18();
        var dest = src.MapTo<DestinationFluentTest1_18>();
        Assert.Equal("Source", dest.DestinationTest2_18?.Name);
    }

    [Fact]
    public void FluentMapping_ComplexObjectWithBindTo_ShouldBindComplexObjectWithFluentOverrideCorrectly()
    {
        var src = new SourceFluentTest1_19();
        var dest = src.MapTo<DestinationFluentTest1_19>();
        Assert.Equal("Source", dest.DestinationTest2_19?.Name2);
        Assert.Equal(25, dest.DestinationTest2_19?.Age);
    }

    [Fact]
    public void FluentMapping_CycleDependencyMapping_ShouldHandleCircularReferencesGracefully()
    {
        var src = new SourceFluentTest15 { Name = "Parent" };
        var child = new SourceFluentTest15 { Name = "Child" };
        src.Child = child;
        child.Child = src; // Circular reference
        var dest = src.MapTo<DestinationFluentTest15>();
        Assert.Equal("Parent", dest.Name);
        // Expect that Child is not mapped (to avoid recursion)
        Assert.Null(dest.Child?.Name);
    }

    [Fact]
    public void FluentMapping_FallbackMapping_ShouldUseFallbackBindToCandidate()
    {
        var src = new SourceFluentTest6();
        var dest = src.MapTo<DestinationFluentTest6>();
        Assert.Equal("FallbackValue", dest.B);
    }

    [Fact]
    public void FluentMapping_InnerMapping_ShouldMapNestedPropertiesCorrectly()
    {
        var src = new SourceFluentTest2();
        var dest = src.MapTo<DestinationFluentTest2>();
        Assert.Equal("OuterValue", dest.Outer);
        Assert.Equal("InnerValue", dest.Inner?.MyInner);
    }

    [Fact]
    public void FluentMapping_NestedCollectionMapping_ShouldMapCollectionsCorrectly()
    {
        var src = new CollectionSourceFluent1();
        var dest = src.MapTo<CollectionDestinationFluent1>();
        Assert.Equal(src.Items.Count, dest.Items.Count);
        Assert.Equal(src.Items[0].Value, dest.Items[0].Value);
        Assert.Equal(src.Items[0].Number, dest.Items[0].Number);
        Assert.Equal(src.ArrayItems.Length, dest.ArrayItems.Length);
        Assert.Equal(src.ArrayItems[0].Value, dest.ArrayItems[0].Value);
        Assert.Equal(src.ArrayItems[0].Number, dest.ArrayItems[0].Number);
    }

    [Fact]
    public void FluentMapping_ListMappingWithBindTo_ShouldMapListCorrectly()
    {
        var src = new Src1_13_Fluent();
        var dest = src.MapTo<Dest1_13_Fluent>();
        Assert.Equal(src.Name, dest.Name);
        Assert.Equal(src.Src2List.Count, dest.Src2List_13.Count);
        Assert.Equal(src.Src2List[0].Age, dest.Src2List_13[0].Age);
        Assert.Equal(src.Src2List[0].Name, dest.Src2List_13[0].Name);
    }

    [Fact]
    public void FluentMapping_ListMappingWithPropertyBindTo_ShouldMapPropertiesOnListItemsCorrectly()
    {
        var src = new Src1_14_Fluent();
        var dest = src.MapTo<Dest1_14_Fluent>();
        Assert.Equal(src.Name, dest.Name);
        Assert.Equal(src.Src2List.Count, dest.Src2List_13.Count);
        Assert.Equal(src.Src2List[0].Age, dest.Src2List_13[0].Age2);
        Assert.Equal(src.Src2List[0].Name, dest.Src2List_13[0].Name);
    }

    [Fact]
    public void FluentMapping_MultiComplexBindingMapping_ShouldMapMultipleComplexSourcesCorrectly()
    {
        var src1 = new SourceFluentTest1_17();
        var src2 = new SourceFluentTest2_17();
        var dest1 = src1.MapTo<DestinationFluentTest1_17>();
        var dest2 = src2.MapTo<DestinationFluentTest1_17>();
        Assert.Equal("Source1", dest1.DestinationTest2_17.Name);
        Assert.Equal("Source2", dest2.DestinationTest2_17.Name);
    }

    [Fact]
    public void FluentMapping_MultiSourceMapping_ShouldMapDifferentSourcesToSameDestination()
    {
        var src1 = new SourceFluentTest1_16();
        var src2 = new SourceFluentTest2_16();
        var dest1 = src1.MapTo<DestinationFluentTest16>();
        var dest2 = src2.MapTo<DestinationFluentTest16>();
        Assert.Equal("Source1", dest1.Name);
        Assert.Equal("Source2", dest2.Name);
    }

    [Fact]
    public void FluentMapping_NestedRelativeBindTo_ShouldMapFlattenedInnerPropertyCorrectly()
    {
        var src = new SourceFluentTest9();
        var dest = src.MapTo<DestinationFluentTest9>();
        Assert.Equal("Value9", dest.A.Z);
    }

    [Fact]
    public void FluentMapping_RelativeBindTo_ShouldMapNestedRelativePropertyCorrectly()
    {
        var src = new SourceFluentTest7();
        var dest = src.MapTo<DestinationFluentTest7>();
        Assert.Equal("Value7", dest.A.X);
    }
}