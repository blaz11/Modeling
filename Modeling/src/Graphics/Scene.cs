using System;
using System.Collections.Generic;
using System.Linq;
using Modeling.Helpers;
using Modeling.Models;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D10.Buffer;

namespace Modeling.Graphics
{
    public class Scene : IScene
    {
        private ISceneHost _host;
        private InputLayout _vertexLayout;
        private DataStream _vertexStream;
        private Buffer _vertices;
        private Buffer _matrixBuffer;
        private PixelShader _pixelShader;
        private VertexShader _vertexShader;

        private Matrix _viewMatrix;
        private Matrix _projectionMatrix;
        private Matrix _viewProjection;
        private Matrix _worldViewProjectionMatrix;

        private const int ELEMENT_SIZE = 32;

        private readonly IList<IModel> _modelsOnTheScene;
        private int _verticesCount;

        public Scene()
        {
            _projectionMatrix = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, 4 / 3, 0.1f, 100.0f);
            _modelsOnTheScene = new List<IModel>();
        }

        public void SetupViewMatrix(Vector3 eye, Vector3 target, Vector3 up)
        {
            _viewMatrix = Matrix.LookAtLH(eye, target, up);
            SetUpViewProjectionMatrix();
        }

        private void SetUpViewProjectionMatrix()
        {
            _viewProjection = Matrix.Multiply(_viewMatrix, _projectionMatrix);
        }

        public void Attach(ISceneHost host)
        {
            _host = host;

            var device = host.Device;
            if (device == null)
            {
                throw new Exception("Scene host device is null");
            }

            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("Shaders.fx", "VS", "vs_4_0");
            _vertexShader = new VertexShader(device, vertexShaderByteCode);

            var pixelShaderByteCode = ShaderBytecode.CompileFromFile("Shaders.fx", "PS", "ps_4_0");
            _pixelShader = new PixelShader(device, pixelShaderByteCode);

            _vertexLayout = new InputLayout(device,
                ShaderSignature.GetInputSignature(vertexShaderByteCode),
                new[]
                {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                    new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
                });

            _matrixBuffer = new Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None);

            device.VertexShader.SetConstantBuffer(0, _matrixBuffer);
            device.VertexShader.Set(_vertexShader);
            device.PixelShader.Set(_pixelShader);
            device.InputAssembler.InputLayout = _vertexLayout;
            device.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;

            device.Flush();
        }

        public void Detach()
        {
            Disposer.RemoveAndDispose(ref _vertices);
            Disposer.RemoveAndDispose(ref _vertexLayout);
            Disposer.RemoveAndDispose(ref _pixelShader);
            Disposer.RemoveAndDispose(ref _pixelShader);
            Disposer.RemoveAndDispose(ref _vertexStream);
        }

        public void Update(TimeSpan sceneTime)
        {
            //var time = (float)sceneTime.TotalMilliseconds / 1000.0f;
            _worldViewProjectionMatrix = //Matrix.RotationX(time) *
                                         //Matrix.RotationY(time) *
                                         //Matrix.RotationZ(time * .7f) *
                                        _viewProjection;
            _worldViewProjectionMatrix.Transpose();
        }

        public void Render()
        {
            var device = _host.Device;
            if (device == null)
            {
                return;
            }

            device.UpdateSubresource(ref _worldViewProjectionMatrix, _matrixBuffer);
            device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertices, ELEMENT_SIZE, 0));

            device.Draw(_verticesCount, 0);
        }

        private void GenerateVerticesStream()
        {
            var vertices = GetVerticesToDraw();
            _verticesCount = vertices.Count;

            if (_verticesCount == 0)
            {
                Disposer.RemoveAndDispose(ref _vertexStream);
                Disposer.RemoveAndDispose(ref _vertices);
                return;
            }

            var verticesSize = _verticesCount * ELEMENT_SIZE;

            _vertexStream = new DataStream(verticesSize, true, true);
            _vertexStream.WriteRange(vertices.ToArray());
            _vertexStream.Position = 0;

            _vertices = new Buffer(_host.Device, _vertexStream, new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = verticesSize,
                Usage = ResourceUsage.Default
            });
        }

        private IList<Vector4> GetVerticesToDraw()
        {
            var result = new List<Vector4>();
            foreach (var model in _modelsOnTheScene)
            {
                for (var index = 0; index < model.ModelPoints.Length; index++)
                {
                    var edges = model.GetEdgesForPoint(index);
                    foreach (var edge in edges)
                    {
                        result.Add(edge);
                        result.Add(model.ModelColor);
                    }
                }
            }
            return result;
        }

        private void ModelUpdated(object sender, EventArgs eventArgs)
        {
            GenerateVerticesStream();
        }

        public void AddModel(IModel model)
        {
            _modelsOnTheScene.Add(model);
            model.ModelUpdated += ModelUpdated;
            GenerateVerticesStream();
        }


        public void RemoveModel(IModel model)
        {
            _modelsOnTheScene.Remove(model);
            model.ModelUpdated -= ModelUpdated;
            GenerateVerticesStream();
        }
    }
}