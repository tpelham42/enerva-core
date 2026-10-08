using EnervaCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;




public delegate void AssetLoadedDelegate(string assetID, System.Object assetObject);
public delegate void AssetLoadProgressDelegate(float progress);
public delegate void AssetsLoadedDelegate();

public class AssetRegistryManager : IManager, IManagerPostInit {

    //Addressable Path -> Loaded Object
    private Dictionary<string, System.Object> _loadedAssetsByPath = new Dictionary<string, System.Object>();
    //ID -> Path
    private Dictionary<string, string> _assetIDToPathDict = new Dictionary<string, string>();


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

    public void RegisterAssetID(string path, string assetID=null) {        
        if (path == null) throw new ArgumentNullException(this + $" :: Passed in Path value is null!");

       if(_loadedAssetsByPath.ContainsKey(path) == false) {
            //Register the path with a null value. Loading get's handled under LoadAssets
            _loadedAssetsByPath.Add(path, null);
        }

        if (assetID != null) {
            if (_assetIDToPathDict.ContainsKey(assetID) == false) {
                _assetIDToPathDict[assetID] = path;
            }
            else {
                UnityEngine.Debug.LogWarning(this + $" :: AssetID '{assetID}' is already registered with path '{_assetIDToPathDict[assetID]}'. New path '{path}' will not be registered with id '{assetID}'.");
            }
        }
    }

    public virtual void LoadAssets() {
        _totalAssetsToLoad = _loadedAssetsByPath.Count;
        foreach (var asset in _loadedAssetsByPath) {
            string assetID = asset.Key;
            Addressables.LoadAssetAsync<System.Object>(assetID).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded) {
                    var loadedAsset = handle.Result;
                    _loadedAssetsByPath[assetID] = loadedAsset;   
                    
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

    public T GetInstanceByPath<T>(string path) {
        if (_loadedAssetsByPath.ContainsKey(path)) {
            System.Object obj = _loadedAssetsByPath[path];
            if (obj != null) {
                return (T)obj;
            }
        }

        return default(T);
    }

    public T GetInstanceByID<T>(string assetID) {
        if (_assetIDToPathDict.ContainsKey(assetID)) {
            string path = _assetIDToPathDict[assetID];
            return GetInstanceByPath<T>(path);
        }

        return default(T);
    }

    #region IManager Implementation
    public void Initialize() {
        
    }

    public void OnDestroyed() {
        
    }

    public void OnPostInit() {
        LoadAssets();
    }
    #endregion
}

