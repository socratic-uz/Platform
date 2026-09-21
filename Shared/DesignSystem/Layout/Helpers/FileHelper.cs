using System.Linq;

using Google.Protobuf.Collections;

namespace Shared.Helpers
{
    public static class FileHelper
    {
        public static string MinIOUrl { get; set; }

        public static string ToProfileImage(this string image)
        {
            return BuildImageUrl(image, "profile");
        }
        public static string ToProfileImage(this Domain.Entities.File? file)
        {
            return BuildImageUrl(file?.Url, "profile");
        }
        public static string ToProductImage(this string image)
        {
            return BuildImageUrl(image, "product");
        }
        public static string ToProductImage(this Domain.Entities.File? file)
        {
            return BuildImageUrl(file?.Url, "product");
        }
        public static string ToOrganizationImage(this string image)
        {
            return BuildImageUrl(image, "org");
        }
        public static string ToOrganizationImage(this Domain.Entities.File? file)
        {
            return BuildImageUrl(file?.Url, "org");
        }
        public static string[] ToProfileImageArray(this IEnumerable<Domain.Entities.File> images)
        {
            return images.Select(x => ToProfileImage(x)).ToArray();
        }
        public static string[] ToProductImageArray(this IEnumerable<Domain.Entities.File> images)
        {
            return images.Select(x => ToProductImage(x)).ToArray();
        }
        public static string[] ToOrganizationImageArray(this IEnumerable<Domain.Entities.File> images)
        {
            return images.Select(x => ToOrganizationImage(x)).ToArray();
        }

        private static string BuildImageUrl(string? image, string category)
        {
            if (string.IsNullOrWhiteSpace(image))
            {
                return $"{MinIOUrl}/img/{category}/default.png";
            }

            if (Uri.TryCreate(image, UriKind.Absolute, out _))
            {
                return image;
            }

            var normalized = image.TrimStart('/');

            if (normalized.StartsWith("img/", StringComparison.OrdinalIgnoreCase))
            {
                return $"{MinIOUrl}/{normalized}";
            }

            if (normalized.Contains('/'))
            {
                return $"{MinIOUrl}/img/{normalized}";
            }

            return $"{MinIOUrl}/img/{category}/{normalized}";
        }
    }
}
