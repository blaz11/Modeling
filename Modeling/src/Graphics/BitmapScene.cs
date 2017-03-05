using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Modeling.Annotations;
using SharpDX;
using Matrix = SharpDX.Matrix;

namespace Modeling.Graphics
{
    public class BitmapScene : INotifyPropertyChanged
    {
        private WriteableBitmap _imageSource;
        public WriteableBitmap ImageSource
        {
            get
            {
                return _imageSource;
            }
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }

        private int _aRadius = 1;
        public int ARadius
        {
            get
            {
                return _aRadius;
            }
            set
            {
                _aRadius = value;
                Render();
                OnPropertyChanged();
            }
        }

        private int _bRadius = 1;
        public int BRadius
        {
            get
            {
                return _bRadius;
            }
            set
            {
                _bRadius = value;
                Render();
                OnPropertyChanged();
            }
        }

        private int _cRadius = 1;
        public int CRadius
        {
            get
            {
                return _cRadius;
            }
            set
            {
                _cRadius = value;
                Render();
                OnPropertyChanged();
            }
        }

        private int _mParameter;
        public int MParameter
        {
            get
            {
                return _mParameter;
            }
            set
            {
                _mParameter = value;
                Render();
                OnPropertyChanged();
            }
        }

        //public int Width
        //{
        //    get
        //    {
        //        return _width;
        //    }
        //    set
        //    {
        //        _width = value;
        //        SizeChanged(Width, Height);
        //        OnPropertyChanged();
        //    }
        //}

        //public int Height
        //{
        //    get
        //    {
        //        return _height;
        //    }
        //    set
        //    {
        //        _height = value;
        //        SizeChanged(Width, Height);
        //        OnPropertyChanged();
        //    }
        //}

        private Matrix _projectionMatrix;
        private Matrix _viewMatrix;
        private Matrix _viewProjectionMatrix;
        private int _width;
        private int _height;

        public BitmapScene(double width, double height)
        {
            var eye = new Vector3(0.0f, 0.0f, -15.0f);
            var target = new Vector3(0.0f, 0.0f, 0.0f);
            var up = Vector3.Up;
            _viewMatrix = Matrix.LookAtLH(eye, target, up);
            SizeChanged(width, height);
        }

        public void SizeChanged(double width, double height)
        {
            _projectionMatrix = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)(width / height), 0.1f, 100.0f);
            _width = (int)width;
            _height = (int)height;
            _viewProjectionMatrix = _viewMatrix * _projectionMatrix;
            ImageSource = BitmapFactory.New(_width, _height);
            Render();
        }

        public void Render()
        {
            var zProvider = new ZProvider(_viewProjectionMatrix, ARadius, BRadius, CRadius);
            _imageSource.Clear(Colors.Black);
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    var r = zProvider.GetZ((double)i / _width, (double)j / _height);
                    if (r == null)
                    {
                        continue;
                    }
                    _imageSource.SetPixel(i, j, Colors.Orange);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}