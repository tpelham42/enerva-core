using EnervaCore.YieldRoutines;
using EnervaCore.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace EnervaCore {
    public class InputManager : IManager, IManagerGameStart {
        InputKeyData _captureKey;
        YieldRoutine _captureRoutine = null;

        private Dictionary<string, InputKey> _inputKeys;

        public Action<InputManager, InputKeyData, KeyCode> CBKeyCodeCaptured;

        public void Initialize() {
            _inputKeys = new Dictionary<string, InputKey>();
            _captureKey = null;
            _captureRoutine = null;
        }

        public void OnDestroyed() {
            _inputKeys.Clear();
        }

        public void OnGameStart() {
            //Build input key dictionary
            List<InputKeyData> inputKeyList = ECM.DB.GetTemplatesByTypeList<InputKeyData>();
            foreach (var inputKey in inputKeyList) {
                if (_inputKeys.ContainsKey(inputKey.ID))
                    continue;

                InputKey ik = ECM.DB.GetInstance<InputKey>(inputKey.ID);
                if(ik != null) {
                    _inputKeys.Add(inputKey.ID, ik);
                }
            }
        }

        public InputKey GetInputKey(string keyID) {
            if (_inputKeys.ContainsKey(keyID))
                return _inputKeys[keyID];

            return null;
        }

        public bool IsCapturingKey {
            get { return _captureKey != null; }
        }

        public void StartKeyCapture(InputKeyData inputKey) {
            _captureKey = inputKey;

            if (_captureRoutine == null) {
                _captureRoutine = new YieldRoutine(HandleInputKey());
            }
        }

        int GetMouseButtonKey(KeyCode keyCode) {
            int mk0 = 323;
            int codeIndex = (int)keyCode;

            for (int i = 0; i < 7; i++) {
                int current = mk0 + i;
                if (codeIndex == current) {
                    //Return the mouse button index                
                    return codeIndex - mk0;
                }
            }

            return -1;
        }

        public bool GetButton(string id) {
            if (IsCapturingKey)
                return false;

            if (InputEnabled() == false)
                return false;

            if (_inputKeys.ContainsKey(id) == false)
                return false;

            int mouseButton = GetMouseButtonKey(_inputKeys[id].CurrentKey);
            if (mouseButton != -1) {
                return Input.GetMouseButton(mouseButton);
            }

            return Input.GetKey(_inputKeys[id].CurrentKey);
        }

        public bool GetButtonUp(string id) {
            if (IsCapturingKey)
                return false;

            if (InputEnabled() == false)
                return false;

            if (_inputKeys.ContainsKey(id) == false)
                return false;

            int mouseButton = GetMouseButtonKey(_inputKeys[id].CurrentKey);
            if (mouseButton != -1) {
                return Input.GetMouseButtonUp(mouseButton);
            }

            return Input.GetKeyUp(_inputKeys[id].CurrentKey);
        }

        public bool IsModifierShiftHeld {
            get {
                return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            }
        }

        bool InputEnabled() {
            return true;
        }

        IEnumerator HandleInputKey() {

            while (IsCapturingKey) {
                foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))) {
                    if (Input.GetKey(vKey)) {
                        if (CBKeyCodeCaptured != null) {
                            CBKeyCodeCaptured(this, _captureKey, vKey);
                        }

                        yield return new YTWaitForSeconds(0.1f, true);

                        _captureKey = null;
                        _captureRoutine = null;
                    }
                }

                yield return null;
            }
        }
    }
}