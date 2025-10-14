using ModularPanels.DrawLib;

namespace ModularPanels.ButtonLib
{
    public interface IControl : DrawLib.IDrawable, IDrawTransformable
    {
        public IClickable[] GetClickables();
    }
}
