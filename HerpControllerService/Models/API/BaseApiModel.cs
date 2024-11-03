namespace HerpControllerService.Models;

public class BaseApiModel
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}