using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnervaCore {
    public class AttributeData : ECObjectData {
        
        public float BaseValue { get; set; }

        public float MinValue { get; set; } = float.MinValue;
        public float MaxValue { get; set; } = float.MaxValue;

        public AttributeData() {
            if (string.IsNullOrEmpty(SpawnType)) {
                SpawnType = "EnervaCore.Objects.Attribute"; // choose the default you need
            }
        }
    }
}
