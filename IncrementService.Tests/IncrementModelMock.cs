using System;
using System.Collections.Generic;
using System.Text;
using IncrementService.Models;

namespace IncrementService.Tests
{
    public class IncrementModelMock : IIncrementData
    {
        public IncrementModelMock()
        {
            IncrementModelMock.DefaultLastUsedTime = DateTime.Parse("12/12/2019 3:30:00PM");
        }

        public static string DefaultKeyOne => "One";
        public static string DefaultKeyTwo => "Two";
        public static DateTime DefaultLastUsedTime { get; private set; }
        public static long DefaultNextValue => 42;

        public DataResultDto AddIncrement(string IncrementKey, long initialValue)
        {
            return new DataResultDto(true, 0, "",
                new List<IncrementDto> { new IncrementDto { Key = IncrementKey, LastUsed = DefaultLastUsedTime, NextValue = initialValue }
                });
        }

        public DataResultDto GetAllIncrements()
        {
            return new DataResultDto(true, 0, "",
                new List<IncrementDto> { new IncrementDto { Key = DefaultKeyOne, LastUsed = DefaultLastUsedTime, NextValue = DefaultNextValue },
                    new IncrementDto { Key = DefaultKeyTwo, LastUsed = DefaultLastUsedTime, NextValue = DefaultNextValue }
                });
        }

        public DataResultDto GetIncrement(string IncrementKey)
        {
            return new DataResultDto(true, 0, "",
                new List<IncrementDto> { new IncrementDto { Key = IncrementKey, LastUsed = DefaultLastUsedTime, NextValue = DefaultNextValue }
                });
        }

        public DataResultDto Increment(string IncrementKey)
        {
            return new DataResultDto(true, 0, "",
                new List<IncrementDto> { new IncrementDto { Key = IncrementKey, LastUsed = DefaultLastUsedTime, NextValue = DefaultNextValue }
                });
        }

        public DataResultDto RemoveIncrement(string IncrementKey)
        {
            return new DataResultDto(true, 0, "",
                new List<IncrementDto> { new IncrementDto { Key = IncrementKey, LastUsed = DefaultLastUsedTime, NextValue = 0 }
                });
        }
    }
}
