using ModularPanels.DrawLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.PanelLib
{
    public class TextStyle
    {
        public string font = "Calibri";
        public Color color = Color.Black;
        public int size = 30;
        public bool bold = false;
    }

    public class PanelText(GridPos gridPos, float angle) : DrawTransform, IDrawable, IDrawTransformable
    {
        readonly DrawingPos _pos = Grid.Instance.TransformPos(gridPos);
        readonly float _angle = angle;

        string _text = "";
        TextStyle _style = new();

        public string Text
        {
            get => _text;
            set => _text = value;
        }

        public float Angle
        {
            get => _angle;
        }

        public TextStyle Style
        {
            get => _style;
            set => _style = value;
        }

        public void Draw(DrawingContext context)
        {
            context.drawing.DrawText(context.graphics, _text, Transform(_pos), _style, _angle);
        }

        public List<DrawTransform> GetTransforms()
        {
            return [this];
        }
    }
}
