using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer.UI
{
    //UI contentController extends UIElementController.
    //It holds prefabs and allows you to easily instantiate them into the gameobject. This is meant for use with layout groups
    public abstract class UIContentController : UIElementController
    {
        //prefabs in content
        public GameObject[] prefabs;

        public GameObject InstantiatePrefabInContent(string name)
        {
            //find prefab and instantiate it  
            GameObject newPrefab = Instantiate<GameObject>(System.Array.Find<GameObject>(prefabs, x => x.name == name));
            //set parent so we keep scale properties 
            newPrefab.GetComponent<RectTransform>().SetParent(this.transform);
            //return created prefab
            return newPrefab;
        }

        //set info inside of prefab
        public abstract void SetPrefabInfo(GameObject obj, object[] args);

        //build content
        public abstract void BuildContent();
    }
}
