using System.Collections.Generic;


namespace IncrementService.Models
{
    public class ModelResponse
    {
        public ModelResponse(bool isSuccess, int errorCode, string errorMessage, List<IncrementRow> results)
        {
            IsSuccess = isSuccess;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            Results = results;
        }

        public bool IsSuccess { get; }
        public int ErrorCode { get; }
        public string ErrorMessage { get; }
        public List<IncrementRow> Results { get; }
    }
}
