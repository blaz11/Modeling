using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D10.Buffer;
using Device = SharpDX.Direct3D10.Device;

namespace Modeling.Models
{
    public abstract class ModelBase
    {
        public Color4 ModelColor { get; set; }

        public Vector4 ModelPosition { get; set; }

        public IList<Vector4> Vertices { get; set; }

        public IList<uint> Indices { get; set; }

        public IList<Tuple<int, int>> Edges { get; set; }

        private bool _renderable;
        private Buffer _verticesBuffer;
        private Buffer _indicesBuffer;
        private Device _device;

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
    }
}