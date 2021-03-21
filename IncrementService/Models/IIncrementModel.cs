

namespace IncrementService.Models
{
    public interface IIncrementModel
    {
        ModelResponse AddIncrement(string incrementKey, long initialValue);
        ModelResponse RemoveIncrement(string incrementKey);
        ModelResponse Increment(string incrementKey);
        ModelResponse GetAllIncrements();
        ModelResponse GetIncrement(string incrementKey);
    }
}
