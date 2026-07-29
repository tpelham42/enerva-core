using System;
using UnityEngine;

namespace EnervaCore {
    public abstract class ECBehavior : MonoBehaviour {      

        
        void Start() {
            if (ECM.Main.Initialized)
                OnECStart();
            else
                ECM.Main.ECMInitialized += OnECMInitialized;
           
        }

        private void OnECMInitialized(EnervaCoreManager manager) {
            OnECStart();
        }

        /// <summary>
        /// Called once EnervaCore and all managers have been initialized
        /// </summary>
        protected virtual void OnECStart() { }
    }
}
