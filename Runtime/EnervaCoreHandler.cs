using UnityEngine;

namespace EnervaCore {
    /**
     * Attaches to a GameObject in the scene and initializes the EnervaCore framework. 
     * It also calls the Update method of the GameManager every frame. 
     */
    public class EnervaCoreHandler : MonoBehaviour {
        [Tooltip("Project Name is used to identify the project within the EnervaCore framework. It is also used to setup specific runtime data folders.")]
        public string ProjectName = "EnervaCore Test Game";
        private EnervaCoreManager _enervaGameManager;


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
