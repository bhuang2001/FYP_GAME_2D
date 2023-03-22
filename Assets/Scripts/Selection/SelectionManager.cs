using System.Collections;
using System.Collections.Generic;
using System;
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
    public GameObject routerUI, powerButton, viewTableButton, forwardingTableUI;

    // To access the selected router's properties
    private Router router;
    // To access the line renderer properties of the links
    private LineRenderer lr;
    // To access the link manager to know when a link is being added so that it doesn't select it and try to highlight
    // null line renderer
    private LinkManager linkManager;

    // For creating the forwarding table entries
    public RowUI rowUI;
    public GameObject forwardingTableContent;
    

    // Start is called before the first frame update
    void Start()
    {
        buildingSystem = GameObject.Find("Grid").GetComponent<BuildingSystem>();
        linkManager = GameObject.Find("Link Creator").GetComponent<LinkManager>();
        // Not active so it couldn't be found?
        //forwardingTableContent = GameObject.Find("Content");
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
                if(hit.collider.gameObject.CompareTag("Link") && linkManager.costEntered == true)
                {
                    // for links, get the parent of the collider object
                    Select(hit.collider.gameObject.transform.parent.gameObject);
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
        if(obj.CompareTag("Router"))
        {
            routerUI.SetActive(true);
            powerButton.SetActive(true);
            viewTableButton.SetActive(true);
            forwardingTableUI.SetActive(false);
        }
        else if(obj.CompareTag("Host"))
        {
            routerUI.SetActive(true);
            powerButton.SetActive(false);
            viewTableButton.SetActive(false);
            forwardingTableUI.SetActive(false);
        }
        else if (obj.CompareTag("Link"))
        {
            routerUI.SetActive(true);
            viewTableButton.SetActive(false);
            powerButton.SetActive(true);
            forwardingTableUI.SetActive(false);
        }
        selectedObject = obj;
    }

    private void Deselect ()
    {
        
        routerUI.SetActive(false);
        selectedObject.GetComponent<Outline>().enabled = false;
        // If the forwarding table isn't on then deselecting will also remove the selectedObject
        if(!forwardingTableUI.activeSelf)
        {
        selectedObject = null;
        }

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
            // Set sorting order higher than the animation lines so that it appears in front of it
            lr.sortingOrder = 2;
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
            // Set back to 0 when it returns to normal (White)
            lr.sortingOrder = 0;
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

    public void ToggleRouter()
    {
        // If selected Object is router
        if(selectedObject.CompareTag("Router"))
        {
            Router routerComponent = selectedObject.GetComponent<Router>();
            if(routerComponent.routerStatus == true)
            {
                routerComponent.routerStatus = false;
            }
            else if (routerComponent.routerStatus == false)
            {
                routerComponent.routerStatus = true;
            }
        }
        // If selected object is a link
        else if(selectedObject.CompareTag("Link"))
        {
            // selectedObject is the collider object, need to access the parent
            Link linkComponent = selectedObject.GetComponent<Link>();
            if(linkComponent.directOn == true)
            {
                linkComponent.directOn = false;
            }
            else if (linkComponent.directOn == false)
            {
                linkComponent.directOn = true;
            }

        }
    }

    public void CreateForwardingTable()
    {
        // Check if there is a selected object that is a router 
        if(selectedObject != null && selectedObject.CompareTag("Router") == true)
        {
            // Destroy the previous forwarding table entries if there exists any
            foreach(Transform child in forwardingTableContent.transform)
            {
                Destroy(child.gameObject);
            }
            // current selected router 
            GameObject routerObject = selectedObject;
            // stores connected hosts on the router that is being checked
            GameObject hostObject;
            GameObject connectedRouterObj;
            Router routerComponent = routerObject.GetComponent<Router>();
            Router connectedRouter;
            Link linkComponent;
            Host hostComponent;
            string linkName;
            string outgoingLinkName;
            string connectedRouterName;
            string nextHopIPString = "";
            uint nextHopIP;
            RowUI row;
            // Traverse each of the links that connect to the routers that the selected router knows
            for(int i = 0; i < routerComponent.parentLinks.Length; i++)
            {
                // Get the link name
                linkName = routerComponent.parentLinks[i];
                connectedRouterName = "Router" + i;
                Debug.Log(connectedRouterName);
                // Get the router that the link connects to 
                connectedRouterObj = GameObject.Find(connectedRouterName);
                // Check if the router exists (null check)
                if(connectedRouterObj != null)
                {
                    connectedRouter = connectedRouterObj.GetComponent<Router>();
                // If the router is the one that is selected then get the forwarding entries for the hosts
                // connected directly to it, if there are any
                if(connectedRouter == routerComponent)
                {
                    if(routerComponent.connectedHosts.Count != 0)
                    {
                        // Get each host 
                        foreach(string host in connectedRouter.connectedHosts)
                        {
                            hostObject = GameObject.Find(host);
                            hostComponent = hostObject.GetComponent<Host>();
                            // Get the link that the host is connected to 
                            linkComponent = GameObject.Find(hostComponent.linkInterface).GetComponent<Link>();
                            // Check if host is connected
                            if(hostComponent.connected == true)
                            {
                            row = Instantiate(rowUI,forwardingTableContent.transform).GetComponent<RowUI>();

                            row.network.text = linkComponent.subnetIPString;
                            // If the host is on entity1 then the host portion of the IP on the router port is 2
                            // Otherwise, the host portion of the IP on the router port is 1.
                            if(linkComponent.entity1 == hostObject.name)
                            {
                                nextHopIP = linkComponent.subnetIP + (uint)2;
                                nextHopIPString = ConvertToIPString(nextHopIP);
                            }
                            else if (linkComponent.entity2 == hostObject.name)
                            {
                                nextHopIP = linkComponent.subnetIP + (uint)1;
                                nextHopIPString = ConvertToIPString(nextHopIP);
                            }
                            row.nextHop.text = nextHopIPString; 
                            row.outgoingLink.text = hostComponent.linkInterface;
                            }
                        }
                    }
                }
                // If the parent link is a valid path 
                if(linkName != "None")
                {
                    linkComponent = GameObject.Find(linkName).GetComponent<Link>(); 
                    // Make a new entry
                    row = Instantiate(rowUI,forwardingTableContent.transform).GetComponent<RowUI>();
                    // Destination network is the subnet IP string of the link 
                    row.network.text = linkComponent.subnetIPString;
                    // Obtaining the next hop address which corresponds to the port IP on the selected router
                    // Using this method rather than iterating through the entire port IP array to check for
                    // it
                    outgoingLinkName = routerComponent.outgoingLinks[i];
                    // Get the link component of the outgoing link to the destination network associated with the 
                    // router being checked
                    linkComponent = GameObject.Find(outgoingLinkName).GetComponent<Link>();
                    if(linkComponent.entity1 == selectedObject.name)
                    {
                        nextHopIP = linkComponent.subnetIP + (uint)1;
                        nextHopIPString = ConvertToIPString(nextHopIP);
                    }
                    else if (linkComponent.entity2 == selectedObject.name)
                    {
                        nextHopIP = linkComponent.subnetIP + (uint)2;
                        nextHopIPString = ConvertToIPString(nextHopIP);
                    }
                    // Set the next hop address after it is found
                    row.nextHop.text = nextHopIPString;
                    // Set the outgoing link to the outgoing links array of the selected router
                    row.outgoingLink.text = outgoingLinkName;

                    // If the selected router has connected hosts, add the entries for their subnet
                    // Destination network address is the host's network found on the link it is connected on
                    // Next hop IP address is the same as the next hop ip address of the router the host is connected
                    // to. Same applies to the outgoing link.
                    if(connectedRouter.connectedHosts.Count != 0)
                    {
                        foreach(string host in connectedRouter.connectedHosts)
                        {
                            hostObject = GameObject.Find(host);
                            hostComponent = hostObject.GetComponent<Host>();
                            // Get the link interface of the host 
                            linkComponent = GameObject.Find(hostComponent.linkInterface).GetComponent<Link>();
                            // Check if host is connected
                            if(hostComponent.connected == true)
                            {
                            row = Instantiate(rowUI,forwardingTableContent.transform).GetComponent<RowUI>();
                            row.network.text = linkComponent.subnetIPString;
                            row.nextHop.text = nextHopIPString; 
                            row.outgoingLink.text = outgoingLinkName;
                            }
                        }
                    }
                }
                }
            }
        }


    }
    
    private string ConvertToIPString(uint ip)
    {   
        string ipString = "";
        byte[] bytes= BitConverter.GetBytes(ip);
        if(BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
     // subnetIPString = BitConverter.ToString(bytes);

        for ( int i = 0; i < 4; i++)
        {
            int octet = 0xFF & bytes[i];
            ipString += "." + octet;
        }
        ipString = ipString.Substring(1);
        return ipString;
    }

    public void ViewForwardingTable()
    {
        routerUI.SetActive(false);
        CreateForwardingTable();
        forwardingTableUI.SetActive(true);
        
    }

    public void ExitForwardingTable()
    {
        forwardingTableUI.SetActive(false);
        
    }
}

