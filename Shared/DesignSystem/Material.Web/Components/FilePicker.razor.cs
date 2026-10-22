using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Assets.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using Microsoft.Extensions.Localization;

namespace Material.Web.Components;

public partial class FilePicker : ComponentBase
{
    [Inject] public IStringLocalizer L { get; set; } = default!;

    protected string inputId = $"file-picker-{Guid.NewGuid():N}";
    protected bool isDragging = false;
    protected Type? fileType;

    // Universal Mode Parameters
    [Parameter] public IList? Attachments { get; set; }
    
    // Legacy Mode Parameters
    [Parameter] public IList? ExistingImages { get; set; }
    [Parameter] public IList? NewImagesContent { get; set; }
    [Parameter] public IList? NewImagesFileName { get; set; }
    [Parameter] public IList? NewImagesContentType { get; set; }

    [Parameter] public Func<string, string>? ImageUrlResolver { get; set; }

    [Parameter] public string? Label { get; set; }
    [Parameter] public string? UploadLabel { get; set; }
    [Parameter] public string? RemoveLabel { get; set; }
    [Parameter] public bool Multiple { get; set; } = true;
    [Parameter] public string Accept { get; set; } = "*";
    [Parameter] public int MaxAllowedFiles { get; set; } = 10;
    [Parameter] public long MaxFileSize { get; set; } = 15 * 1024 * 1024; // 15MB

    [Parameter] public EventCallback OnChanged { get; set; }
    [Parameter] public EventCallback OnReset { get; set; }

    protected int TotalFilesCount => (Attachments?.Count ?? 0) + (ExistingImages?.Count ?? 0) + (NewImagesContent?.Count ?? 0);

    protected string EffectiveLabel => Label ?? (L != null ? L[Loc.Common.FilePickerAttachedFiles] : "Attached files");
    protected string EffectiveUploadLabel => UploadLabel ?? (L != null ? L[Loc.Common.UploadImage] : "Upload");
    protected string EffectiveRemoveLabel => RemoveLabel ?? (L != null ? L[Loc.Actions.DeleteAction] : "Delete");

    protected string? GetFilesBadgeText()
    {
        if (TotalFilesCount <= 0) return null;
        var unit = TotalFilesCount == 1 ? L[Loc.Common.FilePickerCountOne] : (TotalFilesCount < 5 ? L[Loc.Common.FilePickerCountFew] : L[Loc.Common.FilePickerCountMany]);
        return $"{TotalFilesCount} {unit}";
    }

    protected string GetPreviewText()
    {
        return TotalFilesCount > 0
            ? string.Format(L[Loc.Common.FilePickerLoadedFiles], TotalFilesCount)
            : L[Loc.Common.FilePickerNoFiles];
    }

    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> PropertyCache = new();

    protected override void OnInitialized()
    {
        if (Attachments != null)
        {
            var listType = Attachments.GetType();
            fileType = listType.IsGenericType 
                ? listType.GetGenericArguments()[0] 
                : (listType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))?.GetGenericArguments()[0] ?? typeof(object));
        }
    }

    protected void HandleDragEnter() => isDragging = true;
    protected void HandleDragLeave() => isDragging = false;
    protected void HandleDragOver() { }
    protected void HandleDrop() => isDragging = false;

    protected T? GetProperty<T>(object obj, string propName)
    {
        if (obj == null) return default;
        try
        {
            var type = obj.GetType();
            var prop = PropertyCache.GetOrAdd((type, propName), key => key.Item1.GetProperty(key.Item2));
            var val = prop?.GetValue(obj);
            if (val is T typedVal) return typedVal;
        }
        catch {}
        return default;
    }

    protected bool IsImage(string contentType, string fileName)
    {
        if (!string.IsNullOrEmpty(contentType) && contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return true;

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".webp" || ext == ".gif" || ext == ".bmp";
    }

    protected object? GetPropertyObject(object obj, string propName)
    {
        if (obj == null) return null;
        try
        {
            var type = obj.GetType();
            var prop = PropertyCache.GetOrAdd((type, propName), key => key.Item1.GetProperty(key.Item2));
            return prop?.GetValue(obj);
        }
        catch {}
        return null;
    }

    protected string GetBase64DataUrl(byte[] bytes, string contentType)
    {
        var base64 = Convert.ToBase64String(bytes);
        var mime = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
        return $"data:{mime};base64,{base64}";
    }

    protected string GetPreviewUrl(string url)
    {
        if (ImageUrlResolver != null)
        {
            return ImageUrlResolver(url);
        }
        return url;
    }

    protected string GetFileIcon(string contentType, string fileName)
    {
        var mime = contentType.ToLowerInvariant();
        var ext = Path.GetExtension(fileName).ToLowerInvariant();

        if (mime == "application/pdf" || ext == ".pdf") return Icons.PictureAsPdf;
        
        if (mime.Contains("zip") || mime.Contains("compressed") || mime.Contains("archive") || 
            ext == ".zip" || ext == ".rar" || ext == ".7z" || ext == ".tar" || ext == ".gz")
            return Icons.Archive;

        if (mime.Contains("word") || mime.Contains("document") || ext == ".doc" || ext == ".docx" || ext == ".rtf" || ext == ".odt")
            return Icons.Description;

        if (mime.Contains("excel") || mime.Contains("sheet") || ext == ".xls" || ext == ".xlsx" || ext == ".ods" || ext == ".csv")
            return Icons.TableView;

        if (mime.Contains("presentation") || mime.Contains("powerpoint") || ext == ".ppt" || ext == ".pptx")
            return Icons.CoPresent;

        if (mime.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) || ext == ".mp3" || ext == ".wav" || ext == ".ogg" || ext == ".m4a")
            return Icons.Audiotrack;

        if (mime.StartsWith("video/", StringComparison.OrdinalIgnoreCase) || ext == ".mp4" || ext == ".avi" || ext == ".mkv" || ext == ".mov")
            return Icons.VideoFile;

        if (mime.StartsWith("text/", StringComparison.OrdinalIgnoreCase) || ext == ".txt" || ext == ".html" || ext == ".css" || ext == ".js" || ext == ".json")
            return Icons.Code;

        return Icons.InsertDriveFile;
    }

    protected string GetIconClass(string contentType, string fileName)
    {
        var mime = contentType.ToLowerInvariant();
        var ext = Path.GetExtension(fileName).ToLowerInvariant();

        if (mime == "application/pdf" || ext == ".pdf") return Classes.Picker.PdfType;
        if (mime.Contains("zip") || mime.Contains("compressed") || ext == ".zip" || ext == ".rar" || ext == ".7z") return Classes.Picker.ArchiveType;
        if (mime.Contains("word") || ext == ".doc" || ext == ".docx") return Classes.Picker.DocType;
        if (mime.Contains("excel") || ext == ".xls" || ext == ".xlsx" || ext == ".csv") return Classes.Picker.SheetType;
        if (mime.StartsWith("audio/", StringComparison.OrdinalIgnoreCase)) return Classes.Picker.AudioType;
        if (mime.StartsWith("video/", StringComparison.OrdinalIgnoreCase)) return Classes.Picker.VideoType;

        return Classes.Picker.GenericType;
    }

    protected (string fileName, string contentType, string url, bool isNew, byte[]? bytesData) GetAttachmentData(int index)
    {
        if (Attachments == null || index < 0 || index >= Attachments.Count)
            return ("", "", "", false, null);

        var attachment = Attachments[index];
        if (attachment == null)
            return ("", "", "", false, null);

        var fileName = GetProperty<string>(attachment, "FileName") ?? "";
        var contentType = GetProperty<string>(attachment, "ContentType") ?? "";
        var url = GetProperty<string>(attachment, "Url") ?? "";
        
        var contentObj = GetPropertyObject(attachment, "Content");
        var isNew = false;
        byte[]? bytesData = null;
        if (contentObj is Google.Protobuf.ByteString bs)
        {
            isNew = !bs.IsEmpty;
            bytesData = bs.ToByteArray();
        }
        else if (contentObj is byte[] arr)
        {
            isNew = arr.Length > 0;
            bytesData = arr;
        }

        return (fileName, contentType, url, isNew, bytesData);
    }

    protected (string previewUrl, string fileName) GetExistingImageData(int index)
    {
        if (ExistingImages == null || index < 0 || index >= ExistingImages.Count)
            return ("", "image");

        var item = ExistingImages[index];
        if (item is string url)
        {
            var previewUrl = GetPreviewUrl(url);
            var fileName = Path.GetFileName(url);
            if (string.IsNullOrEmpty(fileName)) fileName = "image";
            return (previewUrl, fileName);
        }
        return ("", "image");
    }

    protected (string dataUrl, string contentType, string fileName) GetNewImageData(int index)
    {
        if (NewImagesContent == null || index < 0 || index >= NewImagesContent.Count)
            return ("", "image/png", "new-file");

        var contentObj = NewImagesContent[index];
        byte[] bytes = Array.Empty<byte>();

        if (contentObj is byte[] byteArray)
        {
            bytes = byteArray;
        }
        else if (contentObj is Google.Protobuf.ByteString byteString)
        {
            bytes = byteString.ToByteArray();
        }
        else if (contentObj != null)
        {
            var method = contentObj.GetType().GetMethod("ToByteArray");
            if (method != null)
            {
                bytes = (byte[])method.Invoke(contentObj, null)!;
            }
        }

        var base64Data = Convert.ToBase64String(bytes);
        var contentType = "image/png";
        var fileName = "new-file";
        if (NewImagesContentType != null && index < NewImagesContentType.Count && NewImagesContentType[index] is string ct)
        {
            contentType = ct;
        }
        if (NewImagesFileName != null && index < NewImagesFileName.Count && NewImagesFileName[index] is string fn)
        {
            fileName = fn;
        }
        var dataUrl = $"data:{contentType};base64,{base64Data}";
        return (dataUrl, contentType, fileName);
    }

    protected async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        var files = Multiple ? e.GetMultipleFiles(MaxAllowedFiles) : new[] { e.File };

        foreach (var file in files)
        {
            try
            {
                await using var stream = file.OpenReadStream(MaxFileSize);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var bytes = ms.ToArray();

                if (Attachments != null && fileType != null)
                {
                    var newAttachment = Activator.CreateInstance(fileType);
                    if (newAttachment != null)
                    {
                        fileType.GetProperty("Url")?.SetValue(newAttachment, "");
                        
                        var contentProp = fileType.GetProperty("Content");
                        if (contentProp != null)
                        {
                            if (contentProp.PropertyType == typeof(Google.Protobuf.ByteString))
                            {
                                contentProp.SetValue(newAttachment, Google.Protobuf.ByteString.CopyFrom(bytes));
                            }
                            else
                            {
                                contentProp.SetValue(newAttachment, bytes);
                            }
                        }
                        
                        fileType.GetProperty("FileName")?.SetValue(newAttachment, file.Name);
                        fileType.GetProperty("ContentType")?.SetValue(newAttachment, file.ContentType ?? "application/octet-stream");

                        Attachments.Add(newAttachment);
                    }
                }
                
                if (NewImagesContent != null)
                {
                    var listType = NewImagesContent.GetType();
                    var itemType = listType.IsGenericType 
                        ? listType.GetGenericArguments()[0] 
                        : (listType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))?.GetGenericArguments()[0] ?? typeof(object));

                    if (itemType == typeof(Google.Protobuf.ByteString))
                    {
                        NewImagesContent.Add(Google.Protobuf.ByteString.CopyFrom(bytes));
                    }
                    else if (itemType == typeof(byte[]))
                    {
                        NewImagesContent.Add(bytes);
                    }
                    else
                    {
                        NewImagesContent.Add(bytes);
                    }
                }

                if (NewImagesFileName != null)
                {
                    NewImagesFileName.Add(file.Name);
                }

                if (NewImagesContentType != null)
                {
                    NewImagesContentType.Add(string.IsNullOrWhiteSpace(file.ContentType)
                        ? "application/octet-stream"
                        : file.ContentType);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file {file.Name}: {ex.Message}");
            }
        }

        await OnChanged.InvokeAsync();
        StateHasChanged();
    }

    protected async Task RemoveAttachment(int index)
    {
        if (Attachments != null && index >= 0 && index < Attachments.Count)
        {
            Attachments.RemoveAt(index);
            await OnChanged.InvokeAsync();
            StateHasChanged();
        }
    }

    protected async Task RemoveExistingImage(int index)
    {
        if (ExistingImages != null && index >= 0 && index < ExistingImages.Count)
        {
            ExistingImages.RemoveAt(index);
            await OnChanged.InvokeAsync();
            StateHasChanged();
        }
    }

    protected async Task RemoveNewImage(int index)
    {
        if (NewImagesContent != null && index >= 0 && index < NewImagesContent.Count)
        {
            NewImagesContent.RemoveAt(index);
            
            if (NewImagesFileName != null && index < NewImagesFileName.Count)
            {
                NewImagesFileName.RemoveAt(index);
            }
            
            if (NewImagesContentType != null && index < NewImagesContentType.Count)
            {
                NewImagesContentType.RemoveAt(index);
            }

            await OnChanged.InvokeAsync();
            StateHasChanged();
        }
    }

    public async Task ClearAll()
    {
        Attachments?.Clear();
        ExistingImages?.Clear();
        NewImagesContent?.Clear();
        NewImagesFileName?.Clear();
        NewImagesContentType?.Clear();

        await OnChanged.InvokeAsync();
        await OnReset.InvokeAsync();
        StateHasChanged();
    }
}
