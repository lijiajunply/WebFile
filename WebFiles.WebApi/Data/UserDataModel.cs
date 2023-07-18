using System.ComponentModel.DataAnnotations;

namespace WebFiles.WebApi.Data;

public class UserDataModel
{
    [Key]
    public int Key { get; set; }
    
    public string UserName { get; set; }
    public string Password { get; set; }
    public string? UserImage { get; set; }

    public List<FileDataModel> FileModels { get; set; } = new List<FileDataModel>();
}