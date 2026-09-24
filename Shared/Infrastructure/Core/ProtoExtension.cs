using System.Linq;
using Google.Protobuf.Collections;
using SharedKernel.ValueObjects;

namespace Shared.Extensions
{
    public static class ProtoExtension
    {
        public static string ToString(this RepeatedField<string> repeatedField, string separator = ", ")
        {
            return string.Join(separator, repeatedField.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        public static string ToString(this MapField<int, string> mapField, string separator = ", ")
        {
            return string.Join(separator, mapField.Select(x => $"{((Language)x.Key).ToString()}: {x.Value}"));
        }
    }
}
