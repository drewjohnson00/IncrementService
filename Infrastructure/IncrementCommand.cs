namespace Infrastructure;

public class IncrementCommand
{
    public required string Key { get; set; }
    public long? PreviousValue { get; set; } = 0;
}
