using SharpDX.Direct3D10;

namespace Modeling.Graphics
{
    public interface ISceneHost
    {
        Device Device { get; }
    }
}