using System;
using System.Collections;
using System.Globalization;

namespace CrypTool.CertificateLibrary.Util
{
    public class Country : IComparable
    {

        #region Get Country/-ies

        /// <summary>
        /// Returns an ArrayList containing Country objects for all countries.
        /// </summary>
        /// <returns></returns>
        public static ArrayList getCountries()
        {
            // At the time of writing, there are 127 countries listed in c#... so this is little performance boost
            ArrayList countries = new ArrayList(127);
            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                // Filter all neutral and invariant cultures
                if (!ci.IsNeutralCulture && !ci.Name.Equals(""))
                {
                    RegionInfo regInfo = new RegionInfo(ci.LCID);
                    Country reg = new Country(regInfo.DisplayName, ci.TwoLetterISOLanguageName.ToUpper());
                    if (!countries.Contains(reg))
                    {
                        countries.Add(reg);
                    }
                }
            }
            countries.TrimToSize();
            countries.Sort();
            return countries;
        }

        /// <summary>
        /// Returns the country name for the specified code.
        /// If no country can be found, returns an empty string.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string getCountry(string code)
        {
            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                if (!ci.IsNeutralCulture && !ci.Name.Equals("") && code.Equals(ci.TwoLetterISOLanguageName.ToUpper()))
                {
                    return new RegionInfo(ci.LCID).DisplayName;
                }
            }
            return string.Empty;
        }

        #endregion


        #region Constructor

        public Country(string displayName, string code)
        {
            DisplayName = displayName;
            Code = code;
        }

        #endregion


        #region Object methods (override)

        public override string ToString()
        {
            return DisplayName;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("Region.CompareTo Argument is null");
            }

            Country reg = obj as Country;
            if (reg == null)
            {
                throw new ArgumentException("Region.CompareTo Argument is no instance of Region");
            }
            return DisplayName.CompareTo(reg.DisplayName);
        }

        public override bool Equals(object other)
        {
            if (other == null)
            {
                return false;
            }

            Country reg = other as Country;
            if (reg == null)
            {
                return false;
            }
            return DisplayName.Equals(((Country)other).DisplayName);
        }

        public override int GetHashCode()
        {
            return (DisplayName.GetHashCode() + Code.GetHashCode());
        }

        #endregion


        #region Properties

        public string DisplayName { get; private set; }

        public string Code { get; private set; }

        #endregion

    }
}
