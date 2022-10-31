using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
/*
public class PlaceableObject : MonoBehaviour
{
   public bool Placed { get; private set;}
   private Vector3 origin;
   public BoundsInt area;
   
   // Checking to see if the area that the object is on is available or not.
   public bool CanBePlaced()
   {
    Vector3Int objectPositionInt = BuildingSystem.current.gridlayout.LocalToCell(transform.position);
    BoundsInt areaTemp = area;
    areaTemp.position = objectPositionInt;

    if(BuildingSystem.current.CanTakeArea(areaTemp)) 
    {
        return true;
    }
    else 
    {
     return false;
    }
   }

// Place object on the area 
   public void Place () 
   {
    Vector3Int objectPositionInt = BuildingSystem.current.gridlayout.LocalToCell(transform.position);
    BoundsInt areaTemp = area;
    areaTemp.position = objectPositionInt;

    Placed = true;
    BuildingSystem.current.TakeArea(areaTemp);
   }

   public void CheckPlacement ()
    { 
        if (CanBePlaced()) 
        { 
            Place();
            origin = transform.position;
        }
        else
        {
            Destroy(transform.gameObject);
        }
    // need to make ItemManager class still 
    //    ItemManager.current.ShopButton_click();
    }
}

*/