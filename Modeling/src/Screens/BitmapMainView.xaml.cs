using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Modeling.Graphics;

namespace Modeling.Screens
{
    /// <summary>
    /// Interaction logic for BitmapMainView.xaml
    /// </summary>
    public partial class BitmapMainView : Window
    {
        private BitmapScene _bitmapScene;

        public BitmapMainView()
        {
            InitializeComponent();
            _bitmapScene = new BitmapScene(1024, 768);
            DataContext = _bitmapScene;
        }

        private void Image_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _bitmapScene.SizeChanged(e.NewSize.Width, e.NewSize.Height);
        }
    }
}