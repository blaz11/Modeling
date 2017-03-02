using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Modeling.Models.Torus
{
    public class Torus : ModelBase
    {
        public float BigR { get; set; }
        public float SmallR { get; set; }
        public uint T { get; set; }

        public Torus()
        {
            ModelColor = new Color4(128.0f, 0.0f, 0.0f, 1.0f);
            BigR = 4;
            SmallR = 2;
            T = 500;
            Vertices = new List<Vector4>();
            Indices = new List<uint>();
            GenerateTorus();
        }

        private void GenerateTorus()
        {
            GenerateTorusVertices();
            GenerateTorusIndices();
            SetupForRendering();
        }

        private void GenerateTorusVertices()
        {
            Vertices.Clear();
            var step = MathUtil.TwoPi / T;
            for (float alfa = 0; alfa <= MathUtil.TwoPi; alfa += step)
            {
                var bigRPlussmallRCosAlfa = BigR + Math.Cos(alfa) * SmallR;
                for (float beta = 0; beta <= MathUtil.TwoPi; beta += step)
                {
                    var x = bigRPlussmallRCosAlfa * Math.Cos(beta);
                    var y = bigRPlussmallRCosAlfa * Math.Sin(beta);
                    var z = SmallR * Math.Sin(alfa);
                    Vertices.Add(new Vector4((float)x, (float)y, (float)z, 1.0f));
                }
            }
        }

        private void GenerateTorusIndices()
        {
            Indices.Clear();
            for (uint i = 0; i < T; i++)
            {
                for (uint j = 0; j < T; j++)
                {
                    Indices.Add(i + j * T);
                }
                uint i1;
                if (i + 1 < T)
                {
                    i1 = i + 1;

                }
                else if (i != 0)
                {
                    i1 = 0;
                }
                else
                {
                    continue;
                }
                for (uint j = 0; j < T; j++)
                {
                    Indices.Add(i + j * T);
                    Indices.Add(i1 + j * T);
                }
            }
        }
    }
}