using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Modeling.Annotations;
using Modeling.Graphics;
using Modeling.Models.SimpleModel;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using Modeling.Models.Torus;

namespace Modeling.Screens
{
    public class ModelingMainViewModel : INotifyPropertyChanged
    {
        public int Width => 1280;
        public int Height => 900;

        private Camera _camera;
        public Camera Camera
        {
            get
            {
                return _camera;
            }
            set
            {
                _camera = value;
                OnPropertyChanged();
            }
        }

        private IScene _scene;
        public IScene Scene
        {
            get
            {
                return _scene;
            }
            set
            {
                if (Equals(value, _scene))
                {
                    return;
                }
                _scene = value;
                OnPropertyChanged();
            }
        }

        public async void PutItem()
        {
            UpdateSceneWithCamera();
            var torus = new Torus();
            var grid = new SimpleGrid();
            Scene.AddModel(torus);
            await Task.Delay(TimeSpan.FromSeconds(5));
           // Scene.RemoveModel(grid);
        }

        public void MouseMoved(IInputElement inputElement, MouseEventArgs e)
        {
            Camera.MouseMoved(inputElement, e);
            UpdateSceneWithCamera();
        }

        public void OnMouseWheel(IInputElement inputElement, MouseWheelEventArgs e)
        {
            Camera.OnMouseWheel(inputElement, e);
            UpdateSceneWithCamera();
        }

        private void UpdateSceneWithCamera()
        {
            _scene.SetupViewMatrix(Camera.EyePosition, Camera.TargetPosition, Camera.Up);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}