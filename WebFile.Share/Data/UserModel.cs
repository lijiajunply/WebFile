using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebFile.Share.Data;

public class UserModel
{
    [Key]
    [Column(TypeName = "varchar(256)")]
    [Required(ErrorMessage = "名字出错")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "密码出错")] public string Password { get; set; }

    public List<FileModel> Files { get; set; } = new();

    public List<FolderModel> GetFolder(string url = "")
        => Files.Where(x => x.Url == url)
            .Select(x => x.ToFolder()).ToList();

    public override bool Equals(object? obj)
    {
        if (obj is not UserModel user) return false;
        return user.UserName == UserName && user.Password == Password;
    }

    // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
    public override int GetHashCode() => base.GetHashCode();

    protected bool Equals(UserModel other)
        => UserName == other.UserName && Password == other.Password && Files.Equals(other.Files);
}