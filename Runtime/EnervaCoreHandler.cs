using UnityEngine;

namespace EnervaCore {
    /// <summary>
    /// EnervaCoreHandler is a MonoBehaviour that serves as the entry point for initializing the EnervaCore framework within a Unity project. 
    /// It manages the lifecycle of the EnervaCoreManager, ensuring that it is properly initialized and updated during the game's runtime.
    /// </summary>
    
    public class EnervaCoreHandler : MonoBehaviour {
        [Tooltip("Project Name is used to identify the project within the EnervaCore framework. It is also used to setup specific runtime data folders.")]
        public string ProjectName = "EnervaCore Test Game";
        private EnervaCoreManager _enervaGameManager;

        private void Awake() {
            //Ensure that the EnervaCoreHandler persists across scene loads to maintain the state of the EnervaCoreManager.
            DontDestroyOnLoad(gameObject);
        }

        void Start() {
            _enervaGameManager = ECM.Main;
            _enervaGameManager.Initialize(ProjectName);            
        }

        void Update() {
            if (_enervaGameManager != null) {
                _enervaGameManager.Update();
            }
        }
    }
}
