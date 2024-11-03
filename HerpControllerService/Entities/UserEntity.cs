#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HerpControllerService.Enums;

namespace HerpControllerService.Entities;

[Table("users")]
public class UserEntity : BaseEntity
{
    [Column("username")]
    [Required]
    public string Username { get; set; }

    [Column("password")]
    [Required]
    public string Password { get; set; }
    
    [Column("salt")]
    public string Salt { get; set; }
    
    [Column("status", TypeName = "varchar")]
    [Required]
    public UserStatus Status { get; set; }
    
    [Column("refresh_token")]
    public string RefreshToken { get; set; }
    
    [Column("refresh_token_expiry")]
    public DateTime? RefreshTokenExpiry { get; set; }
}