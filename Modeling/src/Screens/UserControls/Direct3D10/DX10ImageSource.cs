using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Modeling.Helpers;
using SharpDX.Direct3D10;
using SharpDX.Direct3D9;

namespace Modeling.Screens.UserControls.Direct3D10
{
    public class DX10ImageSource : D3DImage, IDisposable, IInputElement
    {
        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr GetDesktopWindow();
        private static int _activeClients;
        private static Direct3DEx _d3DContext;
        private static DeviceEx _d3DDevice;
        private Texture _renderTarget;

        public event MouseButtonEventHandler PreviewMouseLeftButtonDown;
        public event MouseButtonEventHandler MouseLeftButtonDown;
        public event MouseButtonEventHandler PreviewMouseLeftButtonUp;
        public event MouseButtonEventHandler MouseLeftButtonUp;
        public event MouseButtonEventHandler PreviewMouseRightButtonDown;
        public event MouseButtonEventHandler MouseRightButtonDown;
        public event MouseButtonEventHandler PreviewMouseRightButtonUp;
        public event MouseButtonEventHandler MouseRightButtonUp;
        public event MouseEventHandler PreviewMouseMove;
        public event MouseEventHandler MouseMove;
        public event MouseWheelEventHandler PreviewMouseWheel;
        public event MouseWheelEventHandler MouseWheel;
        public event MouseEventHandler MouseEnter;
        public event MouseEventHandler MouseLeave;
        public event MouseEventHandler GotMouseCapture;
        public event MouseEventHandler LostMouseCapture;
        public event StylusDownEventHandler PreviewStylusDown;
        public event StylusDownEventHandler StylusDown;
        public event StylusEventHandler PreviewStylusUp;
        public event StylusEventHandler StylusUp;
        public event StylusEventHandler PreviewStylusMove;
        public event StylusEventHandler StylusMove;
        public event StylusEventHandler PreviewStylusInAirMove;
        public event StylusEventHandler StylusInAirMove;
        public event StylusEventHandler StylusEnter;
        public event StylusEventHandler StylusLeave;
        public event StylusEventHandler PreviewStylusInRange;
        public event StylusEventHandler StylusInRange;
        public event StylusEventHandler PreviewStylusOutOfRange;
        public event StylusEventHandler StylusOutOfRange;
        public event StylusSystemGestureEventHandler PreviewStylusSystemGesture;
        public event StylusSystemGestureEventHandler StylusSystemGesture;
        public event StylusButtonEventHandler StylusButtonDown;
        public event StylusButtonEventHandler PreviewStylusButtonDown;
        public event StylusButtonEventHandler PreviewStylusButtonUp;
        public event StylusButtonEventHandler StylusButtonUp;
        public event StylusEventHandler GotStylusCapture;
        public event StylusEventHandler LostStylusCapture;
        public event KeyEventHandler PreviewKeyDown;
        public event KeyEventHandler KeyDown;
        public event KeyEventHandler PreviewKeyUp;
        public event KeyEventHandler KeyUp;
        public event KeyboardFocusChangedEventHandler PreviewGotKeyboardFocus;
        public event KeyboardFocusChangedEventHandler GotKeyboardFocus;
        public event KeyboardFocusChangedEventHandler PreviewLostKeyboardFocus;
        public event KeyboardFocusChangedEventHandler LostKeyboardFocus;
        public event TextCompositionEventHandler PreviewTextInput;
        public event TextCompositionEventHandler TextInput;

        public bool IsMouseOver
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsMouseDirectlyOver
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsMouseCaptured
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsStylusOver
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsStylusDirectlyOver
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsStylusCaptured
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsKeyboardFocusWithin
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsKeyboardFocused
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool Focusable
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public DX10ImageSource()
        {
            StartD3D();
            _activeClients++;
            MouseMove += DX10ImageSource_MouseMove;
        }

        private void DX10ImageSource_MouseMove(object sender, MouseEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            SetRenderTargetDX10(null);
            Disposer.RemoveAndDispose(ref _renderTarget);

            _activeClients--;
            EndD3D();
        }

        public void InvalidateD3DImage()
        {
            if (_renderTarget != null)
            {
                Lock();
                AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
                Unlock();
            }
        }

        public void SetRenderTargetDX10(Texture2D renderTarget)
        {
            if (_renderTarget != null)
            {
                _renderTarget = null;

                Lock();
                SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
                Unlock();
            }

            if (renderTarget == null)
            {
                return;
            }

            if (!IsShareable(renderTarget))
            {
                throw new ArgumentException("Texture must be created with ResourceOptionFlags.Shared");
            }

            var format = TranslateFormat(renderTarget);
            if (format == Format.Unknown)
            {
                throw new ArgumentException("Texture format is not compatible with OpenSharedResource");
            }

            var handle = GetSharedHandle(renderTarget);
            if (handle == IntPtr.Zero)
            {
                throw new ArgumentNullException("Handle");
            }

            _renderTarget = new Texture(_d3DDevice, renderTarget.Description.Width,
                renderTarget.Description.Height, 1, Usage.RenderTarget, format, Pool.Default, ref handle);
            using (var surface = _renderTarget.GetSurfaceLevel(0))
            {
                Lock();
                SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
                Unlock();
            }
        }

        private void StartD3D()
        {
            if (_activeClients != 0)
            {
                return;
            }

            _d3DContext = new Direct3DEx();

            var presentparams = new PresentParameters
            {
                Windowed = true,
                SwapEffect = SwapEffect.Discard,
                DeviceWindowHandle = GetDesktopWindow(),
                PresentationInterval = PresentInterval.Default
            };

            _d3DDevice = new DeviceEx(_d3DContext, 0, DeviceType.Hardware, IntPtr.Zero,
                CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve, 
                presentparams);
        }

        private void EndD3D()
        {
            if (_activeClients != 0)
            {
                return;
            }

            Disposer.RemoveAndDispose(ref _renderTarget);
            Disposer.RemoveAndDispose(ref _d3DDevice);
            Disposer.RemoveAndDispose(ref _d3DContext);
        }

        private IntPtr GetSharedHandle(Texture2D texture)
        {
            var resource = texture.QueryInterface<SharpDX.DXGI.Resource>();
            var result = resource.SharedHandle;
            resource.Dispose();
            return result;
        }

        private static Format TranslateFormat(SharpDX.Direct3D10.Texture2D texture)
        {
            switch (texture.Description.Format)
            {
                case SharpDX.DXGI.Format.R10G10B10A2_UNorm:
                    return Format.A2B10G10R10;
                case SharpDX.DXGI.Format.R16G16B16A16_Float:
                    return Format.A16B16G16R16F;
                case SharpDX.DXGI.Format.B8G8R8A8_UNorm:
                    return Format.A8R8G8B8;
                default:
                    return Format.Unknown;
            }
        }

        private static bool IsShareable(SharpDX.Direct3D10.Texture2D texture)
        {
            return (texture.Description.OptionFlags & SharpDX.Direct3D10.ResourceOptionFlags.Shared) != 0;
        }

        public void RaiseEvent(RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void AddHandler(RoutedEvent routedEvent, Delegate handler)
        {
            throw new NotImplementedException();
        }

        public void RemoveHandler(RoutedEvent routedEvent, Delegate handler)
        {
            throw new NotImplementedException();
        }

        public bool CaptureMouse()
        {
            throw new NotImplementedException();
        }

        public void ReleaseMouseCapture()
        {
            throw new NotImplementedException();
        }

        public bool CaptureStylus()
        {
            throw new NotImplementedException();
        }

        public void ReleaseStylusCapture()
        {
            throw new NotImplementedException();
        }

        public bool Focus()
        {
            throw new NotImplementedException();
        }
    }
}