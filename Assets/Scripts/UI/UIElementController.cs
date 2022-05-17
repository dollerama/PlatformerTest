using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Platformer.UI
{
    //UIElementController holds a reference to all contents of a UI object and allows you to grab them by name
    //additionally you can bind actions that control how the element reacts over time
    public class UIElementController : MonoBehaviour
    {
        public List<GameObject> contents;
        private bool _init = false;
        public System.Action<GameObject> binding;

        public void Awake()
        {
            Initialize();
        }

        //create new binding on element
        //A binding for example could be setting the text on an element to the timer running in game
        public void CreateBind(System.Action<GameObject> b)
        {
            binding = b;
        }

        //invoke binding updates
        public void Update()
        {
            binding?.Invoke(gameObject);
        }

        //set contents so we can find them later
        public void Initialize(bool force = false)
        {
            //if already run we will return unless you pass true in the parameters to force it to re-init.
            if (_init && !force) return;

            contents = new List<GameObject>();

            //do a recursive add
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

        //grab item based on name
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
}
