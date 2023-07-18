using WebFiles.AppModel;
using WebFiles.WebApi.Data;


namespace WebFiles.WebApi.ModelOpera;

public static class ToApp
{
    public static FileAppModel ToFile(FileDataModel model)
    {
        return new FileAppModel() { FileName = Path.GetFileName(model.FilePath)};
    }

    public static UserAppModel ToUser(UserDataModel model)
    {
        return new UserAppModel()
        {
            UserName = model.UserName,
            Files = model.FileModels.Select(ToFile).ToList()
        };
    }
}