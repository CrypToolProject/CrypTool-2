using System.Numerics;
using System.Windows.Shapes;

namespace LatticeCrypto.Models
{
    public class LatticePoint
    {
        public BigInteger logicX;
        public BigInteger logicY;
        public Ellipse ellipse;

        public LatticePoint(BigInteger logicX, BigInteger logicY)
        {
            this.logicX = logicX;
            this.logicY = logicY;
        }
    }
}
