using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Microsoft.AspNetCore.Components;

namespace Domain.Abstractions
{
    public static class ProtobufStateExtensions
    {
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access", Justification = "Protobuf state strings are preserved via BackendJsonContext")]
        public static void PersistProtobufList(this PersistentComponentState state, string key, IEnumerable<IMessage> list)
        {
            var formatter = JsonFormatter.Default;
            var jsonStrings = list.Select(item => formatter.Format(item)).ToList();
            var json = System.Text.Json.JsonSerializer.Serialize(jsonStrings, SharedKernel.Serialization.BackendJsonContext.Default.ListString);
            state.PersistAsJson(key, json);
        }

        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access", Justification = "Protobuf state strings are preserved via BackendJsonContext")]
        public static bool TryTakeProtobufList<T>(this PersistentComponentState state, string key, out List<T> list) where T : class, IMessage<T>, new()
        {
            list = new List<T>();
            if (state.TryTakeFromJson<string>(key, out var json) && !string.IsNullOrEmpty(json))
            {
                var jsonStrings = System.Text.Json.JsonSerializer.Deserialize(json, SharedKernel.Serialization.BackendJsonContext.Default.ListString);
                if (jsonStrings != null)
                {
                    var parser = JsonParser.Default;
                    list = jsonStrings.Select(str => parser.Parse<T>(str)).ToList();
                }
                return true;
            }
            return false;
        }

        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access", Justification = "Protobuf state strings are preserved via BackendJsonContext")]
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with RequiresDynamicCodeAttribute may break functionality when AOT compiling", Justification = "Protobuf reflection list instantiation uses preserved element types")]
        public static bool TryTakeProtobufListReflection(this PersistentComponentState state, string key, System.Type elementType, out IList list)
        {
            var listType = typeof(List<>).MakeGenericType(elementType);
            list = (IList)System.Activator.CreateInstance(listType)!;
            
            if (state.TryTakeFromJson<string>(key, out var json) && !string.IsNullOrEmpty(json))
            {
                var jsonStrings = System.Text.Json.JsonSerializer.Deserialize(json, SharedKernel.Serialization.BackendJsonContext.Default.ListString);
                if (jsonStrings != null)
                {
                    var parser = JsonParser.Default;
                    var descriptorProperty = elementType.GetProperty("Descriptor", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (descriptorProperty != null)
                    {
                        var descriptor = (MessageDescriptor)descriptorProperty.GetValue(null)!;
                        foreach (var jsonStr in jsonStrings)
                        {
                            var parsed = parser.Parse(jsonStr, descriptor);
                            list.Add(parsed);
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }
}
