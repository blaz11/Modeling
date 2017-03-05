using Caliburn.Micro;
using Modeling.Annotations;
using Modeling.Graphics;
using Modeling.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Modeling.Screens
{
    public class ModelingMainViewModel : INotifyPropertyChanged
    {
        public int Width { get; set; } = 1280;
        public int Height { get; set; } = 800;

        private IObservableCollection<ModelBase> _models;
        public IObservableCollection<ModelBase> Models
        {
            get
            {
                return _models;
            }
            set
            {
                _models = value;
                OnPropertyChanged();
            }
        }

        private ModelBase _selectedModel;
        public ModelBase SelectedModel
        {
            get
            {
                return _selectedModel;
            }
            set
            {
                _selectedModel = value;
                Camera.TargetPosition = SelectedModel.ModelPosition;
                UpdateSceneWithCamera();
                OnPropertyChanged();
            }
        }

        public Camera Camera { get; set; }

        public WriteableBitmap ImageSource { get; set; }

        //private IScene _scene;
        //public IScene Scene
        //{
        //    get
        //    {
        //        return _scene;
        //    }
        //    set
        //    {
        //        if (Equals(value, _scene))
        //        {
        //            return;
        //        }
        //        _scene = value;
        //        OnPropertyChanged();
        //    }
        //}

        public ModelingMainViewModel()
        {
                Models = new BindableCollection<ModelBase>();
        }

        public void PutItem()
        {
            UpdateSceneWithCamera();
            //var torus = new Torus("test torus");
            //Scene.AddModel(torus);
            //Models.Add(torus);
            //var grid = new SimpleGrid("test grid");
            //Scene.AddModel(grid);
            //Models.Add(grid);
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

        public void OnKeyDown(KeyEventArgs e)
        {
            if (SelectedModel == null)
            {
                return;
            }
            SelectedModel.OnKeyDown(e);
            Camera.TargetPosition = SelectedModel.ModelPosition;
            UpdateSceneWithCamera();
        }

        private void UpdateSceneWithCamera()
        {
            //_scene.SetupViewMatrix(Camera.EyePosition, Camera.TargetPosition, Camera.Up);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}