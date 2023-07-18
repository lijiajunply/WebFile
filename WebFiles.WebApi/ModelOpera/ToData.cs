using WebFiles.AppModel;
using WebFiles.WebApi.Data;

namespace WebFiles.WebApi.ModelOpera;

public class ToData
{
    public static FileDataModel ToFile(FileAppModel model)
    {
        return new FileDataModel();
    }

    public static UserDataModel ToUser(LoginModel model)
    {
        return new UserDataModel()
        {
            UserName = model.UserName,
            Password = model.Password
        };
    }
}