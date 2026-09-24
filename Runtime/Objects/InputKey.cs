using UnityEngine;

namespace EnervaCore.Objects {
    public class InputKey : ECObject {

        public InputKeyData KeyData {
            get {
                if (_keyData == null)
                    _keyData = Template as InputKeyData;

                return _keyData;
            }
        }

        public KeyCode CurrentKey {
            get {
                if (_activeKey == KeyCode.None)
                    _activeKey = KeyData.KeyCode;

                return _activeKey;
            }
        }

        public KeyCode DefaultKey {
            get {
                if (KeyData != null) {
                    return KeyData.KeyCode;
                }

                return KeyCode.None;
            }
        }

        

        private InputKeyData _keyData;
        private KeyCode _activeKey = KeyCode.None;
        
    }

}