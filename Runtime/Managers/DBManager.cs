using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace EnervaCore.Managers {
    public class DBManager : IManager {

        //Flat list of all ECObjectData items from all mods, for easy access        
        public Dictionary<string, ECObjectData> ItemTemplatesByName;
        public Dictionary<Type, HashSet<ECObjectData>> ItemTemplatesByType;
        public Dictionary<string, Dictionary<Type, HashSet<ECObjectData>>> ItemTemplatesByModAndType;


        public DBManager() {
            ItemTemplatesByName = new Dictionary<string, ECObjectData>();
            ItemTemplatesByType = new Dictionary<Type, HashSet<ECObjectData>>();
            ItemTemplatesByModAndType = new Dictionary<string, Dictionary<Type, HashSet<ECObjectData>>>();  
        }

        #region IManager Implementation
        public void Initialize() {
            
        }

        public void OnDestroyed() {
            ClearData();
        }
        #endregion

        public T GetInstance<T>(string templateName) where T : ECObject {
            if (!ItemTemplatesByName.ContainsKey(templateName)) {
                UnityEngine.Debug.LogWarning(this + " :: GetInstance :: Template with name " + templateName + " does not exist in the database.");
                return null;
            }

            var template = ItemTemplatesByName[templateName];

            string spawnTypeName = template.SpawnType;
            Type spawnType = null;

            if (!string.IsNullOrWhiteSpace(spawnTypeName)) {
                // Try to resolve exactly first (assembly-qualified name)
                spawnType = Type.GetType(spawnTypeName);

                // If not found, search loaded assemblies for a matching full name or short name
                if (spawnType == null) {
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                        try {
                            // exact with namespace
                            spawnType = asm.GetType(spawnTypeName);
                            if (spawnType != null) break;

                            // fallback: match by simple type name (e.g. "MyObjectClass")
                            spawnType = asm.GetTypes().FirstOrDefault(t => t.Name == spawnTypeName);
                            if (spawnType != null) break;
                        }
                        catch (ReflectionTypeLoadException) {
                            // ignore assemblies that can't be reflected fully
                        }
                    }
                }
            }
            else {
                UnityEngine.Debug.LogError(this + $" :: GetInstance Error. Spawn Type for {template.ID} not defined!");
            }

            ECObject instance = null;

            try {
                if (spawnType != null && typeof(T).IsAssignableFrom(spawnType)) {
                    instance = (T)Activator.CreateInstance(spawnType);
                }
                else {
                    // fallback to a plain ECObject if spawnType is missing/invalid
                    instance = default(T);
                }

                // assign template
                instance.Template = template;
                instance.OnInit();
            }
            catch (Exception ex) {
                UnityEngine.Debug.LogError(this + " :: GetInstance :: Failed to create instance for type " + spawnTypeName + " - " + ex);
                return null;
            }

            return instance as T;
        }

        public void AddTemplate(GameMod mod, ECObjectData template) {
            if (template == null) {
                throw new Exception(this + $" :: Unable to Add Template to DB. Template is null!");
            }

            if (string.IsNullOrEmpty(template.ID)) {
                throw new Exception(this + $" :: Unable to add template to DB. Id value not set for passed in type {template.GetType().ToString()}");
            }

            // Add the template to the flat list
            if (ItemTemplatesByName.ContainsKey(template.ID)) {
                throw new Exception(this +  $" :: Template with ID {template.ID} already exists in the database.");
            }

            ItemTemplatesByName[template.ID] = template;

            //Add the template to the dictionary by type
            if (!ItemTemplatesByType.ContainsKey(template.GetType())) {
                ItemTemplatesByType[template.GetType()] = new HashSet<ECObjectData>();
            }
            
            if (ItemTemplatesByType[template.GetType()].Contains(template)) {
                throw new Exception($"Template with ID {template.ID} already exists in the database for type {template.GetType()}.");
            }

            ItemTemplatesByType[template.GetType()].Add(template);


            // Add the template to the dictionary by mod and type
            if (!ItemTemplatesByModAndType.ContainsKey(mod.ID)) {
                ItemTemplatesByModAndType[mod.ID] = new Dictionary<Type, HashSet<ECObjectData>>();
            }

            if (!ItemTemplatesByModAndType[mod.ID].ContainsKey(template.GetType())) {
                ItemTemplatesByModAndType[mod.ID][template.GetType()] = new HashSet<ECObjectData>();
            }

            if (ItemTemplatesByModAndType[mod.ID][template.GetType()].Contains(template)) {
                throw new Exception($"Template with ID {template.ID} already exists in the database for mod {mod.ID} and type {template.GetType()}.");
            }

            ItemTemplatesByModAndType[mod.ID][template.GetType()].Add(template);
        }
        

        public void RemoveTemplate(string templateID) {
           if(!ItemTemplatesByName.ContainsKey(templateID)) {
                UnityEngine.Debug.LogWarning(this + " :: RemoveTemplate :: Template with ID " + templateID + " does not exist in the database.");
                return;
            }

            var templateToRemove = ItemTemplatesByName[templateID];
            ItemTemplatesByName.Remove(templateID);

            if(ItemTemplatesByType.ContainsKey(templateToRemove.GetType())) {
                ItemTemplatesByType[templateToRemove.GetType()].Remove(templateToRemove);
            }
        }

        public T GetTemplateByType<T>(string templateID) where T : ECObjectData {
            if(!ItemTemplatesByName.ContainsKey(templateID)) {
                return null;
            }
            
            var template = ItemTemplatesByName[templateID];            
            if(template is T typedTemplate) {
                return typedTemplate;
            }

            return null;
        }

        /// <summary>
        /// Returns a HashSet of ECObjectData matching the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public HashSet<ECObjectData> GetTemplatesByType<T>() where T : ECObjectData {
            if(!ItemTemplatesByType.ContainsKey(typeof(T))) {
                return null;
            }

            return ItemTemplatesByType[typeof(T)];
        }

        /// <summary>
        /// Returns a List<typeparamref name="T"/> of objects matching the specified concrete type as well as objects that are assignable from T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetTemplatesByTypeList<T>() where T : ECObjectData {
            var result = new List<T>();
            var targetType = typeof(T);

            if (ItemTemplatesByType == null || ItemTemplatesByType.Count == 0) {
                return result;
            }

            // ItemTemplatesByType uses concrete template types as keys.
            // Collect all entries whose key type is assignable to T (includes derived types),
            // and cast each ECObjectData to T when possible.
            foreach (var kvp in ItemTemplatesByType) {
                var storedType = kvp.Key;
                if (targetType.IsAssignableFrom(storedType)) {
                    foreach (var data in kvp.Value) {
                        if (data is T typed) {
                            result.Add(typed);
                        }
                    }
                }
            }

            return result;
        }

        public Dictionary<Type, HashSet<ECObjectData>> GetTemplatesByTypeByMod(string modID) {
            if(!ItemTemplatesByModAndType.ContainsKey(modID)) {
                return null;
            }

            return ItemTemplatesByModAndType[modID];
        }

        private void ClearData() {
            ItemTemplatesByName.Clear();
            ItemTemplatesByType.Clear();
        }
        
    }
}