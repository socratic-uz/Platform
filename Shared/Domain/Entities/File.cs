using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class File
    {
        [Display(Name = "URL")]
        public string? Url { get; set; }

        [Display(Name = "Содержимое")]
        public byte[]? Content { get; set; }

        [Display(Name = "Имя файла")]
        public string? FileName { get; set; }

        [Display(Name = "Тип контента")]
        public string? ContentType { get; set; }

        public static implicit operator SharedKernel.Protos.File(File f)
        {
            if (f == null) return null!;
            var proto = new SharedKernel.Protos.File
            {
                Url = f.Url ?? "",
                FileName = f.FileName ?? "",
                ContentType = f.ContentType ?? ""
            };
            if (f.Content != null)
            {
                proto.Content = Google.Protobuf.ByteString.CopyFrom(f.Content);
            }
            return proto;
        }

        public static implicit operator File(SharedKernel.Protos.File f)
        {
            if (f == null) return null!;
            return new File
            {
                Url = f.Url,
                Content = f.Content?.ToArray(),
                FileName = f.FileName,
                ContentType = f.ContentType
            };
        }

        public override string ToString() => FileName ?? Url ?? "";
    }
}
