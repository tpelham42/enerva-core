using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public interface IPoolObject {
    void OnBecomeActive();
    void OnReturnedToPool();
}

public class PrefabObjectPool {
    List<GameObject> _pool;
    GameObject _prefabTemplate;
    string _containerName;
    GameObject _poolContainer;

    public PrefabObjectPool(GameObject prefabObject, string ContainerName) {
        _pool = new List<GameObject>();
        _containerName = ContainerName;
        _prefabTemplate = prefabObject;
    }

    GameObject PoolContainer {
        get {
            if (_poolContainer == null) {
                _poolContainer = new GameObject(_containerName);
            }

            return _poolContainer;
        }
    }

    public void Warm(int count) {
        if (_pool.Count >= count)
            return;

        int addTotal = count - _pool.Count;
        for (int i = 0; i < addTotal; i++) {
            Return(NewItem(null));
        }
    }

    public void Destroy() {
        foreach (GameObject go in _pool) {
            MonoBehaviour.Destroy(go);
        }

        MonoBehaviour.Destroy(_poolContainer);
        _pool.Clear();
        _pool = null;
    }

    public void Clear() {
        foreach (GameObject go in _pool) {
            MonoBehaviour.Destroy(go);
        }

        _pool.Clear();
    }

    public GameObject Get(Transform parent = null) {
        if (_prefabTemplate == null) {
            UnityEngine.Debug.LogError(this + " :: Error Instantiating Pool Object. Prefab Template Object Is Null!");
            return null;
        }

        if (_pool.Count > 0) {
            GameObject poolItem = _pool[0];
            IPoolObject poolInterface = poolItem.GetComponent<IPoolObject>();
            if (poolInterface != null)
                poolInterface.OnBecomeActive();

            _pool.RemoveAt(0);

            poolItem.SetActive(true);

            return poolItem;
        }

        return NewItem(parent);
    }

    private GameObject NewItem(Transform parent) {
        GameObject newItemGO;
        if (parent == null)
            newItemGO = MonoBehaviour.Instantiate(_prefabTemplate, PoolContainer.transform);
        else
            newItemGO = MonoBehaviour.Instantiate(_prefabTemplate, parent);

        IPoolObject poolInterface = newItemGO.GetComponent<IPoolObject>();
        /*
        if (poolInterface == null) {
            Debug.LogError(this + " :: Error Creating New Pool Object. Instantiated Prefab Missing IPoolObject interface!");
            return null;
        }*/

        if (poolInterface != null)
            poolInterface.OnBecomeActive();

        return newItemGO;
    }

    public void Return(GameObject go) {
        IPoolObject poolInterface = go.GetComponent<IPoolObject>();
        /*
        if (poolInterface == null) {
            Debug.LogError(this + " :: Error Releasing Pool Object. Release Object is missing IPoolObject interface!");
            return;
        }*/

        if (poolInterface != null)
            poolInterface.OnReturnedToPool();

        go.transform.SetParent(PoolContainer.transform);
        go.SetActive(false);

        _pool.Add(go);
    }

    public void DestroyObject(GameObject go) {
        _pool.Remove(go);
        MonoBehaviour.Destroy(go);
    }
}

