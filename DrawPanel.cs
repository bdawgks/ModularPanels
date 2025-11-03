

namespace ModularPanels
{
    public class DrawPanel : Panel
    {
        ScrollBar? scrollBar;
        Control? parent;
        int scrollMargin = 0;
        int minWidth;
        int initWidth;

        public event EventHandler<ScrollEventArgs>? ScrollEvents;

        public DrawPanel()
        {
            DoubleBuffered = true;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20;
                return cp;
            }
        }

        public void InitScrollbar(ScrollBar scrollBar, Control parent, int margin)
        {
            this.scrollBar = scrollBar;
            this.parent = parent;
            scrollMargin = margin;
            initWidth = Width;

            scrollBar.Scroll += MoveScrollBar;
            UpdateScrollbar();
        }

        public void SetMinWidth(int width)
        {
            minWidth = width;
            UpdateScrollbar();
        }

        public void UpdateScrollbar()
        {
            if (parent == null || scrollBar == null)
                return;

            Width = Math.Max(minWidth, initWidth);

            Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            if (parent.Width >= Width)
            {
                scrollBar.Visible = false;
                Anchor |= AnchorStyles.Left | AnchorStyles.Right;
                Left = 0;
                return;
            }

            scrollBar.Visible = true;
            int diff = (ClientRectangle.Width - parent.Width) / 2;
            scrollBar.Minimum = -diff - scrollMargin;
            scrollBar.Maximum = diff + scrollMargin;

            MoveScrollBar(null, new ScrollEventArgs(ScrollEventType.EndScroll, scrollBar.Value));
        }

        public void ScrollView(int delta)
        {
            if (scrollBar == null || !scrollBar.Visible)
                return;

            int newVal = scrollBar.Value + delta;
            newVal = Math.Clamp(newVal, scrollBar.Minimum, scrollBar.Maximum);

            if (newVal != scrollBar.Value)
            {
                scrollBar.Value = newVal;
                MoveScrollBar(null, new ScrollEventArgs(ScrollEventType.EndScroll, newVal));
            }
        }

        private void MoveScrollBar(object? sender, ScrollEventArgs e)
        {
            if (parent == null)
                return;

            int diff = (ClientRectangle.Width - parent.Width) / 2;
            Left = -e.NewValue - diff - scrollMargin / 2;
            Width = minWidth;

            ScrollEvents?.Invoke(this, e);
        }
    }
}
