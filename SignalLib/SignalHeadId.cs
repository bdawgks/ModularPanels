using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModularPanels.SignalLib
{
    [JsonConverter(typeof(SignalHeadIdJsonConverter))]
    public readonly struct SignalHeadId
    {
        public readonly string id;
        public readonly string? head = null;
        public readonly BoundarySignal.BoundaryDir? boundaryDir = null;

        public SignalHeadId(string idStr, BoundarySignal.BoundaryDir? bDirOverride = null)
        {
            if (idStr.Contains(':'))
            {
                string[] split = idStr.Split(':');
                if (split.Length < 2 || split.Length > 3)
                    throw new Exception(string.Format("Signal ID could not be parsed, invalid delimiters [{0}]", idStr));

                id = split[0];
                head = split[1];
                if (split.Length > 2 && bDirOverride == null)
                {
                    if (Enum.TryParse(split[2], out BoundarySignal.BoundaryDir dir))
                    {
                        boundaryDir = dir;
                    }
                }
                else if (bDirOverride != null)
                    boundaryDir = bDirOverride;

                if (string.IsNullOrEmpty(head))
                    throw new Exception(string.Format("Signal ID could not be parsed, head name must not be empty [{0}]", idStr));
            }
            else
            {
                id = idStr;
            }
            if (string.IsNullOrEmpty(id))
                throw new Exception(string.Format("Signal ID could not be parsed, signal ID must not be empty [{0}]", idStr));
        }

        public SignalHeadId(string sigId, string? headId, BoundarySignal.BoundaryDir? bDir)
        {
            id = sigId;
            head = headId;
            boundaryDir = bDir;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(head))
                return id;

            return string.Format("{0}:{1}", id, head);
        }

        public static implicit operator SignalHeadId(string s) { return new SignalHeadId(s); }
        public static implicit operator string(SignalHeadId shid) { return shid.ToString(); }
    }

    [JsonConverter(typeof(SignalHeadIdOutOnlyJsonConverter))]
    public readonly struct SignalHeadIdOutOnly(string id)
    {
        readonly SignalHeadId _id = new(id, BoundarySignal.BoundaryDir.Out);

        public static implicit operator SignalHeadId(SignalHeadIdOutOnly shid) { return shid._id; }
        public static implicit operator SignalHeadIdOutOnly(string s) { return new SignalHeadIdOutOnly(s); }
        public static implicit operator string(SignalHeadIdOutOnly shid) { return shid._id.ToString(); }
    }

    [JsonConverter(typeof(SignalHeadIdInOnlyJsonConverter))]
    public readonly struct SignalHeadIdInOnly(string id)
    {
        readonly SignalHeadId _id = new(id, BoundarySignal.BoundaryDir.In);

        public static implicit operator SignalHeadId(SignalHeadIdInOnly shid) { return shid._id; }
        public static implicit operator SignalHeadIdInOnly(string s) { return new SignalHeadIdInOnly(s); }
        public static implicit operator string(SignalHeadIdInOnly shid) { return shid._id.ToString(); }
    }

    public class SignalHeadIdJsonConverter : JsonConverter<SignalHeadId>
    {
        public override SignalHeadId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? idStr = reader.GetString();
            return idStr == null ? throw new Exception(string.Format("Invalid signal ID: Line {0}", reader.Position)) : new(idStr);
        }

        public override void Write(Utf8JsonWriter writer, SignalHeadId value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class SignalHeadIdOutOnlyJsonConverter : JsonConverter<SignalHeadIdOutOnly>
    {
        public override SignalHeadIdOutOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? idStr = reader.GetString();
            return idStr == null ? throw new Exception(string.Format("Invalid signal ID: Line {0}", reader.Position)) : new(idStr);
        }

        public override void Write(Utf8JsonWriter writer, SignalHeadIdOutOnly value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class SignalHeadIdInOnlyJsonConverter : JsonConverter<SignalHeadIdInOnly>
    {
        public override SignalHeadIdInOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? idStr = reader.GetString();
            return idStr == null ? throw new Exception(string.Format("Invalid signal ID: Line {0}", reader.Position)) : new(idStr);
        }

        public override void Write(Utf8JsonWriter writer, SignalHeadIdInOnly value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
