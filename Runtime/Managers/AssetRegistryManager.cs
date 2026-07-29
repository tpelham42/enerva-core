using EnervaCore;
using System;
using System.Collections.Generic;




public delegate void AssetLoadedDelegate(string assetID, System.Object assetObject);

public class AssetRegistryManager : IManager, IManagerGMInitialized {

    //Addressable Path -> Loaded Object
    private Dictionary<string, System.Object> _loadedAssets = new Dictionary<string, System.Object>();

    public event AssetLoadedDelegate OnAssetLoaded;

    public AssetRegistryManager() { }

    public void RegisterAssetID(string path) {        
        if (path == null) throw new ArgumentNullException(this + $" :: Passed in Path value is null!");

       if(_loadedAssets.ContainsKey(path) == false) {
            //Register the path with a null value. Loading get's handled under LoadAssets
            _loadedAssets.Add(path, null);
        }
    }

    public virtual void LoadAssets() {        
    }

    public T GetInstance<T>(string path) {
        if (_loadedAssets.ContainsKey(path)) {
            System.Object obj = _loadedAssets[path];
            if (obj != null) {
                return (T)obj;
            }
        }

        return default(T);
    }

    #region IManager Implementation
    public void Initialize() {
        
    }

    public void OnDestroyed() {
        
    }

    public void OnGameManagerInitialized() {
        LoadAssets();
    }
    #endregion
}

