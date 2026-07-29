using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnervaCore {
    public class ECObject {
        
        public ECObjectData Template { get; set; }

        //List of loaded components
        private List<ECObjectDataComponent> _components;

        //Called from DBManager after object has been instantiated and it's template has been set
        public virtual void OnInit() { }
    }
}