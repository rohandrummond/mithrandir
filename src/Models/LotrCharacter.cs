namespace mithrandir.Models;

public class LotRCharacter
{
    public required string Name { get; init; }
    public required string Title { get; init; }
    public required List<string> Traits { get; init; }
    public required string Quote { get; init; }
    public required string AsciiArt { get; init; }
}
