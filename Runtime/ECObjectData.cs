using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor.Build.Content;
using UnityEngine;

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
    }
}
