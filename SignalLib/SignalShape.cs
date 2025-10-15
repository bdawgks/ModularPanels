using ModularPanels.PanelLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ModularPanels.SignalLib
{
    internal class SignalShape
    {
        readonly string _id;
        readonly HashSet<string>? _aspect;
        Shape? _shape = null;
        readonly Vector2 _offset;
        readonly float _angle;
        readonly ShapeMirror _mirror;
        readonly string? _headId;

        public string ID { get { return _id; } }
        public Shape? Shape { get { return _shape; } }

        public string? HeadID { get { return _headId; } }

        public SignalShape(string id, string[] aspect, Vector2 offset, float angle, string? headId = null, ShapeMirror mirror = ShapeMirror.None)
        {
            _id = id;
            _offset = offset;
            _angle = angle;
            _mirror = mirror;
            _headId = headId;

            if (aspect != null)
                _aspect = [.. aspect];
        }

        public void InitShape(ShapeBank bank)
        {
            _shape = bank[_id];
        }

        public bool VisibleWithAspect(string aspect)
        {
            if (aspect == null || aspect == "")
                return true;

            if (_aspect == null || _aspect.Count < 1)
                return true;

            return _aspect.Contains(aspect);
        }

        public void DrawShape(Graphics g, PointF origin, float originAngle, float scale = 1.0f)
        {
            if (_shape == null)
                return;

            Vector2 oTransformed = MathLib.RotateVector(_offset * scale, originAngle);

            _shape.DrawShape(g, origin, oTransformed, originAngle + _angle, scale, _mirror);
        }
    }
}
