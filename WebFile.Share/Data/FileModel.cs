using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebFile.Share.Data;

public class FileModel
{
    public string Path { get; set; }
    public string Url { get; set; } = "";
    public bool IsFolder { get; set; }

    public UserModel Owner { get; set; }

    [Key]
    [Column(TypeName = "varchar(256)")]
    public string Id { get; set; }

    public string GetUrl() => GetUrl(Path);
    public static string GetUrl(string path) => $"wwwroot/UserFiles/{path}";

    public FolderModel ToFolder() => new() { IsFolder = IsFolder, Path = Path, Url = Url, Id = Id };
}

public class FolderModel
{
    public string Path { get; set; }
    public string Url { get; set; }
    public string Id { get; set; }

    public Stream GetStream(string root)
        => new FileStream($@"{root}\UserFiles\{Path}", FileMode.Open);

    public bool IsFolder { get; set; }

    public string ToUrl()
        => IsFolder ? $"/FolderView/{Id}" : $"/FileView/{Id}";

    public string GetMIME()
    {
        var ext = System.IO.Path.GetExtension(Path).Replace(".", "");
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

public class FileInfoModel
{
    public string Size { get; set; }

    public FileInfoModel(FolderModel model)
    {
        var info = new FileInfo(FileModel.GetUrl(model.Path));
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

    public FileInfoModel(FileModel model)
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