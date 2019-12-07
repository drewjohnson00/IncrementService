using System;


namespace IncrementService.Models
{
    public class IncrementDto
    {
        public string Key { get; set; }
        public long NextVaue { get; set; }
        public DateTimeOffset LastUsed { get; set; }
    }
}