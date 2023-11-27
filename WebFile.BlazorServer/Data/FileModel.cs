﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebFile.BlazorServer.Data;

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
    
}