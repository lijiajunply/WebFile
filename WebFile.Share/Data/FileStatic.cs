using System.Security.Cryptography;
using System.Text;

namespace WebFile.Share.Data;

public static class FileStatic
{
    public static string HashEncryption(this string str)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(str)));

    public static Stream GetStream(this IFile file, string root)
        => new FileStream($@"{root}\UserFiles\{file.Path}", FileMode.Open);

    public static string ToWebUrl(this IFile file)
        => file.IsFolder ? $"/FolderView/{file.Id}" : $"/FileView/{file.Id}";

    public static long FileSize(this FileSystemInfo info)
    {
        if (!info.Exists) return 0;
        return info switch
        {
            FileInfo f => f.Length,
            DirectoryInfo d => d.GetFileSystemInfos().Sum(FileSize),
            _ => 0
        };
    }

    public static string FileSizeString(this long l)
    {
        var unit = new[] { "B", "KB", "MB", "GB", "TB" };
        var i = 0;
        while (true)
        {
            if (l / 1024 < 1024)
                break;
            l /= 1024;
            i++;
        }

        return $"{l}{unit[i]}";
    }
}

public static class FileStringStatic
{
    public static string GetUrl(this string path) => $"wwwroot/UserFiles/{path}";
    public static string GetUrl(this IFile file) => GetUrl(file.Path);

    public static string GetUrlWithoutWWW(this IFile file) => $"/UserFiles/{file.Path}";

    public static string GetMIME(this IFile file)
    {
        var ext = Path.GetExtension(file.Path).Replace(".", "");
        switch (ext)
        {
            case "txt":
                return "text/plain";
            case "html":
            case "css":
            case "js":
            case "csv":
            case "xml":
                return $"text/{ext}";
            case ".jpeg":
            case ".jpg":
                return "image/jpeg";
            case "png":
            case "gif":
            case "bmp":
            case "webp":
                return $"image/{ext}";
            case "svg":
                return "image/svg+xml";
            case "mp3":
                return "audio/mpeg";
            case "wav":
                return "audio/wav";
            case "mp4":
            case "ogg":
            case "webm":
                return $"video/{ext}";
            case "mov":
                return "video/quicktime";
            case "json":
            case "pdf":
            case "zip":
            case "gzip":
                return $"application/{ext}";
            case "bin":
            case "exe":
            case "dll":
            case "class":
                return "application/octet-stream";
            case "xls":
                return "application/vnd.ms-excel";
            case "xlsx":
                return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            case "doc":
                return "application/msword";
            case "docx":
                return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            case "ppt":
                return "application/vnd.ms-powerpoint";
            case "pptx":
                return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
            default:
                return ext;
        }
    }
}

public static class FileBoolStatic
{
    public static bool IsCode(this IFile file)
    {
        var ext = Path.GetExtension(file.Path).Replace(".", "");
        return Extensions.Any(x => x == ext);
    }

    public static IEnumerable<string> Extensions
        => new[]
        {
            "bat", "sh", "c", "cpp", "hpp", "h", "hxx", "go", "rs", "rust", "mm", "swift", "cs", "fs", "vb", "vba",
            "java", "jsp", "kt", "dart", "groovy", "lua", "js", "ts", "scss", "css", "vue", "html", "py", "py2", "py3",
            "jl", "m", "R", "php", "sql", "xml", "yaml", "json", "xaml", "axaml", "svg", "razor", "cshtml", "txt"
        };

    public static bool IsImage(this string s)
        => s is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" or ".svg";

    public static bool IsImage(this IFile file)
        => Path.GetExtension(file.Path).IsImage();

    public static bool IsVideo(this string s)
        => s is ".mp3" or ".wav" or ".mp4" or ".ogg" or ".webm" or ".mov";

    public static bool IsVideo(this IFile file)
        => Path.GetExtension(file.Path).IsVideo();

    public static string ToFileIcon(this IFile item, string size = "6x")
    {
        if (item.IsFolder)
            return $"fa-regular fa-folder fa-{size}";

        if (item.IsCode())
            return $"fa-regular fa-file-code fa-{size}";

        if (item.IsImage())
            return $"fa-regular fa-file-image fa-{size}";

        if (item.IsVideo())
            return $"fa-regular fa-file-video fa-{size}";

        return $"fa-regular fa-file fa-{size}";
    }
}