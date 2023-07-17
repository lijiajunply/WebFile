using System.ComponentModel.DataAnnotations;

namespace WebFiles.DataModel;

public class UserModel
{
    [Key]
    public int Key { get; set; }
    
    public string UserName { get; set; }
    public string Password { get; set; }
    public string? UserImage { get; set; }

    public List<FileModel> FileModels { get; set; } = new List<FileModel>();
}