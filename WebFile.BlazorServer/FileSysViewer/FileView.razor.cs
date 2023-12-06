using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using WebFile.BlazorServer.Providers;
using WebFile.Share.Data;

namespace WebFile.BlazorServer.FileSysViewer;

public sealed partial class FileView
{
    [Parameter] public string? Text { get; set; }
    public string Theme { get; set; } = "vs";
    private string Lang { get; set; } = "csharp";
    private bool IsCode { get; set; }
    private FileModel Model { get; set; } = new();
    private FileInfoModel? Info { get; set; }
    private string? CodeContext { get; set; }
    private string? CodeExtension { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(Text))
        {
            await Message.Show(new MessageOption() { Content = "无数据，正在为您跳转" });
            NavigationManager.NavigateTo("");
            return;
        }

        var claims = (await AuthStateProvider.GetAuthenticationStateAsync()).User;
        var user = claims.ToUser();

        if (user == null) return;

        await base.OnInitializedAsync();
        await using var context = await DbContext.CreateDbContextAsync();
        var model = await context.FileModel.Include(x => x.Owner).FirstOrDefaultAsync(x => x.Id == Text);
        if (model == null) return;
        if (!model.Owner.Equals(user)) return;
        Model = model;
        Info = new FileInfoModel(Model);

        var ext = Path.GetExtension(Model.Path).Replace(".", "");
        IsCode = FileBoolStatic.Extensions.Any(x => x == ext);
        if (IsCode)
        {
            CodeExtension = ext;
            var reader = new StreamReader(new FileStream(Model.Path.GetUrl(), FileMode.Open));
            CodeContext = await reader.ReadToEndAsync();
            reader.Dispose();
            Lang = ExtToLang();
        }

        StateHasChanged();
    }

    private string ExtToLang()
    {
        switch (CodeExtension)
        {
            case "bat":
            case "sh":
                return "shell";
            case "h":
                return "c";
            case "hpp":
                return "cpp";
            case "rs":
                return "rust";
            case "cs":
                return "csharp";
            case "fs":
                return "fsharp";
            case "kt":
                return "kotlin";
            case "js":
                return "javascript";
            case "ts":
                return "typescript";
            case "py":
            case "py3":
                return "python";
            case "py2":
                return "python2";
            case "jl":
                return "julia";
            case "axaml":
                return "xaml";
            default:
                return CodeExtension ?? "";
        }
    }

    public async Task GoBack()
    {
        await using var context = await DbContext.CreateDbContextAsync();
        var user = await context.Users.Include(x => x.Files).FirstOrDefaultAsync(x => x.Equals(Model.Owner));
        if(user == null)return;
        var model = user.Files.FirstOrDefault(x => x.Path == Model.Url);
        NavigationManager.NavigateTo(model == null?"":model.ToWebUrl(),true);
    }
}