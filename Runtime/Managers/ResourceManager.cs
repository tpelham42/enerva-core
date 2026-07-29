using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EnervaCore {

    public class ResourceManager : IManager{
        //Dictionary of Dictionaries
        //Top level key is mod id => (Resource Path => Resource Object)
        //
        Dictionary<string, Dictionary<string, UnityEngine.Object>> _loadedResources;

        #region IManager Implementation
        public void Initialize() {
            _loadedResources = new Dictionary<string, Dictionary<string, UnityEngine.Object>>();
        }

        public void OnDestroyed() {
            _loadedResources.Clear();
        }

        #endregion

        public T GetResource<T>(string resourcePath, string modid = null) where T : UnityEngine.Object {
            if (string.IsNullOrEmpty(resourcePath))
                return default(T);

            if (modid == null) {
                modid = ECM.Settings.CoreModID;
            }

            if (_loadedResources.ContainsKey(modid)) {
                if (_loadedResources[modid].ContainsKey(resourcePath)) {
                    return _loadedResources[modid][resourcePath] as T;
                }
            }
            else {
                _loadedResources.Add(modid, new Dictionary<string, UnityEngine.Object>());
            }

            T resource = Resources.Load<T>(resourcePath);
            if (resource != null) {
                _loadedResources[modid].Add(resourcePath, resource);
                
            }

            return resource;
        }
    }
}
