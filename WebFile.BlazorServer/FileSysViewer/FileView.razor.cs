using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using WebFile.BlazorServer.Providers;
using WebFile.Share.Data;

namespace WebFile.BlazorServer.FileSysViewer;

public sealed partial class FileView
{
    [Parameter] public string? Text { get; set; }
    private string Theme { get; set; } = "vs";
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
            CodeContext = await File.ReadAllTextAsync(Model.Path.GetUrl());
            Lang = ExtToLang();
        }

        StateHasChanged();
    }

    private string ExtToLang()
    {
        return CodeExtension switch
        {
            "bat" or "sh" => "shell",
            "h" => "c",
            "hpp" => "cpp",
            "rs" => "rust",
            "cs" => "csharp",
            "fs" => "fsharp",
            "kt" => "kotlin",
            "js" => "javascript",
            "ts" => "typescript",
            "py" or "py3" => "python",
            "py2" => "python2",
            "jl" => "julia",
            "axaml" => "xaml",
            _ => CodeExtension ?? ""
        };
    }

    private async Task GoBack()
    {
        await using var context = await DbContext.CreateDbContextAsync();
        var user = await context.Users.Include(x => x.Files).FirstOrDefaultAsync(x => x.Equals(Model.Owner));
        if(user == null)return;
        var model = user.Files.FirstOrDefault(x => x.Path == Model.Url);
        NavigationManager.NavigateTo(model == null?"":model.ToWebUrl(),true);
    }
}