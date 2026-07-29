using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;


namespace EnervaCore {

    /**
     * Handles loading and parsing of mod xml data from mod folders. 
     *
     * Mod Folder Structure:
     *   - Mods/ModName/ModInfo.xml
     *     > Mods/ModName/Data/*.xml
     */
    public class ModManager : IManager, IManagerGMInitialized {
        
        private Dictionary<string, GameMod> _mods;
        private Dictionary<string, GameMod> _enabledMods;

        private GameMod CoreMod;

        //Stores a list of registered data types.
        private Dictionary<string, Type> _ecObjectDataTypes;
        private Dictionary<string, Type> _ecObjectDataComponentTypes;

        // Xml serializer caches
        private readonly Dictionary<Type, XmlSerializer> _serializerCache = new Dictionary<Type, XmlSerializer>();

        public Dictionary<string, GameMod> Mods => _mods;        

        public void Initialize() {

            _mods = new Dictionary<string, GameMod>();
            _enabledMods = new Dictionary<string, GameMod>();

            InitializeModRegistry();

            LoadModFolders();

            CoreMod = FindModByID(ECM.Settings.CoreModID);            
        }

        //Called after all other managers have been initialized. This is where we can safely activate
        //the core mod and any other mods that need to be activated after the game manager is initialized.
        public void OnGameManagerInitialized() {
            if (CoreMod == null) {
                Debug.LogError(this + " :: Core Mod Not Found! Please ensure the core mod is installed and has a valid modinfo.xml file.");
                return;
            }

            CoreMod.Activate();
        }

        /// Initializes the mod registry by using reflection to find all subclasses of ECObjectData and storing them in a dictionary for later use.
        public void InitializeModRegistry() {
            //Init ECObjectData Types Dictionary
            if (_ecObjectDataTypes != null) {
                _ecObjectDataTypes.Clear();
            }
            else {
                _ecObjectDataTypes = new Dictionary<string, Type>();
            }

            // Force reflection to find all subclasses of ECObjectData
            var subTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(ECObjectData).IsAssignableFrom(p) && !p.IsAbstract);

            foreach (var type in subTypes) {
                _ecObjectDataTypes.Add(type.Name, type);                
            }
            
            
            //Init ECObjectData Component Types Dictionary
            if (_ecObjectDataComponentTypes != null) {
                _ecObjectDataComponentTypes.Clear();
            }
            else {
                _ecObjectDataComponentTypes = new Dictionary<string, Type>();
            }
            
            // Force reflection to find all subclasses of ECObjectData
            subTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(ECObjectDataComponent).IsAssignableFrom(p) && !p.IsAbstract);

            foreach (var type in subTypes) {                
                _ecObjectDataComponentTypes.Add(type.Name, type);
            }
        }

        private void LoadModFolders() {
            //Load Internal Mods (Primarily Core)
            Debug.Log(this + " :: Loading Internal Mods From: " + ECM.Settings.InternalModPath);
            DirectoryInfo dirInfo = new DirectoryInfo(ECM.Settings.InternalModPath);
            LoadModsFromFolder(dirInfo);

            //Load Public Mods (User Mods)
            Debug.Log(this + " :: Loading Public Mods From: " + ECM.Settings.PublicModPath);
            dirInfo = new DirectoryInfo(ECM.Settings.PublicModPath);
            LoadModsFromFolder(dirInfo);
        }

        //Load all mods from a given directory. Each subdirectory is treated as a mod folder.
        void LoadModsFromFolder(DirectoryInfo dirInfo) {
            if(dirInfo.Exists == false) {
                Debug.LogWarning(this + " :: Mod Directory Does Not Exist: " + dirInfo.FullName);
                return;
            }

            DirectoryInfo[] modFolders = dirInfo.GetDirectories();
            foreach (DirectoryInfo modFolder in modFolders) {
                GameMod gm = CreateModFromFolder(modFolder);

                if (gm == null)
                    continue;

                gm.Source = GameModSource.Internal;

                _mods.Add(gm.ModFolderPath, gm);
            }
        }

        //Create a GameMod object from a given folder. The folder must contain a modinfo.xml file.
        private GameMod CreateModFromFolder(DirectoryInfo directoryInfo) {
            GameMod gameMod = null;
            ModInfo modInfo = null;

            //Get array of files form directoryInfo
            FileInfo[] rootModFiles = directoryInfo.GetFiles();
            foreach (FileInfo modFile in rootModFiles) {

                //Load ModInfo.xml in found folder under mods folder
                if (modFile.Name.ToLower() == "modinfo.xml") {
                    
                    using (var stream = new FileStream(modFile.FullName, FileMode.Open)) {
                        var xml = new XmlSerializer(typeof(ModInfo));
                        
                        try {
                            modInfo = (ModInfo)xml.Deserialize(stream);
                        }
                        catch (Exception e) {
                            Debug.LogError(this + " :: Error Loading ModInfo In Mod Directory '" + directoryInfo.Name + "'. Skipping load.\n Details: " + e.ToString());
                        }

                        if (modInfo != null && modInfo.IsValid()) {
                            gameMod = new GameMod(directoryInfo.FullName, modInfo);
                            break;
                        }
                    }                    
                }
            }

            if(gameMod != null) {
                Debug.Log(this + " :: Mod Info Found. Name: " + modInfo.Name);
                //Load Game Mod Data Files
                LoadGameModData(gameMod);
            }

            return gameMod;
        }

        void LoadGameModData(GameMod GameMod) {
            if (GameMod == null) {
                Debug.LogError(this + " :: LoadGameModData Error. Passed In GameMod is null!");
                return;
            }

            //Clear any existing data in case this is a reload
            GameMod.ClearData();

            DirectoryInfo modDir = new DirectoryInfo(GameMod.ModFolderPath);
            DirectoryInfo[] modDirs = modDir.GetDirectories();

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;

            //Look at each directory in the mods folder as xml data files 
            //should be in subdirectories. Only modinfo.xml should be in the root
            //of the mod directory
            foreach (DirectoryInfo subDir in modDirs) {
                //Loop over each *.xml file in the directory
                FileInfo[] files = subDir.GetFiles("*.xml");

                foreach (FileInfo file in files) {
                    bool isCollection = false;

                    // try collection-per-element first
                    try {
                        List<ECObjectData> collection = DeserializeCollectionPerElement(file.FullName);
                        if (collection != null && collection.Count > 0) {
                            foreach (ECObjectData item in collection) {
                                if (item == null) {
                                    Debug.LogError(this + $" :: Deserialized collection contains a null item!");
                                    continue;
                                }
                                
                                if (string.IsNullOrEmpty(item.ID)) {
                                    Debug.LogError(this + $" :: Error parsing ECObjectData of type '{item.GetType().ToString()}' in file '{file.Name}'. Id is undefined. Skipped");
                                    continue;
                                }

                                GameMod.AddData(item);                                
                            }

                            isCollection = true;
                        }
                        
                    }
                    catch (Exception e) {
                        UnityEngine.Debug.LogError($"ModManager :: Collection parse failed for {file.FullName}: {e}");
                    }

                    if (!isCollection) {
                        ECObjectData item = DeserializeSingle(file, GameMod); // your existing single-item deserializer
                        if (item != null) {
                            if (string.IsNullOrEmpty(item.ID) == false) {
                                GameMod.AddData(item);
                            }
                            else {
                                Debug.LogError(this + $" :: Error parsing ECObjectData of type '{item.GetType().ToString()}' in file '{file.Name}'. Id is undefined. Skipped");
                            }   
                        }
                        else {
                            Debug.LogWarning(this + $" :: Failed to deserialize single item from {file.FullName}. Skipping.");  
                        }

                        
                    }
                }
            }
        }

        private XmlSerializer GetOrCreateSerializerForType(Type type) {
            //Get a cached serializer if it exists, otherwise create a new one and cache it.
            if (_serializerCache.TryGetValue(type, out var cached)) return cached;

            // For ECObjectData-derived types include component types as extraTypes so nested <components> elements work.
            XmlSerializer serializer;
            if (typeof(ECObjectData).IsAssignableFrom(type)) {
                var compTypes = _ecObjectDataComponentTypes?.Values.ToArray() ?? new Type[0];
                serializer = new XmlSerializer(type, compTypes);
            }
            else {
                serializer = new XmlSerializer(type);
            }

            _serializerCache[type] = serializer;
            return serializer;
        }

        // Deserializes a collection file by iterating child elements and resolving types at runtime.
        private List<ECObjectData> DeserializeCollectionPerElement(string filePath) {
            var results = new List<ECObjectData>();
            var settings = new XmlReaderSettings { IgnoreWhitespace = true, IgnoreComments = true };

            using (var reader = XmlReader.Create(filePath, settings)) {
                reader.MoveToContent(); // positioned at wrapper root e.g. <ObjectDatas>

                if (reader.IsEmptyElement) return results;

                // Iterate over nodes to find and parse elements               
                while (reader.Read()) {
                    if (reader.NodeType != XmlNodeType.Element) continue;

                    string elementName = reader.Name;
                    if (!_ecObjectDataTypes.TryGetValue(elementName, out var concreteType)) {
                        concreteType = Type.GetType(elementName);
                    }

                    if (concreteType == null) {
                        UnityEngine.Debug.LogWarning(this + $" :: Unknown data element '{elementName}' in {filePath}. Skipping.");
                        reader.Skip();
                        continue;
                    }

                    // Load the entire element into an XDocument so we can both deserialize and inspect nested nodes
                    using (var subtree = reader.ReadSubtree()) {
                        try {
                            var xdoc = XDocument.Load(subtree);
                            var root = xdoc.Root;
                            if (root == null) continue;

                            // Deserialize the ECObjectData from the XElement
                            var ser = GetOrCreateSerializerForType(concreteType);
                            ECObjectData obj = null;
                            using (var r = root.CreateReader()) {
                                obj = (ECObjectData)ser.Deserialize(r);
                            }

                            if (obj != null) {
                                // Manually deserialize components using registry
                                var comps = DeserializeComponentsFromElement(root);
                                if (comps != null && comps.Count > 0) {
                                    obj.Components = comps;
                                    // set parent pointer if needed
                                    foreach (var c in obj.Components)
                                        c.Parent = obj;
                                }

                                //Notify all components that they've been loaded
                                foreach(var  c in obj.Components) {
                                    c.OnLoaded();
                                }

                                results.Add(obj);
                            }
                        }
                        catch (Exception ex) {
                            UnityEngine.Debug.LogError(this + $" :: Failed to deserialize element '{elementName}' in {filePath}: {ex}");
                        }
                    }
                    // outer reader will be positioned on EndElement; loop continues
                }
            }

            return results;
        }

        ECObjectData DeserializeSingle(FileInfo file, GameMod GameMod) {
            //Determine Class Type From Root Node Name
            string classTypeName = GetRootNodeClassType(file.FullName);
            Type classType = _ecObjectDataTypes.ContainsKey(classTypeName) ? _ecObjectDataTypes[classTypeName] : null;

            if (classType == null) {
                Debug.LogWarning(this + " :: Class Type Not Found In Registry For Game Data: " + classTypeName.ToString());
                classType = Type.GetType(classTypeName);
                return null;
            }

            if (classType == null || classType.IsNotPublic) {
                Debug.LogError(this + " :: Error Finding Class Type For Game Data: " + classType.ToString());
                return null;
            }

            ECObjectData giData = null;

            var settings = new XmlReaderSettings { IgnoreWhitespace = true, IgnoreComments = true };
            using (var reader = XmlReader.Create(file.FullName, settings)) {
                reader.MoveToContent();
                // Load full document for this root into XDocument
                var xdoc = XDocument.Load(reader);
                var root = xdoc.Root;
                if (root == null) return null;

                try {
                    var serializer = new XmlSerializer(classType, _ecObjectDataComponentTypes.Values.ToArray());
                    using (var r = root.CreateReader()) {
                        giData = (ECObjectData)serializer.Deserialize(r);
                    }

                    if (giData != null) {
                        // manually ensure components populated via registry-aware parsing
                        var comps = DeserializeComponentsFromElement(root);
                        if (comps != null && comps.Count > 0) {
                            giData.Components = comps;
                            foreach (var c in giData.Components) c.Parent = giData;
                        }
                    }
                }
                catch (Exception e) {
                    Debug.LogError(this + " :: Error Loading GameItemData('" + file.Name + "') In Mod Directory '" + GameMod.ModFolderPath + "'. Skipping load.\n Details: " + e.ToString());
                    return null;
                }
            }

            return giData;
        }

        private List<ECObjectDataComponent> DeserializeComponentsFromElement(XElement rootElement) {
            var results = new List<ECObjectDataComponent>();

            if (rootElement == null) return results;

            var compsElement = rootElement.Element("components");
            if (compsElement == null) return results;

            foreach (var child in compsElement.Elements()) {
                string elementName = child.Name.LocalName;

                if (!_ecObjectDataComponentTypes.TryGetValue(elementName, out var compType)) {
                    compType = Type.GetType(elementName);
                }

                if (compType == null) {
                    Debug.LogWarning(this + $" :: Unknown component element '{elementName}'. Skipping.");
                    continue;
                }

                try {
                    var serializer = GetOrCreateSerializerForType(compType);
                    using (var reader = child.CreateReader()) {
                        var compObj = (ECObjectDataComponent)serializer.Deserialize(reader);
                        if (compObj != null) {
                            Debug.Log(this + $" :: Deserialized component: {compObj.GetType()}");
                            results.Add(compObj);
                        }
                    }
                }
                catch (Exception ex) {
                    Debug.LogError(this + $" :: Failed to deserialize component '{elementName}': {ex}");
                }
            }

            return results;
        }

        public string GetRootNodeClassType(string filePath) {
            // XmlReaderSettings allows us to ignore whitespace and comments
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;

            using (XmlReader reader = XmlReader.Create(filePath, settings)) {
                // Moves to the first element (the root)
                if (reader.MoveToContent() == XmlNodeType.Element) {
                    // GetAttribute returns the value of the attribute by name
                    return reader.Name;
                }
            }

            return null; 
        }

        public GameMod FindModByID(string modID) {
            foreach (var mod in _mods.Values) {
                if (mod.ID == modID) {
                    return mod;
                }
            }
            return null;
        }



        public void OnDestroyed() {
            
        }

        
    }
}

