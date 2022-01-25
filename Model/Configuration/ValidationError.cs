namespace AccountApi.Model.Configuration;

public class ValidationError
{
    public string Field { get; set; }
    public string Message { get; set; }

    public ValidationError(string field, string message)
    {
        Field = field.Replace("$.", "") != string.Empty ? field.Replace("$.", "") : null;
        Message = message;
    }
}
