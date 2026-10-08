using System.Collections.Generic;
using System;

namespace EnervaCore.Interfaces {
    /// <summary>
    /// Public contract for runtime EC object instances. Implemented by ECObjectInstance base class.
    /// Keep this small and focused so manager systems can depend on the interface.
    /// </summary>
    public interface IECObjectInstance {
        ECObjectData Template { get; set; }
        List<IECObjectComponentInstance> Components { get; set; }

        /// <summary>
        /// Called after the instance has been created and its Template property set.
        /// </summary>
        void OnInit();
    }
}
