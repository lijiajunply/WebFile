using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebFile.BlazorServer.Data;

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
            .Select(x => new FolderModel() { Path = x.Path, Url = url, Id = x.Id }).ToList();
}

public class FolderModel
{
    public string Path { get; set; }
    public string Url { get; set; }
    public string Id { get; set; }
    public Stream GetStream(string root)
        => new FileStream($@"{root}\UserFiles\{Path}",FileMode.Open);

    public bool IsFolder() => string.IsNullOrEmpty(Path);
}