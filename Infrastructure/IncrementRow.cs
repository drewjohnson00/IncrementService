using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure;

public class IncrementKey
{
    [Column("IncrementKey")]
    [StringLength(maximumLength: 50, MinimumLength = 3, ErrorMessage = "Invalid key length.")]
    public required string Key { get; set; }
    public required long PreviousValue { get; set; }
    public required DateTimeOffset LastUsed { get; set; }
}
