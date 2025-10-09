using System.Drawing;
using System.Text.Json;

namespace PanelLib
{
    namespace JSONData
    {
        public struct ShapeOutline
        {
            public string Color { get; set; }
            public float Width { get; set; }
        }

        public struct Shape
        {
            public string ID { set; get; }
            public int[][] Polygon { get; set; }
            public string Color { set; get; }
            public ShapeOutline? Outline { get; set; }
            public int[] Ellipse { get; set; }
            public int? Circle { get; set; }
            public int[] Rectangle { get; set; }
        }

        public struct SignalShape
        {
            public string Type { get; set; }
            public int[] Offset { get; set; }
            public float Angle { get; set; }
            public string[] Aspect { get; set; }
            public string Mirror { get; set; }
            public string? Head { get; set; }
        }

        public struct Signal
        {
            public string Name { get; set; }
            public string? Ruleset { get; set; }
            public string? StartIndication { get; set; }
            public IList<SignalShape> Shapes { get; set; }
        }

        public struct SignalRule
        {
            public string Indication { get; set; }
            public string NextAspect { get; set; }
            public string Aspect { get; set; }
        }

        public struct SignalRuleset
        {
            public string ID { get; set; }
            public IList<SignalRule> Rules { get; set; }
            public IList<string>? Aspects { get; set; }
            public string? DefaultIndication { get; set; }
        }

        public class SignalLibrary
        {
            public IList<Signal>? Signals { get; set; }
            public IList<Shape>? Shapes { get; set; }
            public IList<SignalRuleset>? Rulesets { get; set; }
        }
        public static class JSONLoader
        {
            private static T? LoadFile<T>(string path) where T : class
            {
                if (!File.Exists(path))
                    return null;

                try
                {
                    string json = File.ReadAllText(path);
                    return JsonSerializer.Deserialize<T>(json);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            public static bool LoadSignalLibrary(SignalSpace space, string path)
            {
                SignalLibrary? data = LoadFile<SignalLibrary>(path);

                if (data == null)
                    return false;

                if (data.Shapes != null)
                {
                    foreach (Shape shapeData in data.Shapes)
                    {
                        Color fillColor = Color.FromName(shapeData.Color);
                        Outline outline = new(fillColor, 1.0f);
                        if (shapeData.Outline != null)
                        {
                            ShapeOutline outlineData = (ShapeOutline)shapeData.Outline;
                            outline.color = Color.FromName(outlineData.Color);
                            outline.width = outlineData.Width;
                        }

                        PanelLib.Shape? shape = null;
                        if (shapeData.Polygon != null)
                        {
                            shape = new PolygonShape(shapeData.Polygon, fillColor, outline);
                        }
                        else if (shapeData.Rectangle != null && shapeData.Rectangle.Length == 4)
                        {
                            Point corner = new(shapeData.Rectangle[0], shapeData.Rectangle[1]);
                            Size size = new(shapeData.Rectangle[2], shapeData.Rectangle[3]);
                            shape = new RectangleShape(corner, size, fillColor, outline);
                        }
                        else if (shapeData.Ellipse != null && shapeData.Ellipse.Length == 2)
                        {
                            Size size = new(shapeData.Ellipse[0], shapeData.Ellipse[1]);
                            shape = new EllipseShape(size, fillColor, outline);
                        }
                        else if (shapeData.Circle != null)
                        {
                            int diameter = (int)shapeData.Circle;
                            Size size = new(diameter, diameter);
                            shape = new EllipseShape(size, fillColor, outline);
                        }

                        if (shape != null)
                            space.Bank.ShapeBank.AddShape(shapeData.ID, shape);
                    }
                }

                if (data.Rulesets != null)
                {
                    foreach (SignalRuleset rsData in data.Rulesets)
                    {
                        PanelLib.SignalRuleset ruleset = space.Bank.CreateRuleset(rsData.ID);

                        if (rsData.DefaultIndication != null)
                        {
                            ruleset.DefaultIndication = rsData.DefaultIndication;
                        }

                        foreach (SignalRule ruleData in rsData.Rules)
                        {
                            SignalRuleIndication indicationRules = ruleset.AddIndication(ruleData.Indication);
                            indicationRules.SetAspect(ruleData.NextAspect, ruleData.Aspect);
                        }

                        if (rsData.Aspects != null)
                        {
                            int i = 0;
                            foreach (string aspect in rsData.Aspects)
                            {
                                ruleset.SetAspectIndex(aspect, i);
                                i++;
                            }
                        }
                    }
                }

                if (data.Signals != null)
                {
                    foreach (Signal sig in data.Signals)
                    {
                        PanelLib.SignalRuleset? ruleset = null;
                        if (sig.Ruleset != null)
                            ruleset = space.Bank.GetRuleset(sig.Ruleset);

                        SignalType type = new(sig.Name, ruleset);

                        if (sig.Shapes != null && type is SignalTypeDraw typeDraw)
                        {
                            foreach (SignalShape shape in sig.Shapes)
                            {
                                ShapeMirror mirror = ShapeMirror.None;
                                if (shape.Mirror != null)
                                {
                                    _ = Enum.TryParse(shape.Mirror, out mirror);
                                }
                                typeDraw.AddShape(shape.Type, shape.Aspect, shape.Offset, shape.Angle, shape.Head, mirror);
                            }
                        }

                        if (sig.StartIndication != null)
                            type.StartIndication = sig.StartIndication;
                        else if (ruleset != null)
                            type.StartIndication = ruleset.DefaultIndication;

                        space.Bank.AddType(type);
                    }

                    space.Bank.InitShapes();
                }

                return true;
            }
        }
    }
}
