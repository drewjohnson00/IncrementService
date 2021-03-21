using System;
using System.Collections.Generic;


namespace IncrementService.Models
{
    public class ModelResponse
    {
        public ModelResponse(bool isSuccess, string errorMessage, List<IncrementRow> results)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            Results = results;
        }

        public bool IsSuccess { get; }
        public string ErrorMessage { get; }
        public List<IncrementRow> Results { get; }
    }
}
