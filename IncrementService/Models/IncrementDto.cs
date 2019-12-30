using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IncrementService.Models
{
    [Table("Keys")]
    public class IncrementDto
    {
        [Key]
        public long Id { get; set; }

        [Column("IncrementKey")]
        public string Key { get; set; }
        public long NextValue { get; set; }
        public DateTimeOffset LastUsed { get; set; }
    }
}