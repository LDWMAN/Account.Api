namespace AccountApi.Model.Entity;

public class BaseEntity
{
public DateTime CreateDate { get; set; } = DateTime.Now;
public DateTime UpdateDate { get; set; }
}