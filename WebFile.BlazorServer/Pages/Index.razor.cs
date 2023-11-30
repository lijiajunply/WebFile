using System.Diagnostics.CodeAnalysis;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using WebFile.Share.Data;
using WebFile.BlazorServer.Providers;

namespace WebFile.BlazorServer.Pages;

public sealed partial class Index
{
    [Inject] [NotNull] public IDbContextFactory<WebFileContext>? DbFactory { get; set; }
    [Inject] [NotNull] public ToastService? ToastService { get; set; }
    [Inject] [NotNull] public IWebHostEnvironment? WebHostEnvironment { get; set; }
    [Inject] [NotNull] public NavigationManager? NavigationManager { get; set; }
    [Inject] [NotNull] public DownloadService? DownloadService { get; set; }
    [Inject] [NotNull] public DialogService? DialogService { get; set; }
    [Inject] [NotNull] public AuthenticationStateProvider? AuthStateProvider { get; set; }

    private bool IsAuthenticated { get; set; }
    private UserModel User { get; set; } = new();
    private List<FolderModel> FolderModels { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var claims = (await AuthStateProvider.GetAuthenticationStateAsync()).User;
        var user = claims.ToUser();

        if (user == null)
            return;

        await using var context = await DbFactory.CreateDbContextAsync();
        var userModel = await context.Users.Include(x => x.Files)
            .FirstOrDefaultAsync(x => x.Equals(user));
        if (userModel == null) return;

        IsAuthenticated = true;
        User = user;
        FolderModels = userModel.GetFolder();
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
            System.Text.Encoding.Default.GetBytes($"{DateTime.Now:s}{Guid.NewGuid()}"))}/{arg.OriginFileName}";
        var saveFilePath = Path.Combine(filePath, fileName);

        if (await arg.SaveToFileAsync(saveFilePath))
        {
            var pwd = $"{User.UserName}{DateTime.Now:s}{Guid.NewGuid()}{arg.OriginFileName}".HashEncryption()
                .Replace("/", "-");
            user.Files.Add(new FileModel() { Path = fileName, Id = pwd });
            await context.SaveChangesAsync();
            await ToastService.Success("上传文件成功");
            FolderModels = user.GetFolder();
            StateHasChanged();
            return;
        }

        await ToastService.Error("上传文件", $"保存文件失败 {arg.OriginFileName}");
        var fileInfo = new FileInfo(saveFilePath);
        if (fileInfo.Exists) fileInfo.Delete();
        var dirInfo = new DirectoryInfo(Path.GetDirectoryName(saveFilePath)!);
        if (dirInfo.Exists) dirInfo.Delete();
    }

    private async Task OnDelete(IFile model)
    {
        await using var context = await DbFactory.CreateDbContextAsync();
        var userModel = await context.Users.Include(x => x.Files)
            .FirstOrDefaultAsync(x => x.Equals(User));
        if (userModel == null) return;
        var item = userModel.Files.FirstOrDefault(x => x.Id == model.Id);
        if (item == null) return;

        if (!item.IsFolder)
        {
            DeleteFile(item);
        }
        else
        {
            var list = userModel.Files.Where(x => x.Url == model.Path).ToList();
            foreach (var t in list.Where(t => t.Id != model.Id))
            {
                DeleteFile(t);
                userModel.Files.Remove(t);
            }
        }

        userModel.Files.Remove(item);
        await context.SaveChangesAsync();
        FolderModels = userModel.GetFolder();
        StateHasChanged();
    }

    private async Task OnDownload(IFile model)
    {
        await DownloadService.DownloadFromStreamAsync(Path.GetFileName(model.Path),
            model.GetStream(WebHostEnvironment.WebRootPath));
    }

    private Color GetIconColor(IFile model)
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

                if (user.Files.Any(x => Path.GetFileName(x.Path) == path))
                {
                    await ToastService.Error("添加文件夹", "有重名文件夹");
                    return false;
                }

                var pwd = $"{User.UserName}{DateTime.Now:s}{Guid.NewGuid().ToString()}{path}".HashEncryption()
                    .Replace("/", "-");
                user.Files.Add(new FileModel() { Path = path, Id = pwd, IsFolder = true });
                await context.SaveChangesAsync();
                await ToastService.Success("添加文件夹成功");
                FolderModels = user.GetFolder();
                StateHasChanged();
                return true;
            }
        };

        await DialogService.ShowEditDialog(option);
    }

    private Task OnOpen(IFile model)
    {
        NavigationManager.NavigateTo(model.ToWebUrl());
        return Task.CompletedTask;
    }

    private void DeleteFile(IFile file)
    {
        var path = $@"{WebHostEnvironment.WebRootPath}\UserFiles\{file.Path}";
        var fileInfo = new FileInfo(path);
        fileInfo.Delete();
        var dirInfo = new DirectoryInfo(Path.GetDirectoryName(path)!);
        dirInfo.Delete();
    }
}

public class FolderNameModel
{
    public string Name { get; set; }
    public override string ToString() => Name;
}