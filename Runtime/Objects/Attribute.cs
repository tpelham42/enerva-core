using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EnervaCore.Objects {
    public class Attribute : ECObject {
        public float BaseValue { get; set; } = 0f;
        public float CurrentValue { get; set; } = 0f;
        public List<AttributeEffectData> Effects { get; set; } = new List<AttributeEffectData>();

        public override void OnInit() {
            base.OnInit();
            AttributeData data = Template as AttributeData;
            if (data != null) {                
                BaseValue = data.BaseValue;
            }
            else {
                Debug.LogError(this + " :: Template Data is null!");
            }

            CalculateCurrentValue();
        }

        public void AddEffect(AttributeEffectData effect) {
            if(effect == null) {
                Debug.LogError(this + " :: Attempted to add a null effect to attribute.");
                return;
            }

            Effects.Add(effect);

            CalculateCurrentValue();
        }

        public void RemoveEffect(AttributeEffectData effect) {
            if (effect == null) {
                Debug.LogError(this + " :: Attempted to remove a null effect to attribute.");
                return;
            }

            Effects.Remove(effect);

            CalculateCurrentValue();
        }

        private void CalculateCurrentValue() {
            float AddSum = 0f;
            float MultiplierSum = 0f;
            float OverrideValue = 0f;
            bool hasOverride = false;

            foreach (var effect in Effects) {
                switch (effect.Operation) {
                    case AttributeEffectData.AttributeEffectOperation.Add:
                        AddSum += effect.Value;
                        break;
                    case AttributeEffectData.AttributeEffectOperation.Multiply:
                        MultiplierSum += effect.Value;
                        break;
                    case AttributeEffectData.AttributeEffectOperation.Override:
                        OverrideValue = effect.Value;
                        hasOverride = true;
                        break;
                }
            }

            if(hasOverride) {
                CurrentValue = OverrideValue;
            } else {
                CurrentValue = (BaseValue + AddSum) * (1f + MultiplierSum);
            }

            AttributeData attributeData = (AttributeData)Template;
            Mathf.Clamp(CurrentValue, attributeData.MinValue, attributeData.MaxValue);
        }

    }
}
