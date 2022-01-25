using Account.API.Model.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Account.API.Filter
{
    public class ValidationFailedResult : ObjectResult
    {
        public ValidationFailedResult(ModelStateDictionary modelState) : base(new ValidationResultConfig(modelState))
        {
            StatusCode = StatusCodes.Status422UnprocessableEntity;
        }
    }
}