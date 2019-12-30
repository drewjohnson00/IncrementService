using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IncrementService.Models
{
    public class DataResultDto
    {

        public DataResultDto() { }

        public DataResultDto(bool isSuccess, int errorCode, string errorMessage, List<IncrementDto> results)
        {
            IsSuccess = isSuccess;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            Results = results;
        }

        public bool IsSuccess { get; private set; }
        public int ErrorCode { get; private set; }
        public string ErrorMessage { get; private set; }
        public List<IncrementDto> Results { get; private set; }
    }
}
