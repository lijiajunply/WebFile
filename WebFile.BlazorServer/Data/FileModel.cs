using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebFile.BlazorServer.Data;

public class FileModel
{
    public string Path { get; set; }

    public UserModel Owner { get; set; }

    [Key]
    [Column(TypeName = "varchar(256)")]
    public string Id { get; set; }

    public string GetUrl() => GetUrl(Path);
    public static string GetUrl(string path) => $"wwwroot/UserFiles/{path}";
}