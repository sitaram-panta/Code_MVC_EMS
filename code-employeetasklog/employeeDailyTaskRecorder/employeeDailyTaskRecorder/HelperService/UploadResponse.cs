using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;

public class UploadResponse
{
    public bool Status { get; set; }
    public string FileName { get; set; }
    public string FullPath { get; set; }
    public string VirtualFullPath { get; set; }
    public int ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
}

public static class UploadHelper
{
    public static string GetFolderStructByDate(DateTime d)
    {
        return $"/{d.Year}/{d.Month}/{d.Day}/";
    }

    public static string GetNewFileName()
    {
        Guid newGUID = Guid.NewGuid();
        return newGUID.ToString();
    }

    public static UploadResponse UploadFile(IFormFile MainPhoto, string Destination)
    {
        var allowedExtensions = new[] { ".jpg", ".gif", ".png" };
        return UploadFile(MainPhoto, Destination, allowedExtensions, "", 100, 100);
    }

    public static UploadResponse UploadFile(IFormFile MainPhoto, string Destination, string OldFileName)
    {
        var allowedExtensions = new[] { ".jpg", ".gif", ".png" };
        return UploadFile(MainPhoto, Destination, allowedExtensions, OldFileName, 100, 100);
    }

    public static UploadResponse UploadFile(IFormFile MainPhoto, string Destination, string[] allowedExtensions)
    {
        return UploadFile(MainPhoto, Destination, allowedExtensions, "", 100, 100);
    }

    public static UploadResponse UploadProfilePhoto(IFormFile MainPhoto, string Destination)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".gif", ".png" };
        return UploadFile(MainPhoto, Destination, allowedExtensions, "", 100, 100);
    }

    public static UploadResponse UploadFile(IFormFile MainPhoto, string Destination, string[] allowedExtensions, string OldFileName)
    {
        var imageExt = new[] { ".jpg", ".gif", ".png" };
        UploadResponse NewResponse = new UploadResponse() { Status = false, ErrorCode = 999, ErrorMessage = "Unknown" };

        string newName = "";
        allowedExtensions = allowedExtensions.Select(x => x.Trim()).ToArray();

        if (MainPhoto != null && MainPhoto.Length > 0)
        {
            var newExt = Path.GetExtension(MainPhoto.FileName).ToLower();
            newName = OldFileName != "" ? OldFileName : GetNewFileName() + newExt;

            NewResponse.VirtualFullPath = Destination + "/" + newName;
            if (allowedExtensions.Contains(newExt))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", NewResponse.VirtualFullPath);

                try
                {
                    if (!Directory.Exists(Path.GetDirectoryName(path)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    }
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    // You may need to replace the following line with the appropriate image resizing code for ASP.NET Core
                    MainPhoto.CopyTo(new FileStream(path, FileMode.Create));

                    NewResponse.Status = true;
                    NewResponse.FileName = newName;
                    NewResponse.FullPath = path;
                }
                catch (Exception ex)
                {
                    NewResponse.ErrorCode = 501;
                    NewResponse.ErrorMessage = ex.Message;
                }
            }
            else
            {
                NewResponse.ErrorCode = 102;
                NewResponse.ErrorMessage = "Invalid File Type";
            }
        }
        else
        {
            NewResponse.ErrorCode = 101;
            NewResponse.ErrorMessage = "No File uploaded. Please check file size.";
        }
        return NewResponse;
    }

    public static UploadResponse UploadFile(IFormFile MainPhoto, string Destination, string[] allowedExtensions, string OldFileName, int resizeImageHeight, int resizeImageWidth)
    {
        var imageExt = new[] { ".jpg", ".gif", ".png" };
        UploadResponse NewResponse = new UploadResponse() { Status = false, ErrorCode = 999, ErrorMessage = "Unknown" };

        string newName = "";
        allowedExtensions = allowedExtensions.Select(x => x.Trim()).ToArray();

        if (MainPhoto != null && MainPhoto.Length > 0)
        {
            var newExt = Path.GetExtension(MainPhoto.FileName).ToLower();
            newName = OldFileName != "" ? OldFileName : GetNewFileName() + newExt;
            if (allowedExtensions.Contains(newExt))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Destination, newName);

                try
                {
                    if (!Directory.Exists(Path.GetDirectoryName(path)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    }
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    // You may need to replace the following line with the appropriate image resizing code for ASP.NET Core
                    MainPhoto.CopyTo(new FileStream(path, FileMode.Create));

                    NewResponse.Status = true;
                    NewResponse.FullPath = path;
                    NewResponse.FileName = newName;
                }
                catch (Exception ex)
                {
                    NewResponse.ErrorCode = 501;
                    NewResponse.ErrorMessage = ex.Message;
                }
            }
            else
            {
                NewResponse.ErrorCode = 102;
                NewResponse.ErrorMessage = "Invalid File Type";
            }
        }
        else
        {
            NewResponse.ErrorCode = 101;
            NewResponse.ErrorMessage = "No File uploaded. Please check file size.";
        }
        return NewResponse;
    }

    public static bool CheckAndCreateDirectory(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        return true;
    }

    public static string SLAFilePath()
    {
        // You will need to replace the following code with the appropriate logic for generating the SLAFilePath in ASP.NET Core
        return "";
    }
}
