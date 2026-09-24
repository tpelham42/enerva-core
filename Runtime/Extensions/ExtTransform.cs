using UnityEngine;

public static class ExtTransform { 
    public static void DestroyChildren(this Transform t) {
        if (t == null)
            return;

        foreach (Transform child in t) {
            MonoBehaviour.Destroy(child.gameObject);
        }
    }
}