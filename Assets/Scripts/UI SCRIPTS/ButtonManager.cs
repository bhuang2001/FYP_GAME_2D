using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public GameObject addEntity, addRouter,addHost, addLink;
    

    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // When add entity button is pressed, it should hide itself and display the 3 options i.e router, host, link
    public void AddEntity()
    {
        addRouter.SetActive(true);
        addHost.SetActive(true);
        addLink.SetActive(true);
        addEntity.SetActive(false);
    }

    // When router, host or link is pressed , hide all of them and display the add entity button 
    public void Hide ()
    {
        addRouter.SetActive(false);
        addHost.SetActive(false);
        addLink.SetActive(false);
        addEntity.SetActive(true);
    }
}
