using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ScreenCraft
{
    public interface IEditorState
    {
        void HandleMouseDown(object sender, MouseButtonEventArgs e);
        void HandleMouseMove(object sender, MouseEventArgs e);
        void HandleMouseUp(object sender, MouseButtonEventArgs e);
    }
}
