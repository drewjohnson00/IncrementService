using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace IncrementService.Models
{
    public interface IIncrementData
    {
        DataResultDto AddIncrement(string IncrementKey, long initialValue);
        DataResultDto RemoveIncrement(string IncrementKey);
        DataResultDto Increment(string IncrementKey);
        DataResultDto GetAllIncrements();
        DataResultDto GetIncrement(string IncrementKey);
    }
}
