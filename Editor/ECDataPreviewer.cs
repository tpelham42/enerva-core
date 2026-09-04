using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EnervaCore {
    
    public class ECDataPreviewer : EditorWindow {

        const int MaxItemsToShow = 12;
        const int MaxDepth = 3;

        private Dictionary<string, bool> _modFoldoutStates = new Dictionary<string, bool>();
        private Dictionary<string, bool> _typesFoldoutStates = new Dictionary<string, bool>();
        private Dictionary<string, bool> _objectDataFoldoutStates = new Dictionary<string, bool>();

        [MenuItem("Window/EnervaCore/EC Data Previewer")]
        static void BuildWindow() {
            
            ECDataPreviewer previewWindow = (ECDataPreviewer)EditorWindow.GetWindow(typeof(ECDataPreviewer));
            previewWindow.titleContent = new GUIContent("EC Data Previewer");
            previewWindow.Show();
            previewWindow.Init();
        }

        void Init() {
            // Initialization code here            
            Debug.Log(this + " :: EC Data Previewer Initialized.");
        }

        void OnGUI() {
            if(Application.isPlaying == false) {
                EditorGUILayout.HelpBox("EC Data Previewer is only available in Play Mode.", MessageType.Info);
                return;
            }

            ModManager MM = ECM.Main.GetManager<ModManager>();
           
            foreach (var modGroup in ECM.DB.ItemTemplatesByModAndType) {
                GameMod mod = MM.FindModByID(modGroup.Key);

                if(mod == null) {
                    EditorGUILayout.HelpBox($"Mod with ID '{modGroup.Key}' not found in ModManager.", MessageType.Warning);
                    continue;
                }

                RenderMod(modGroup.Key, mod);
            }
        }

        void RenderMod(string modID, GameMod mod) {
            if(_modFoldoutStates.ContainsKey(modID) == false) {
                _modFoldoutStates[modID] = false; // Initialize foldout state for this mod
            }

            _modFoldoutStates[modID] = EditorGUILayout.BeginFoldoutHeaderGroup(_modFoldoutStates[modID], $"Mod: {mod.ID}");                      

            if (_modFoldoutStates[modID]) {
                Dictionary<Type, HashSet<ECObjectData>> templatesByType = ECM.DB.GetTemplatesByTypeByMod(modID);
                Debug.Log(this + " :: Rendering Mod: " + modID + " with " + templatesByType.Count + " types of templates.");

                EditorGUI.indentLevel++; // Increase indent
                foreach (var ObjectDataByType in templatesByType) {
                    RenderObjectDataType(ObjectDataByType.Key, ObjectDataByType.Value);
                }
                EditorGUI.indentLevel--; // Decrease indent
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void RenderObjectDataType(Type ObjectDataType, HashSet<ECObjectData> ObjectDataSet) {
            
            string ObjectDataTypeName = ObjectDataType.Name;
            if (_typesFoldoutStates.ContainsKey(ObjectDataTypeName) == false) {
                _typesFoldoutStates[ObjectDataTypeName] = false; // Initialize foldout state for this type
            }

            _typesFoldoutStates[ObjectDataTypeName] = EditorGUILayout.Foldout(_typesFoldoutStates[ObjectDataTypeName], $"{ObjectDataType.Name}");

            if (!_typesFoldoutStates[ObjectDataTypeName]) {
                return; // Skip rendering properties if the foldout is collapsed
            }

            EditorGUI.indentLevel++; // Increase indent
            foreach (var ObjectData in ObjectDataSet) {
                RenderObjectData(ObjectData);
            }
            EditorGUI.indentLevel--; // Decrease indent
        }

        void RenderObjectData(ECObjectData ObjectData) {

            if(_objectDataFoldoutStates.ContainsKey(ObjectData.ID) == false) {
                _objectDataFoldoutStates[ObjectData.ID] = false; // Initialize foldout state for this object data
            }

            _objectDataFoldoutStates[ObjectData.ID] = EditorGUILayout.Foldout(_objectDataFoldoutStates[ObjectData.ID], $"ID: {ObjectData.ID}");

            if (_objectDataFoldoutStates[ObjectData.ID] == false)
                return;

            PropertyInfo[] properties = ObjectData.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties) {
                /*
                if (property.Name.Equals("ID", StringComparison.OrdinalIgnoreCase)
                    || property.Name.Equals("hideflags", StringComparison.OrdinalIgnoreCase)
                    ) {
                    continue;
                }*/
                if (!property.CanRead) continue;
                var value = property.GetValue(ObjectData);
                var display = ECDataPreviewer.FormatValue(value);
                EditorGUILayout.LabelField($"  {property.Name}: {display}");
            }

            foreach (var component in ObjectData.Components) {
                EditorGUILayout.LabelField($"  Component: {component.GetType().Name}");
                PropertyInfo[] componentProperties = component.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var property in componentProperties) {
                    if (property.Name.Equals("hideflags", StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }
                    EditorGUILayout.LabelField($"    {property.Name}: {property.GetValue(component)}");
                }
            }
        }

        public static string FormatValue(object value) => FormatValueInternal(value, 0);

        static string FormatValueInternal(object value, int depth) {
            if (depth > MaxDepth) return "…";
            if (value == null) return "null";

            var type = value.GetType();

            // primitives, enums, decimals
            if (type.IsPrimitive || value is decimal || value is DateTime || value is TimeSpan) return value.ToString();
            if (type.IsEnum) return value.ToString();
            if (value is string s) return $"\"{s}\"";

            // non-generic IDictionary (Hashtable, etc.)
            if (value is IDictionary nonGenericDict) {
                return FormatDictionaryEnumerator(nonGenericDict.Cast<DictionaryEntry>(), depth);
            }

            // generic IDictionary<,> detection
            var dictInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            if (dictInterface != null) {
                // IDictionary<,> implements IEnumerable of KeyValuePair<,>
                var enumerable = value as IEnumerable;
                if (enumerable != null) {
                    // Reflect over Key/Value of each KeyValuePair item
                    var items = new List<string>();
                    int count = 0;
                    foreach (var item in enumerable) {
                        if (count++ >= MaxItemsToShow) { items.Add("…"); break; }
                        var itemType = item.GetType();
                        var keyProp = itemType.GetProperty("Key");
                        var valProp = itemType.GetProperty("Value");
                        var keyStr = keyProp != null ? FormatValueInternal(keyProp.GetValue(item), depth + 1) : "<key?>";
                        var valStr = valProp != null ? FormatValueInternal(valProp.GetValue(item), depth + 1) : "<val?>";
                        items.Add($"{keyStr}: {valStr}");
                    }
                    return "{" + string.Join(", ", items) + "}";
                }
            }

            // Any other IEnumerable (lists, arrays, collections) except string
            if (value is IEnumerable enumerableValue) {
                var items = new List<string>();
                int count = 0;
                foreach (var item in enumerableValue) {
                    if (count++ >= MaxItemsToShow) { items.Add("…"); break; }
                    items.Add(FormatValueInternal(item, depth + 1));
                }
                return "[" + string.Join(", ", items) + "]";
            }

            // Fallback - try ToString() but be defensive about default object.ToString()
            var toString = value.ToString();
            if (!string.IsNullOrEmpty(toString) && toString != type.FullName) return toString;

            // Fallback - reflect public properties to give a brief summary
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.CanRead)
                            .Take(6)
                            .Select(p => $"{p.Name}={FormatValueInternal(p.GetValue(value), depth + 1)}");
            return $"{type.Name}{{{string.Join(", ", props)}}}";
        }

        static string FormatDictionaryEnumerator(IEnumerable<DictionaryEntry> entries, int depth) {
            var items = new List<string>();
            int count = 0;
            if(entries == null) return "{}";
            foreach (var de in entries) {
                if (count++ >= MaxItemsToShow) { items.Add("…"); break; }
                items.Add($"{FormatValueInternal(de.Key, depth + 1)}: {FormatValueInternal(de.Value, depth + 1)}");
            }
            return "{" + string.Join(", ", items) + "}";
        }
    }
}
