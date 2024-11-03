using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HerpControllerService.Entities;

public class BaseEntity
{
    [Key]
    [Required]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("created_at")]
    [Required]
    public DateTime CreatedAt { get; set; }
    
    [Column("modified_at")]
    public DateTime? ModifiedAt { get; set; }
}