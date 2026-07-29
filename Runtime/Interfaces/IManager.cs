using System;
using System.Collections.Generic;
using System.Text;

namespace EnervaCore {
    public interface IManager {
        void Initialize();
        void OnDestroyed();
    }
}
