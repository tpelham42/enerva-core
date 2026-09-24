using System;
using System.Collections.Generic;
using System.Text;

namespace EnervaCore {

    /// <summary>
    /// IManager interface defines the basic lifecycle methods for a manager. Initialize is called
    /// for each manager as it's registered with the EnervaCoreManager. This means not all other 
    /// managers may be ready yet.
    /// 
    /// OnDestroyed is called when the EnervaCoreManager is destroyed.
    /// </summary>
    public interface IManager {

        /// <summary>
        /// Called when the manager is registered with the EnervaCoreManager. Other managers are not guaranteed to be initialized at this point, so this is useful for managers that don't depend on other managers.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Called when the EnervaCoreManager is destroyed.
        /// </summary>
        void OnDestroyed();
    }

    
    public interface IManagerPostInit {
        /// <summary>
        /// Called after all managers have been initialized. This is useful
        /// for managers that depend on other managers being initialized first. Game Assets not guaranteed to be loaded at this point.
        /// </summary>
        void OnPostInit();        
    }

    public interface IManagerGameStart {
        /// <summary>
        /// Called after all managers have been initialized and all game assets have been loaded.
        /// </summary>
        void OnGameStart();
    }

    /// <summary>
    /// IManagerUpdate interface defines the Update method for managers that need to perform per-frame updates.
    /// 
    /// Update is called every frame with the delta time since the last frame. 
    /// UpdateRaw is called every frame with the raw delta time since the last frame, without any time scaling applied.
    /// </summary>
    public interface IManagerUpdate {
        void Update(float deltaTime);
        void UpdateRaw(float deltaTime);

    }

}
