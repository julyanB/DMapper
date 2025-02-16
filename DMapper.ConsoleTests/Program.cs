using System;
using System.Collections;
using System.Collections.Generic;
using BenchmarkDotNet.Running;
using DMapper.Attributes;
using DMapper.Extensions; // Contains the MapTo extension method.
using DMapper.Helpers;
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
using TestCases.Benchmarks;


class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Test 1: Basic Mapping");
        var src1 = new Source1 { Id = 1, Name = "Test1", Source = "SourceValue" };
        var dest1 = src1.MapTo<Destination1>();
        Console.WriteLine($"Id: {dest1.Id}, Name: {dest1.Name}, Destination: {dest1.Destination}");
        Console.WriteLine($"Source2.DestinationName3: {dest1.Source2?.DestinationName3}");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 2: Inner Mapping with [BindTo]");
        var src2 = new SourceTest2();
        var dest2 = src2.MapTo<DestinationTest2>();
        Console.WriteLine($"Outer: {dest2.Outer}");
        Console.WriteLine($"Inner.MyInner: {dest2.Inner?.MyInner}");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 3: ComplexBind Mapping (Absolute Key)");
        var src3 = new SourceTest3();
        var dest3 = src3.MapTo<DestinationTest3>();
        Console.WriteLine($"Data: {dest3.Data}");
        Console.WriteLine($"NestedDestination.Info: {dest3.NestedDestination?.Info}");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 4: ComplexBind on Inner Property");
        var src4 = new SourceTest4();
        var dest4 = src4.MapTo<DestinationTest4>();
        Console.WriteLine($"Sub.Info: {dest4.Sub?.Info}");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 5: Array Mapping");
        var src5 = new SourceTest5();
        var dest5 = src5.MapTo<DestinationTest5>();
        Console.WriteLine("Items: " + (dest5.Items != null ? string.Join(", ", dest5.Items) : "null"));
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 6: Fallback Mapping Using [BindTo] Candidate");
        var src6 = new SourceTest6();
        var dest6 = src6.MapTo<DestinationTest6>();
        Console.WriteLine($"B: {dest6.B} (expected 'FallbackValue')");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 7: Nested Relative BindTo (Candidate \"B.C\")");
        var src7 = new SourceTest7();
        var dest7 = src7.MapTo<DestinationTest7>();
        Console.WriteLine($"A.X: {dest7.A?.X} (expected 'Value7')");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 8: Absolute BindTo with full path \"N.O\"");
        var src8 = new SourceTest8();
        var dest8 = src8.MapTo<DestinationTest8>();
        Console.WriteLine($"X: {dest8.X} (expected 'OValue')");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 9: Nested Relative BindTo on inner property");
        var src9 = new SourceTest9();
        var dest9 = src9.MapTo<DestinationTest9>();
        Console.WriteLine($"A.Z: {dest9.A?.Z} (expected 'Value9')");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 10: Mapping a Collection Property (List of Complex Objects)");
        var src10 = new Src1();
        var dest10 = src10.MapTo<Dest1>();
        Console.WriteLine($"Name: {dest10.Name}");
        Console.WriteLine($"Src2List count: {dest10.Src2List.Count}");
        Console.WriteLine($"Src2List[0].Age: {dest10.Src2List[0].Age}");
        Console.WriteLine($"Src2List[0].Name: {dest10.Src2List[0].Name}");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 11: Collection Mapping (List & Array) of Complex Objects");
        var src11 = new CollectionSource1();
        var dest11 = src11.MapTo<CollectionDestination1>();
        Console.WriteLine("List Items:");
        foreach (var item in dest11.Items)
        {
            Console.WriteLine($"  Value: {item.Value}, Number: {item.Number}");
        }
        Console.WriteLine("Array Items:");
        if (dest11.ArrayItems != null)
        {
            foreach (var item in dest11.ArrayItems)
            {
                Console.WriteLine($"  Value: {item.Value}, Number: {item.Number}");
            }
        }
        Console.WriteLine(new string('-', 40));

        // Console.WriteLine("Test 12: Nested Collection Mapping (List of Lists)");
        // var src12 = new NestedCollectionSource();
        // var dest12 = src12.MapTo<NestedCollectionDestination>();
        // int outerIndex = 0;
        // if (dest12.NestedItems != null)
        // {
        //     foreach (var innerList in dest12.NestedItems)
        //     {
        //         Console.WriteLine($"  Inner list {outerIndex}:");
        //         if (innerList != null)
        //         {
        //             foreach (var item in innerList)
        //             {
        //                 Console.WriteLine($"    Value: {item.Value}, Number: {item.Number}");
        //             }
        //         }
        //         outerIndex++;
        //     }
        // }
        // Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 13: Mapping a List with [BindTo]");
        var src13 = new Src1_13();
        var dest13 = src13.MapTo<Dest1_13>();
        Console.WriteLine($"Name: {dest13.Name}");
        Console.WriteLine($"Src2List_13 count: {dest13.Src2List_13.Count}");
        Console.WriteLine($"Src2List_13[0].Age: {dest13.Src2List_13[0].Age}");
        Console.WriteLine($"Src2List_13[0].Name: {dest13.Src2List_13[0].Name}");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 14: Mapping a List with [BindTo] on their props as well");
        var src14 = new Src1_14();
        var dest14 = src14.MapTo<Dest1_14>();
        Console.WriteLine($"Name: {dest14.Name}");
        Console.WriteLine($"Src2List_13 count: {dest14.Src2List_13.Count}");
        Console.WriteLine($"Src2List_13[0].Age2: {dest14.Src2List_13[0].Age2}");
        Console.WriteLine($"Src2List_13[0].Name: {dest14.Src2List_13[0].Name}");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 15: Cycle Dependency Mapping (Stack Overflow Detection)");
        var source15 = new SourceTest15 { Name = "Parent" };
        var child15 = new SourceTest15 { Name = "Child" };
        source15.Child = child15;
        child15.Child = source15; // Circular reference
        var dest15 = source15.MapTo<DestinationTest15>();
        Console.WriteLine("Mapping completed successfully:");
        Console.WriteLine($"  Parent Name: {dest15.Name}");
        Console.WriteLine($"  Child Name: {dest15.Child?.Name}");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 16: Multi source with same destination");
        var src1_16 = new SourceTest1_16();
        var src2_16 = new SourceTest2_16();
        var dest1_16 = src1_16.MapTo<DestinationTest16>();
        var dest2_16 = src2_16.MapTo<DestinationTest16>();
        Console.WriteLine($"Name: {dest1_16.Name}");
        Console.WriteLine($"Name: {dest2_16.Name}");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 17: Multi ComplexBinding source with same destination");
        var src1_17 = new SourceTest1_17();
        var src2_17 = new SourceTest2_17();
        var dest1_17 = src1_17.MapTo<DestinationTest1_17>();
        var dest2_17 = src2_17.MapTo<DestinationTest1_17>();
        Console.WriteLine($"Name: {dest1_17.DestinationTest2_17.Name}");
        Console.WriteLine($"Name: {dest2_17.DestinationTest2_17.Name}");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 18: Multi ComplexBinding source with complex destination");
        var src1_18 = new SourceTest1_18();
        var dest1_18 = src1_18.MapTo<DestinationTest1_18>();
        Console.WriteLine($"Name: {dest1_18.DestinationTest2_18.Name}");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 19: Multi ComplexBinding source with complex destination with [BindTo] on complex object");
        var src1_19 = new SourceTest1_19();
        var dest1_19 = src1_19.MapTo<DestinationTest1_19>();
        Console.WriteLine($"Name2: {dest1_19.DestinationTest2_19.Name2}");
        Console.WriteLine($"Age: {dest1_19.DestinationTest2_19.Age}");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
        
        //uncomment this to run benchmarks
        //var summary = BenchmarkRunner.Run<ComplexMappingBenchmarks>();
    }
}
