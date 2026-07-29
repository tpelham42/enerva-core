using System;
using System.Linq;
using System.Xml.Serialization;

namespace EnervaCore {
    public class ECObjectDataComponent {

        [XmlIgnore]
        public ECObjectData Parent { get; set; }

        [XmlAttribute("id")]
        public string ID { get; set; }

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

        
        public bool HasTag(string tag) {
            if (string.IsNullOrEmpty(tag)) 
                return false;

            return Tags.Any(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase));
        }

        public void AddTag(string tag) {
            if (string.IsNullOrEmpty(tag)) 
                return;
            
            var list = Tags.ToList();
            if (!list.Any(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase))) {
                list.Add(tag);
                Tags = list.ToArray(); // will update serialized string
            }
        }

        //Called by ModManager after all components have been parsed and added to parent
        public virtual void OnLoaded() { }
    }
}
