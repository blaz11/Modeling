using System;
using System.Collections.Generic;
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
        private Buffer _matrixBuffer;
        private Buffer _colorBuffer;
        private PixelShader _pixelShader;
        private VertexShader _vertexShader;

        private Matrix _viewMatrix;
        private Matrix _projectionMatrix;
        private Matrix _viewProjection;
        private Matrix _worldViewProjectionMatrix;

        private readonly IList<ModelBase> _modelsOnTheScene;

        public Scene()
        {
            _projectionMatrix = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, 4 / 3, 0.1f, 100.0f);
            _modelsOnTheScene = new List<ModelBase>();
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
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0)
                });

            _matrixBuffer = new Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None);
            _colorBuffer = new Buffer(device, Utilities.SizeOf<Color4>(), ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None);

            device.InputAssembler.InputLayout = _vertexLayout;
            device.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            device.VertexShader.SetConstantBuffer(0, _matrixBuffer);
            device.VertexShader.SetConstantBuffer(1, _colorBuffer);
            device.VertexShader.Set(_vertexShader);
            device.PixelShader.Set(_pixelShader);

            device.Flush();
        }

        public void Detach()
        {
            Disposer.RemoveAndDispose(ref _vertexLayout);
            Disposer.RemoveAndDispose(ref _pixelShader);
        }

        public void Update(TimeSpan sceneTime)
        {
            //var time = (float)sceneTime.TotalMilliseconds / 1000.0f;
            //_worldViewProjectionMatrix = //Matrix.RotationX(time) *
            //                             //Matrix.RotationY(time) *
            //                             //Matrix.RotationZ(time * .7f) *
            //                            _viewProjection;
            //_worldViewProjectionMatrix.Transpose();
        }

        public void Render()
        {
            var device = _host.Device;
            if (device == null)
            {
                return;
            }
            for (var i = 0; i < _modelsOnTheScene.Count; i++)
            {
                var modelColor = _modelsOnTheScene[i].ModelColor;
                var worldViewProjectionMatrixForModel = 
                    Matrix.Translation(_modelsOnTheScene[i].ModelPosition) * 
                    Matrix.Scaling(_modelsOnTheScene[i].Scale) * _viewProjection;
                worldViewProjectionMatrixForModel.Transpose();
                device.UpdateSubresource(ref modelColor, _colorBuffer);
                device.UpdateSubresource(ref worldViewProjectionMatrixForModel, _matrixBuffer);
                _modelsOnTheScene[i].Render();
            }
        }

        public void AddModel(ModelBase model)
        {
            _modelsOnTheScene.Add(model);
            model.ConnectDevice(_host.Device);
        }


        public void RemoveModel(ModelBase model)
        {
            _modelsOnTheScene.Remove(model);
        }
    }
}