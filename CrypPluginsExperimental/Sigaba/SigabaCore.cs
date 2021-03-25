using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;


namespace Sigaba
{
    public class SigabaCore
    {
        private readonly Sigaba _facade;
        private readonly SigabaSettings _settings;
        private readonly SigabaPresentation _sigpa;

        public Rotor[] ControlRotors { get; set; }
        public Rotor[] CipherRotors { get; set; }
        public Rotor[] IndexRotors { get; set; }

        public Rotor[] CodeWheels { get; set; }

        public int[,] PresentationLetters = new int[5,20];

        public System.Timers.Timer aTimer = new System.Timers.Timer();

       
        public Boolean b2 = true;

        public SigabaCore(Sigaba facade, SigabaPresentation sigpa)
        {
            _sigpa = sigpa;
            _facade = facade;
            _settings = (SigabaSettings)_facade.Settings;

            CodeWheels = new Rotor[15];

            CipherRotors = new Rotor[5];
            ControlRotors = new Rotor[5];
            IndexRotors = new Rotor[5];
        }

        public void stepCipherRotors(int[] steps)
        {
            foreach (int i in steps)
                CipherRotors[i].IncrementPosition();
        }

        public void stepCipherRotors()
        {
            stepCipherRotors(Control().Distinct().ToArray());
        }

        public void stepControlRotors_SC() //Stamp Challenge
        {
            const int sc = 14;
            const int sc2 = 14;

            if (ControlRotors[2].Position == sc) 
            {
                if (ControlRotors[1].Position == sc2)
                    ControlRotors[3].IncrementPosition();
                ControlRotors[1].IncrementPosition();
            }
            ControlRotors[2].IncrementPosition();
        }

        public void stepControlRotors()
        {
            if (ControlRotors[2].Position == 14)
            {
                if (ControlRotors[1].Position == 14)
                    ControlRotors[3].IncrementPosition();
                ControlRotors[1].IncrementPosition();
            }
            ControlRotors[2].IncrementPosition();
        }

        public object[] Encrypt(String plaintext)
        {
            StringBuilder ciphertext = new StringBuilder();
            List<int[]> repeatList = new List<int[]>();

            foreach (char c in plaintext)
            {
                ciphertext.Append((char)(Cipher(c - 65) + 65));

                int[] controlarr = Control().Distinct().ToArray();
                repeatList.Add(controlarr);

                stepCipherRotors(controlarr);
                stepControlRotors_SC();
            }

            if (Sigaba.verbose)
                Console.WriteLine(String.Join(", ", repeatList.Select(li => "new int[] {" + String.Join(",", li) + "}")));

            UpdateSettings();

            return new object[] { ciphertext.ToString(), repeatList.ToArray() };
        }

        public string EncryptPresentation(String plaintext)
        {
            Boolean b2 = true;
            _sigpa.stopPresentation = false;

            StringBuilder ciphertext = new StringBuilder();

            _sigpa.SetCipher(plaintext);

            List<int[]> repeatList = new List<int[]>();

            foreach (char c in plaintext)
            {
                if(!b2 || _sigpa.stopPresentation)
                    break;

                ciphertext.Append((char)(CipherPresentation(c - 65) + 65));

                int[] controlarr = ControlPresentation().Distinct().ToArray();
                repeatList.Add(controlarr);

                stepCipherRotors(controlarr);
                stepControlRotors();

                _sigpa.FillPresentation(PresentationLetters);
                _sigpa.Callback = true;

                while(b2 && _sigpa.Callback)
                {
                }

                UpdateSettings();
            }

            if(Sigaba.verbose)
                Console.WriteLine(String.Join(", ", repeatList.Select(li => "new int[] {" + String.Join(",", li) + "}")));

            return ciphertext.ToString();
        }

        private void UpdateSettings()
        {
            _settings.CipherKey = CipherRotors.Aggregate("", (current, r) => String.Concat(current, (char) (r.Position + 65)));
            _settings.ControlKey = ControlRotors.Reverse().Aggregate("", (current, r) => String.Concat(current, (char)(r.Position + 65)));
        }

        public void SetKeys()
        {
            for (int i = 0; i < 5; i++)
            {
                CipherRotors[i].Position = _settings.CipherKey[i] - 65;
                IndexRotors[i].Position = _settings.IndexKey[i] - 48;
                ControlRotors[4-i].Position = _settings.ControlKey[i] - 65; 
            }
        }

        public void SetInternalConfig()
        {
            CipherRotors[0] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.CipherRotor1], _settings.CipherKey[0] - 65, _settings.CipherRotor1Reverse);
            CipherRotors[1] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.CipherRotor2], _settings.CipherKey[1] - 65, _settings.CipherRotor2Reverse);
            CipherRotors[2] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.CipherRotor3], _settings.CipherKey[2] - 65, _settings.CipherRotor3Reverse);
            CipherRotors[3] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.CipherRotor4], _settings.CipherKey[3] - 65, _settings.CipherRotor4Reverse);
            CipherRotors[4] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.CipherRotor5], _settings.CipherKey[4] - 65, _settings.CipherRotor5Reverse);

            ControlRotors[4] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.ControlRotor5], _settings.ControlKey[0] - 65, _settings.ControlRotor5Reverse);
            ControlRotors[3] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.ControlRotor4], _settings.ControlKey[1] - 65, _settings.ControlRotor4Reverse);
            ControlRotors[2] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.ControlRotor3], _settings.ControlKey[2] - 65, _settings.ControlRotor3Reverse);
            ControlRotors[1] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.ControlRotor2], _settings.ControlKey[3] - 65, _settings.ControlRotor2Reverse);
            ControlRotors[0] = new Rotor(SigabaConstants.ControlCipherRotors[_settings.ControlRotor1], _settings.ControlKey[4] - 65, _settings.ControlRotor1Reverse);

            IndexRotors[0] = new Rotor(SigabaConstants.IndexRotors[_settings.IndexRotor1], _settings.IndexKey[0] - 48, _settings.IndexRotor1Reverse);
            IndexRotors[1] = new Rotor(SigabaConstants.IndexRotors[_settings.IndexRotor2], _settings.IndexKey[1] - 48, _settings.IndexRotor2Reverse);
            IndexRotors[2] = new Rotor(SigabaConstants.IndexRotors[_settings.IndexRotor3], _settings.IndexKey[2] - 48, _settings.IndexRotor3Reverse);
            IndexRotors[3] = new Rotor(SigabaConstants.IndexRotors[_settings.IndexRotor4], _settings.IndexKey[3] - 48, _settings.IndexRotor4Reverse);
            IndexRotors[4] = new Rotor(SigabaConstants.IndexRotors[_settings.IndexRotor5], _settings.IndexKey[4] - 48, _settings.IndexRotor5Reverse);
        }

        public int[] Control()
        {
            int[] FGHI = new int[] { 5, 6, 7, 8 };

            for (int i = 0; i < 4; i++)
            {
                foreach (var rotor in ControlRotors)
                    FGHI[i] = rotor.DeCiph(FGHI[i]);

                FGHI[i] = SigabaConstants.Transform[_settings.Type][FGHI[i]];

                foreach (var rotor in IndexRotors)
                    FGHI[i] = rotor.Ciph(FGHI[i]);

                FGHI[i] = SigabaConstants.Transform2[FGHI[i]];
            }

            return FGHI;
        }

        public int[] ControlPresentation()
        {
            int[] FGHI = new int[] {5,6,7,8};

            for (int i = 0; i < 4; i++) PresentationLetters[i, 0] = FGHI[i];

            for (int i = 0; i < ControlRotors.Length; i++)
                for (int j = 0; j < 4; j++)
                    PresentationLetters[j, i + 1] = FGHI[j] = ControlRotors[i].DeCiph(FGHI[j]);

            if (Sigaba.verbose)
                Console.WriteLine(String.Join(" ", FGHI));

            for (int i = 0; i < 4; i++)
                PresentationLetters[i, ControlRotors.Length + 2] = FGHI[i] = SigabaConstants.Transform[_settings.Type][FGHI[i]];

            if (Sigaba.verbose)
                Console.WriteLine(String.Join(" ", FGHI));

            for (int i = 0; i < IndexRotors.Length; i++)
            {
                for (int j = 0; j < 4; j++)
                    PresentationLetters[j, ControlRotors.Length + i + 3] = FGHI[j] = IndexRotors[i].Ciph(FGHI[j]);

                if (Sigaba.verbose) 
                    Console.WriteLine(String.Join(" ", FGHI));
            }

            for (int i = 0; i < 4; i++)
                PresentationLetters[i, ControlRotors.Length + IndexRotors.Length + 4] = FGHI[i] = SigabaConstants.Transform2[FGHI[i]];

            if (Sigaba.verbose)
                Console.WriteLine(String.Join(" ", Enumerable.Range(0, 4).Select(x => PresentationLetters[x, 14])));

            return FGHI;
        }

        public int Cipher(int c)
        {
            return (_settings.Action == 0)
                ? CipherRotors.Aggregate(c, (current, rotor) => rotor.Ciph(current))
                : CipherRotors.Reverse().Aggregate(c, (current, rotor) => rotor.DeCiph(current));
        }

        public int CipherPresentation(int c)
        {
            if (_settings.Action == 0)
            {
                PresentationLetters[4, 5] = c;
                for (int i = 0; i < CipherRotors.Length; i++)
                    PresentationLetters[4, CipherRotors.Length - i - 1] = c = CipherRotors[i].Ciph(c);
            }
            else
            {
                PresentationLetters[4, 0] = c;
                for (int i = CipherRotors.Length - 1; i >= 0; i--)
                    PresentationLetters[4, i + 1] = c = CipherRotors[i].DeCiph(c);
            }

            return c;
        }

        public void stop()
        {
            b2 = false;
            _sigpa.Stop();
        }

        public void settings_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            _sigpa.St.SetSpeedRatio((double) 50 / _settings.PresentationSpeed);
            _sigpa.SpeedRatio = (double)50 / _settings.PresentationSpeed;
        }
    }
}