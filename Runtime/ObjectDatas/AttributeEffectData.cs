using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace EnervaCore {    
    public class AttributeEffectData : ECObjectData {
        public enum AttributeEffectOperation { Add, Multiply, Override }

        public float Value { get; set; } = 0f;

        [XmlIgnore]
        public AttributeEffectOperation Operation { get; set; } = AttributeEffectOperation.Add;

        // This string property is what XmlSerializer will read/write.
        [XmlElement("Operation")]
        public string OperationString {
            get => Operation.ToString();
            set {
                if (string.IsNullOrWhiteSpace(value)) {
                    Operation = AttributeEffectOperation.Add;
                    return;
                }

                // Try parse by name (case-insensitive)
                if (Enum.TryParse<AttributeEffectOperation>(value, true, out var parsedByName)) {
                    Operation = parsedByName;
                    return;
                }

                // Try parse numeric value
                if (int.TryParse(value, out var intVal) && Enum.IsDefined(typeof(AttributeEffectOperation), intVal)) {
                    Operation = (AttributeEffectOperation)intVal;
                    return;
                }

                // Fallback default
                Operation = AttributeEffectOperation.Add;
            }
        }

        public float Duration { get; set; } = 0f;
    }
}
