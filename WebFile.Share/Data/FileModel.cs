using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace WebFile.Share.Data;

public class FileModel : IFile
{
    public string Path { get; set; }
    public string Url { get; set; } = "";
    public bool IsFolder { get; set; }

    public UserModel Owner { get; set; }

    [Key]
    [Column(TypeName = "varchar(256)")]
    public string Id { get; set; }

    public FolderModel ToFolder() => new() { IsFolder = IsFolder, Path = Path, Url = Url, Id = Id };
}

public class FolderModel : IFile
{
    public string Path { get; set; }
    public string Url { get; set; }
    public string Id { get; set; }

    public Stream GetStream(string root)
        => new FileStream($@"{root}\UserFiles\{Path}", FileMode.Open);

    public bool IsFolder { get; set; }

    public string ToUrl()
        => IsFolder ? $"/FolderView/{Id}" : $"/FileView/{Id}";
}

public class FileInfoModel
{
    public string Size { get; set; }

    public FileInfoModel(IFile model)
    {
        var info = new FileInfo(model.GetUrl());
        var unit = new[] { "B", "kB", "MB", "GB", "TB" };
        var i = 0;
        var l = info.Length;
        while (true)
        {
            if (l / 1024 < 1024)
                break;
            l /= 1024;
            i++;
        }

        Size = $"{l}{unit[i]}";
    }
}

public interface IFile
{
    public string Path { get; set; }
    public string Url { get; set; }
    public string Id { get; set; }
    public bool IsFolder { get; set; }
}

public static class FileStatic
{
    public static string HashEncryption(this string str)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(str)));

    public static bool IsCode(this IFile file)
    {
        var ext = Path.GetExtension(file.Path).Replace(".", "");
        return Extensions.Any(x => x == ext);
    }

    private static IEnumerable<string> Extensions
        => new[]
        {
            "bat", "sh", "c", "cpp", "hpp", "h", "hxx", "go", "rs", "rust", "mm", "swift", "cs", "fs", "vb", "vba",
            "java", "jsp", "kt", "dart", "groovy", "lua", "js", "ts", "scss", "css", "vue", "html", "py", "py2", "py3",
            "jl", "m", "R", "php", "sql", "xml", "yaml", "json", "xaml", "axaml", "svg", "razor", "cshtml"
        };

    public static bool IsImage(this string s)
        => s is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" or ".svg";

    public static bool IsImage(this IFile file)
        => Path.GetExtension(file.Path).IsImage();

    public static bool IsVideo(this string s)
        => s is ".mp3" or ".wav" or ".mp4" or ".ogg" or ".webm" or ".mov";

    public static bool IsVideo(this IFile file)
        => Path.GetExtension(file.Path).IsVideo();

    public static string ToFileIcon(this IFile item,string size = "6x")
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