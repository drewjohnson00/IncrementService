using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using IncrementService.Models;

namespace IncrementService.Tests
{
    public class IncrementModelMock : IIncrementModel
    {
        public static string DefaultKeyOne => "One";
        public static string DefaultKeyTwo => "Two";
        public static readonly DateTime DefaultLastUsedTime = DateTime.Parse("12/12/2019 3:30:00PM", CultureInfo.InvariantCulture);
        public static long DefaultPreviousValue => 42;

        public ModelResponse AddIncrement(string incrementKey, long initialValue)
        {
            return new ModelResponse(true, 0, "",
                new List<IncrementRow> { new IncrementRow { Key = incrementKey, LastUsed = DefaultLastUsedTime, PreviousValue = initialValue }
                });
        }

        public ModelResponse GetAllIncrements()
        {
            return new ModelResponse(true, 0, "",
                new List<IncrementRow> { new IncrementRow { Key = DefaultKeyOne, LastUsed = DefaultLastUsedTime, PreviousValue = DefaultPreviousValue },
                    new IncrementRow { Key = DefaultKeyTwo, LastUsed = DefaultLastUsedTime, PreviousValue = DefaultPreviousValue }
                });
        }

        public ModelResponse GetIncrement(string IncrementKey)
        {
            return new ModelResponse(true, 0, "",
                new List<IncrementRow> { new IncrementRow { Key = IncrementKey, LastUsed = DefaultLastUsedTime, PreviousValue = DefaultPreviousValue }
                });
        }

        public ModelResponse Increment(string IncrementKey)
        {
            return new ModelResponse(true, 0, "",
                new List<IncrementRow> { new IncrementRow { Key = IncrementKey, LastUsed = DefaultLastUsedTime, PreviousValue = DefaultPreviousValue }
                });
        }

        public ModelResponse RemoveIncrement(string IncrementKey)
        {
            return new ModelResponse(true, 0, "",
                new List<IncrementRow> { new IncrementRow { Key = IncrementKey, LastUsed = DefaultLastUsedTime, PreviousValue = 0 }
                });
        }
    }
}
