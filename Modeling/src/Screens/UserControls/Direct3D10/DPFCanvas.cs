using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Modeling.Graphics;
using Modeling.Helpers;
using SharpDX;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D10.Device1;

namespace Modeling.Screens.UserControls.Direct3D10
{
    public class DPFCanvas : Image, ISceneHost
    {
        private Device _device;
        private Texture2D _renderTarget;
        private Texture2D _depthStencil;
        private RenderTargetView _renderTargetView;
        private DepthStencilView _depthStencilView;
        private DX10ImageSource _d3DSurface;
        private readonly Stopwatch _renderTimer;
        private IScene _renderScene;
        private bool _sceneAttached;

        public Color4 ClearColor = SharpDX.Color.Black;

        public static readonly DependencyProperty SceneProperty = 
            DependencyProperty.Register(nameof(Scene), typeof(IScene), typeof(DPFCanvas));

        public IScene Scene
        {
            get
            {
                return _renderScene;
            }
            set
            {
                if (ReferenceEquals(_renderScene, value))
                {
                    return;
                }

                _renderScene?.Detach();

                _sceneAttached = false;
                _renderScene = value;
            }
        }

        public SharpDX.Direct3D10.Device Device
        {
            get
            {
                return _device;
            }
        }

        public DPFCanvas()
        {
            _renderTimer = new Stopwatch();
            Loaded += Window_Loaded;
            Unloaded += Window_Closing;
            // TODO: Remove this hack
            DataContextChanged += (sender, args) =>
            {
                var modelingMainView = DataContext as ModelingMainViewModel;
                Scene = modelingMainView.Scene;
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsInDesignMode)
            {
                return;
            }

            StartD3D();
            StartRendering();
        }

        private void Window_Closing(object sender, RoutedEventArgs e)
        {
            if (IsInDesignMode)
            {
                return;
            }

            StopRendering();
            EndD3D();
        }

        private void StartD3D()
        {
            _device = new Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport, FeatureLevel.Level_10_0);

            _d3DSurface = new DX10ImageSource();
            _d3DSurface.IsFrontBufferAvailableChanged += OnIsFrontBufferAvailableChanged;

            CreateAndBindTargets();

            Source = _d3DSurface;
        }

        private void EndD3D()
        {
            if (_renderScene != null)
            {
                _renderScene.Detach();
                _sceneAttached = false;
            }

            _d3DSurface.IsFrontBufferAvailableChanged -= OnIsFrontBufferAvailableChanged;
            Source = null;

            Disposer.RemoveAndDispose(ref _d3DSurface);
            Disposer.RemoveAndDispose(ref _renderTargetView);
            Disposer.RemoveAndDispose(ref _depthStencilView);
            Disposer.RemoveAndDispose(ref _renderTarget);
            Disposer.RemoveAndDispose(ref _depthStencil);
            Disposer.RemoveAndDispose(ref _device);
        }

        private void CreateAndBindTargets()
        {
            _d3DSurface.SetRenderTargetDX10(null);

            Disposer.RemoveAndDispose(ref _renderTargetView);
            Disposer.RemoveAndDispose(ref _depthStencilView);
            Disposer.RemoveAndDispose(ref _renderTarget);
            Disposer.RemoveAndDispose(ref _depthStencil);

            var width = Math.Max((int)ActualWidth, 100);
            var height = Math.Max((int)ActualHeight, 100);

            var colorDescription = new Texture2DDescription
            {
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.Shared,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1
            };

            var depthDescription = new Texture2DDescription
            {
                BindFlags = BindFlags.DepthStencil,
                Format = Format.D32_Float_S8X24_UInt,
                Width = width,
                Height = height,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.None,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1,
            };

            _renderTarget = new Texture2D(_device, colorDescription);
            _depthStencil = new Texture2D(_device, depthDescription);
            _renderTargetView = new RenderTargetView(_device, _renderTarget);
            _depthStencilView = new DepthStencilView(_device, _depthStencil);

            _d3DSurface.SetRenderTargetDX10(_renderTarget);
        }

        public void MouseMove(object e)
        {

        }

        private void StartRendering()
        {
            if (_renderTimer.IsRunning)
            {
                return;
            }

            CompositionTarget.Rendering += OnRendering;
            _renderTimer.Start();
        }

        private void StopRendering()
        {
            if (!_renderTimer.IsRunning)
            {
                return;
            }

            CompositionTarget.Rendering -= OnRendering;
            _renderTimer.Stop();
        }

        private void OnRendering(object sender, EventArgs e)
        {
            if (!_renderTimer.IsRunning)
            {
                return;
            }

            Render(_renderTimer.Elapsed);
            _d3DSurface.InvalidateD3DImage();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            CreateAndBindTargets();
            base.OnRenderSizeChanged(sizeInfo);
        }

        private void Render(TimeSpan sceneTime)
        {
            var device = _device;
            if (device == null)
            {
                return;
            }

            var renderTarget = _renderTarget;
            if (renderTarget == null)
            {
                return;
            }

            var targetWidth = renderTarget.Description.Width;
            var targetHeight = renderTarget.Description.Height;

            device.OutputMerger.SetTargets(_depthStencilView, _renderTargetView);
            device.Rasterizer.SetViewports(new Viewport(0, 0, targetWidth, targetHeight, 0.0f, 1.0f));

            device.ClearRenderTargetView(_renderTargetView, ClearColor);
            device.ClearDepthStencilView(_depthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);

            if (Scene != null)
            {
                if (!_sceneAttached)
                {
                    _sceneAttached = true;
                    _renderScene.Attach(this);
                }

                Scene.Update(_renderTimer.Elapsed);
                Scene.Render();
            }

            device.Flush();
        }

        private void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // this fires when the screensaver kicks in, the machine goes into sleep or hibernate
            // and any other catastrophic losses of the d3d device from WPF's point of view
            if (_d3DSurface.IsFrontBufferAvailable)
            {
                CreateAndBindTargets();
                StartRendering();
            }
            else
            {
                StopRendering();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the control is in design mode
        /// (running in Blend or Visual Studio).
        /// </summary>
        public static bool IsInDesignMode
        {
            get
            {
                var prop = DesignerProperties.IsInDesignModeProperty;
                var isDesignMode = (bool)DependencyPropertyDescriptor.FromProperty(prop, typeof(FrameworkElement)).Metadata.DefaultValue;
                return isDesignMode;
            }
        }
    }
}