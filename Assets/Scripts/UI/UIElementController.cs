using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class UIElementController : MonoBehaviour
{ 
    public List<GameObject> contents;
    private bool _init = false;
    public System.Action<GameObject> binding;

    public void Awake()
    {
        Initialize();
    }

    public void CreateBind(System.Action<GameObject> b)
    {
        binding = b;
    }

    public void Update()
    {
        binding?.Invoke(gameObject);
    }

    public void Initialize(bool force = false)
    {
        if (_init && !force) return;

        contents = new List<GameObject>();

        foreach (Transform t in this.transform)
        {
            contents.Add(t.gameObject);
            foreach (Transform t2 in t)
            {
                contents.Add(t2.gameObject);
            }
        }
        _init = true;
    }

    public GameObject GetItem(string name)
    {
        foreach (GameObject g in contents)
        {
            if (g.name == name)
            {
                return g;
            }
        }
        return null;
    }
}
