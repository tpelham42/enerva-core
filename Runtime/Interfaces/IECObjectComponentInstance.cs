using System.Collections.Generic;
using System;

namespace EnervaCore.Interfaces {
    /// <summary>
    /// Public contract for runtime EC object component instances. Implemented by ECObjectComponentInstance base class.
    /// Keep this small and focused so manager systems can depend on the interface.
    /// </summary>
    public interface IECObjectComponentInstance {
        public ECObjectComponentData ComponentData { get; set; }
        public IECObjectInstance Parent { get; set; }

        /// <summary>
        /// Called after the instance has been created and its ComponentData property set. Other components on Parent may not be initialized yet, so be careful about accessing them here.
        /// </summary>
        public void OnInit();
    }
}
