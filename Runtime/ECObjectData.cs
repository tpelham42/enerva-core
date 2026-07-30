
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Android;

namespace EnervaCore {    
    public class ECObjectData {

        [XmlAttribute("id")]
        public string ID { get; set; }

        [XmlAttribute("spawnType")]
        public string SpawnType { get; set; }

        [XmlArray("components")]
        [XmlArrayItem(typeof(ECObjectDataComponent))]
        public List<ECObjectDataComponent> Components { get; set; } = new List<ECObjectDataComponent>();

        public T FindComponent<T>(string tag = null) where T : ECObjectDataComponent {
            foreach (ECObjectDataComponent component in Components) {
                //Attempt to cast component as T
                T rt = component as T;
                if (rt != null) {
                    //if no tag was passed in then just return matching component
                    if (tag == null)
                        return rt;

                    //Else return if it matches tag
                    if(rt.HasTag(tag)) {
                        return rt;
                    }                    
                }
            }

            return null;
        }

        /// <summary>
        /// Returns an asset stored in CAssets by id
        /// </summary>
        /// <typeparam name="T">Class return type </typeparam>
        /// <param name="AssetID">The id defined in CAssets->Asset xml</param>
        /// <returns></returns>
        protected T GetAsset<T>(string AssetID) where T : class {
            CAssets Assets = FindComponent<CAssets>();

            if (Assets == null) {
                UnityEngine.Debug.Log(this + " :: Error Getting Asset. Unable to find CAssets component!");
                return null;
            }

            return Assets.GetAsset<T>(AssetID);
        }

        /// <summary>
        /// Called from ModManager when the corresponding mod has been activated
        /// </summary>
        public virtual void OnDataActivated() { }

        /// <summary>
        /// Called from ModManager when the corresponding mod has been deactivated
        /// </summary>
        public virtual void OnDataDeactivated() { }
    }
}
