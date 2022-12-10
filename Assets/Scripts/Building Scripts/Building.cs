using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public bool Placed { get ; private set; }
    public BoundsInt area;

    #region Build Methods

    public bool CanBePlaced()
    {
        Vector3Int positionInt = BuildingSystem.current.gridlayout.LocalToCell(transform.position);
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;

        if( BuildingSystem.current.CanTakeArea(areaTemp)) 
        {
        return true;
        }
        return false;
    }

    public void Place()
    {
        Vector3Int positionInt = BuildingSystem.current.gridlayout.LocalToCell(transform.position);
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;   
        Placed = true;
        BuildingSystem.current.TakeArea(areaTemp);
    }
    #endregion

}



