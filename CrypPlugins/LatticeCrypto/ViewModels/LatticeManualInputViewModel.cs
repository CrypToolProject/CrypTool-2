using LatticeCrypto.Models;
using System.Linq;
using System.Numerics;
using System.Windows.Controls;

namespace LatticeCrypto.ViewModels
{
    public class LatticeManualInputViewModel : BaseViewModel
    {
        public LatticeND Lattice { get; set; }

        public LatticeManualInputViewModel()
        {
            Lattice = new LatticeND(2, 2, false);
            NotifyPropertyChanged("Lattice");
        }

        public void NewLattice(int n, int m)
        {
            Lattice = new LatticeND(n, m, false);
            NotifyPropertyChanged("Lattice");
        }

        public LatticeND SetLattice(Grid grid, bool useRowVectors)
        {
            for (int i = 0; i < Lattice.N; i++)
            {
                Lattice.Vectors[i] = new VectorND(Lattice.M);
            }

            foreach (TextBox control in grid.Children.OfType<TextBox>())
            {
                if (!useRowVectors)
                {
                    Lattice.Vectors[Grid.GetColumn(control) / 2].values[Grid.GetRow(control)] = string.IsNullOrEmpty(control.Text) ? 0 : BigInteger.Parse(control.Text);
                }
                else
                {
                    Lattice.Vectors[Grid.GetRow(control) / 2].values[Grid.GetColumn(control)] = string.IsNullOrEmpty(control.Text) ? 0 : BigInteger.Parse(control.Text);
                }
            }

            if (Lattice.N == Lattice.M)
            {
                Lattice.Determinant = Lattice.CalculateDeterminant(Lattice.Vectors);
            }
            //Lattice.AngleBasisVectors = Lattice.Vectors[0].AngleBetween(Lattice.Vectors[1]);
            Lattice.UseRowVectors = useRowVectors;
            return Lattice;
        }
    }
}
