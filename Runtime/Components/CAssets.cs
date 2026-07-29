using EnervaCore;
using System.Collections.Generic;
using System.Xml.Serialization;


public class CAssets : ECObjectDataComponent {
    [XmlElement("Asset")]
    public List<string> Assets { get; set; }

    Dictionary<string, System.Object> AssetsDictionary { get; set; } = new Dictionary<string, System.Object>();

    public override void OnLoaded() {
        base.OnLoaded();


        AssetRegistryManager arm = ECM.Main.GetManager<AssetRegistryManager>();
        if(arm == null) {
            UnityEngine.Debug.LogError(this + " :: Asset Registry Manager is null!");
            return;
        }

        arm.OnAssetLoaded += OnRegistryAssetLoaded;

        //Register asset data for AssetRegistry
        foreach (var assetPath in Assets) {
            //Init storage for loaded object
            AssetsDictionary.Add(assetPath, null);

            //Notify Asset Registry. The callback above will pass the object
            //over when it's loaded
            arm.RegisterAssetID(assetPath);
        }
    }

    private void OnRegistryAssetLoaded(string assetID, object assetObject) {
        if (AssetsDictionary.ContainsKey(assetID)) {
            AssetsDictionary[assetID] = assetObject;    
        }
    }

    public T GetAsset<T>(string assetID) where T : class {
        if (AssetsDictionary.ContainsKey(assetID)) {
            return (T)AssetsDictionary[assetID];
        }

        return null;
    }
}