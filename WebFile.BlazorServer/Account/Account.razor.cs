using System.Diagnostics.CodeAnalysis;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using WebFile.BlazorServer.Providers;
using WebFile.Share.Data;

namespace WebFile.BlazorServer.Account;

public sealed partial class Account
{
    [Inject] [NotNull] public IDbContextFactory<WebFileContext>? DbFactory { get; set; }
    [Inject] [NotNull] public ToastService? ToastService { get; set; }
    [Inject] [NotNull] public MessageService? MessageService { get; set; }
    [Inject] [NotNull] public IWebHostEnvironment? WebHostEnvironment { get; set; }
    [Inject] [NotNull] public NavigationManager? NavigationManager { get; set; }
    [Inject] [NotNull] public DownloadService? DownloadService { get; set; }
    [Inject] [NotNull] public DialogService? DialogService { get; set; }
    [Inject] [NotNull] public AuthenticationStateProvider? AuthStateProvider { get; set; }

    private bool IsAuthenticated { get; set; }
    private UserModel User { get; set; } = new();
    private string? Size { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var claims = (await AuthStateProvider.GetAuthenticationStateAsync()).User;
        var user = claims.ToUser();

        if (user == null) return;
        await using var context = await DbFactory.CreateDbContextAsync();
        var userModel = await context.Users.Include(x => x.Files)
            .FirstOrDefaultAsync(x => x.Equals(user));
        if (userModel == null) return;

        IsAuthenticated = true;
        User = user;
        var info = new DirectoryInfo($@"{WebHostEnvironment.WebRootPath}/UserFiles/{User.UserName}");
        Size = info.FileSize().ToFileSizeString();
    }

    private async Task ChangeSubmit(EditContext arg)
    {
    }
}