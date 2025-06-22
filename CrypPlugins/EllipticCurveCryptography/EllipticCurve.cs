/*                              
   Copyright 2025 Nils Kopal, CrypTool Project

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Numerics;

namespace CrypTool.Plugins.EllipticCurveCryptography
{
    public abstract class EllipticCurve
    {
        protected BigInteger _a, _b, _p;

        public BigInteger A => _a;

        public BigInteger B => _b;

        public BigInteger P => _p;

        public abstract bool IsPointOnCurve(Point point);
        public abstract Point Add(Point p, Point q);
        public abstract Point Multiply(BigInteger s, Point p);
    }

    public class WeierstraßCurve : EllipticCurve
    {
        public WeierstraßCurve(BigInteger a, BigInteger b, BigInteger p)
        {
            _a = a;
            _b = b;
            _p = p;
        }

        /// <summary>
        /// Checks, if a given point is on the curve
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public override bool IsPointOnCurve(Point point)
        {
            if (point == null || point.IsInfinity)
            {
                return true;
            }

            if (point.Curve == null)
            {
                throw new ArgumentException("Point has no assigned curve");
            }

            BigInteger x = point.X;
            BigInteger y = point.Y;
            BigInteger a = A;
            BigInteger b = B;
            BigInteger p = P;

            BigInteger lhs = y * y % p;
            BigInteger rhs = (x * x * x + a * x + b) % p;

            if (lhs < 0)
            {
                lhs += p;
            }
            if (rhs < 0)
            {
                rhs += p;
            }

            return lhs == rhs;
        }

        /// <summary>
        /// Adds to points p and q and returns r = p + q
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public override Point Add(Point p, Point q)
        {
            if (!p.Curve.Equals(q.Curve))
            {
                throw new Exception("Points do not share the same curve!");
            }

            if (p.IsInfinity)
            {
                return q;
            }
            if (q.IsInfinity)
            {
                return p;
            }

            if (p.X.Equals(q.X))
            {
                if (!p.Y.Equals(q.Y) || p.Y.Mod(_p).IsZero)
                {
                    return new Point { IsInfinity = true, Curve = p.Curve };
                }
            }

            BigInteger lambda;

            if (p.X.Equals(q.X) && p.Y.Equals(q.Y))
            {
                BigInteger num = p.X.Pow(2).Multiply(3).Add(_a).Mod(_p);
                BigInteger den = p.Y.Multiply(2).ModInverse(_p);
                lambda = num.Multiply(den).Mod(_p);
            }
            else
            {
                BigInteger num = q.Y.Subtract(p.Y).Mod(_p);
                BigInteger den = q.X.Subtract(p.X).ModInverse(_p);
                lambda = num.Multiply(den).Mod(_p);
            }

            BigInteger xR = lambda.Pow(2).Subtract(p.X).Subtract(q.X).Mod(_p);
            BigInteger yR = lambda.Multiply(p.X.Subtract(xR)).Subtract(p.Y).Mod(_p);

            return new Point
            {
                X = xR,
                Y = yR,
                IsInfinity = false,
                Curve = this
            };
        }


        /// <summary>
        /// Multiplies a point p with a scalar s
        /// </summary>
        /// <param name="s"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public override Point Multiply(BigInteger s, Point p)
        {
            s = s.Mod(((WeierstraßCurve)p.Curve).P);

            Point result = new Point { IsInfinity = true, Curve = p.Curve };
            Point addend = p;

            while (s > 0)
            {
                if (!s.And(BigInteger.One).Equals(BigInteger.Zero))
                {
                    result = Add(result, addend);
                }

                addend = Add(addend, addend);
                s = s.ShiftRight(1);
            }

            if (!result.IsInfinity)
            {
                result.X = result.X.Mod(_p);
                result.Y = result.Y.Mod(_p);
            }
            return result;
        }

        /// <summary>
        /// Returns true, if the given curve is equal to this curve
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is WeierstraßCurve c && _a == c._a && _b == c._b && _p == c._p;
        }
    }

    public class MontgomeryCurve : EllipticCurve
    {
        public MontgomeryCurve(BigInteger A, BigInteger B, BigInteger p)
        {
            _a = A.Mod(p);
            _b = B.Mod(p);
            _p = p;
        }

        public override bool IsPointOnCurve(Point P)
        {
            if (P == null || P.IsInfinity)
            {
                return true;
            }

            BigInteger lhs = _b.Multiply(P.Y.Pow(2)).Mod(_p);
            BigInteger rhs = P.X.Pow(3)
                               .Add(_a.Multiply(P.X.Pow(2)))
                               .Add(P.X).Mod(_p);

            return lhs == rhs;
        }

        public override Point Add(Point P, Point Q)
        {
            if (!P.Curve.Equals(Q.Curve))
            {
                throw new Exception("Points not on same curve");
            }

            if (P.IsInfinity)
            {
                return Q;
            }
            if (Q.IsInfinity)
            {
                return P;
            }

            // P = -Q?
            if (P.X == Q.X && (P.Y.Add(Q.Y)).Mod(_p).IsZero)
            {
                return new Point { IsInfinity = true, Curve = this };
            }

            BigInteger lambda;

            if (P.X == Q.X && P.Y == Q.Y)
            {
                // Doubling
                if (P.Y.IsZero)
                {
                    return new Point { IsInfinity = true, Curve = this };
                }

                BigInteger num = P.X.Pow(2).Multiply(3)
                    .Add(_a.Multiply(P.X.Multiply(2)))
                    .Add(BigInteger.One).Mod(_p);

                BigInteger den = _b.Multiply(P.Y).Multiply(2).Mod(_p);
                if (den.IsZero)
                {
                    throw new Exception("Doubling failed: Division by zero");
                }

                lambda = num.Multiply(den.ModInverse(_p)).Mod(_p);
            }
            else
            {
                // Addition
                BigInteger dx = Q.X.Subtract(P.X).Mod(_p);
                if (dx.IsZero)
                {
                    return new Point { IsInfinity = true, Curve = this };
                }

                BigInteger dy = Q.Y.Subtract(P.Y).Mod(_p);
                lambda = dy.Multiply(dx.ModInverse(_p)).Mod(_p);
            }

            BigInteger xR = _b.Multiply(lambda.Pow(2))
                .Subtract(_a)
                .Subtract(P.X)
                .Subtract(Q.X).Mod(_p);

            BigInteger yR = lambda.Multiply(P.X.Subtract(xR))
                .Subtract(P.Y).Mod(_p);

            return new Point { X = xR, Y = yR, IsInfinity = false, Curve = this };
        }

        public override Point Multiply(BigInteger s, Point P)
        {
            s = s.Mod(_p);
            Point R = new Point { IsInfinity = true, Curve = this };
            Point addend = P;

            while (s > 0)
            {
                if (!s.And(BigInteger.One).IsZero)
                {
                    R = Add(R, addend);
                }

                addend = Add(addend, addend);
                s = s >> 1;
            }

            return R;
        }

        public override bool Equals(object obj)
        {
            return obj is MontgomeryCurve c && _a == c._a && _b == c._b && _p == c._p;
        }
    }

    public class TwistedEdwardsCurve : EllipticCurve
    {
        private readonly BigInteger _d, _p;

        public TwistedEdwardsCurve(BigInteger a, BigInteger d, BigInteger p)
        {
            _a = a.Mod(p);
            _d = d.Mod(p);
            _p = p;
        }

        /// <summary>
        /// Returns true if the point P is on the curve
        /// </summary>
        /// <param name="P"></param>
        /// <returns></returns>
        public override bool IsPointOnCurve(Point P)
        {
            if (P == null || P.IsInfinity)
            {
                return true;
            }
            BigInteger lhs = _a.Multiply(P.X.Pow(2)).Add(P.Y.Pow(2)).Mod(_p);
            BigInteger rhs = BigInteger.One.Add(_d.Multiply(P.X.Pow(2)).Multiply(P.Y.Pow(2))).Mod(_p);
            return lhs == rhs;
        }

        private Point AddEd(Point P, Point Q)
        {
            // Twisted Edwards addition formulas (projective-free)
            BigInteger x1 = P.X, y1 = P.Y;
            BigInteger x2 = Q.X, y2 = Q.Y;

            BigInteger denomX = BigInteger.One.Add(
                                 _d.Multiply(x1).Multiply(x2).Multiply(y1).Multiply(y2)).Mod(_p)
                                 .ModInverse(_p);

            BigInteger denomY = BigInteger.One.Subtract(
                                 _d.Multiply(x1).Multiply(x2).Multiply(y1).Multiply(y2)).Mod(_p)
                                 .ModInverse(_p);

            BigInteger x3 = (x1.Multiply(y2).Add(y1.Multiply(x2))).Multiply(denomX).Mod(_p);
            BigInteger y3 = (y1.Multiply(y2).Subtract(_a.Multiply(x1).Multiply(x2)))
                             .Multiply(denomY).Mod(_p);

            return new Point { X = x3, Y = y3, IsInfinity = false, Curve = this };
        }

        /// <summary>
        /// Adds point P and point Q
        /// </summary>
        /// <param name="P"></param>
        /// <param name="Q"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public override Point Add(Point P, Point Q)
        {
            if (!P.Curve.Equals(Q.Curve))
            {
                throw new Exception("Points not on same curve");
            }

            if (P.IsInfinity)
            {
                return Q;
            }
            if (Q.IsInfinity)
            {
                return P;
            }

            // In Edwards' geometry is -P = ( -x , y )
            if (P.X == Q.X.Negate().Mod(_p) && P.Y == Q.Y)
            {
                return new Point { IsInfinity = true, Curve = this };
            }

            return AddEd(P, Q);
        }

        /// <summary>
        /// Multiplies a scalar s with the point P
        /// </summary>
        /// <param name="s"></param>
        /// <param name="P"></param>
        /// <returns></returns>
        public override Point Multiply(BigInteger s, Point P)
        {
            s = s.Mod(_p);
            Point R = new Point { IsInfinity = true, Curve = this };
            Point addend = P;

            while (s > 0)
            {
                if (!s.And(BigInteger.One).Equals(BigInteger.Zero))
                {
                    R = Add(R, addend);
                }

                addend = Add(addend, addend);
                s = s.ShiftRight(1);
            }
            return R;
        }

        /// <summary>
        /// Returns true, if both curves are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is TwistedEdwardsCurve c && _a == c._a && _d == c._d && _p == c._p;
        }
    }
}