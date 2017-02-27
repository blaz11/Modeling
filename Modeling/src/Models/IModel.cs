using System;
using SharpDX;

namespace Modeling.Models
{
    public interface IModel
    {
        Color4 ModelColor { get; set; }

        Vector4 ModelPosition { get; set; }

        Vector4[] ModelPoints { get; set; }

        EventHandler ModelUpdated { get; set; }

        Vector4[] GetEdgesForPoint(int pointIndex);

    }
}