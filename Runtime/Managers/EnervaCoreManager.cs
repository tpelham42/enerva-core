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

        

        public void Initialize(string ProjectName) {

            Settings = new ECSettings(ProjectName);

            Debug.Log(this + " :: Initializing ECM...");
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            InitManagers(assemblies);
            RunGameManagerInitializedInterfaces(assemblies);

            OnECMInitialized();
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
        
        //Loads and Automatically registers assembly classes that implement the IManager interface
        private void InitManagers(Assembly[] assemblies) {
            _initialized = true;

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

                        Debug.Log(this + " :: Initialized Manager " + manager.GetType().ToString());                            
                    }
                }
            }
        }

        private void RunGameManagerInitializedInterfaces(Assembly[] assemblies) {
            foreach(IManager manager in _managersList) {
                IManagerGMInitialized igmi = manager as IManagerGMInitialized;
                if (igmi != null) {
                    igmi.OnGameManagerInitialized();
                }
            }

            IEnumerable<IGameManagerInitialized> list = FindObjectsOfType<MonoBehaviour>().OfType<IGameManagerInitialized>();
            if (list != null) {
                foreach (IGameManagerInitialized igmi in list) {
                    igmi?.OnGameManagerInitialized();
                }
            }
        }

        protected virtual void OnECMInitialized() => ECMInitialized?.Invoke(this);
    }
}