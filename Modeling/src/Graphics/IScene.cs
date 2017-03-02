using System;
using Modeling.Models;
using SharpDX;

namespace Modeling.Graphics
{
    public interface IScene
    {
        void AddModel(ModelBase model);
        void RemoveModel(ModelBase model);
        void SetupViewMatrix(Vector3 eye, Vector3 target, Vector3 up);

        void Attach(ISceneHost host);
        void Detach();
        void Update(TimeSpan timeSpan);
        void Render();
    }
}