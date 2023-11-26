using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebFile.BlazorServer.Data;

public class UserModel
{
    [Key]
    [Column(TypeName = "varchar(256)")]
    [Required(ErrorMessage = "名字出错")]
    public string UserName { get; set; }
    
    [Required(ErrorMessage = "密码出错")]
    public string Password { get; set; }
}