using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using EnervaCore.Utility;
using UnityEngine;

namespace EnervaCore {

    /**
     * EnervaCoreManager class is the top level Manager for the EnervaCore Library. It handles storage life cycle of all other management classes and data.
     */
    public class EnervaCoreManager : Singleton<EnervaCoreManager> {
        private List<IManager> _managersList;
        private List<IManagerUpdate> _updateManagersList;
        private bool _initialized;

        public ECSettings Settings { get; private set; } = null;
        public event Action<EnervaCoreManager> ECMInitialized;

        
        /// <summary>
        /// Initializes the Enerva Core System. This must be called to spin up Managers and utilities used by Enerva Core
        /// </summary>
        /// <param name="ProjectName"></param>
        public void Initialize(string ProjectName) {

            Settings = new ECSettings(ProjectName);

            Debug.Log(this + " :: Initializing Enerva Core Manager...");
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            //Phase 1: Initialize all managers that implement the IManager interface            
            InitManagers(assemblies);

            //Phase 2: Run Post Init for managers that implement PostInit interface
            //This is done here to ensure all managers have been initialized before running PostInit
            RunPostInitInterfaces();

            //Phase 3: Handle callback and game start once ARM has finished loading all assets.
            //If ARM has already finished loading, then we can just run the callback and game
            //start immediately.
            AssetRegistryManager ARM = GetManager<AssetRegistryManager>();
            if (ARM.AssetLoadProgress < 1.0f) {
                GetManager<AssetRegistryManager>().OnAllAssetsLoaded += () => {
                    _initialized = true;
                    OnECMInitialized();
                };
            }
            else {
                _initialized = true;
                OnECMInitialized();
            }
        }

        public override void OnDestroy() {
            Shutdown();
        } 
        
        public void Shutdown() {
            foreach (IManager manager in _managersList) {
                manager.OnDestroyed();
            }

            _managersList = null;

            _updateManagersList.Clear();
            _updateManagersList = null;
        }

        #region Properties
        public bool Initialized {
            get { return _initialized; }
        }
        #endregion

        public void Update() {
            if (_updateManagersList != null) {
                foreach (IManagerUpdate managerUpdate in _updateManagersList) {
                    managerUpdate.Update(Time.deltaTime);

                    //TODO: Handle raw time separately from regular update
                    managerUpdate.UpdateRaw(Time.deltaTime);
                }
            }
        }

        public T GetManager<T>() where T : IManager{
            foreach(IManager m in _managersList) {
                if(m is T) {
                    return (T)m;
                }
            }

            return default;
        }

        /// <summary>
        /// Loads and Automatically registers assembly classes that implement the IManager interface
        /// </summary>
        /// <param name="assemblies"></param>
        private void InitManagers(Assembly[] assemblies) {
            

            Debug.Log(this + " :: Initializing Managers...");

            _managersList = new List<IManager>();
            _updateManagersList = new List<IManagerUpdate>();

            foreach (var assembly in assemblies){
                foreach (var t in assembly.GetTypes()){
                    if (t.GetInterfaces().Contains(typeof(IManager))){
                        IManager manager = Activator.CreateInstance(t) as IManager;
                        try {
                            manager.Initialize();
                            _managersList.Add(manager);
                        }
                        catch (Exception e) {
                            Debug.LogError(this + " :: Failed to Initialize Manager " + manager.GetType().ToString() + " with Exception: " + e.ToString());
                            continue;
                        }

                        IManagerUpdate managerUpdate = manager as IManagerUpdate;
                        if (managerUpdate != null) {
                            _updateManagersList.Add(managerUpdate);
                        }

                        //Debug.Log(this + " :: Initialized Manager " + manager.GetType().ToString());                            
                    }
                }
            }
        }

        private void RunPostInitInterfaces() {
            foreach(IManager manager in _managersList) {
                IManagerPostInit igmi = manager as IManagerPostInit;
                if (igmi != null) {
                    igmi.OnPostInit();
                }
            }
        }

        private void RunGameStartInterfaces() {
            foreach (IManager manager in _managersList) {
                IManagerGameStart igmi = manager as IManagerGameStart;
                if (igmi != null) {
                    igmi.OnGameStart();
                }
            }
        }

        protected virtual void OnECMInitialized() {
            RunGameStartInterfaces();
            ECMInitialized?.Invoke(this);
        }
    }
}