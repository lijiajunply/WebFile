using System.ComponentModel.DataAnnotations;

namespace WebFiles.DataModel;

public class FileModel
{
    [Key]
    public int Key { get; set; }
    public string FilePath { get; set; }
}