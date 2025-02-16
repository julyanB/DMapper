// Models for Test 1

using DMapper.Attributes;

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