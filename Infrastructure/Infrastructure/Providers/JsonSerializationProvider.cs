using Newtonsoft.Json.Linq;
using ServiceStack.Text;

namespace AFT.RegoV2.Infrastructure.Providers
{
    public interface IJsonSerializationProvider
    {
        T DeserializeFromString<T>(string value);
        dynamic DeserializeAsDynamic(string value);
        string SerializeToString<T>(T obj);
    }
    public sealed class JsonSerializationProvider : IJsonSerializationProvider
    {
        T IJsonSerializationProvider.DeserializeFromString<T>(string value)
        {
            return JsonSerializer.DeserializeFromString<T>(value);
        }
        dynamic IJsonSerializationProvider.DeserializeAsDynamic(string value)
        {
            return JObject.Parse(value);
        }
        string IJsonSerializationProvider.SerializeToString<T>(T obj)
        {
            return JsonSerializer.SerializeToString(obj);
        }
    }
}