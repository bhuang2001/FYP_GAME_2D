using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    public GameObject selectedObject;
    public TextMeshProUGUI objNameText;

    private BuildingSystem buildingSystem;
    public GameObject objUI;

    // Start is called before the first frame update
    void Start()
    {
        buildingSystem = GameObject.Find("Grid").GetComponent<BuildingSystem>();
    }

    // Update is called once per frame
    void Update()
    {
/*      if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, 3000))
            {
                if(hit.collider.gameObject.CompareTag("NetworkEntities"))
                {
                    Select(hit.collider.gameObject);
                }
            }
        } */
        if(Input.GetMouseButtonDown(0))
        {
            Vector2 raycastPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(raycastPosition,Vector2.zero);
        

        if(hit.collider != null) 
        {
            if(hit.collider.gameObject.CompareTag("NetworkEntities")) 
            {
            Debug.Log(hit.collider.gameObject.name);
            Select(hit.collider.gameObject);
            }
        }
        }
        if (Input.GetMouseButtonDown(1) && selectedObject != null) 
        {
            Deselect();
        }
    }
    
    private void Select(GameObject obj)
    {
        if (obj == selectedObject)
        return;
        if (selectedObject != null)
        {
            Deselect();
        }
        Outline outline = obj.GetComponent<Outline>();
        if( outline == null) 
        {
            obj.AddComponent<Outline>();
        }
        else
        {
            outline.enabled = true;
        }
        objNameText.text = obj.name;
        objUI.SetActive(true);
        selectedObject = obj;
    }

    private void Deselect ()
    {
        objUI.SetActive(false);
        selectedObject.GetComponent<Outline>().enabled = false;
        selectedObject = null;
    }


}
