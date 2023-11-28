using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace ScreenCraft
{
    public class DottedBorderAdorner : Adorner
    {
        public DottedBorderAdorner(UIElement adornedElement) : base(adornedElement) { }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var adornedElementRect = new Rect(this.AdornedElement.RenderSize);

            var dashPen = new Pen(Brushes.White, 1);
            dashPen.DashStyle = DashStyles.Dash;

            // Отображаем пунктирную границу вокруг элемента
            drawingContext.DrawRectangle(null, dashPen, adornedElementRect);
        }
    }
}
