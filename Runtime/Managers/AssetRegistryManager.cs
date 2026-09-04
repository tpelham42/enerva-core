using EnervaCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;




public delegate void AssetLoadedDelegate(string assetID, System.Object assetObject);
public delegate void AssetLoadProgressDelegate(float progress);
public delegate void AssetsLoadedDelegate();

public class AssetRegistryManager : IManager, IManagerGMInitialized {

    //Addressable Path -> Loaded Object
    private Dictionary<string, System.Object> _loadedAssets = new Dictionary<string, System.Object>();


    public event AssetLoadedDelegate OnAssetLoaded;
    public event AssetLoadProgressDelegate OnAssetLoadProgress;
    public event AssetsLoadedDelegate OnAllAssetsLoaded;

    private int _totalAssetsToLoad = 0;
    private int _totalAssetsLoaded = 0;

    public AssetRegistryManager() { }

    public float AssetLoadProgress {
        get {
            if (_totalAssetsToLoad == 0) return 1.0f;

            return (float)_totalAssetsLoaded / (float)_totalAssetsToLoad;
        }
    }

    public void RegisterAssetID(string path) {        
        if (path == null) throw new ArgumentNullException(this + $" :: Passed in Path value is null!");

       if(_loadedAssets.ContainsKey(path) == false) {
            //Register the path with a null value. Loading get's handled under LoadAssets
            _loadedAssets.Add(path, null);
        }
    }

    public virtual void LoadAssets() {
        _totalAssetsToLoad = _loadedAssets.Count;
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

                _totalAssetsLoaded++;

                OnAssetLoadProgress?.Invoke(AssetLoadProgress);

                if(AssetLoadProgress >= 1.0f) {
                    OnAllAssetsLoaded?.Invoke();
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

