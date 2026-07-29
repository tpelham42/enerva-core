using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnervaCore {
    public class ModInfo {
        public string Name;
        public string Author;
        public string URL;
        public int BuildNumber;
        public string Version;
        public float VersionNum;
        public string Summary;

        public ModInfo() {             
            Summary = string.Empty;
            URL = string.Empty;
        }

        public bool IsValid() {
            if (Name == null || Author == null || Version == null)
                return false;

            return true;
        }
    }
}