using EnervaCore.Interfaces;
using System.Collections.Generic;

namespace EnervaCore {
    public class ECObjectInstance : Interfaces.IECObjectInstance {

        public ECObjectData Template { get; set; }

        public List<IECObjectComponentInstance> Components { get; set; } = new List<IECObjectComponentInstance>();

        //Called from DBManager after object has been instantiated and it's template has been set
        public virtual void OnInit() { }

        protected T CastTemplate<T>() where T : ECObjectData {
            if (!(Template is T t)) {
                UnityEngine.Debug.LogError($"{this} :: Template not of expected type {typeof(T).FullName}");
                return null;
            }

            return t;
        }

        public T GetComponentByType<T>() where T : IECObjectComponentInstance {
            foreach (var component in Components) {
                if (component is T t) {
                    return t;
                }
            }

            UnityEngine.Debug.LogError($"{this} :: Component not found for type {typeof(T).FullName}");
            return default(T);
        }
    }
}