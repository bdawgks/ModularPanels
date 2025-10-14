using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModularPanels.JsonLib
{
    public class TypedId<IType, OType>(IType id) where OType : class
    {
        readonly IType _id = id;
        OType? _object;

        public IType Id { get { return _id; } }

        public OType? Get()
        {
            return _object;
        }
        public bool IsNull { get { return _object == null; } }

        internal void SetObject(OType? obj)
        {
            _object = obj;
        }
    }

    [JsonConverter(typeof(StringIdJsonConverter))]
    public class StringId<OType>(string id) : TypedId<string, OType>(id) where OType : class
    {
    }

    public class StringIdJsonConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
                return false;

            if (typeToConvert.GetGenericTypeDefinition() != typeof(StringId<>))
                return false;

            return typeToConvert.GetGenericArguments()[0].IsClass;
        }
        public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
        {
            Type[] typeArguments = type.GetGenericArguments();
            Type objectType = typeArguments[0];

            JsonConverter converter = (JsonConverter)Activator.CreateInstance(typeof(StringIdConverterInner<>).MakeGenericType([objectType]))!;

            //JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            //    typeof(StringIdConverterInner<>).MakeGenericType(
            //        [objectType]),
            //    BindingFlags.Instance | BindingFlags.Public,
            //    binder: null,
            //    args: [options],
            //    culture: null)!;

            return converter;
        }

        private class StringIdConverterInner<TObj> : JsonConverter<StringId<TObj>> where TObj : class
        {
            public override StringId<TObj>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    string? idStr = reader.GetString();
                    if (idStr == null)
                        return null;

                    return new(idStr);
                }
                return null;
            }

            public override void Write(Utf8JsonWriter writer, StringId<TObj> value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }
    }
}
