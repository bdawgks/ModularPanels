using ModularPanels.DrawLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModularPanels.SignalLib
{
    [JsonConverter(typeof(SignalHeadIdJsonConverter))]
    public readonly struct SignalHeadId
    {
        public readonly string id;
        public readonly string? head = null;

        public SignalHeadId(string idStr)
        {
            if (idStr.Contains(':'))
            {
                string[] split = idStr.Split(':');
                if (split.Length != 2)
                    throw new Exception(string.Format("Signal ID could not be parsed, invalid delimiters [{0}]", idStr));

                id = split[0];
                head = split[1];

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

        public override string ToString()
        {
            if (string.IsNullOrEmpty(head))
                return id;

            return string.Format("{0}:{1}", id, head);
        }

        public static implicit operator SignalHeadId(string s) { return new SignalHeadId(s); }
        public static implicit operator string(SignalHeadId shid) { return shid.ToString(); }
    }

    public class SignalHeadIdJsonConverter : JsonConverter<SignalHeadId>
    {
        public override SignalHeadId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? idStr = reader.GetString();
            if (idStr == null)
            {
                throw new Exception(string.Format("Invalid signal ID: Line {0}", reader.Position));
            }

            return new(idStr);
        }

        public override void Write(Utf8JsonWriter writer, SignalHeadId value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
