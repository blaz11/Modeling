using System;
using SharpDX;

namespace Modeling.Models.SimpleModel
{
    public class SimpleGrid : IModel
    {
        public Color4 ModelColor { get; set; }

        public Vector4[] ModelPoints { get; set; }

        public EventHandler ModelUpdated { get; set; }

        public Vector4 ModelPosition { get; set; }

        private int[][] _edgesTable;

        public Vector4[] GetEdgesForPoint(int pointIndex)
        {
            var result = new Vector4[2];
            result[0] = ModelPoints[pointIndex];
            result[1] = ModelPoints[_edgesTable[pointIndex][0]];
            return result;
        }


        public SimpleGrid()
        {
            const float size = 1.5f;
            ModelPoints = new[]
            {
                new Vector4(-size, 0.0f, 0.0f, 1.0f),
                new Vector4(size, 0.0f, 0.0f, 1.0f),
                new Vector4(size, size, 0.0f, 1.0f),
                new Vector4(-size, size, 0.0f, 1.0f),
            };

            _edgesTable = new int[ModelPoints.Length][];

            for (var i = 0; i < ModelPoints.Length; i++)
            {
                _edgesTable[i] = new int[1];

                if (i + 1 < ModelPoints.Length)
                {
                    _edgesTable[i][0] = i + 1;
                }
                else
                {
                    _edgesTable[i][0] = 0;
                }
            }
        }
    }
}