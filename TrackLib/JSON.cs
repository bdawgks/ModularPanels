using ModularPanels.DrawLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModularPanels.TrackLib
{
    internal struct JsonDataTrackNode
    {
        public string ID {  get; set; }
        public GridPos Pos {  get; set; }
        public bool? Square {  get; set; }
    }

    public class TrackNodeJsonConverter : JsonConverter<TrackNode>
    {
        public override TrackNode? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            JsonDataTrackNode data = JsonSerializer.Deserialize<JsonDataTrackNode>(ref reader, options);
            return new(data.ID, data.Pos)
            {
                squareEnd = data.Square != null && data.Square.Value
            };
        }

        public override void Write(Utf8JsonWriter writer, TrackNode value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
