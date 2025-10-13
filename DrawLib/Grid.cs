using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModularPanels.DrawLib
{
    /// <summary>
    /// Grid singleton.
    /// 
    /// The grid defines pixel increments for whole number grid positions. Grid positions are still relative to module position.
    /// </summary>
    public class Grid
    {
        readonly int _gridSize = 5;

        static Grid? _instance;

        public static Grid Instance
        {
            get
            {
                _instance ??= new Grid();
                return _instance;
            }
        }

        public DrawingPos TransformPos(GridPos pos)
        {
            return new()
            {
                x = pos.x * _gridSize,
                y = pos.y * _gridSize
            };
        }
    }
    
    /// <summary>
    /// Represents a transformed grid position with pixel precision, relative to drawing origin.
    /// </summary>
    public struct DrawingPos
    {
        public int x, y;

        public readonly Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }

        public static DrawingPos FromVector2(Vector2 v)
        {
            return new() 
            {
                x = (int)MathF.Round(v.X),
                y = (int)MathF.Round(v.Y),
            };
        }
    }

    /// <summary>
    /// Represents a grid position in whole number coordinates, relative to drawing origin.
    /// </summary>
    [JsonConverter(typeof(GridPosJsonConverter))]
    public struct GridPos
    {
        public int x, y;
    }

    public class GridPosJsonConverter : JsonConverter<GridPos>
    {
        public override GridPos Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            GridPos pos = new();
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                if (reader.TokenType != JsonTokenType.Number)
                    throw new Exception("Grid position invalid: " + reader.Position.ToString());
                pos.x = reader.GetInt32();

                reader.Read();
                if (reader.TokenType != JsonTokenType.Number)
                    throw new Exception("Grid position invalid: " + reader.Position.ToString());
                pos.y = reader.GetInt32();

                reader.Read();
                if (reader.TokenType != JsonTokenType.EndArray)
                    throw new Exception("Grid position invalid: " + reader.Position.ToString());
            }
            return pos;
        }

        public override void Write(Utf8JsonWriter writer, GridPos value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
