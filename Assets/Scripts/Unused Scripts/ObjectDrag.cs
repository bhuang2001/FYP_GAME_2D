using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public class ObjectDrag : MonoBehaviour
{
    private Vector3 startPos;
    private float deltaX, deltaY;
    // Start is called before the first frame update
    void Start()
    {
        startPos = Input.mousePosition;
        startPos = Camera.main.ScreenToWorldPoint(startPos);

        deltaX = startPos.x - transform.position.x;
        deltaY = startPos.y - transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 pos = new Vector3(mousePosition.x - deltaX, mousePosition.y - deltaY);
        Vector3Int cellPosition = BuildingSystem.current.gridlayout.WorldToCell(pos);
        transform.position = BuildingSystem.current.gridlayout.CellToLocalInterpolated(cellPosition);
    }

    private void MouseButtonReleased()
    {
        if (Input.GetMouseButtonUp(0))
        {
            gameObject.GetComponent<PlaceableObject>().CheckPlacement();
            Destroy(this);
        }
    }
}
*/