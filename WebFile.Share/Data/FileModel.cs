﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebFile.Share.Data;

public class FileModel : IFile
{
    public string Path { get; set; } = "";
    public string Url { get; set; } = "";
    public bool IsFolder { get; set; }

    public UserModel Owner { get; init; } = new();

    [Key]
    [Column(TypeName = "varchar(256)")]
    public string Id { get; set; } = "";

    public FolderModel ToFolder() => new() { IsFolder = IsFolder, Path = Path, Url = Url, Id = Id };
}

public class FolderModel : IFile
{
    public string Path { get; set; } = "";
    public string Url { get; set; } = "";
    public string Id { get; set; } = "";

    public bool IsFolder { get; set; }
}

public class FileInfoModel
{
    public string Size { get; set; }

    public FileInfoModel(IFile model)
    {
        var info = new FileInfo(model.GetUrl());
        Size = info.FileSize().FileSizeString();
    }
}

public interface IFile
{
    public string Path { get; set; }
    public string Url { get; set; }
    public string Id { get; set; }
    public bool IsFolder { get; set; }
}