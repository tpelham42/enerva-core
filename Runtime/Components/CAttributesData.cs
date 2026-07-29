using System;
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
    public class CAttributesData : ECObjectDataComponent {
        [XmlElement("Attribute")]
        public List<CAttributesXmlRow> Attributes { get; set; } = new List<CAttributesXmlRow>();


        public override void OnLoaded() {
            base.OnLoaded();
        }
    }

    public class CAttributesXmlRow {
        [XmlAttribute("id")]
        public string Id;
    }
}
