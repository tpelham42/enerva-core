using EnervaCore.Objects;
using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace EnervaCore {
    /**
     * Xml Reference
     * <CAttributesData>
     *   <Attribute id="examaple" />
     *   <Attribute id="example2" />
     * </CAttributesData>
     * 
     *  id value must reference a valid AttributeData record. The default value can optionally be overriden by
     *  setting the value in the InnerText of the Attribute node.
     */
    public class CAttributesData : ECObjectComponentData {
        [XmlElement("Attribute")]
        public List<CAttributesXmlRow> Attributes { get; set; } = new List<CAttributesXmlRow>();

        private Attribute _attribute;


        public override void OnLoaded() {
            base.OnLoaded();
        }

        public Attribute GetAttribute(string AttributeID) {
            if (string.IsNullOrEmpty(AttributeID)) {
                Debug.LogError(this + " :: GetAttribute called with null or empty AttributeID.");
                return null;
            }

            //Returning existing instance if it exists and matches the requested ID
            if (_attribute != null && _attribute.Template.ID == AttributeID) {
                return _attribute;
            }

            
            _attribute = ECM.DB.GetInstance<Attribute>(AttributeID);
            
            if (_attribute == null) {
                Debug.LogError(this + " :: GetAttribute failed to find attribute with ID: " + AttributeID);
                return null;
            }
            return _attribute;
        }
    }

    public class CAttributesXmlRow {
        [XmlAttribute("id")]
        public string Id;
    }
}
