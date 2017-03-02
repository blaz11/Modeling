using SharpDX;
using System;
using System.Collections.Generic;

namespace Modeling.Models
{
    public class Torus : ModelBase
    {
        private float _bigR;
        private float _smallR;
        private uint _t;

        public float BigR
        {
            get
            {
                return _bigR;
            }
            set
            {
                _bigR = value;
                OnPropertyChanged();
                GenerateTorus();
            }
        }

        public float SmallR
        {
            get
            {
                return _smallR;
            }
            set
            {
                _smallR = value;
                OnPropertyChanged();
                GenerateTorus();
            }
        }

        public uint T
        {
            get
            {
                return _t;
            }
            set
            {
                _t = value;
                OnPropertyChanged();
                GenerateTorus();
            }
        }

        public Torus(string name) : base(name)
        {
            Shape = "Torus";
            _bigR = 4;
            _smallR = 2;
            _t = 50;
            ModelColor = new Color4(0, 0, 0, 1.0f);
            ModelPosition = new Vector3(0.0f, 0.0f, 0.0f);
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
            Vertices = new List<Vector4>();
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
            Indices = new List<uint>();
            for (uint i = 0; i < T; i++)
            {
                // j = 0
                Indices.Add(i);
                for (uint j = 1; j < T - 1; j++)
                {
                    Indices.Add(i + j * T);
                    Indices.Add(i + j * T);
                }
                // j = T - 1
                Indices.Add(i + (T - 1) * T);
                // First to last
                Indices.Add(i);
                Indices.Add(i + (T - 1) * T);

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