using System.Linq;
using AccountApi.Model.Configuration;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Account.API.Model.Configuration;

public class ValidationResultConfig
{
    public string Message { get; }
    public List<ValidationError> Errors { get; }

    public ValidationResultConfig(ModelStateDictionary modalState)
    {
        Message = "Model Error, 모델 상태를 확인하세요.";
        Errors = modalState.Keys
        .SelectMany(key => modalState[key].Errors.Select(x => new ValidationError(key, x.ErrorMessage)))
        .ToList();
    }
}