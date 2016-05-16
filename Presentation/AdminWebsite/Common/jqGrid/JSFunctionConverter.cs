using System;
using Newtonsoft.Json;

namespace AFT.RegoV2.AdminWebsite.Common.jqGrid
{
    public class JSFunction
    {
        public string Definition { get; set; }
    }

    public class JSFunctionConverter : JsonConverter
    {
        public override bool CanConvert(Type valueType)
        {
            return valueType == typeof(JSFunction);
        }

        ////public override object ReadJson(JsonReader reader, Type objectType, JsonSerializer serializer)
        ////{
        ////    throw new NotImplementedException();
        ////}

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue((value as JSFunction).Definition);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
