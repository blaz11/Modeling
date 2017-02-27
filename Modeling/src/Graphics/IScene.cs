using System;
using Modeling.Models;

namespace Modeling.Graphics
{
    public interface IScene
    {
        void AddModel(IModel model);
        void RemoveModel(IModel model);

        void Attach(ISceneHost host);
        void Detach();
        void Update(TimeSpan timeSpan);
        void Render();
    }
}