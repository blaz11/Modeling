using System;
using SharpDX;
using System.Windows;
using System.Windows.Input;
using Point = System.Windows.Point;

namespace Modeling.Graphics
{
    public class Camera
    {
        public Vector3 EyePosition { get; private set; }
        public Vector3 TargetPosition { get; set; }
        public Vector3 Up { get; private set; }

        private Point _oldMousePosition;
        private double _phi;
        private double _theta;
        private int _angleSign = 1;
        private double _radius;
        private double distance;

        private const double ROTATION_MULTIPLIER = 0.005f;
        private const double ANGLE_EPSILON = 1e-4f;
        private const double ZOOM_SPEED = 0.003f;
        private const int CAMERA_RADIUS = 8;

        public Camera()
        {
            EyePosition = new Vector3(0, 0, -10.0f);
            TargetPosition = new Vector3(0, 0, 0);
            Up = Vector3.UnitY;

            _phi = MathUtil.Pi;
            _theta = MathUtil.PiOverTwo;
            _radius = CAMERA_RADIUS * 3;
            distance = CAMERA_RADIUS;
        }

        public void MouseMoved(IInputElement element, MouseEventArgs e)
        {
            var newPosition = e.GetPosition(element);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                CameraRotate(newPosition);
            }
            _oldMousePosition = newPosition;
            EyePosition = GenerateEyeVector();
        }

        public void OnMouseWheel(IInputElement inputElement, MouseWheelEventArgs e)
        {
            var zoomChange = distance * e.Delta * ZOOM_SPEED;
            _radius -= zoomChange;
            EyePosition = GenerateEyeVector();
        }

        private void CameraRotate(Point newPosition)
        {
            if (_oldMousePosition == null)
            {
                return;
            }
            var dx = (newPosition.X - _oldMousePosition.X) * ROTATION_MULTIPLIER;
            var dy = (newPosition.Y - _oldMousePosition.Y) * ROTATION_MULTIPLIER;
            _theta += dy * _angleSign;
            _phi += dx * _angleSign;
            if (_theta > MathUtil.Pi)
            {
                _theta = MathUtil.Pi - ANGLE_EPSILON;
                _phi += MathUtil.Pi;
                Up *= -1;
                _angleSign *= -1;
            }
            else if (_theta < ANGLE_EPSILON)
            {
                _theta = ANGLE_EPSILON;
                _phi += MathUtil.Pi;
                Up *= -1;
                _angleSign *= -1;
            }

            if (_phi < 0.0f)
            {
                _phi += MathUtil.TwoPi;
            }
            else if (_phi > MathUtil.TwoPi)
            {
                _phi -= MathUtil.TwoPi;
            }
        }

        private Vector3 GenerateEyeVector()
        {
            var sinTheta = Math.Sin(_theta);
            var x = -_radius * sinTheta * Math.Cos(_phi);
            var y = -_radius * Math.Cos(_theta);
            var z = -_radius * sinTheta * Math.Sin(_phi);
            return new Vector3((float)x, (float)y, (float)z);
        }
    }
}