using System.Text;

namespace KeyTextBox
{
    public class SimpleKeyManager : IKeyManager
    {
        private string _key;

        private string _format;
        public string Format
        {
            get => _format;
            set
            {
                _format = value;
                if (!ValidKey(GetKey()))
                {
                    SetKey(GetDefaultKey(value));
                }
            }
        }

        public SimpleKeyManager(string format)
        {
            Format = format;
        }

        private string GetDefaultKey(string format)
        {
            int fcount = 0;
            StringBuilder dk = new StringBuilder();
            while (fcount < format.Length)
            {
                if (format[fcount] == '[')
                {
                    dk.Append('*');
                    do
                    {
                        fcount++;
                    } while (format[fcount] != ']');
                }
                else
                {
                    dk.Append(format[fcount]);
                }
                fcount++;
            }
            return dk.ToString();
        }

        private void OnOnKeyChanged()
        {
            if (OnKeyChanged != null)
            {
                OnKeyChanged(_key);
            }
        }

        public event KeyChangedHandler OnKeyChanged;

        public void SetKey(string key)
        {
            if (key != _key)
            {
                _key = key;
                OnOnKeyChanged();
            }
        }

        public string GetKey()
        {
            return _key;
        }

        public string GetFormat()
        {
            return Format;
        }

        private bool ValidKey(string key)
        {
            if (key == null)
            {
                return false;
            }

            string format = Format;
            int fcount = 0;
            int kcount = 0;

            while (kcount < key.Length)
            {
                System.Collections.Generic.List<char> possibleChars = FormatHelper.GetPossibleCharactersFromFormat(format, fcount);
                if (possibleChars == null)
                {
                    return false;
                }

                if (key[kcount] == '[')
                {
                    if (possibleChars.Count < 2)
                    {
                        return false;
                    }

                    kcount++;
                    char lastChar = (char)0;
                    do
                    {
                        if (key[kcount] == '-')
                        {
                            if (lastChar == 0)
                            {
                                return false;
                            }

                            kcount++;
                            if (!possibleChars.Contains(key[kcount]))
                            {
                                return false;
                            }

                            for (char c = lastChar; c < key[kcount]; c++)
                            {
                                if (!possibleChars.Contains(c))
                                {
                                    return false;
                                }
                            }
                            lastChar = (char)0;
                        }
                        else if (!possibleChars.Contains(key[kcount]))
                        {
                            return false;
                        }
                        else
                        {
                            lastChar = key[kcount];
                        }

                        kcount++;
                    } while (key[kcount] != ']');
                }
                else if (key[kcount] == '*')
                {
                    if (possibleChars.Count < 2)
                    {
                        return false;
                    }
                }
                else
                {
                    if (!possibleChars.Contains(key[kcount]))
                    {
                        return false;
                    }
                }

                fcount = FormatHelper.GetNextFormatIndex(format, fcount);
                kcount++;
            }
            return kcount >= key.Length && fcount >= format.Length;
        }
    }
}