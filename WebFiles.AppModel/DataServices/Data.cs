namespace WebFiles.AppModel.DataServices;

public abstract class Data
{
    public HttpClient Client { get; set; }

    public Data(string baseUrl)
    {
        Client = new HttpClient() { BaseAddress = new Uri(baseUrl) };
    }
}