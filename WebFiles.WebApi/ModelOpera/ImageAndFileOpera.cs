namespace WebFiles.WebApi.ModelOpera;

public static class ImageAndFileOpera
{
    private static async Task<string> GetUserImage(string key)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "UserInfo",$"{key}");
        var data = await File.ReadAllBytesAsync(path);
        return Convert.ToBase64String(data);
    }

    private static byte[] ImageFromString(string context)
    {
        return Convert.FromBase64String(context);
    }
}