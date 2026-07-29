
using EnervaCore.Managers;
using System.Data;

/**
 * Utility Class that has static references to core managers
 * */
namespace EnervaCore {
    public class ECM {
        public static EnervaCoreManager Main {
            get { return EnervaCoreManager.Instance; }
        }

        public static ECSettings Settings {
            get { return Main != null ? Main.Settings : null; }
        }

        public static DBManager DB{
            get { return Main != null ? Main.GetManager<DBManager>() : null; }
        }
    }
}