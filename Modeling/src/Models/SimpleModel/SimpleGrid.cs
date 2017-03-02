using System.Collections.Generic;
using SharpDX;

namespace Modeling.Models.SimpleModel
{
    public class SimpleGrid : ModelBase
    {
        public SimpleGrid()
        {
            const float size = 0.5f;
            Vertices = new List<Vector4>()
            {
                new Vector4(-size, 0.0f, 0.0f, 1.0f),
                new Vector4(size, 0.0f, 0.0f, 1.0f),
                new Vector4(size, size / 2, 0.0f, 1.0f),
                new Vector4(-size, size / 2, 0.0f, 1.0f),
            };

            ModelColor = new Color4(255.0f, 0.0f, 0.0f, 0.0f);

            Indices = new List<uint>()
            {
                0, 1,
                1, 2,
                2, 3,
                3, 0
            };
        }
    }
}