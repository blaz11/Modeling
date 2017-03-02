using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Modeling.Annotations;
using SharpDX;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D10.Buffer;
using Device = SharpDX.Direct3D10.Device;

namespace Modeling.Models
{
    public abstract class ModelBase : INotifyPropertyChanged
    {
        public Color4 ModelColor
        {
            get
            {
                return _modelColor;
            }
            set
            {
                OnPropertyChanged();
                _modelColor = value;
            }
        }

        public Vector3 ModelPosition { get; set; }

        public float Scale { get; set; } = 1.0f;

        public IList<Vector4> Vertices { get; set; }

        public IList<uint> Indices { get; set; }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Shape
        {
            get
            {
                return _shape;
            }
            set
            {
                _shape = value;
                OnPropertyChanged();
            }
        }

        private const float MOVE_CONSTANT = 0.1f;

        private bool _renderable;
        private Buffer _verticesBuffer;
        private Buffer _indicesBuffer;
        private Device _device;
        private string _name;
        private string _shape;
        private Color4 _modelColor;

        protected ModelBase(string name)
        {
            Name = name;
        }

        public void ConnectDevice(Device device)
        {
            DisconnectDevice();
            _device = device;
            SetupForRendering();
        }

        public void DisconnectDevice()
        {
            _device = null;
            Dispose();
        }

        public void Render()
        {
            if (!_renderable)
            {
                return;
            }
            var vertexBufferBinding = new VertexBufferBinding(_verticesBuffer, Utilities.SizeOf<Vector4>(), 0);
            _device.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);
            _device.InputAssembler.SetIndexBuffer(_indicesBuffer, Format.R32_UInt, 0);
            _device.DrawIndexed(Indices.Count, 0, 0);
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            var dx = 0.0f;
            var dy = 0.0f;
            var dz = 0.0f;
            if (e.KeyboardDevice.IsKeyDown(Key.Back))
            {
                ModelPosition = new Vector3(dx, dy, dz);
                return;
            }
            if (e.KeyboardDevice.IsKeyDown(Key.A))
            {
                dx += MOVE_CONSTANT;
            }
            if (e.KeyboardDevice.IsKeyDown(Key.D))
            {
                dx -= MOVE_CONSTANT;
            }
            if (e.KeyboardDevice.IsKeyDown(Key.W))
            {
                dy += MOVE_CONSTANT;
            }
            if (e.KeyboardDevice.IsKeyDown(Key.S))
            {
                dy -= MOVE_CONSTANT;
            }
            if (e.KeyboardDevice.IsKeyDown(Key.Q))
            {
                dz += MOVE_CONSTANT;
            }
            if (e.KeyboardDevice.IsKeyDown(Key.E))
            {
                dz -= MOVE_CONSTANT;
            }

            ModelPosition = ModelPosition + new Vector3(dx, dy, dz);
            if (e.KeyboardDevice.IsKeyDown(Key.OemPlus))
            {
                Scale += MOVE_CONSTANT;
            }
            if (e.KeyboardDevice.IsKeyDown(Key.OemMinus))
            {
                Scale -= MOVE_CONSTANT;
            }
            if (Scale < 0)
            {
                Scale = 0;
            }
        }

        private void Dispose()
        {
            DisposeBuffers();
            _device?.Dispose();
            _renderable = false;
        }

        private void DisposeBuffers()
        {
            if (_verticesBuffer != null && !_verticesBuffer.IsDisposed)
            {
                _verticesBuffer?.Dispose();
            }
            if (_indicesBuffer != null && !_indicesBuffer.IsDisposed)
            {
                _indicesBuffer?.Dispose();
            }
        }

        protected void SetupForRendering()
        {
            if (Vertices == null || Indices == null || _device == null)
            {
                return;
            }
            DisposeBuffers();
            _verticesBuffer = GenerateBuffer(BindFlags.VertexBuffer, Vertices,
                Utilities.SizeOf<Vector4>());
            _indicesBuffer = GenerateBuffer(BindFlags.IndexBuffer, Indices, sizeof(uint));
            _renderable = true;
        }

        private Buffer GenerateBuffer<T>(BindFlags bindFlags,
            IList<T> data, int dataElementSize) where T : struct
        {
            var size = data.Count * dataElementSize;
            using (var stream = new DataStream(size, true, true))
            {
                stream.WriteRange(data.ToArray());
                stream.Position = 0;
                var buffer = new Buffer(_device, stream, new BufferDescription()
                {
                    BindFlags = bindFlags,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                    SizeInBytes = size,
                    Usage = ResourceUsage.Default
                });
                return buffer;
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