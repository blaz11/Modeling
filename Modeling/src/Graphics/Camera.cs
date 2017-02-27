using SharpDX;
using System.Windows;
using System.Windows.Input;

namespace Modeling.Graphics
{
    public class Camera
    {
        public Vector3 EyePosition { get; private set; }
        public Vector3 TargetPosition { get; private set; }
        public Vector3 Up { get; private set; }

        public Camera()
        {
            EyePosition = new Vector3(0, 0, -10.0f);
            TargetPosition = new Vector3(0, 0, 0);
            Up = Vector3.UnitY;
        }

        public void MouseMoved(IInputElement element, MouseEventArgs e)
        {
        }
    }
}