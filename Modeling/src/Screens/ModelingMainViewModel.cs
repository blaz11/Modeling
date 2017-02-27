using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Modeling.Annotations;
using Modeling.Graphics;
using Modeling.Models.SimpleModel;

namespace Modeling.Screens
{
    public class ModelingMainViewModel : INotifyPropertyChanged
    {
        private IScene _scene;
        public int Width => 1280;
        public int Height => 900;

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
            var grid = new SimpleGrid();
            Scene.AddModel(grid);
            await Task.Delay(TimeSpan.FromSeconds(5));
            Scene.RemoveModel(grid);
        }

        public void MouseMoved(MouseEventArgs e)
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}