using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModularPanels.JsonLib
{
    public class BankKey<KType, OType>(KType key) where OType : class
    {
        readonly KType _key = key;
        InternalKey<KType, OType>? _iKey;

        public KType Key { get { return _key; } }

        internal InternalKey<KType, OType>? InternalKey { get { return _iKey; } }


        public OType? Object
        {
            get => _iKey?.Get();
        }

        public bool IsNull { get { return _iKey == null || _iKey.IsNull; } }

        public bool TryGet([NotNullWhen(true)]out OType? obj)
        {
            obj = Object;
            return !IsNull;
        }

        internal void SetInternalKey(InternalKey<KType, OType>? iKey)
        {
            _iKey = iKey;
        }
    }

    internal class InternalKey<KType, OType>(KType key) where OType : class
    {
        readonly KType _key = key;
        OType? _object;

        internal KType Key { get { return _key; } }

        internal OType? Get()
        {
            return _object;
        }

        internal bool IsNull { get { return _object == null; } }

        internal void SetObject(OType? obj)
        {
            _object = obj;
        }
    }

    [JsonConverter(typeof(StringIdJsonConverter))]
    public class StringKey<OType>(string key) : BankKey<string, OType>(key) where OType : class
    {
    }

    public class StringIdJsonConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
                return false;

            if (typeToConvert.GetGenericTypeDefinition() != typeof(StringKey<>))
                return false;

            return typeToConvert.GetGenericArguments()[0].IsClass;
        }
        public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
        {
            Type[] typeArguments = type.GetGenericArguments();
            Type objectType = typeArguments[0];

            JsonConverter converter = (JsonConverter)Activator.CreateInstance(typeof(StringIdConverterInner<>).MakeGenericType([objectType]))!;

            return converter;
        }

        private class StringIdConverterInner<TObj> : JsonConverter<StringKey<TObj>> where TObj : class
        {
            public override StringKey<TObj>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

            public override void Write(Utf8JsonWriter writer, StringKey<TObj> value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }
    }
}
