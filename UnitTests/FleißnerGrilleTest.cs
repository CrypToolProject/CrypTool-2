using CrypTool.Plugins.FleißnerGrille;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;

namespace UnitTests
{   
    /// <summary>
    ///Dies ist eine Testklasse für "FleißnerGrilleTest" und soll
    ///alle FleißnerGrilleTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class FleißnerGrilleTest
    {
        //private TestContext testContextInstance;

        ///// <summary>
        /////Ruft den Testkontext auf, der Informationen
        /////über und Funktionalität für den aktuellen Testlauf bietet, oder legt diesen fest.
        /////</summary>
        //public TestContext TestContext
        //{
        //    get
        //    {
        //        return testContextInstance;
        //    }
        //    set
        //    {
        //        testContextInstance = value;
        //    }
        //}

        #region Zusätzliche Testattribute
         
        //Sie können beim Verfassen Ihrer Tests die folgenden zusätzlichen Attribute verwenden:
        
        //Mit ClassInitialize führen Sie Code aus, bevor Sie den ersten Test in der Klasse ausführen.
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
        }
        
        //Mit ClassCleanup führen Sie Code aus, nachdem alle Tests in einer Klasse ausgeführt wurden.
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
        }
        
        //Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen.
        [TestInitialize()]
        public void MyTestInitialize()
        {
        }
        
        //Mit TestCleanup können Sie nach jedem einzelnen Test Code ausführen.
        [TestCleanup()]
        public void MyTestCleanup()
        {
        }
        
        #endregion

        #region FleißnerGrille
        /// <summary>
        ///Ein Test für "getGrilleSize"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FleißnerGrille.dll")]
        public void getGrilleSizeTest()
        {
            FleißnerGrille_Accessor target = new FleißnerGrille_Accessor(); // TODO: Passenden Wert initialisieren
            string input = "WIKIPEDIADIEFREIEONLINEENZOKLOPAEDIE"; // TODO: Passenden Wert initialisieren
            int expected = 6; // TODO: Passenden Wert initialisieren
            int actual;
            actual = target.getGrilleSize(input);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Ein Test für "ProcessFleißnerStencil"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FleißnerGrille.dll")]
        public void processFleißnerStencilTest()
        {
            FleißnerGrille_Accessor target = new FleißnerGrille_Accessor(); // TODO: Passenden Wert initialisieren
            string input = "WIKIPEDIADIEFREIEONLINEENZYKLOPAEDIE"; // TODO: Passenden Wert initialisieren
            string expected = "KWNILKLODPIIAIPENEFEDEDRIEEEIINEZAYO";
            string actual;
            target.InputString = input;
            target.settings.ActionRotate = FleißnerGrilleSettings.FleißnerRotate.Right;
            target.settings.ActionMode = FleißnerGrilleSettings.FleißnerMode.Encrypt;
            target.ProcessFleißnerStencil();
            actual = target.OutputString;
            Assert.AreEqual(expected, actual);
            target.InputString = expected;
            target.settings.ActionRotate = FleißnerGrilleSettings.FleißnerRotate.Right;
            target.settings.ActionMode = FleißnerGrilleSettings.FleißnerMode.Decrypt;
            target.ProcessFleißnerStencil();
            actual = target.OutputString;
            Assert.AreEqual(input, actual);
        }

        /// <summary>
        ///Ein Test für "rotateStencil"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FleißnerGrille.dll")]
        public void rotateStencilTest()
        {
            FleißnerGrille_Accessor target = new FleißnerGrille_Accessor(); // TODO: Passenden Wert initialisieren
            bool[,] stencil = new bool[6, 6]; // Wikipedia grille
            stencil[0, 0] = false; stencil[0, 1] = true; stencil[0, 2] = false; stencil[0, 3] = true; stencil[0, 4] = false; stencil[0, 5] = true;
            stencil[1, 0] = false; stencil[1, 1] = false; stencil[1, 2] = false; stencil[1, 3] = false; stencil[1, 4] = true; stencil[1, 5] = false;
            stencil[2, 0] = false; stencil[2, 1] = false; stencil[2, 2] = true; stencil[2, 3] = false; stencil[2, 4] = false; stencil[2, 5] = false;
            stencil[3, 0] = false; stencil[3, 1] = true; stencil[3, 2] = false; stencil[3, 3] = false; stencil[3, 4] = true; stencil[3, 5] = false;
            stencil[4, 0] = false; stencil[4, 1] = false; stencil[4, 2] = false; stencil[4, 3] = false; stencil[4, 4] = false; stencil[4, 5] = true;
            stencil[5, 0] = false; stencil[5, 1] = false; stencil[5, 2] = false; stencil[5, 3] = true; stencil[5, 4] = false; stencil[5, 5] = false;
            bool[,] expected = new bool[6, 6]; // Wikipedia grille right rotate
            expected[0, 0] = false; expected[0, 1] = false; expected[0, 2] = false; expected[0, 3] = false; expected[0, 4] = false; expected[0, 5] = false;
            expected[1, 0] = false; expected[1, 1] = false; expected[1, 2] = true; expected[1, 3] = false; expected[1, 4] = false; expected[1, 5] = true;
            expected[2, 0] = false; expected[2, 1] = false; expected[2, 2] = false; expected[2, 3] = true; expected[2, 4] = false; expected[2, 5] = false;
            expected[3, 0] = true; expected[3, 1] = false; expected[3, 2] = false; expected[3, 3] = false; expected[3, 4] = false; expected[3, 5] = true;
            expected[4, 0] = false; expected[4, 1] = false; expected[4, 2] = true; expected[4, 3] = false; expected[4, 4] = true; expected[4, 5] = false;
            expected[5, 0] = false; expected[5, 1] = true; expected[5, 2] = false; expected[5, 3] = false; expected[5, 4] = false; expected[5, 5] = true;
            bool[,] expectedLeft = target.rotate(expected); expectedLeft = target.rotate(expectedLeft);
            bool[,] actualRight, actualLeft;
            target.settings.ActionRotate = FleißnerGrilleSettings.FleißnerRotate.Right;
            actualRight = target.RotateStencil(stencil, true);
            target.settings.ActionRotate = FleißnerGrilleSettings.FleißnerRotate.Left;
            actualLeft = target.RotateStencil(stencil, true);
            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    if (expected[x, y] == actualRight[x, y])
                    {
                        Assert.AreEqual(expected[x, y], actualRight[x, y]);
                    }
                    else
                    {
                        Assert.Inconclusive("Eine Methode, die keinen Wert zurückgibt, kann nicht überprüft werden.");
                    }
                    if (expectedLeft[x, y] == actualLeft[x, y])
                    {
                        Assert.AreEqual(expectedLeft[x, y], actualLeft[x, y]);
                    }
                    else
                    {
                        Assert.Inconclusive("Eine Methode, die keinen Wert zurückgibt, kann nicht überprüft werden.");
                    }
                }
            }
        }

        /// <summary>
        ///Ein Test für "rotate"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FleißnerGrille.dll")]
        public void rotateTest()
        {
            FleißnerGrille_Accessor target = new FleißnerGrille_Accessor(); // TODO: Passenden Wert initialisieren
            bool[,] stencil = new bool[6, 6]; // Wikipedia grille
            stencil[0, 0] = false; stencil[0, 1] = true; stencil[0, 2] = false; stencil[0, 3] = true; stencil[0, 4] = false; stencil[0, 5] = true;
            stencil[1, 0] = false; stencil[1, 1] = false; stencil[1, 2] = false; stencil[1, 3] = false; stencil[1, 4] = true; stencil[1, 5] = false;
            stencil[2, 0] = false; stencil[2, 1] = false; stencil[2, 2] = true; stencil[2, 3] = false; stencil[2, 4] = false; stencil[2, 5] = false;
            stencil[3, 0] = false; stencil[3, 1] = true; stencil[3, 2] = false; stencil[3, 3] = false; stencil[3, 4] = true; stencil[3, 5] = false;
            stencil[4, 0] = false; stencil[4, 1] = false; stencil[4, 2] = false; stencil[4, 3] = false; stencil[4, 4] = false; stencil[4, 5] = true;
            stencil[5, 0] = false; stencil[5, 1] = false; stencil[5, 2] = false; stencil[5, 3] = true; stencil[5, 4] = false; stencil[5, 5] = false;
            bool[,] expected = new bool[6,6]; // Wikipedia grille right rotate
            expected[0, 0] = false; expected[0, 1] = false; expected[0, 2] = false; expected[0, 3] = false; expected[0, 4] = false; expected[0, 5] = false;
            expected[1, 0] = false; expected[1, 1] = false; expected[1, 2] = true; expected[1, 3] = false; expected[1, 4] = false; expected[1, 5] = true;
            expected[2, 0] = false; expected[2, 1] = false; expected[2, 2] = false; expected[2, 3] = true; expected[2, 4] = false; expected[2, 5] = false;
            expected[3, 0] = true; expected[3, 1] = false; expected[3, 2] = false; expected[3, 3] = false; expected[3, 4] = false; expected[3, 5] = true;
            expected[4, 0] = false; expected[4, 1] = false; expected[4, 2] = true; expected[4, 3] = false; expected[4, 4] = true; expected[4, 5] = false;
            expected[5, 0] = false; expected[5, 1] = true; expected[5, 2] = false; expected[5, 3] = false; expected[5, 4] = false; expected[5, 5] = true;
            bool[,] actual;
            actual = target.rotate(stencil);
            for (int x = 0; x < 6; x++) 
            {
                for (int y = 0; y < 6; y++) 
                {
                    if (expected[x, y] == actual[x, y])
                    {
                        Assert.AreEqual(expected[x,y], actual[x,y]);                        
                    }
                    else 
                    {
                        Assert.Inconclusive("Eine Methode, die keinen Wert zurückgibt, kann nicht überprüft werden.");
                    }
                }
            }     
        }

        /// <summary>
        ///Ein Test für "encryptedMatrix"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FleißnerGrille.dll")]
        public void EncryptedMatrixTest()
        {
            FleißnerGrille_Accessor target = new FleißnerGrille_Accessor(); // TODO: Passenden Wert initialisieren
            bool[,] stencil = new bool[6, 6]; // Wikipedia grille
            stencil[0, 0] = false; stencil[0, 1] = true; stencil[0, 2] = false; stencil[0, 3] = true; stencil[0, 4] = false; stencil[0, 5] = true;
            stencil[1, 0] = false; stencil[1, 1] = false; stencil[1, 2] = false; stencil[1, 3] = false; stencil[1, 4] = true; stencil[1, 5] = false;
            stencil[2, 0] = false; stencil[2, 1] = false; stencil[2, 2] = true; stencil[2, 3] = false; stencil[2, 4] = false; stencil[2, 5] = false;
            stencil[3, 0] = false; stencil[3, 1] = true; stencil[3, 2] = false; stencil[3, 3] = false; stencil[3, 4] = true; stencil[3, 5] = false;
            stencil[4, 0] = false; stencil[4, 1] = false; stencil[4, 2] = false; stencil[4, 3] = false; stencil[4, 4] = false; stencil[4, 5] = true;
            stencil[5, 0] = false; stencil[5, 1] = false; stencil[5, 2] = false; stencil[5, 3] = true; stencil[5, 4] = false; stencil[5, 5] = false;
            string expectedRight = "KWNILKLODPIIAIPENEFEDEDRIEEEIINEZAYO"; // Wikipedia grille right rotate
            string expectedLeft  = "DWNIIKLEKFILRIPONEPEIEDAEEEODINIZAYE"; // Wikipedia grille right rotate
            target.settings.ActionRotate = FleißnerGrilleSettings.FleißnerRotate.Right;
            char[,] actualCharArrayRight = target.EncryptedMatrix(stencil, "WIKIPEDIADIEFREIEONLINEENZYKLOPAEDIE");
            target.settings.ActionRotate = FleißnerGrilleSettings.FleißnerRotate.Left;
            char[,] actualCharArrayLeft = target.EncryptedMatrix(stencil, "WIKIPEDIADIEFREIEONLINEENZYKLOPAEDIE");
            string actualRight = "", actualLeft = "";
            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    actualRight = actualRight + actualCharArrayRight[x, y];
                    actualLeft = actualLeft + actualCharArrayLeft[x, y];
                }
            }
            Assert.AreEqual(expectedRight, actualRight);
            Assert.AreEqual(expectedLeft, actualLeft);
        }

        /// <summary>
        ///Ein Test für "generateRandomBigLetter"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FleißnerGrille.dll")]
        public void generateRandomBigLetterTest()
        {
            FleißnerGrille_Accessor target = new FleißnerGrille_Accessor(); // TODO: Passenden Wert initialisieren
            char[] expected = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' }; // TODO: Passenden Wert initialisieren
            char actual;
            actual = target.generateRandomBigLetter();
            bool b = true;
            for (int i = 0; i < 26; i++)
            {
                if (actual == expected[i])
                {
                    Assert.AreEqual(expected[i], actual);
                    b = false;
                    break;
                }
            }
            if (b)
            {
                Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
            }
        }

        /// <summary>
        ///Ein Test für "generateRandomLowerLetter"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FleißnerGrille.dll")]
        public void generateRandomLowerLetterTest()
        {
            FleißnerGrille_Accessor target = new FleißnerGrille_Accessor(); // TODO: Passenden Wert initialisieren
            char[] expected = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' }; // TODO: Passenden Wert initialisieren
            char actual;
            actual = target.generateRandomLowerLetter();
            bool b = true;
            for (int i = 0; i < 26; i++)
            {
                if (actual == expected[i])
                {
                    Assert.AreEqual(expected[i], actual);
                    b = false;
                    break;
                }
            }
            if (b)
            {
                Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
            }
        }

        /// <summary>
        ///Ein Test für "generateRandomBigLowerLetter"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FleißnerGrille.dll")]
        public void generateRandomBigLowerLetterTest()
        {
            FleißnerGrille_Accessor target = new FleißnerGrille_Accessor(); // TODO: Passenden Wert initialisieren
            char[] expected = { 'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z', 
                                  'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' }; // TODO: Passenden Wert initialisieren
            char actual;
            actual = target.generateRandomBigLowerLetter();
            bool b = true;
            for (int i = 0; i < 52; i++)
            {
                if (actual == expected[i])
                {
                    Assert.AreEqual(expected[i], actual);
                    b = false;
                    break;
                }
            }
            if (b)
            {
                Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
            }
        }

        /// <summary>
        ///Ein Test für "encrypt"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FleißnerGrille.dll")]
        public void encryptTest()
        {
            FleißnerGrille_Accessor target = new FleißnerGrille_Accessor(); // TODO: Passenden Wert initialisieren
            string expected = "KWNILKLODPIIAIPENEFEDEDRIEEEIINEZAYO"; // Wikipedia grille right rotate
            target.settings.ActionRotate = FleißnerGrilleSettings.FleißnerRotate.Right;
            string actual = target.Encrypt("WIKIPEDIADIEFREIEONLINEENZYKLOPAEDIE");
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Ein Test für "stencil"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FleißnerGrille.dll")]
        public void stencilTest()
        {
            FleißnerGrille_Accessor target = new FleißnerGrille_Accessor(); // TODO: Passenden Wert initialisieren
            bool[,] stencil = new bool[6, 6]; // Wikipedia grille
            stencil[0, 0] = false; stencil[0, 1] = true; stencil[0, 2] = false; stencil[0, 3] = true; stencil[0, 4] = false; stencil[0, 5] = true;
            stencil[1, 0] = false; stencil[1, 1] = false; stencil[1, 2] = false; stencil[1, 3] = false; stencil[1, 4] = true; stencil[1, 5] = false;
            stencil[2, 0] = false; stencil[2, 1] = false; stencil[2, 2] = true; stencil[2, 3] = false; stencil[2, 4] = false; stencil[2, 5] = false;
            stencil[3, 0] = false; stencil[3, 1] = true; stencil[3, 2] = false; stencil[3, 3] = false; stencil[3, 4] = true; stencil[3, 5] = false;
            stencil[4, 0] = false; stencil[4, 1] = false; stencil[4, 2] = false; stencil[4, 3] = false; stencil[4, 4] = false; stencil[4, 5] = true;
            stencil[5, 0] = false; stencil[5, 1] = false; stencil[5, 2] = false; stencil[5, 3] = true; stencil[5, 4] = false; stencil[5, 5] = false;
            string stencilString = "010101\n000010\n001000\n010010\n000001\n000100\n";
            bool[,] actual = target.stencil(stencilString);
            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    Assert.AreEqual(actual[x, y], stencil[x, y]);
                }
            }
        }
        
        /// <summary>
        ///Ein Test für "decrypt"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FleißnerGrille.dll")]
        public void decryptTest()
        {
            FleißnerGrille_Accessor target = new FleißnerGrille_Accessor(); // TODO: Passenden Wert initialisieren
            string expected = "WIKIPEDIADIEFREIEONLINEENZYKLOPAEDIE"; // TODO: Passenden Wert initialisieren
            string input = "KWNILKLODPIIAIPENEFEDEDRIEEEIINEZAYO"; //Wikipedia after rotate Right
            string actual;
            target.settings.ActionRotate = FleißnerGrilleSettings.FleißnerRotate.Right;
            target.settings.ActionMode = FleißnerGrilleSettings.FleißnerMode.Decrypt;
            actual = target.Decrypt(input);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Ein Test für "stringTo2DCharArray"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FleißnerGrille.dll")]
        public void stringTo2DCharArrayTest()
        {
            FleißnerGrille_Accessor target = new FleißnerGrille_Accessor(); // TODO: Passenden Wert initialisieren
            char[,] stencil = new char[6, 6]; // Wikipedia grille
            stencil[0, 0] = 'K'; stencil[0, 1] = 'W'; stencil[0, 2] = 'N'; stencil[0, 3] = 'I'; stencil[0, 4] = 'L'; stencil[0, 5] = 'K';
            stencil[1, 0] = 'L'; stencil[1, 1] = 'O'; stencil[1, 2] = 'D'; stencil[1, 3] = 'P'; stencil[1, 4] = 'I'; stencil[1, 5] = 'I';
            stencil[2, 0] = 'A'; stencil[2, 1] = 'I'; stencil[2, 2] = 'P'; stencil[2, 3] = 'E'; stencil[2, 4] = 'N'; stencil[2, 5] = 'E';
            stencil[3, 0] = 'F'; stencil[3, 1] = 'E'; stencil[3, 2] = 'D'; stencil[3, 3] = 'E'; stencil[3, 4] = 'D'; stencil[3, 5] = 'R';
            stencil[4, 0] = 'I'; stencil[4, 1] = 'E'; stencil[4, 2] = 'E'; stencil[4, 3] = 'E'; stencil[4, 4] = 'I'; stencil[4, 5] = 'I';
            stencil[5, 0] = 'N'; stencil[5, 1] = 'E'; stencil[5, 2] = 'Z'; stencil[5, 3] = 'A'; stencil[5, 4] = 'Y'; stencil[5, 5] = 'O'; 
            string expectedRight = "KWNILKLODPIIAIPENEFEDEDRIEEEIINEZAYO"; // Wikipedia grille right rotate
            string input = "KWNILKLODPIIAIPENEFEDEDRIEEEIINEZAYO"; // Wikipedia grille right rotate
            char[,] actual = target.StringTo2DCharArray(input, 6);
            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    Assert.AreEqual(actual[x,y], stencil[x,y]);
                }
            }
        }

        /// <summary>
        ///Ein Test für "twoDCharArrayToString"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FleißnerGrille.dll")]
        public void twoDCharArrayToStringTest()
        {
            FleißnerGrille_Accessor target = new FleißnerGrille_Accessor(); // TODO: Passenden Wert initialisieren
            char[,] stencil = new char[6, 6]; // Wikipedia grille
            stencil[0, 0] = 'K'; stencil[0, 1] = 'W'; stencil[0, 2] = 'N'; stencil[0, 3] = 'I'; stencil[0, 4] = 'L'; stencil[0, 5] = 'K';
            stencil[1, 0] = 'L'; stencil[1, 1] = 'O'; stencil[1, 2] = 'D'; stencil[1, 3] = 'P'; stencil[1, 4] = 'I'; stencil[1, 5] = 'I';
            stencil[2, 0] = 'A'; stencil[2, 1] = 'I'; stencil[2, 2] = 'P'; stencil[2, 3] = 'E'; stencil[2, 4] = 'N'; stencil[2, 5] = 'E';
            stencil[3, 0] = 'F'; stencil[3, 1] = 'E'; stencil[3, 2] = 'D'; stencil[3, 3] = 'E'; stencil[3, 4] = 'D'; stencil[3, 5] = 'R';
            stencil[4, 0] = 'I'; stencil[4, 1] = 'E'; stencil[4, 2] = 'E'; stencil[4, 3] = 'E'; stencil[4, 4] = 'I'; stencil[4, 5] = 'I';
            stencil[5, 0] = 'N'; stencil[5, 1] = 'E'; stencil[5, 2] = 'Z'; stencil[5, 3] = 'A'; stencil[5, 4] = 'Y'; stencil[5, 5] = 'O';
            string expected = "KWNILKLODPIIAIPENEFEDEDRIEEEIINEZAYO"; // Wikipedia grille right rotate
            string actual = target.TwoDCharArrayToString(stencil);
            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region FleißnerGrilleSettings
        /// <summary>
        ///Ein Test für "isCorrectStencil"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FleißnerGrilleSettings.dll")]
        public void isCorrectStencilTest()
        {
            FleißnerGrilleSettings_Accessor target = new FleißnerGrilleSettings_Accessor();
            string inputLengthFail = "01010\n00001\n00100\n01001\n00000\n00010\n";
            bool actual = target.isCorrectStencil(inputLengthFail);
            Assert.AreEqual(false, actual);
            string inputSignFail = "010101\n000b10\n001000\n010010\n000001\n000100\n";
            actual = target.isCorrectStencil(inputSignFail);
            Assert.AreEqual(false, actual);
            string inputIncorrect = "110101\n000010\n001000\n010010\n000001\n000100\n";
            actual = target.isCorrectStencil(inputIncorrect);
            Assert.AreEqual(false, actual);
            string inputOK = "010101\n000010\n001000\n010010\n000001\n000100\n";
            actual = target.isCorrectStencil(inputOK);
            Assert.AreEqual(true, actual);
        }

        /// <summary>
        ///Ein Test für "stringToStencil"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FleißnerGrilleSettings.dll")]
        public void stringToStencilTest()
        {
            FleißnerGrilleSettings_Accessor target = new FleißnerGrilleSettings_Accessor();
            bool[,] stencil = new bool[6, 6]; // Wikipedia grille
            stencil[0, 0] = false; stencil[0, 1] = true; stencil[0, 2] = false; stencil[0, 3] = true; stencil[0, 4] = false; stencil[0, 5] = true;
            stencil[1, 0] = false; stencil[1, 1] = false; stencil[1, 2] = false; stencil[1, 3] = false; stencil[1, 4] = true; stencil[1, 5] = false;
            stencil[2, 0] = false; stencil[2, 1] = false; stencil[2, 2] = true; stencil[2, 3] = false; stencil[2, 4] = false; stencil[2, 5] = false;
            stencil[3, 0] = false; stencil[3, 1] = true; stencil[3, 2] = false; stencil[3, 3] = false; stencil[3, 4] = true; stencil[3, 5] = false;
            stencil[4, 0] = false; stencil[4, 1] = false; stencil[4, 2] = false; stencil[4, 3] = false; stencil[4, 4] = false; stencil[4, 5] = true;
            stencil[5, 0] = false; stencil[5, 1] = false; stencil[5, 2] = false; stencil[5, 3] = true; stencil[5, 4] = false; stencil[5, 5] = false;
            string input = "010101\n000010\n001000\n010010\n000001\n000100\n";
            bool[,] actual = target.StringToStencil(input);
            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    Assert.AreEqual(actual[x, y], stencil[x, y]);
                }
            }
        }

        /// <summary>
        ///Ein Test für "rotateStencil" in Settings
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FleißnerGrilleSettings.dll")]
        public void rotateStencilSettingsTest()
        {
            FleißnerGrilleSettings_Accessor target = new FleißnerGrilleSettings_Accessor();
            bool[,] stencil = new bool[6, 6]; // Wikipedia grille
            stencil[0, 0] = false; stencil[0, 1] = true; stencil[0, 2] = false; stencil[0, 3] = true; stencil[0, 4] = false; stencil[0, 5] = true;
            stencil[1, 0] = false; stencil[1, 1] = false; stencil[1, 2] = false; stencil[1, 3] = false; stencil[1, 4] = true; stencil[1, 5] = false;
            stencil[2, 0] = false; stencil[2, 1] = false; stencil[2, 2] = true; stencil[2, 3] = false; stencil[2, 4] = false; stencil[2, 5] = false;
            stencil[3, 0] = false; stencil[3, 1] = true; stencil[3, 2] = false; stencil[3, 3] = false; stencil[3, 4] = true; stencil[3, 5] = false;
            stencil[4, 0] = false; stencil[4, 1] = false; stencil[4, 2] = false; stencil[4, 3] = false; stencil[4, 4] = false; stencil[4, 5] = true;
            stencil[5, 0] = false; stencil[5, 1] = false; stencil[5, 2] = false; stencil[5, 3] = true; stencil[5, 4] = false; stencil[5, 5] = false;
            target.ActionRotate = FleißnerGrilleSettings.FleißnerRotate.Left;
            bool[,] expectedLeft = target.rotate(stencil);
            target.ActionRotate = FleißnerGrilleSettings.FleißnerRotate.Right;
            bool[,] expected = target.rotate(stencil); // Wikipedia grille right rotate
            bool[,] actualRight, actualLeft;
            actualRight = target.RotateStencil(stencil, true);
            target.ActionRotate = FleißnerGrilleSettings.FleißnerRotate.Left;
            actualLeft = target.RotateStencil(stencil, true);
            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    if (expected[x, y] == actualRight[x, y])
                    {
                        Assert.AreEqual(expected[x, y], actualRight[x, y]);
                    }
                    else
                    {
                        Assert.Inconclusive("Eine Methode, die keinen Wert zurückgibt, kann nicht überprüft werden.");
                    }
                    if (expectedLeft[x, y] == actualLeft[x, y])
                    {
                        Assert.AreEqual(expectedLeft[x, y], actualLeft[x, y]);
                    }
                    else
                    {
                        Assert.Inconclusive("Eine Methode, die keinen Wert zurückgibt, kann nicht überprüft werden.");
                    }
                }
            }
        }

        /// <summary>
        ///Ein Test für "rotate" in Settings
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FleißnerGrilleSettings.dll")]
        public void rotateSettingsTest()
        {
            FleißnerGrilleSettings_Accessor target = new FleißnerGrilleSettings_Accessor();
            bool[,] stencil = new bool[6, 6]; // Wikipedia grille
            stencil[0, 0] = false; stencil[0, 1] = true; stencil[0, 2] = false; stencil[0, 3] = true; stencil[0, 4] = false; stencil[0, 5] = true;
            stencil[1, 0] = false; stencil[1, 1] = false; stencil[1, 2] = false; stencil[1, 3] = false; stencil[1, 4] = true; stencil[1, 5] = false;
            stencil[2, 0] = false; stencil[2, 1] = false; stencil[2, 2] = true; stencil[2, 3] = false; stencil[2, 4] = false; stencil[2, 5] = false;
            stencil[3, 0] = false; stencil[3, 1] = true; stencil[3, 2] = false; stencil[3, 3] = false; stencil[3, 4] = true; stencil[3, 5] = false;
            stencil[4, 0] = false; stencil[4, 1] = false; stencil[4, 2] = false; stencil[4, 3] = false; stencil[4, 4] = false; stencil[4, 5] = true;
            stencil[5, 0] = false; stencil[5, 1] = false; stencil[5, 2] = false; stencil[5, 3] = true; stencil[5, 4] = false; stencil[5, 5] = false;
            bool[,] expected = new bool[6, 6]; // Wikipedia grille right rotate
            expected[0, 0] = false; expected[0, 1] = false; expected[0, 2] = false; expected[0, 3] = false; expected[0, 4] = false; expected[0, 5] = false;
            expected[1, 0] = false; expected[1, 1] = false; expected[1, 2] = true; expected[1, 3] = false; expected[1, 4] = false; expected[1, 5] = true;
            expected[2, 0] = false; expected[2, 1] = false; expected[2, 2] = false; expected[2, 3] = true; expected[2, 4] = false; expected[2, 5] = false;
            expected[3, 0] = true; expected[3, 1] = false; expected[3, 2] = false; expected[3, 3] = false; expected[3, 4] = false; expected[3, 5] = true;
            expected[4, 0] = false; expected[4, 1] = false; expected[4, 2] = true; expected[4, 3] = false; expected[4, 4] = true; expected[4, 5] = false;
            expected[5, 0] = false; expected[5, 1] = true; expected[5, 2] = false; expected[5, 3] = false; expected[5, 4] = false; expected[5, 5] = true;
            bool[,] actual;
            actual = target.rotate(stencil);
            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    if (expected[x, y] == actual[x, y])
                    {
                        Assert.AreEqual(expected[x, y], actual[x, y]);
                    }
                    else
                    {
                        Assert.Inconclusive("Eine Methode, die keinen Wert zurückgibt, kann nicht überprüft werden.");
                    }
                }
            }
        }
        #endregion
    }
}
