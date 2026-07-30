using EnervaCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;




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
        foreach (var asset in _loadedAssets) {
            string assetID = asset.Key;
            Addressables.LoadAssetAsync<System.Object>(assetID).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded) {
                    var loadedAsset = handle.Result;
                    _loadedAssets[assetID] = loadedAsset;   
                    
                    OnAssetLoaded?.Invoke(assetID, loadedAsset);

                    //UnityEngine.Debug.Log(this + $" :: Loaded Asset: {assetID}");
                }
                else {
                    UnityEngine.Debug.LogError(this + $" :: Error Loading Asset '{assetID}' Exception: {handle.OperationException}");
                }
            };
        }
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

