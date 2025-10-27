using ModularPanels.DrawLib;

namespace ModularPanels.ButtonLib
{
    public interface IControl : IDrawable, IDrawTransformable
    {
        public IClickable[] GetClickables();
    }
}
