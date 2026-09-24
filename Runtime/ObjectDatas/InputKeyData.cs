using EnervaCore;
using System;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Serialization;
using UnityEngine;

namespace EnervaCore {
    public class InputKeyData : ECObjectData {
        [XmlElement("Key")]
        public string KeyName {
            get => KeyCode.ToString();
            set {
                var trimmedValue = value.Trim();

                //Attempt parse of string to keycode
                if (Enum.TryParse<KeyCode>(trimmedValue, true, out var parsed) == false) {
                    // fall back to numeric parse if needed
                    if (int.TryParse(trimmedValue, out var n) && Enum.IsDefined(typeof(KeyCode), n))
                        parsed = (KeyCode)n;
                    else
                        parsed = KeyCode.None; // choose a safe default
                }

                KeyCode = parsed;
            }
        }

        public InputKeyData() {
            if (string.IsNullOrEmpty(SpawnType)) {
                SpawnType = "EnervaCore.Objects.InputKey"; // choose the default you need
            }
        }


        /// <summary>
        /// Stores the value as a unity keycode value
        /// </summary>
        [XmlIgnore]
        public KeyCode KeyCode { get; private set; }
    }
}