using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVImporter.Models
{
    internal class ValidateCSV
    {
        public static readonly HashSet<string> CurrencyCodes;

        static ValidateCSV()
        {
            CurrencyCodes = GetCurrencySymbols();
        }

        public static bool Validate(string[] fields)
        {
            bool ValidItem = true;

            if (ValidItem && fields.Length != 4)
                ValidItem = false;

            if (ValidItem && !CurrencyCodes.Contains(fields[2]))
                ValidItem = false;

            decimal value;
            if (ValidItem && !decimal.TryParse(fields[3], out value))
                ValidItem = false;

            return ValidItem;
        }

        #region CurrencyCodes

        private static HashSet<string> GetCurrencySymbols()
        {
            var RegionInfos = (from culture in CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures)
                               where culture.Name.Length > 0 && !culture.IsNeutralCulture
                               let region = new System.Globalization.RegionInfo(culture.LCID)
                               select region).Select(R => R.ISOCurrencySymbol).ToList();

            return new HashSet<string>(RegionInfos);
        }

        #endregion 

    }
}
