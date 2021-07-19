using UnityEngine;
using UnityEditor;

static public class GameObjectExtensions
{
    static public void ClearChildren(this GameObject go)
    {
        for (int i = go.transform.childCount - 1; i >= 0; i--)
        {
            //if (!EditorApplication.isPlaying)
            //{
            //    Object.DestroyImmediate(go.transform.GetChild(i).gameObject);
            //}
            //else
            //{
            //    GameObject.Destroy(go.transform.GetChild(i).gameObject);
            //}
        } 
    }
}
