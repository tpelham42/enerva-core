using EnervaCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;


/// <summary>
/// Loads a list of <Asset>PathKey</Asset> and handles registering them with the AssetRegistry System. Data what uses this component
/// can use GetAsset<T> to get a specified asset.
/// </summary>
public class CAssets : ECObjectDataComponent {
    [XmlElement("Asset")]
    public List<CAssetXmlRow> Assets { get; set; }

    [XmlIgnore]
    //Asset ID attribute to Addressables Path
    public Dictionary<string, string> AssetIDReferenceDictionary { get; private set; } = new Dictionary<string, string>();

    [XmlIgnore]
    //Addressables Path -> Loaded Object Data
    public Dictionary<string, System.Object> AssetsDictionary { get; set; } = new Dictionary<string, System.Object>();

    public override void OnLoaded() {
        base.OnLoaded();


        AssetRegistryManager arm = ECM.Main.GetManager<AssetRegistryManager>();
        if(arm == null) {
            UnityEngine.Debug.LogError(this + " :: Asset Registry Manager is null!");
            return;
        }

        arm.OnAssetLoaded += OnRegistryAssetLoaded;

        //Register asset data for AssetRegistry
        foreach (var assetInfo in Assets) {
            //Init storage for loaded object
            AssetIDReferenceDictionary.Add(assetInfo.ID, assetInfo.AssetPath);
            AssetsDictionary.Add(assetInfo.AssetPath, null);

            UnityEngine.Debug.Log(this + $" :: Loading Asset Data ID: {assetInfo.ID} Path: {assetInfo.AssetPath}");

            //Notify Asset Registry. The callback above will pass the object
            //over when it's loaded
            arm.RegisterAssetID(assetInfo.AssetPath);
        }
    }

    private void OnRegistryAssetLoaded(string assetID, object assetObject) {
        if (AssetsDictionary.ContainsKey(assetID)) {
            AssetsDictionary[assetID] = assetObject;    
        }
    }

    public T GetAsset<T>(string assetID) where T : class {
        if (string.IsNullOrEmpty(assetID)) {
            UnityEngine.Debug.LogError(this + " :: GetAsset Failed. Passed in AssetID is null or empty!");
            return null;
        }

        if (AssetIDReferenceDictionary.ContainsKey(assetID)) {
            string AssetPath = AssetIDReferenceDictionary[assetID];
            if (AssetsDictionary.ContainsKey(AssetPath)) {
                return (T)AssetsDictionary[AssetPath];
            }
        }
        /*
        if (AssetsDictionary.ContainsKey(assetID)) {
            return (T)AssetsDictionary[assetID];
        }

        */

        return null;
    }
}

public class CAssetXmlRow {
    [XmlAttribute("id")]
    public string ID { get; set; }

    [XmlText]
    public string AssetPath { get; set; }
}