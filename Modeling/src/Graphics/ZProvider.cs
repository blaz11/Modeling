using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Modeling.Graphics
{
    public class ZProvider
    {
        private Matrix _dMPrime;

        public ZProvider(Matrix matrix, float a, float b, float c)
        {
            var diagonalM = Matrix.Identity;
            diagonalM.M11 = a;
            diagonalM.M22 = b;
            diagonalM.M33 = c;
            diagonalM.M44 = -1;
            _dMPrime = diagonalM;
            var invertedM = Matrix.Invert(matrix);
            _dMPrime = diagonalM * invertedM;
            _dMPrime = Matrix.Transpose(invertedM) * _dMPrime;
        }

        public Tuple<double,double> GetZ(double x, double y)
        {
            var s0 = _dMPrime.M11 * x * x + _dMPrime.M22 * y * y + 2 * _dMPrime.M12 * x * y + 2 * _dMPrime.M14 * x +
                       2 * _dMPrime.M24 * y + _dMPrime.M44;
            var s2 = _dMPrime.M33;
            var s1 = 2 * _dMPrime.M23 * y + 2 * _dMPrime.M31 * x + 2 * _dMPrime.M34;

            var delta = s1 * s1 - 4 * s0 * s2;
            if (delta < 0)
            {
                return null;
            }
            if (delta == 0)
            {
                var r = (-s1) / (2 * s2);
                return new Tuple<double, double>(r,r);
            }
            var sqrDelta = Math.Sqrt(delta);
            var r1 = (-s1 - sqrDelta) / (2 * s2);
            var r2 = (-s1 + sqrDelta) / (2* s2);
            return new Tuple<double, double>(r1,r2);
        }
    }
}