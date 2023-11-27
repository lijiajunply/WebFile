using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using WebFile.BlazorServer.Data;

namespace WebFile.BlazorServer.Pages;

public sealed partial class Index
{
    [Inject] [NotNull] public IDbContextFactory<WebFileContext>? DbFactory { get; set; }
    [Inject] [NotNull] public ToastService? ToastService { get; set; }
    [Inject] [NotNull] public IWebHostEnvironment? WebHostEnvironment { get; set; }
    [Inject] [NotNull] public NavigationManager? NavigationManager { get; set; }
    [Inject] [NotNull] public DownloadService? DownloadService { get; set; }
    [Inject] [NotNull] public DialogService? DialogService { get; set; }

    private bool IsAuthenticated { get; set; }
    private UserModel User { get; set; } = new();
    private List<FolderModel> FolderModels { get; set; } = new();

    [CascadingParameter] private Task<AuthenticationState>? authenticationState { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (authenticationState is not null)
        {
            var authState = await authenticationState;
            var user = authState.User;
            IsAuthenticated = user.Identity is not null && user.Identity.IsAuthenticated;
            if (IsAuthenticated)
            {
                var name = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
                var password = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
                {
                    IsAuthenticated = false;
                    return;
                }

                await using var context = await DbFactory.CreateDbContextAsync();
                var userModel = await context.Users.Include(x => x.Files)
                    .FirstOrDefaultAsync(x => x.UserName == name && x.Password == password);
                if (userModel == null)
                {
                    IsAuthenticated = false;
                    return;
                }

                User = new UserModel() { UserName = name, Password = password };
                FolderModels = userModel.GetFolder();
            }
        }
    }

    private async Task OnUploadFile(UploadFile arg)
    {
        var filePath = WebHostEnvironment.WebRootPath + "\\UserFiles";
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        var fileName = $"{User.UserName}/{DateTime.Now:d}/{arg.OriginFileName}";
        var saveFilePath = Path.Combine(filePath, fileName);

        var ret = await arg.SaveToFileAsync(saveFilePath, 1012 * 1024, new CancellationTokenSource().Token);

        if (ret)
        {
            await using var context = await DbFactory.CreateDbContextAsync();

            var user = await context.Users.Include(x => x.Files)
                .FirstOrDefaultAsync(x => x.UserName == User.UserName && x.Password == User.Password);
            if (user == null)
            {
                await ToastService.Error("上传文件", $"保存文件失败 {arg.OriginFileName}");
                return;
            }

            var pwd = Convert.ToBase64String(
                System.Text.Encoding.Default.GetBytes($"{User.UserName}{DateTime.Now:yyyy-M-d}{Guid.NewGuid().ToString()}{arg.OriginFileName}"));
            user.Files.Add(new FileModel() { Path = fileName, Id = pwd });
            await context.SaveChangesAsync();
            await ToastService.Success("上传文件成功");
            FolderModels = user.GetFolder();
            StateHasChanged();
            return;
        }

        await ToastService.Error("上传文件", $"保存文件失败 {arg.OriginFileName}");
    }

    private async Task OnDelete(FolderModel model)
    {
        await using var context = await DbFactory.CreateDbContextAsync();
        var userModel = await context.Users.Include(x => x.Files)
            .FirstOrDefaultAsync(x => x.UserName == User.UserName && x.Password == User.Password);
        if (userModel == null) return;
        var file = await context.FileModel.Include(x => x.Owner).FirstOrDefaultAsync(x => x.Id == model.Id);
        if (file == null) return;
        userModel.Files.Remove(file);
        var fileInfo = new FileInfo($@"{WebHostEnvironment.WebRootPath}\UserFiles\{file.Path}");
        fileInfo.Delete();
        await context.SaveChangesAsync();
        FolderModels = userModel.GetFolder();
        StateHasChanged();
    }

    private async Task OnDownload(FolderModel model)
    {
        await DownloadService.DownloadFromStreamAsync(Path.GetFileName(model.Path),
            model.GetStream(WebHostEnvironment.WebRootPath));
    }

    private Color GetIconColor(FolderModel model)
    {
        var i = FolderModels.FindIndex(fileModel => fileModel.Id == model.Id);
        i %= 11;
        if (i == 0) i = 4;
        return (Color)i;
    }

    private async Task OnAddFolder()
    {
        var option = new EditDialogOption<FolderNameModel>()
        {
            Title = "edit dialog",
            Model = new FolderNameModel(),
            OnCloseAsync = () => Task.CompletedTask,
            OnEditAsync = async edit =>
            {
                var url = edit.Model.ToString();
                
                if (string.IsNullOrEmpty(url))
                {
                    await ToastService.Error("添加文件夹", "未填写信息");
                    return false;
                }

                await using var context = await DbFactory.CreateDbContextAsync();

                var user = await context.Users.Include(x => x.Files)
                    .FirstOrDefaultAsync(x => x.UserName == User.UserName && x.Password == User.Password);
                if (user == null)
                {
                    await ToastService.Error("添加文件夹", "添加文件夹失败");
                    return false;
                }

                var pwd = Convert.ToBase64String(
                    System.Text.Encoding.Default.GetBytes($"{User.UserName}{DateTime.Now:yyyy-M-d}{Guid.NewGuid().ToString()}{url}"));
                user.Files.Add(new FileModel() { Path = "", Id = pwd, Url = url });
                await context.SaveChangesAsync();
                await ToastService.Success("添加文件夹成功");
                FolderModels = user.GetFolder();
                StateHasChanged();
                return true;
            }
        };

        await DialogService.ShowEditDialog(option);
    }

    private static string ToUrl(FolderModel model)
        => model.IsFolder() ? $"/FolderViewer/{model.Id}" : $"/WebFileView/{model.Id}";
}

public class FolderNameModel
{
    public string Name { get; set; }
    public override string ToString() => Name;
}