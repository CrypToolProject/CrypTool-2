using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Windows.Controls;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Control;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.PluginBase.IO;

namespace CrypTool.CubeAttack
{
    [Author("David Oruba", "david.oruba@web.de", "Uni-Bochum", "http://www.ruhr-uni-bochum.de/")]
    [PluginInfo("CrypTool.CubeAttack.Properties.Resources", "PluginCaption", "PluginTooltip", "CubeAttack/DetailedDescription/doc.xml", "CubeAttack/Images/ca_color.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class CubeAttack : ICrypComponent
    {
        #region Private variables

        private CubeAttackSettings settings;
        private string outputKeyBits;
        private enum CubeAttackMode { preprocessing, online, setPublicBits };
        private bool stop = false;
        
        #endregion


        #region Public variables

        public int[] pubVarGlob = null;
        public int indexOutputBit = 1;

        public string outputSuperpoly = null;
        public Matrix superpolyMatrix = null;
        public List<List<int>> listCubeIndexes = null;
        public int[] outputBitIndex = null;
        public int countSuperpoly = 0;
        public Matrix matrixCheckLinearitySuperpolys = null;

        #endregion


        #region Properties (Inputs/Outputs)
        
        [PropertyInfo(Direction.OutputData, 
            "OutputSuperpolyCaption", "OutputSuperpolyTooltip",
            false)]
        public ICrypToolStream OutputSuperpoly
        {
            get
            {
                if (outputSuperpoly != null)
                {
                    return new CStreamWriter(Encoding.UTF8.GetBytes(outputSuperpoly));
                }
                else
                {
                    return null;
                }
            }
            set { }
        }

        [PropertyInfo(Direction.OutputData, 
            "OutputKeyBitsCaption", "OutputKeyBitsTooltip", 
            false)]
        public ICrypToolStream OutputKeyBits
        {
            get
            {
                if (outputKeyBits != null)
                {
                    return new CStreamWriter(Encoding.UTF8.GetBytes(outputKeyBits));
                }
                else
                {
                    return null;
                }
            }
            set { }
        }

        #endregion


        #region Public interface

        /// <summary>
        /// Contructor
        /// </summary>
        public CubeAttack()
        {
            this.settings = new CubeAttackSettings();
        }

        /// <summary>
        /// Get or set all settings for this algorithm
        /// </summary>
        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (CubeAttackSettings)value; }
        }      

        public void Preprocessing()
        {
            ProcessCubeAttack(CubeAttackMode.preprocessing);
        }

        public void Online()
        {
            ProcessCubeAttack(CubeAttackMode.online);
        }

        public void SetPublicBits()
        {
            ProcessCubeAttack(CubeAttackMode.setPublicBits);
        }

        #endregion


        #region IPlugin members

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Fire, if progress bar has to be updated
        /// </summary>
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        /// <summary>
        /// Fire, if new message has to be shown in the status bar
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public UserControl Presentation
        {
            get { return null; }
        }

        public void Stop()
        {
            this.stop = true;
            if (settings.Action == 0) // Action = Preprocessing
            {
                settings.SaveOutputSuperpoly = outputSuperpoly; 
                settings.SaveSuperpolyMatrix = superpolyMatrix;
                settings.SaveListCubeIndexes = listCubeIndexes;
                settings.SaveOutputBitIndex = outputBitIndex;
                settings.SaveCountSuperpoly = countSuperpoly;
                settings.SaveMatrixCheckLinearitySuperpolys = matrixCheckLinearitySuperpolys;
                settings.SavePublicBitSize = settings.PublicVar;
                settings.SaveSecretBitSize = settings.SecretVar;
            }
        }

        public void PostExecution()
        {
            stop = false;
        }

        public void PreExecution()
        {
            stop = false;
            if (settings.Action == 0) // Action = Preprocessing
            { 
                if ((settings.PublicVar != settings.SavePublicBitSize) || (settings.SecretVar != settings.SaveSecretBitSize))
                {
                    settings.SaveCountSuperpoly = 0;
                    settings.SaveListCubeIndexes = null;
                    settings.SaveMatrixCheckLinearitySuperpolys = null;
                    settings.SaveOutputBitIndex = null;
                    settings.SaveOutputSuperpoly = null;
                    settings.SaveSuperpolyMatrix = null;

                    outputSuperpoly = string.Empty;
                    superpolyMatrix = new Matrix(settings.SecretVar, settings.SecretVar + 1);
                    listCubeIndexes = new List<List<int>>();
                    outputBitIndex = new int[settings.SecretVar];
                    countSuperpoly = 0;
                    matrixCheckLinearitySuperpolys = new Matrix(0, settings.SecretVar);
                }

                if (settings.SaveCountSuperpoly != settings.SecretVar)
                {
                    if (settings.SaveOutputSuperpoly != null)
                        outputSuperpoly = settings.SaveOutputSuperpoly;
                    if (settings.SaveSuperpolyMatrix != null)
                        superpolyMatrix = settings.SaveSuperpolyMatrix;
                    if (settings.SaveListCubeIndexes != null)
                        listCubeIndexes = settings.SaveListCubeIndexes;
                    if (settings.SaveOutputBitIndex != null)
                        outputBitIndex = settings.SaveOutputBitIndex;
                    if (settings.SaveCountSuperpoly != 0)
                        countSuperpoly = settings.SaveCountSuperpoly;
                    if (settings.SaveMatrixCheckLinearitySuperpolys != null)
                        matrixCheckLinearitySuperpolys = settings.SaveMatrixCheckLinearitySuperpolys;
                }
            }
        }

        #pragma warning disable 67
		public event StatusChangedEventHandler OnPluginStatusChanged;
        #pragma warning restore

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public void Execute()
        {
            if (settings.MaxCube > settings.PublicVar)
                CubeAttack_LogMessage("Error: Max cube size cannot be greater than public bit size.", NotificationLevel.Error);
            else
            {
                try
                {
                    switch (settings.Action)
                    {
                        case 0:
                            Preprocessing();
                            break;
                        case 1:
                            Online();
                            break;
                        case 2:
                            SetPublicBits();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    CubeAttack_LogMessage("Error: " + ex, NotificationLevel.Error);
                }
                finally
                {
                    ProgressChanged(1.0, 1.0);
                }
            }
        }

        /// <summary>
        /// The function returns the black box output bit.
        /// </summary>
        /// <param name="v">Public variables.</param>
        /// <param name="x">Secret variables.</param>
        /// <returns>Returns the black box output bit, either 0 or 1.</returns>
        public int Blackbox(int[] v, int[] x)
        {
            int result = 0;
            try
            {
                result = CubeattackBlackbox.GenerateBlackboxOutputBit(v, x, indexOutputBit);
            }
            catch (Exception ex)
            {
                stop = true;
                CubeAttack_LogMessage("Error: " + ex, NotificationLevel.Error);
            }
            return result; 
        }

        /// <summary>
        /// The function derives the algebraic structure of the superpoly from the maxterm.
        /// The structure is derived by computing the free term and the coefficients in the superpoly.
        /// </summary>
        /// <param name="pubVarElement">Public variables.</param>
        /// <param name="maxterm">Maxterm.</param>
        /// <returns>Returns the superpoly.</returns>
        public List<int> ComputeSuperpoly(int[] pubVarElement, List<int> maxterm)
        {
            int constant = 0;
            int coeff = 0;
            List<int> superpoly = new List<int>();
            int[] secVarElement = new int[settings.SecretVar];

            if (settings.EnableLogMessages)
                CubeAttack_LogMessage("Start deriving the algebraic structure of the superpoly", NotificationLevel.Info);

            // Compute the free term
            for (ulong i = 0; i < Math.Pow(2, maxterm.Count); i++)
            {
                if (stop)
                    return null;
                for (int j = 0; j < maxterm.Count; j++)
                    pubVarElement[maxterm[j]] = (i & ((ulong)1 << j)) > 0 ? 1 : 0;
                constant ^= Blackbox((int[])pubVarElement.Clone(), (int[])secVarElement.Clone());
            }
            superpoly.Add(constant);

            if (settings.EnableLogMessages)
                CubeAttack_LogMessage("Constant term = " + (constant).ToString(), NotificationLevel.Info);

            // Compute coefficients
            for (int k = 0; k < settings.SecretVar; k++)
            {
                for (ulong i = 0; i < Math.Pow(2, maxterm.Count); i++)
                {
                    if (stop)
                        return null;
                    secVarElement[k] = 1;
                    for (int j = 0; j < maxterm.Count; j++)
                        pubVarElement[maxterm[j]] = (i & ((ulong)1 << j)) > 0 ? 1 : 0;
                    coeff ^= Blackbox((int[])pubVarElement.Clone(), (int[])secVarElement.Clone());
                }
                superpoly.Add(constant ^ coeff);

                if (settings.EnableLogMessages)
                    CubeAttack_LogMessage("Coefficient of x" + k + " = " + (constant ^ coeff), NotificationLevel.Info);
                
                coeff = 0;
                secVarElement[k] = 0;
            }
            return superpoly;
        }

        string SuperpolyAsString(List<int> superpoly)
        {
            List<string> sp = new List<string>();

            for (int i = 0; i < superpoly.Count; i++)
                if (superpoly[i] == 1) sp.Add(i == 0 ? "1" : "x" + (i - 1));

            return (sp.Count == 0) ? "0" : string.Join("+", sp);
        }

        string GetLogMessage(List<int> cubeIndexes, List<int> superpoly, int indexOutputBit, int? value=null)
        {
            cubeIndexes.Sort();

            return "Superpoly: " + SuperpolyAsString(superpoly) + ((value != null) ? " = " + value : "") +
                " \tCube indexes: {" + string.Join(",", cubeIndexes) + "}" +
                " \tOutput bit: " + indexOutputBit + "\n";
        }

        /// <summary>
        /// The function outputs the superpolys, cube indexes and output bits.
        /// </summary>
        /// <param name="cubeIndexes">The cube indexes of the maxterm.</param>
        /// <param name="superpoly">The superpoly for the given cube indexes.</param>
        public void OutputSuperpolys(List<int> cubeIndexes, List<int> superpoly)
        {
            outputSuperpoly += GetLogMessage(cubeIndexes, superpoly, indexOutputBit);
            OnPropertyChanged("OutputSuperpoly");
        }

        /// <summary>
        /// The function outputs the key bits.
        /// </summary>
        /// <param name="res">Result vector</param>
        public void OutputKey(Vector res)
        {
            StringBuilder output = new StringBuilder(string.Empty);
            for (int i=0; i<res.Length; i++)
                output.AppendLine("x" + i + " = " + res[i]);
            outputKeyBits = output.ToString();
        }

        /// <summary>
        /// Test if superpoly is already in matrix.
        /// </summary>
        /// <param name="superpoly">The superpoly.</param>
        /// <param name="matrix">An n x n matrix whose rows contain their corresponding superpolys.</param>
        /// <returns>A boolean value indicating if the superpoly is in the matrix or not.</returns>
        public bool InMatrix(List<int> superpoly, Matrix matrix)
        {
            bool isEqual = true;
            for (int i = 0; i < matrix.Rows; i++)
            {
                isEqual = true;
                for (int j = 0; j < superpoly.Count; j++)
                    if (matrix[i, j] != superpoly[j])
                        isEqual = false;
                if (isEqual)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Test if a maxterm is already known.
        /// </summary>
        /// <param name="cubeList">A list of cube indexes.</param>
        /// <param name="maxterm">The located maxterm.</param>
        /// <returns>A boolean value indicating if the maxterm is already in the list of cubes indexes or not.</returns>
        public bool MaxtermKnown(List<List<int>> cubeList, List<int> maxterm)
        {
            bool isEqual = true;
            for (int i = 0; i < cubeList.Count; i++)
            {
                isEqual = true;
                if (cubeList[i].Count == maxterm.Count)
                {
                    for (int j = 0; j < maxterm.Count; j++)
                        if (!cubeList[i].Contains(maxterm[j]))
                            isEqual = false;
                    if (isEqual)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Test if a superpoly is linear (BLR linearity test).
        /// </summary>
        /// <param name="pubVarElement">Public variables.</param>
        /// <param name="maxterm">The located maxterm.</param>
        /// <returns>A boolean value indicating if the superpoly is probably linear or not.</returns>
        public bool IsSuperpolyLinear(int[] pubVarElement, List<int> maxterm)
        {
            Random rnd = new Random();
            int psLeft = 0;
            int psRight = 0;
            int[] vectorX = new int[settings.SecretVar];
            int[] vectorY = new int[settings.SecretVar];
            int[] vecXY = new int[settings.SecretVar];

            for (int k = 0; k < settings.LinTest; k++)
            {
                if (settings.EnableLogMessages)
                    CubeAttack_LogMessage("Linearity test " + (k + 1) + " of " + settings.LinTest, NotificationLevel.Info);
                
                psLeft = 0;
                psRight = 0;

                // Choose vectors x and y at random
                for (int i = 0; i < settings.SecretVar; i++)
                {
                    vectorX[i] = rnd.Next(0, 2);
                    vectorY[i] = rnd.Next(0, 2);
                }

                pubVarElement = new int[settings.PublicVar];
                for (int i = 0; i < settings.SecretVar; i++)
                    vecXY[i] = (vectorX[i] ^ vectorY[i]);

                for (ulong i = 0; i < Math.Pow(2, maxterm.Count); i++)
                {
                    if (stop)
                        return false;
                    for (int j = 0; j < maxterm.Count; j++)
                        pubVarElement[maxterm[j]] = (i & ((ulong)1 << j)) > 0 ? 1 : 0;
                    psLeft ^= Blackbox((int[])pubVarElement.Clone(), new int[settings.SecretVar]) 
                            ^ Blackbox((int[])pubVarElement.Clone(), (int[])vectorX.Clone()) 
                            ^ Blackbox((int[])pubVarElement.Clone(), (int[])vectorY.Clone());
                    psRight ^= Blackbox((int[])pubVarElement.Clone(), (int[])vecXY.Clone());
                }
                if (psLeft != psRight)
                {
                    if (settings.EnableLogMessages)
                        CubeAttack_LogMessage("Linearity test " + (k + 1) + " failed", NotificationLevel.Info);
                    return false;
                }
                if (stop)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Test if superpoly is a constant value.
        /// </summary>
        /// <param name="pubVarElement">Public variables.</param>
        /// <param name="maxterm">The located maxterm.</param>
        /// <returns>A boolean value indicating if the superpoly is constant or not.</returns>
        public bool IsSuperpolyConstant(int[] pubVarElement, List<int> maxterm)
        {
            Random rnd = new Random();
            int[] vectorX = new int[settings.SecretVar];
            int flag = 0;
            int output = 0;
            int[] secVarElement = new int[settings.SecretVar];

            string outputCube = string.Empty;
            foreach (int element in maxterm)
                outputCube += "v" + element + " ";
            if(settings.ConstTest > 0)
                if (settings.EnableLogMessages)
                    CubeAttack_LogMessage("Test if superpoly of subset " + outputCube + " is constant", NotificationLevel.Info);
            for (int i = 0; i < settings.ConstTest; i++)
            {
                for (int j = 0; j < settings.SecretVar; j++)
                    vectorX[j] = rnd.Next(0, 2);
                for (ulong j = 0; j < Math.Pow(2, maxterm.Count); j++)
                {
                    if (stop)
                        return false;
                    for (int k = 0; k < maxterm.Count; k++)
                        pubVarElement[maxterm[k]] = (j & ((ulong)1 << k)) > 0 ? 1 : 0;
                    output ^= Blackbox(pubVarElement, vectorX);
                }
                if (i == 0)
                    flag = output;
                if (flag != output)
                {
                    if (settings.EnableLogMessages)
                        CubeAttack_LogMessage("Superpoly of subset " + outputCube + " is not constant", NotificationLevel.Info);
                    return false;
                }
                output = 0;
                if (stop)
                    return false;
            }
            if (settings.ConstTest > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Generates a random permutation of a finite set—in plain terms, for randomly shuffling the set.
        /// </summary>
        /// <param name="ilist">A List of values.</param>
        public static void Shuffle(List<int> ilist)
        {
            Random rand = new Random();
            int iIndex;
            int tTmp;
            for (int i = 1; i < ilist.Count; ++i)
            {
                iIndex = rand.Next(i + 1);
                tTmp = ilist[i];
                ilist[i] = ilist[iIndex];
                ilist[iIndex] = tTmp;
            }
        }

        /// <summary>
        /// Test if an n x m matrix contains n linearly independent vectors.
        /// </summary>
        /// <param name="A">n x m matrix.</param>
        /// <returns>A boolean value indicating if the matrix is regular or not.</returns>
        public bool IsLinearIndependent(Matrix A)
        {
            double maxval;
            int maxind;
            double temp;
            int Rang = 0;
            double[,] a = new double[A.Cols, A.Rows];

            for (int i = 0; i < A.Cols; i++)
                for (int j = 0; j < A.Rows; j++)
                    a[i, j] = A[j, i];

            for (int j = 0; j < A.Rows; j++)
            {
                // Find maximum
                maxval = a[j, j];
                maxind = j;
                for (int k = j; k < A.Cols; k++)
                {
                    if (a[k, j] > maxval)
                    {
                        maxval = a[k, j];
                        maxind = k;
                    }
                    if (-a[k, j] > maxval)
                    {
                        maxval = -a[k, j];
                        maxind = k;
                    }
                }

                if (maxval != 0)
                {
                    Rang++;
                    // Swap_Rows(j, maxind)
                    for (int k = j; k < A.Rows; k++)
                    {
                        temp = a[j, k];
                        a[j, k] = a[maxind, k];
                        a[maxind, k] = temp;
                    }

                    // Gauss elimination 
                    for (int i = j + 1; i < A.Cols; i++)
                        for (int k = j + 1; k < A.Rows; k++)
                            a[i, k] = a[i, k] - (a[i, j] / a[j, j] * a[j, k]);
                }
            }

            return A.Rows == Rang;
        }  

        /// <summary>
        /// Preprocessing phase of the cube attack. 
        /// </summary>
        public void PreprocessingPhase()
        {
            indexOutputBit = settings.OutputBit;
            pubVarGlob = null;

            if (countSuperpoly == settings.SecretVar)
            {
                outputSuperpoly = string.Empty;
                superpolyMatrix = new Matrix(settings.SecretVar, settings.SecretVar + 1);
                listCubeIndexes = new List<List<int>>();
                outputBitIndex = new int[settings.SecretVar];
                countSuperpoly = 0;
                matrixCheckLinearitySuperpolys = new Matrix(0, settings.SecretVar);
            }

            if (outputSuperpoly == null)
                outputSuperpoly = string.Empty;
            if (superpolyMatrix == null)
                superpolyMatrix = new Matrix(settings.SecretVar, settings.SecretVar + 1);
            if (listCubeIndexes == null)
                listCubeIndexes = new List<List<int>>();
            if (outputBitIndex == null)
                outputBitIndex = new int[settings.SecretVar];
            if (matrixCheckLinearitySuperpolys == null)
                matrixCheckLinearitySuperpolys = new Matrix(0, settings.SecretVar);

            CubeAttack_LogMessage("Start preprocessing: \nTry to find " + settings.SecretVar + " linearly independent superpolys. (Already found: " + countSuperpoly + ")", NotificationLevel.Info);

            Random rnd = new Random();
            int numberOfVariables = 0;
            List<int> chooseIndexI = new List<int>();
            List<int> superpoly = new List<int>();
            List<int> maxterm = new List<int>();
            List<List<int>> cubeList = new List<List<int>>();
            string outputCube = string.Empty;

            if (countSuperpoly > 0)
            {
                OnPropertyChanged("OutputSuperpoly");
                ProgressChanged((double)countSuperpoly / (double)settings.SecretVar, 1.0);
            }

            // Save all public variables indexes in a list 
            for (int i = 0; i < settings.PublicVar; i++)
                chooseIndexI.Add(i);

            // Find n maxterms and save their in the matrix
            while (countSuperpoly < settings.SecretVar)
            {
                if (stop)
                    return;
                else
                {
                    maxterm = new List<int>();
                    superpoly.Clear();

                    // Generate random size k between 1 and the number of public variables
                    numberOfVariables = rnd.Next(1, settings.MaxCube + 1);

                    // Permutation of the public variables
                    Shuffle(chooseIndexI);

                    // Construct cube of size k. Add k public variables to the cube
                    for (int i = 0; i < numberOfVariables; i++)
                        maxterm.Add(chooseIndexI[i]);

                    if (settings.EnableLogMessages)
                    {
                        outputCube = string.Empty;
                        foreach (int element in maxterm)
                            outputCube += "v" + element + " ";
                        CubeAttack_LogMessage("Start search for maxterm with subterm: " + outputCube, NotificationLevel.Info);
                    }
                    if (settings.OutputBit != indexOutputBit)
                    {
                        // User has changed Output Bit index, store new value
                        indexOutputBit = settings.OutputBit;

                        // Reset list of cube indexes, since a single maxterms can be associated with multiple superpolys from different outputs
                        cubeList = new List<List<int>>();
                    }
                    while (superpoly.Count == 0)
                    {
                        if (maxterm.Count == 0)
                        {
                            if (numberOfVariables < chooseIndexI.Count)
                            {
                                if (settings.EnableLogMessages)
                                    CubeAttack_LogMessage("Subset is empty, add variable v" + chooseIndexI[numberOfVariables], NotificationLevel.Info);
                                maxterm.Add(chooseIndexI[numberOfVariables]);
                                numberOfVariables++;
                            }
                            else
                                break;
                        }
                        if (MaxtermKnown(cubeList, maxterm))
                        {
                            // Maxterm is already known, break and restart with new subset
                            if (settings.EnableLogMessages)
                            {
                                outputCube = string.Empty;
                                foreach (int element in maxterm)
                                    outputCube += "v" + element + " ";
                                CubeAttack_LogMessage("Maxterm " + outputCube + " is already known, restart with new subset", NotificationLevel.Info);
                            }
                            break;
                        }
                        if (IsSuperpolyConstant(new int[settings.PublicVar], maxterm))
                        {
                            if (stop)
                                return;
                            else
                            {
                                if (settings.EnableLogMessages)
                                    CubeAttack_LogMessage("Superpoly is likely constant, drop variable v" + maxterm[0], NotificationLevel.Info);
                                maxterm.RemoveAt(0);
                            }
                        }
                        else if (!IsSuperpolyLinear(new int[settings.PublicVar], maxterm))
                        {
                            if (stop)
                                return;
                            else
                            {
                                if (settings.EnableLogMessages)
                                    CubeAttack_LogMessage("Superpoly is not linear", NotificationLevel.Info);
                                if (numberOfVariables < chooseIndexI.Count)
                                {
                                    if (maxterm.Count < settings.MaxCube)
                                    {
                                        if (settings.EnableLogMessages)
                                            CubeAttack_LogMessage("Add variable v" + chooseIndexI[numberOfVariables], NotificationLevel.Info);
                                        maxterm.Add(chooseIndexI[numberOfVariables]);
                                        numberOfVariables++;
                                    }
                                    else
                                        break;
                                }
                                else
                                    break;
                            }
                        }
                        else
                        {
                            if (stop)
                                return;
                            else
                            {
                                cubeList.Add(maxterm);
                                if (settings.EnableLogMessages)
                                {
                                    outputCube = string.Empty;
                                    foreach (int element in maxterm)
                                        outputCube += "v" + element + " ";
                                    CubeAttack_LogMessage(outputCube + " is new maxterm", NotificationLevel.Info);
                                    outputCube = string.Empty;
                                }
                                superpoly = ComputeSuperpoly(new int[settings.PublicVar], maxterm);
                                if (stop) return;

                                outputCube += GetLogMessage(maxterm, superpoly, indexOutputBit);

                                if (settings.EnableLogMessages)
                                    CubeAttack_LogMessage(outputCube, NotificationLevel.Info);
                                break;
                            }
                        }
                    }//End while (superpoly.Count == 0)

                    if (!InMatrix(superpoly, superpolyMatrix))
                    {
                        List<int> superpolyWithoutConstant = new List<int>();
                        for (int i = 1; i < superpoly.Count; i++)
                            superpolyWithoutConstant.Add(superpoly[i]);

                        matrixCheckLinearitySuperpolys = matrixCheckLinearitySuperpolys.AddRow(superpolyWithoutConstant);
                        if (IsLinearIndependent(matrixCheckLinearitySuperpolys))
                        {
                            for (int j = 0; j < superpoly.Count; j++)
                                superpolyMatrix[countSuperpoly, j] = superpoly[j];
                            listCubeIndexes.Add(maxterm);
                            outputBitIndex[countSuperpoly] = indexOutputBit;
                            if (stop)
                                return;
                            countSuperpoly++;
                            OutputSuperpolys(maxterm, superpoly);
                            CubeAttack_LogMessage("Found " + countSuperpoly + " of " + settings.SecretVar + " linearly independent superpolys", NotificationLevel.Info);
                            ProgressChanged((double)countSuperpoly / (double)settings.SecretVar, 1.0);
                        }
                        else
                            matrixCheckLinearitySuperpolys = matrixCheckLinearitySuperpolys.DeleteLastRow();
                    }
                    if (countSuperpoly == settings.SecretVar)
                        CubeAttack_LogMessage(settings.SecretVar + " linearly independent superpolys have been found, preprocessing phase completed", NotificationLevel.Info);
                }
            }//End while (countSuperpoly < settings.SecretVar)
        }//End PreprocessingPhase

        /// <summary>
        /// Online phase of the cube attack.
        /// </summary>
        public void OnlinePhase()
        {   
            if (settings.ReadSuperpolysFromFile)
            {
                if (File.Exists(settings.OpenFilename))
                {
                    CubeAttack_LogMessage("Read superpolys from file !", NotificationLevel.Info);
                    superpolyMatrix = new Matrix(settings.SecretVar, settings.SecretVar + 1);
                    listCubeIndexes = new List<List<int>>();
                    outputBitIndex = new int[settings.SecretVar];

                    int i = 0;
                    foreach (string sLine in File.ReadAllLines(settings.OpenFilename))
                    {
                        string[] allValues = sLine.Split(' ');
                        string[] variables = allValues[0].Split('+');
                        string[] cubeIndex = allValues[1].Split(',');

                        List<string> variablesList = new List<string>(variables); // Copy to List

                        for (int j = 0; j < variablesList.Count; j++)
                        {
                            if (variablesList[j].Substring(0, 1) == "1")
                            {
                                superpolyMatrix[i, 0] = 1;
                                variablesList.Remove(variablesList[j]);
                            }
                        }
                        for (int j = 0; j < variablesList.Count; j++)
                            if (variablesList[j].Substring(0, 1) == "x")
                                variablesList[j] = variablesList[j].Substring(1);

                        List<int> superpoly = new List<int>();
                        for (int j = 0; j < variablesList.Count; j++)
                            superpoly.Add(Convert.ToInt32(variablesList[j]));
                        for (int j = 0; j < superpoly.Count; j++)
                            superpolyMatrix[i, superpoly[j] + 1] = 1;

                        List<int> maxterm = new List<int>();
                        foreach (string cube in cubeIndex)
                            maxterm.Add(Convert.ToInt32(cube));
                        listCubeIndexes.Add(maxterm);

                        outputBitIndex[i] = Convert.ToInt32(allValues[2]);
                        i++;
                        // Save number of input superpolys
                        countSuperpoly = i;
                    }
                }
                else
                {
                    CubeAttack_LogMessage("Please input a File", NotificationLevel.Error);
                    return;
                }
            }
            if (superpolyMatrix == null || listCubeIndexes == null)
                CubeAttack_LogMessage("Preprocessing phase has to be executed first", NotificationLevel.Error);
            else
            {
                CubeAttack_LogMessage("Start online phase", NotificationLevel.Info);
                outputSuperpoly = string.Empty;
                int[] pubVarElement = new int[settings.PublicVar];

                if (pubVarGlob != null)
                {
                    for (int i = 0; i < settings.PublicVar; i++)
                        pubVarElement[i] = pubVarGlob[i];
                }
                Vector b = new Vector(settings.SecretVar);

                for (int i = 0; i < listCubeIndexes.Count; i++)
                {
                    List<int> superpoly = new List<int>();
                    for (int j = 1; j < superpolyMatrix.Cols; j++)
                        superpoly.Add(superpolyMatrix[i, j]);

                    CubeAttack_LogMessage("Compute value of superpoly " + SuperpolyAsString(superpoly), NotificationLevel.Info);

                    for (ulong k = 0; k < Math.Pow(2, listCubeIndexes[i].Count); k++)
                    {
                        if (stop)
                            return;
                        for (int l = 0; l < listCubeIndexes[i].Count; l++)
                            pubVarElement[listCubeIndexes[i][l]] = (k & ((ulong)1 << l)) > 0 ? 1 : 0;
                        try
                        {
                            b[i] ^= CubeattackBlackbox.GenerateBlackboxOutputBit(pubVarElement, null, outputBitIndex[i]);
                        }
                        catch (Exception ex)
                        {
                            CubeAttack_LogMessage("Error: " + ex, NotificationLevel.Error);
                        }
                    }
                    for (int j = 0; j < settings.PublicVar; j++)
                        pubVarElement[j] = 0;
                    
                    outputSuperpoly += GetLogMessage(listCubeIndexes[i], superpoly, outputBitIndex[i], b[i]);
                    OnPropertyChanged("OutputSuperpoly");

                    ProgressChanged((double)i / (double)listCubeIndexes.Count, 1.0);
                    outputSuperpoly = string.Empty;
                }
                if (listCubeIndexes.Count == settings.SecretVar)
                {
                    CubeAttack_LogMessage("Solve system of equations", NotificationLevel.Info);
                    for (int i = 0; i < settings.SecretVar; i++)
                        b[i] ^= superpolyMatrix[i, 0];
                    // Delete first column and invert
                    OutputKey(superpolyMatrix.DeleteFirstColumn().Inverse() * b);
                    OnPropertyChanged("OutputKeyBits");
                    CubeAttack_LogMessage("Key bits successfully discovered, online phase completed", NotificationLevel.Info);
                }
                else
                    CubeAttack_LogMessage("Not enough linearly independent superpolys have been found in the preprocessing to discover all secret bits !", NotificationLevel.Info);
            }
        }

        /// <summary>
        /// User-Mode to set public bit values manually.
        /// </summary>
        public void SetPublicBitsPhase()
        {
            outputSuperpoly = string.Empty;
            outputKeyBits = string.Empty;
            superpolyMatrix = new Matrix(settings.SecretVar, settings.SecretVar + 1);
            listCubeIndexes = new List<List<int>>();
            pubVarGlob = new int[settings.PublicVar];
            List<int> maxterm = new List<int>();
            bool fault = false;

            if (settings.OutputBit != indexOutputBit)
                indexOutputBit = settings.OutputBit;
            if (settings.SetPublicBits.Length != settings.PublicVar)
                CubeAttack_LogMessage("Input public bits must have size " + settings.PublicVar + " (Currently: " + settings.SetPublicBits.Length + " )", NotificationLevel.Error);
            else
            {
                for (int i = 0; i < settings.SetPublicBits.Length; i++)
                {
                    switch (settings.SetPublicBits[i])
                    {
                        case '0':
                            pubVarGlob[i] = 0;
                            break;
                        case '1':
                            pubVarGlob[i] = 1;
                            break;
                        case '*':
                            maxterm.Add(i);
                            break;
                        default:
                            fault = true;
                            break;
                    }
                }
                if (fault)
                    CubeAttack_LogMessage("The input public bits do not consist only of characters : \'0\',\'1\',\'*\' !", NotificationLevel.Error);
                else
                {
                    if (maxterm.Count > 0)
                    {
                        if (!IsSuperpolyConstant(pubVarGlob, maxterm))
                            if (IsSuperpolyLinear(pubVarGlob, maxterm))
                            {
                                List<int> superpoly = ComputeSuperpoly(pubVarGlob, maxterm);
                                if (!stop)
                                {
                                    for (int i = 0; i < superpoly.Count; i++)
                                        superpolyMatrix[0, i] = superpoly[i];
                                    listCubeIndexes.Add(maxterm);
                                    OutputSuperpolys(maxterm, superpoly);
                                }
                            }
                            else
                            {
                                if(!stop)
                                    CubeAttack_LogMessage("The corresponding superpoly is not a linear polynomial !", NotificationLevel.Info);
                            }
                        else
                            CubeAttack_LogMessage("The corresponding superpoly is constant !", NotificationLevel.Info);
                    }
                    else
                    {
                        string output = "Output bit: " + Blackbox(pubVarGlob, new int[settings.SecretVar]);
                        outputSuperpoly += output;
                        OnPropertyChanged("OutputSuperpoly");
                        CubeAttack_LogMessage(output, NotificationLevel.Info);
                    }
                }
            }
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Does the actual CubeAttack processing
        /// </summary>
        private void ProcessCubeAttack(CubeAttackMode mode)
        {
            switch (mode)
            {
                case CubeAttackMode.preprocessing:
                    PreprocessingPhase();
                    break;
                case CubeAttackMode.online:
                    OnlinePhase();
                    break;
                case CubeAttackMode.setPublicBits:
                    SetPublicBitsPhase();
                    break;
            }
        }

        /// <summary>
        /// Handles log messages
        /// </summary>
        private void CubeAttack_LogMessage(string msg, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(msg, this, logLevel));
            }
        }


        #region IControlEncryption Members 

        private IControlCubeAttack cubeattackBlackbox;
        [PropertyInfo(Direction.ControlMaster, "CubeattackBlackboxCaption", "CubeattackBlackboxTooltip")]
        public IControlCubeAttack CubeattackBlackbox
        {
            get { return cubeattackBlackbox; }
            set
            {
                if (value != null)
                    cubeattackBlackbox = value;
            }
        }

        #endregion

        #endregion


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
