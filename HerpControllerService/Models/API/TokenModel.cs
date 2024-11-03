#nullable disable
namespace HerpControllerService.Models;

public class TokenModel
{
    public string Token { get; set; }
    
    public string RefreshToken { get; set; }
}