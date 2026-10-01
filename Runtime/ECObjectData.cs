
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;



namespace EnervaCore {

    /// <summary>
    /// The base class that acts as adata template for an ECObject, containing its ID, spawn type, tags, and components. 
    /// This class is used to define the values of an ECObject that is loaded from XML data. New data types can be added
    /// by inheriting from ECObjectData. ECObjectData is deserialized from XML by the ModManager and the resulting data is accessed
    /// from the DBManager.
    /// 
    /// Example:
    /// ECM.Main.GetManager<DBManager>().GetTemplatesByType<MyDataType>("myDataID");
    /// 
    /// Example Xml:
    /// <ECObjectData id="myDataID" spawnType="MyNamespace.MyObject">
    /// </ECObjectData>
    /// </summary>
    public class ECObjectData {

        [XmlAttribute("id")]
        public string ID { get; set; }

        [XmlAttribute("spawnType")]
        public string SpawnType { get; set; }

        private string _tagsSerialized;
        private string[] _tagsCache;

        // Serialized as: tags="tag1|tag2|tag3"
        [XmlAttribute("tags")]
        public string TagsSerialized {
            get => _tagsSerialized;
            set {
                _tagsSerialized = value;
                _tagsCache = null;// invalidate cache so Tags property will reload

            }
        }

        public bool HasTag(string tag) {
            if (string.IsNullOrEmpty(tag))
                return true;

            foreach(string t in Tags) {
                if(string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
            }

            return false;
        }

        public bool HasMatchingTags(string[] InTags, TagMatchType TagMatchType) {
            // No filter tags => consider it a match
            if (InTags == null || InTags.Length == 0) return true;

            // Ensure we use the parsed Tags array (handles lazy parsing and empty-case)
            var myTags = Tags ?? Array.Empty<string>();

            if (TagMatchType == TagMatchType.MatchAll) {
                // All provided tags must exist on this object (case-insensitive)
                return InTags.All(t => myTags.Any(mt => string.Equals(mt, t, StringComparison.OrdinalIgnoreCase)));
            }

            // MatchAny: at least one provided tag exists on this object (case-insensitive)
            return InTags.Any(t => myTags.Any(mt => string.Equals(mt, t, StringComparison.OrdinalIgnoreCase)));
        }

        // Runtime view of tags as string[]
        [XmlIgnore]
        public string[] Tags {
            get {
                //If the cache is already populated, return it. 
                if (_tagsCache != null) return _tagsCache;

                //Otherwise, parse the serialized string into an array and cache it.
                if (string.IsNullOrEmpty(_tagsSerialized)) return _tagsCache = new string[0];

                _tagsCache = _tagsSerialized
                    .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => Uri.UnescapeDataString(s).Trim())
                    .ToArray();

                return _tagsCache;
            }
            set {
                _tagsCache = value ?? new string[0];
                // Escape each tag to allow '|' or other special chars inside tags
                _tagsSerialized = string.Join("|", _tagsCache.Select(t => Uri.EscapeDataString((t ?? string.Empty).Trim())));
            }
        }

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

        public List<T> FindComponents<T>(string tag = null) where T : ECObjectDataComponent {
            List<T> matchingComponents = new List<T>();
            
            foreach (ECObjectDataComponent component in Components) {
                //Attempt to cast component as T
                T rt = component as T;
                if (rt != null) {
                    //if no tag was passed in then just return matching component
                    if (tag == null) {
                        matchingComponents.Add(rt);
                    } else if(rt.HasTag(tag)) {
                        matchingComponents.Add(rt);
                    }
                }
            }

            return matchingComponents;
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
