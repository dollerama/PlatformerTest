using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIContentController : UIElementController
{
    public GameObject[] prefabs;

    public GameObject InstantiatePrefabInContent(string name)
    {
        GameObject newPrefab = Instantiate<GameObject>(System.Array.Find<GameObject>(prefabs, x => x.name == name));
        newPrefab.GetComponent<RectTransform>().SetParent(this.transform);
        return newPrefab;
    }

    public abstract void SetPrefabInfo(GameObject obj, object[] args);

    public abstract void BuildContent();
}
