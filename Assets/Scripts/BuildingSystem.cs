using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

// main tile map = background tile map,
// temp tile map = foreground tile map
public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem current;
    public GridLayout gridlayout;
    public Tilemap backgroundTilemap;
    public Tilemap foregroundTilemap;
    
    private static Dictionary<TileType, TileBase> tileBases = new Dictionary<TileType, TileBase>();
    private Building temp;
    private Vector3 prevPos;
    private BoundsInt prevArea;

    #region Unity Methods
    private void Awake() 
    {
        current = this;
    }
    private void Start() 
    {
        // Assets -> Resources -> Tiles 
        // allTiles_sheet_0 is the ground tile
        string tilePath = @"Tiles\";
        tileBases.Add(TileType.Empty, null);
        tileBases.Add(TileType.White, Resources.Load<TileBase>(tilePath + "Taken_Tile"));
        tileBases.Add(TileType.Green, Resources.Load<TileBase>(tilePath + "Available_Tile"));
        tileBases.Add(TileType.Red, Resources.Load<TileBase>(tilePath + "Not_Available_Tile"));
    }

    private void Update()
    {
        if (!temp)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0)) 
        {
            if(EventSystem.current.IsPointerOverGameObject(0))
            {
                return;
            }
            if(!temp.Placed)
            {
                Vector2 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int cellPos = gridlayout.LocalToCell(touchPos);
                if (prevPos != cellPos) 
                {
                    temp.transform.localPosition = gridlayout.CellToLocalInterpolated(cellPos 
                    + new Vector3(0.5f,0.5f,0f));
                    prevPos = cellPos;
                    FollowBuilding();
                }
            }
        }
        else if(Input.GetKeyDown(KeyCode.Space))
        {
            if( temp.CanBePlaced()) 
            {
                temp.Place();
            }
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            ClearArea();
            Destroy(temp.gameObject);
        }
    }

    #endregion 


    #region Tilemap Management
    //initialize current in wait method


    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap) 
    {
        TileBase[] array = new TileBase [area.size.x * area.size.y * area.size.z];
        int counter = 0 ;
        foreach (var v in area.allPositionsWithin)
        {
            Vector3Int position = new Vector3Int(v.x, v.y,0);
            array[counter] = tilemap.GetTile(position);
            counter++;
        }
        return array;
    }

    private static void SetTilesBlock(BoundsInt area, TileType type, Tilemap tilemap) 
    {
        int size = area.size.x * area.size.y * area.size.z;
        TileBase[] tilearray = new TileBase[size];
        FillTiles(tilearray, type);
        // Note SetTilesBlock is not the user defined method, it is part of Unity Tilemaps
        tilemap.SetTilesBlock(area,tilearray);
    }

    private static void FillTiles(TileBase[] array, TileType type) 
    {
        for (int i = 0; i < array.Length; i++) 
        {
            array[i] = tileBases[type];
        }
    }

    private void ClearArea ()
    {
        TileBase[] toClear = new TileBase[prevArea.size.x * prevArea.size.y * prevArea.size.z];
        FillTiles(toClear,TileType.Empty);
        // Note SetTilesBlock is not the user defined method, it is part of Unity Tilemaps
        foregroundTilemap.SetTilesBlock(prevArea, toClear);
    }

    private void FollowBuilding() 
    {
        ClearArea();
        temp.area.position = gridlayout.WorldToCell(temp.gameObject.transform.position);
        BoundsInt buildingArea = temp.area;

        TileBase[] baseArray = GetTilesBlock(buildingArea, backgroundTilemap);

        int size = baseArray.Length;
        TileBase[] tileArray = new TileBase[size];

        for ( int i = 0; i < baseArray.Length; i++)
        {
            if (baseArray[i] == tileBases[TileType.White])
            {
                tileArray [i] = tileBases[TileType.Green];
            }
            else
            {
                FillTiles(tileArray, TileType.Red);
                break;
            }
        }
        // Note SetTilesBlock is not the user defined method, it is part of Unity Tilemaps
        foregroundTilemap.SetTilesBlock(buildingArea, tileArray);
        prevArea = buildingArea;

    }

    public bool CanTakeArea(BoundsInt area)
    {
        TileBase[] baseArray = GetTilesBlock (area, backgroundTilemap);
        foreach(var v in baseArray)
        {
            if (v != tileBases[TileType.White])
            {
                Debug.Log("Cannot Place Here");
                return false;
            }
        }
        return true;
    }
    
    public void TakeArea (BoundsInt area)
    {
        SetTilesBlock(area,TileType.Empty,foregroundTilemap);
        SetTilesBlock(area,TileType.Green,backgroundTilemap);
    }
    #endregion

    #region Building Placement 
    public void InitializeWithObject (GameObject entity)
    {

        temp= Instantiate(entity, Vector3.zero, Quaternion.identity).GetComponent<Building>();
        FollowBuilding();
    }

    #endregion
    
}

public enum TileType 
{
Empty,
White,
Green,
Red
}