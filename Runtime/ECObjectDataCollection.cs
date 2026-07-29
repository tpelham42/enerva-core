using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace EnervaCore {
    /**
     * Represents a collection of ECObjectData objects.
     * This class is used for XML serialization and deserialization of object data collections.
     * <ObjectDatas>
     *   <ECObjectData>
     *   ...
     *   </ECObjectData>
     *   ...
     * </ECObjectDatas>
     * */
    [XmlRoot("ObjectDatas")]
    public class ECObjectDataCollection {        
        public List<ECObjectData> Objects { get; set; } = new List<ECObjectData>();
    }
}
