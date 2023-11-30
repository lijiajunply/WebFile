using System.Diagnostics.CodeAnalysis;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using WebFile.BlazorServer.Providers;
using WebFile.Share.Data;

namespace WebFile.BlazorServer.FileSysViewer;

public sealed partial class AllFile
{
    [Inject] [NotNull] public IDbContextFactory<WebFileContext>? DbFactory { get; set; }
    [Inject] [NotNull] public ToastService? ToastService { get; set; }
    [Inject] [NotNull] public IWebHostEnvironment? WebHostEnvironment { get; set; }
    [Inject] [NotNull] public NavigationManager? NavigationManager { get; set; }
    [Inject] [NotNull] public DownloadService? DownloadService { get; set; }
    [Inject] [NotNull] public DialogService? DialogService { get; set; }
    [Inject] [NotNull] public AuthenticationStateProvider? AuthStateProvider { get; set; }

    private UserModel User { get; set; } = new();
    private bool IsAuthenticated { get; set; }
    private List<FileModel> FolderModels { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var claims = (await AuthStateProvider.GetAuthenticationStateAsync()).User;
        var user = claims.ToUser();
        if (user == null) return;

        await using var context = await DbFactory.CreateDbContextAsync();
        var userModel = await context.Users.Include(x => x.Files).FirstOrDefaultAsync(x => x.Equals(user));
        if(userModel == null)return;

        FolderModels = userModel.Files.Where(x => !x.IsFolder).ToList();
        User = user;
        IsAuthenticated = true;
    }

    private async Task OnDelete(IFile model)
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
        FolderModels = userModel.Files.Where(x => !x.IsFolder).ToList();
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
    
    private Task OnOpen(IFile model)
    {
        NavigationManager.NavigateTo(model.ToWebUrl(),true);
        return Task.CompletedTask;
    }
}