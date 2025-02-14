#region Test Cases

using System;
using System.Collections;
using System.Collections.Generic;
using DMapper.Attributes;
using DMapper.Extensions;
using DMapper.Helpers;

/////////////////////////
// Test 1: Basic Mapping with [BindTo] on top-level properties.
public class Source1
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Source { get; set; }
    public Source2 Source2 { get; set; } = new Source2();
}
public class Source2
{
    public string SourceName2 { get; set; } = "SourceName2";
}
public class Destination1
{
    public int Id { get; set; }
    public string Name { get; set; }
    [BindTo("Source")]
    public string Destination { get; set; }
    public Destination2 Source2 { get; set; }
    
    public string DontChange { get; set; } = "DontChange";
}
public class Destination2
{
    [BindTo("SourceName2")]
    public string DestinationName3 { get; set; }
}

/////////////////////////
// Test 2: Inner Mapping with [BindTo] on nested property.
public class SourceTest2
{
    public string Outer { get; set; } = "OuterValue";
    public InnerSource Inner { get; set; } = new InnerSource();
}
public class InnerSource
{
    public string InnerProp { get; set; } = "InnerValue";
}
public class DestinationTest2
{
    public string Outer { get; set; }
    public InnerDest Inner { get; set; }
}
public class InnerDest
{
    [BindTo("InnerProp")]
    public string MyInner { get; set; }
}

/////////////////////////
// Test 3: ComplexBind mapping using an absolute destination key.
public class SourceTest3
{
    public string Data { get; set; } = "DataFromSource";
    public NestedSource Nested { get; set; } = new NestedSource();
}
public class NestedSource
{
    public string Info { get; set; } = "NestedInfo";
}
public class DestinationTest3
{
    public string Data { get; set; }
    [ComplexBind("NestedDestination.Info", "Nested.Info")]
    public NestedDest NestedDestination { get; set; }
}
public class NestedDest
{
    public string Info { get; set; }
}

/////////////////////////
// Test 4: ComplexBind on inner property with an absolute key.
public class SourceTest4
{
    public string Extra { get; set; } = "ExtraValue";
}
public class DestinationTest4
{
    [ComplexBind("Sub.Info", "Extra")]
    public SubDest Sub { get; set; }
}
public class SubDest
{
    public string Info { get; set; }
}

/////////////////////////
// Test 5: Array mapping with [BindTo].
public class SourceTest5
{
    public string[] Items { get; set; } = new string[] { "Item1", "Item2", "Item3" };
}
public class DestinationTest5
{
    [BindTo("Items")]
    public string[] Items { get; set; }
}

/////////////////////////
// Test 6: Fallback candidate using [BindTo].
public class SourceTest6
{
    public string X { get; set; } = "FallbackValue";
    public string B { get; set; } = "Beta";
}
public class DestinationTest6
{
    [BindTo("X")]
    public string B { get; set; }
}

/////////////////////////
// Test 7: Relative BindTo with nested path "B.C"
public class SourceTest7
{
    public NestedSource7 A { get; set; } = new NestedSource7();
}
public class NestedSource7
{
    public NestedSource7Inner B { get; set; } = new NestedSource7Inner();
}
public class NestedSource7Inner
{
    public string C { get; set; } = "Value7";
}
public class DestinationTest7
{
    public DestinationTest7A A { get; set; } = new DestinationTest7A();
}
public class DestinationTest7A
{
    // Using a relative BindTo attribute. The candidate "B.C" will be merged into the key "A.B.C".
    [BindTo("A.B.C")]
    public string X { get; set; }
}

/////////////////////////
// Test 8: Absolute BindTo with full path.
public class SourceTest8
{
    public string M { get; set; } = "MValue";
    public NestedSource8 N { get; set; } = new NestedSource8();
}
public class NestedSource8
{
    public string O { get; set; } = "OValue";
}
public class DestinationTest8
{
    // Using an absolute BindTo key "N.O"
    [BindTo("N.O")]
    public string X { get; set; }
}

/////////////////////////
// Test 9: Nested relative BindTo on inner property.
public class SourceTest9
{
    public NestedSource9 A { get; set; } = new NestedSource9();
}
public class NestedSource9
{
    public string Y { get; set; } = "Value9";
}
public class DestinationTest9
{
    public DestinationTest9A A { get; set; } = new DestinationTest9A();
}
public class DestinationTest9A
{
    // Using a relative BindTo candidate "Y" to be applied to flattened key "A.Y"
    [BindTo("Y")]
    public string Z { get; set; }
}

/////////////////////////
// Test 10: Mapping a collection property (List of complex objects).
public class Src1
{
    public string Name { get; set; } = "Pesho";
    public List<Src2> Src2List { get; set; } = new List<Src2> { new Src2(), new Src2() };
}
public class Src2
{
    public int Age { get; set; } = 10;
    public string Name { get; set; } = "John";
}
public class Dest1
{
    public string? Name { get; set; }
    public List<Dest2> Src2List { get; set; }
}
public class Dest2
{
    public int? Age { get; set; }
    public string? Name { get; set; }
}

/////////////////////////
// Test 11: Mapping a collection (List & Array) of complex objects.
public class CollectionSource1
{
    public List<CollectionItemSource> Items { get; set; } = new List<CollectionItemSource>
    {
        new CollectionItemSource { Value = "A", Number = 1 },
        new CollectionItemSource { Value = "B", Number = 2 }
    };

    public CollectionItemSource[] ArrayItems { get; set; } = new CollectionItemSource[]
    {
        new CollectionItemSource { Value = "C", Number = 3 },
        new CollectionItemSource { Value = "D", Number = 4 }
    };
}
public class CollectionItemSource
{
    public string Value { get; set; }
    public int Number { get; set; }
}
public class CollectionDestination1
{
    [BindTo("Items")]
    public List<CollectionItemDest> Items { get; set; }
    
    [BindTo("ArrayItems")]
    public CollectionItemDest[] ArrayItems { get; set; }
}
public class CollectionItemDest
{
    public string Value { get; set; }
    public int Number { get; set; }
}

/////////////////////////
// Test 12: Mapping nested collections (a list of lists).
public class NestedCollectionSource
{
    public List<List<CollectionItemSource>> NestedItems { get; set; } = new List<List<CollectionItemSource>>
    {
        new List<CollectionItemSource>
        {
            new CollectionItemSource { Value = "E", Number = 5 },
            new CollectionItemSource { Value = "F", Number = 6 }
        },
        new List<CollectionItemSource>
        {
            new CollectionItemSource { Value = "G", Number = 7 }
        }
    };
}
public class NestedCollectionDestination
{
    public List<List<CollectionItemDest>> NestedItems { get; set; }
}

/////////////////////////
// Test 13: Mapping a Lists with [BindTo]
public class Src1_13
{
    public string Name { get; set; } = "Pesho";
    public List<Src2_13> Src2List { get; set; } = new List<Src2_13> { new Src2_13(), new Src2_13() };
}
public class Src2_13
{
    public int Age { get; set; } = 10;
    public string Name { get; set; } = "John";
}
public class Dest1_13
{
    public string? Name { get; set; }
    
    [BindTo("Src2List")]
    public List<Dest2_13> Src2List_13 { get; set; }
}
public class Dest2_13
{
    public int? Age { get; set; }
    public string? Name { get; set; }
}

/////////////////////////
// Test 14: Mapping a Lists with [BindTo] on they props as well
public class Src1_14
{
    public string Name { get; set; } = "Pesho";
    public List<Src2_14> Src2List { get; set; } = new List<Src2_14> { new Src2_14(), new Src2_14() };
}
public class Src2_14
{
    public int Age { get; set; } = 10;
    public string Name { get; set; } = "John";
}
public class Dest1_14
{
    public string? Name { get; set; }
    
    [BindTo("Src2List")]
    public List<Dest2_14> Src2List_13 { get; set; }
}
public class Dest2_14
{
    [BindTo("Age")]
    public int? Age2 { get; set; }
    public string? Name { get; set; }
}



#endregion

#region Program Entry Point

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Test 1: Basic Mapping");
        var src1 = new Source1 { Id = 1, Name = "Test1", Source = "SourceValue" };
        Destination1 dest1 = new Destination1();
        dest1 = ReflectionHelper.ReplacePropertiesRecursive_V5<Destination1, Source1>(dest1, src1);
        Console.WriteLine($"Id: {dest1.Id}, Name: {dest1.Name}, Destination: {dest1.Destination}");
        Console.WriteLine($"Source2.DestinationName3: {dest1.Source2?.DestinationName3}");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 2: Inner Mapping with [BindTo]");
        var src2 = new SourceTest2();
        DestinationTest2 dest2 = new DestinationTest2();
        dest2 = ReflectionHelper.ReplacePropertiesRecursive_V5<DestinationTest2, SourceTest2>(dest2, src2);
        Console.WriteLine($"Outer: {dest2.Outer}");
        Console.WriteLine($"Inner.MyInner: {dest2.Inner?.MyInner}");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 3: ComplexBind Mapping (Absolute Key)");
        var src3 = new SourceTest3();
        DestinationTest3 dest3 = new DestinationTest3();
        dest3 = ReflectionHelper.ReplacePropertiesRecursive_V5<DestinationTest3, SourceTest3>(dest3, src3);
        Console.WriteLine($"Data: {dest3.Data}");
        Console.WriteLine($"NestedDestination.Info: {dest3.NestedDestination?.Info}");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 4: ComplexBind on Inner Property");
        var src4 = new SourceTest4();
        DestinationTest4 dest4 = new DestinationTest4();
        dest4 = ReflectionHelper.ReplacePropertiesRecursive_V5<DestinationTest4, SourceTest4>(dest4, src4);
        Console.WriteLine($"Sub.Info: {dest4.Sub?.Info}");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 5: Array Mapping");
        var src5 = new SourceTest5();
        DestinationTest5 dest5 = new DestinationTest5();
        dest5 = ReflectionHelper.ReplacePropertiesRecursive_V5<DestinationTest5, SourceTest5>(dest5, src5);
        Console.WriteLine("Items: " + (dest5.Items != null ? string.Join(", ", dest5.Items) : "null"));
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 6: Fallback Mapping Using [BindTo] Candidate");
        var src6 = new SourceTest6();
        DestinationTest6 dest6 = new DestinationTest6();
        dest6 = ReflectionHelper.ReplacePropertiesRecursive_V5<DestinationTest6, SourceTest6>(dest6, src6);
        Console.WriteLine($"B: {dest6.B} (expected 'FallbackValue')");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 7: Nested Relative BindTo (Candidate \"B.C\")");
        var src7 = new SourceTest7();
        DestinationTest7 dest7 = new DestinationTest7();
        dest7 = ReflectionHelper.ReplacePropertiesRecursive_V5<DestinationTest7, SourceTest7>(dest7, src7);
        Console.WriteLine($"A.X: {dest7.A?.X} (expected 'Value7')");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 8: Absolute BindTo with full path \"N.O\"");
        var src8 = new SourceTest8();
        DestinationTest8 dest8 = new DestinationTest8();
        dest8 = ReflectionHelper.ReplacePropertiesRecursive_V5<DestinationTest8, SourceTest8>(dest8, src8);
        Console.WriteLine($"X: {dest8.X} (expected 'OValue')");
        Console.WriteLine(new string('-', 40));

        Console.WriteLine("Test 9: Nested Relative BindTo on inner property");
        var src9 = new SourceTest9();
        DestinationTest9 dest9 = new DestinationTest9();
        dest9 = src9.MapTo<DestinationTest9>();
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

        Console.WriteLine("Test 12: Nested Collection Mapping (List of Lists)");
        var src12 = new NestedCollectionSource();
        var dest12 = src12.MapTo<NestedCollectionDestination>();
        int outerIndex = 0;
        foreach (var innerList in dest12.NestedItems)
        {
            Console.WriteLine($"  Inner list {outerIndex}:");
            foreach (var item in innerList)
            {
                Console.WriteLine($"    Value: {item.Value}, Number: {item.Number}");
            }
            outerIndex++;
        }
        Console.WriteLine(new string('-', 40));
        
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
        

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}

#endregion
