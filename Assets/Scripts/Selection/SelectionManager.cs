using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    public GameObject selectedObject;
    // to assign the links gameobjects to know which to highlight
    public GameObject temp;
    public TextMeshProUGUI objNameText;

    // boolean variable to know if the user wants to select hosts for transmitting data packet or not 
    public bool sendData;
    // boolean variable to know if the selection of hosts are done 
    public bool done;
    // these gameobjects store the hosts that are selected for sending and receiving the data packet
    public GameObject sendingHost = null;
    public GameObject receivingHost = null;

    private BuildingSystem buildingSystem;
    public GameObject objUI;

    // To access the selected router's properties
    private Router router;
    // To access the line renderer properties of the links
    private LineRenderer lr;
    // To access the link manager to know when a link is being added so that it doesn't select it and try to highlight
    // null line renderer
    private LinkManager linkManager;

    // Start is called before the first frame update
    void Start()
    {
        buildingSystem = GameObject.Find("Grid").GetComponent<BuildingSystem>();
        linkManager = GameObject.Find("Link Creator").GetComponent<LinkManager>();
        done = false;
        sendData = false;
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
                if(hit.collider.gameObject.CompareTag("Router") || hit.collider.gameObject.CompareTag("Host")) 
                {
                Debug.Log(hit.collider.gameObject.name);
                Select(hit.collider.gameObject);
                }
                // Must be a router and cannot run while a link is being added
                if(hit.collider.gameObject.CompareTag("Router") && linkManager.costEntered == true)
                {
                    HighlightDestinations(hit.collider.gameObject);
                }
            }
        }
        // Deselect function
        if (Input.GetMouseButtonDown(1) && selectedObject != null) 
        {
            if(selectedObject.CompareTag("Router"))
            {
            RemoveHighlights(selectedObject);
            }
            Deselect();
        }
        //This will run if the button Send Packet is pressed 
        GetSelectedHosts();
    }
    
    private void Select(GameObject obj)
    {
        if (obj == selectedObject)
        {
            return;
        }
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

    public void SendPacketPressed()
    {
        if(sendData == false)
        {
            sendData = true;
        }
        else if (sendData == true)
        {
            sendData = false;
        }
    }

    private void GetSelectedHosts ()
    {
        if (sendData == true)
        {
            if(Input.GetMouseButtonDown(0))
            {
                Vector2 raycastPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(raycastPosition,Vector2.zero);
            

                if(hit.collider != null) 
                {
                    if(hit.collider.gameObject.CompareTag("Host") && sendingHost == null) 
                    {
                        sendingHost = hit.collider.gameObject;
                        Debug.Log("Sending Host Selected as " + hit.collider.gameObject.name);
                    }
                    else if (hit.collider.gameObject.CompareTag("Host") && receivingHost == null)
                    {
                        receivingHost = hit.collider.gameObject;
                        Debug.Log("Receiving Host Selected as " + hit.collider.gameObject.name);
                        // There are hosts selected for sending and receiving so set done to true
                        done = true;
                    }
                }
            }
        }
        else 
        {
            sendingHost = null;
            receivingHost = null;
            done = false;
        } 
    }

        private void HighlightDestinations(GameObject selectedRouter)
    {
        router = selectedRouter.GetComponent<Router>();
        for(int i = 0; i < router.parentLinks.Length; i++)
        {
            // If the link exists then highlight it
            if(router.parentLinks[i] != "None")
            {
            temp = GameObject.Find(router.parentLinks[i]);
            lr = temp.GetComponent<LineRenderer>();
            SetColourGradient(Color.red,Color.red);
            }
            // Setting the start, mid and end points of each line
        }
    }

    private void RemoveHighlights(GameObject selectedRouter)
    {
        Debug.Log("This function ran");
        router = selectedRouter.GetComponent<Router>();
        for(int i = 0; i < router.parentLinks.Length; i++)
        {
            // If the link exists then highlight it
            if(router.parentLinks[i] != "None")
            {
            temp = GameObject.Find(router.parentLinks[i]);
            lr = temp.GetComponent<LineRenderer>();
            SetColourGradient(Color.white,Color.white);
            }
            // Setting the start, mid and end points of each line
        }
    }

    private void SetColourGradient(Color colour1, Color colour2) 
    {
        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(colour1, 0.0f), new GradientColorKey(colour2, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        lr.colorGradient = gradient;
    }
}

