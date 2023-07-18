using System.ComponentModel.DataAnnotations;

namespace WebFiles.WebApi.Data;

public class FileDataModel
{
    [Key]
    public int Key { get; set; }
    public string FilePath { get; set; }
}