using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public string Name = "Default";
    // Add in IP addresses later and other information needed for each network component later

    // Adding these for now in case it is needed
    public ItemType Type;
    public Sprite Icon;
    public GameObject Prefab;

}

public enum ItemType 
{
    Routers,
    Hosts,
    Links
}