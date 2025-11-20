using System.ComponentModel.DataAnnotations;
using Infrastructure;

namespace Repository.Entities;

internal class Keys
{
    [Key]
    public required long Id { get; set; }

    [StringLength(maximumLength: 50, MinimumLength = 3, ErrorMessage = "Invalid key length.")]
    public required string IncrementKey { get; set; }

    public required long PreviousValue { get; set; }

    public required DateTimeOffset LastUsed { get; set; }

    internal IncrementKey ToDto()
    {
        return new IncrementKey
        {
            Key = IncrementKey,
            PreviousValue = PreviousValue,
            LastUsed = LastUsed
        };
    }
}
