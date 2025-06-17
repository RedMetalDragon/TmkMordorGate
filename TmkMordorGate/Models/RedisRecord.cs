namespace TmkMordorGate.Models;

public class RedisRecord
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public List<string>? Items { get; set; }
    public DateTime CreatedDate { get; set; }
}