using System;
using System.Collections.Generic;

namespace IncrementService
{
    public partial class Keys
    {
        public long Id { get; set; }
        public string IncrementKey { get; set; }
        public long NextValue { get; set; }
        public DateTimeOffset LastUsed { get; set; }
    }
}
