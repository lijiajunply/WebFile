namespace WebFiles.AppModel;

public class UserAppModel
{
    public string UserName { get; set; }
    public string? Image { get; set; }
    public List<FileAppModel> Files { get; set; } = new List<FileAppModel>();
}