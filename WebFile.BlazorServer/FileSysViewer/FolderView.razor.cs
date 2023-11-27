using System.Diagnostics.CodeAnalysis;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using WebFile.BlazorServer.Data;
using WebFile.BlazorServer.Pages;

namespace WebFile.BlazorServer.FileSysViewer;

public sealed partial class FolderView
{
    [Parameter] public string? Text { get; set; }
    [Inject] [NotNull] public IDbContextFactory<WebFileContext>? DbFactory { get; set; }
    [Inject] [NotNull] public ToastService? ToastService { get; set; }
    [Inject] [NotNull] public IWebHostEnvironment? WebHostEnvironment { get; set; }
    [Inject] [NotNull] public NavigationManager? NavigationManager { get; set; }
    [Inject] [NotNull] public DownloadService? DownloadService { get; set; }
    [Inject] [NotNull] public DialogService? DialogService { get; set; }
    [Inject] [NotNull] public AuthenticationStateProvider? AuthStateProvider { get; set; }

    private UserModel User { get; set; } = new();
    private bool IsAuthenticated { get; set; }
    private List<FolderModel> FolderModels { get; set; } = new();
    private FolderModel Model = new();
    private string Url { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var claims = (await AuthStateProvider.GetAuthenticationStateAsync()).User;
        var user = claims.ToUser();
        if (user == null) return;

        await using var context = await DbFactory.CreateDbContextAsync();
        var userModel = await context.Users.Include(x => x.Files).FirstOrDefaultAsync(x => x.Equals(user));
        var folder = userModel?.Files.FirstOrDefault(x => x.Id == Text);
        if (folder == null) return;

        FolderModels = userModel!.GetFolder(folder.Path);
        Url = folder.Path;
        User = user;
        Model = folder.ToFolder();
        IsAuthenticated = true;
    }

    private async Task OnUploadFile(UploadFile arg)
    {
        await using var context = await DbFactory.CreateDbContextAsync();

        var user = await context.Users.Include(x => x.Files)
            .FirstOrDefaultAsync(x => x.Equals(User));
        if (user == null)
        {
            await ToastService.Error("上传文件", $"保存文件失败 {arg.OriginFileName}");
            return;
        }

        var filePath = WebHostEnvironment.WebRootPath + "\\UserFiles";
        if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);

        var fileName = $"{User.UserName}/{Convert.ToBase64String(
            System.Text.Encoding.Default.GetBytes($"{DateTime.Now:s}{Guid.NewGuid().ToString()}"))}/{arg.OriginFileName}";
        var saveFilePath = Path.Combine(filePath, fileName);

        if (await arg.SaveToFileAsync(saveFilePath, 1012 * 1024, new CancellationTokenSource().Token))
        {
            var pwd = $"{User.UserName}{DateTime.Now:s}{Guid.NewGuid().ToString()}{arg.OriginFileName}".HashEncryption().Replace("/", "-");
            user.Files.Add(new FileModel() { Path = fileName, Id = pwd, Url = Url });
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
            .FirstOrDefaultAsync(x => x.Equals(User));
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
            Title = "添加文件夹",
            Model = new FolderNameModel(),
            OnCloseAsync = () => Task.CompletedTask,
            OnEditAsync = async edit =>
            {
                var path = edit.Model.ToString();

                if (string.IsNullOrEmpty(path))
                {
                    await ToastService.Error("添加文件夹", "未填写信息");
                    return false;
                }

                await using var context = await DbFactory.CreateDbContextAsync();

                var user = await context.Users.Include(x => x.Files)
                    .FirstOrDefaultAsync(x => x.Equals(User));
                if (user == null)
                {
                    await ToastService.Error("添加文件夹", "添加文件夹失败");
                    return false;
                }

                var pwd = $"{User.UserName}{DateTime.Now:s}{Guid.NewGuid().ToString()}{path}".HashEncryption().Replace("/", "-");
                user.Files.Add(new FileModel() { Path = path, Id = pwd, Url = Url, IsFolder = true });
                await context.SaveChangesAsync();
                await ToastService.Success("添加文件夹成功");
                FolderModels = user.GetFolder();
                StateHasChanged();
                return true;
            }
        };

        await DialogService.ShowEditDialog(option);
    }

    private Task OnOpen(FolderModel model)
    {
        NavigationManager.NavigateTo(model.ToUrl());
        return Task.CompletedTask;
    }
}