using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using TMPro;
using UnityEngine.UI;

public class Link : MonoBehaviour
{

    // Access the renderer of the link to change transparency
    private Renderer linkRenderer;
    //  previous global power status
    public bool prevGlobalPower;
    // IndirectOn is on if its the two routers it connects to are on
    // DirectOn is on if the link is directly turned on
    // Store the previous values
    public bool indirectOn, directOn;
    public bool previousIndirectOn, previousDirectOn;
    // on is true, off is false
    public bool previousStatus, status;
    // store the name of the two entities it connects
    public string entity1;
    public string entity2;
    // Subnet IP of the Link in uint and string format for readability
    public uint subnetIP;
    public string subnetIPString;
    
     // store link cost 
    public int cost;
    // store the positions of the connector points for animations in future?
    public Vector3 startPos, midPos, endPos;

    // two child objects for two colliders
    [SerializeField] private GameObject startCollider;
    [SerializeField] private GameObject endCollider;
    // line renderer field to access line renderer fields
    private LineRenderer lr;
    // boxcollider2d component to edit the size of the colliders
    private BoxCollider2D boxCollider;
    private PolygonCollider2D polyCollider;
    private MeshFilter meshFilter;
    // rigidbody2d component to edit the gravity and body type of the line/link
    private Rigidbody2D rigidBody;
    
    // Both of these needs to be accessed so that it can add the weights to the adjacency matrix each time 
    // a link is established
    public GraphCreator graphCreator;
    public Router router, updateRouter;
    // This is for getting all the known routers to turn on their update status 
    public GameObject[] routers;
    // This is to send update signal to the component responsible for displaying the routing information
    // being transmitted
    private RoutingInformation RIComponent;
    // To access the data transmission properties
    public DataTransmission dataTransmission;
    
    public LayerMask collisionMask;
    // Canvas for link labels
    public GameObject canvas;
    // prefab for the link label
    public GameObject labelPrefab;
    public GameObject linkLabel;


    // Start is called before the first frame update
    void Start()
    {   
        canvas = GameObject.Find("World Canvas");
        // load the prefab for the label
        labelPrefab = Resources.Load("LinkLabel") as GameObject;
        graphCreator = GameObject.Find("Graph Creator").GetComponent<GraphCreator>();
        // Make link status and previous global power 
        // true at start so that the update takes care of the actual status
        status = true;
        previousStatus = status;
        indirectOn = true;
        directOn = true;
        previousDirectOn = directOn;
        previousIndirectOn = indirectOn;
        prevGlobalPower = true;
        /*
        // When link is created, the  status depends on if global power is currently on/off
        if(graphCreator.globalPower == false)
        {
            status = false;
            indirectOn = false;
            directOn = false;
        }
        else
        {
            status = true;
            previousStatus = status;
            
            indirectOn = true;
            directOn = true;
        }
        previousStatus = status;  
        */   
        //gameObject.layer = LayerMask.NameToLayer("Links");
        //collisionMask = Physics2D.GetLayerCollisionMask(gameObject.layer);
        lr = transform.GetComponent<LineRenderer>();
        this.gameObject.tag = "Link";
        dataTransmission = GameObject.Find("Data Transmission").GetComponent<DataTransmission>();
        linkRenderer = gameObject.GetComponent<Renderer>();
        InitializeRigidBody();
        CreateCollider();
        ConvertToIPString(subnetIP);
        addLabel(startPos,endPos,midPos);
    }

    // Update is called once per frame
    void Update()
    {
        if(prevGlobalPower != graphCreator.globalPower)
        {
            // If network is turned on, the link itself is directly turned on and it is also indirectly 
            //turned on by the router connected to it
            if(graphCreator.globalPower == true)
            {
                directOn = true;
                indirectOn = true;
            }
            // If network is turned off, the link itself is directly turned off and it is also indirectly 
            // turned off by the router connected to it
            else if (graphCreator.globalPower == false)
            {
                directOn = false;
                indirectOn = false;
            }
            
        } 
        prevGlobalPower = graphCreator.globalPower;

        //indirectOn is the link status due to the router
        //directOn is the link status due to the link itself
        //NOTE : directOn has priority over indirectOn , if the link itself is up/down, then the status follows
        // Check if one is changed, determine the outcome 
        // Very Unlikely that both conditions will change in one frame but still considered
        if(previousDirectOn != directOn || previousIndirectOn != indirectOn)
        {
            Debug.Log("Change occurred");
        // Outcomes :
        // 1. Previously, routers are up and link is up 
        //    If routers are still up, link was turned off, then link will turn off
        //    If link was still up, but one of the routers are down, then link will turn off
        //    If link is down and one of the router is down, then link turns off (very unlikely)
        //    Hence, if either condition is false, then link status is off
            if( previousIndirectOn == true && previousDirectOn == true )
            {
                if(indirectOn == false || directOn == false)
                {
                    status = false;
                }
            }
        // 2. Previously, routers are up and link is down
        //    If routers are still up, but link is turned on, then link status should turn on
        //    If one of the routers are down and link is still down , then link status is down
        //    If one of the routers are down and link is up, then link is still down (very unlikely)
            else if (previousIndirectOn == true && previousDirectOn == false)
            {
                if(indirectOn == true && directOn == true)
                {
                    status = true;
                }
                else if (indirectOn == false && directOn == false)
                {
                    status = false;
                }
                else if (indirectOn == false && directOn == true)
                {
                    status = false;
                }
            }
        // 3. Previously, one of the routers are down and link is up
        //    Possibly, the link status could be down or up depending on whether it went from up/up or down/down
        //    to down/up.
        //    If one of the routers is still down and link is down then, the link status is down
        //    If both routers are up and link is up then, the link status is up
        //    If both routers are on and link is down then, the link status is down
            else if (previousIndirectOn == false && previousDirectOn == true)
            {
                if(indirectOn == false && directOn == false)
                {
                    status = false;
                }
                else if (indirectOn == true && directOn == true)
                {
                    status = true;
                }
                else if (indirectOn == true && directOn == false)
                {
                   status = false;
                }
                
            }
        //4. Previously, if one of the routers are down and the link is down
        //   If one of therouters are back up and the link is down, then the link is still down.
        //   If one of the routers are down and the link is up, then the link is up
        //   If both routers are up and the link is up then the link status is up
            else if (previousIndirectOn== false && previousDirectOn == false)
            {
                // Turning on router should turn link back on if it was off
                if(indirectOn == true && directOn == false)
                {
                    status = true;
                }
                else if (indirectOn == false && directOn == true)
                {
                    status = true;
                }
                else if (indirectOn == true && directOn == true)
                {
                    status = true;
                }

            }
        }   

            
        
        if(previousStatus != status)
        {
            if(status == false)
            {
                TurnLinkOff();
                SetTransparent();
                // Set to half transparency
            }
            else if ( status == true )
            {
                AddEdgeCost();
                SetOpaque();
                // Set to opaque
            }
        }
        // Force the link to be up/down depending on the actual status if it is also up/down
        directOn = status;
        previousStatus = status;
        previousDirectOn = directOn;
        previousIndirectOn = indirectOn;
    }

    void OnTriggerEnter2D (Collider2D other)
    {
        graphCreator = GameObject.Find("Graph Creator").GetComponent<GraphCreator>();
        Collider2D [] myCollider = new Collider2D [1];
        ContactFilter2D contactFilter = new ContactFilter2D().NoFilter();
        int colliderNum = other.OverlapCollider(contactFilter,myCollider);
        Debug.Log ("Other collider is :" + other.gameObject.name);
        Debug.Log("Collider detected is :" + myCollider[0].gameObject.name);
        string s = LayerMask.LayerToName(other.transform.gameObject.layer);
        Debug.Log("Layer of other collider is " + s);
        
        //This is the beginning point of the line, store it in entity1
        if(myCollider[0].gameObject.name == "Collider1")
        {
            //Debug.Log("Collider is Collider1");
            // a line could trigger a collision with another line and place the name of the collider instead
            if (other.gameObject.CompareTag("Router") || other.gameObject.CompareTag("Host"))
            {
                entity1 = other.gameObject.name;
                // Disable trigger after it has been activated
                myCollider[0].gameObject.GetComponent<Collider2D>().enabled = false;
 
                myCollider[0].isTrigger = false;
            }
        }
        else if (myCollider[0].gameObject.name == "Collider2")
        {
            //Debug.Log("Collider is Collider2");
            if (other.gameObject.CompareTag("Router") || other.gameObject.CompareTag("Host"))
            {
                entity2 = other.gameObject.name;
                // Disable trigger after it has been activated
                myCollider[0].gameObject.GetComponent<Collider2D>().enabled = false;
                myCollider[0].isTrigger = false;
            
            }
        }
        // Add the edge into adjacency matrix each time the link is created and collides with two routers
        // the if statement ensures that only after the second entity on the link is detected then it 
        // does the remaining processing once instead of twice.
        if(entity1 != null && entity2 != null)
        {
        Invoke("AddEdgeCost",0f);
        Invoke("AddEdgeLink",0f);
        // Let routers know there was a change in adjacency matrices
        Invoke("UpdateOn",0f);
        addColliderToLine(startPos,endPos,"SelectionCollider1",midPos);
        }
    }

    /*
    void OnCollisionEnter2D (Collision2D collision)
    {
        Collider2D other = collision.collider;
        Debug.Log("Incoming collider game object is : " + other.gameObject.name);
    }
    */

    void CreateCollider() 
    {
        // create the collider on one end of the line ( start position) 
        float width = lr.startWidth;
        startCollider = new GameObject("Collider1");
        startCollider.tag = "LinkCollider";
        //startCollider.layer = LayerMask.NameToLayer("Links");
        startCollider.transform.position = startPos;
        startCollider.transform.SetParent(transform);
        startCollider.AddComponent<BoxCollider2D>();
        boxCollider = startCollider.GetComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(2*width,2*width);
        boxCollider.isTrigger = true;
       

        // create the collider on other end of the line (end position) 
        endCollider = new GameObject("Collider2");
        endCollider.tag = "LinkCollider";
        //endCollider.layer = LayerMask.NameToLayer("Links");
        endCollider.transform.position = endPos;
        endCollider.transform.SetParent(transform);
        endCollider.AddComponent<BoxCollider2D>();
        boxCollider = endCollider.GetComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(2*width,2*width);
        boxCollider.isTrigger = true;
       
        
        
        
    }

    // Following method adds collider for selection , note that this 
    // must be ran after the entities on the link are assigned to avoid 
    // triggers
    private void addColliderToLine(Vector3 start, Vector3 end, string colliderName, Vector3 position)
    {
        GameObject colliderObject = new GameObject(colliderName);
        colliderObject.layer = LayerMask.NameToLayer("Ignore Triggers");
        colliderObject.tag = "Link";
        BoxCollider2D col = colliderObject.AddComponent<BoxCollider2D> ();
        col.transform.parent = lr.transform; // Collider is added as child object of line
        float lineLength = Vector3.Distance (start, end) / 2; // length of line divided by 2
        col.size = new Vector3 (lineLength, 0.15f, 0f); // size of collider is set where X is length of line, Y is width of line, Z will be set as per requirement
        //Vector3 midPoint = (start + end) / 2;
        col.transform.position = position; // setting position of collider object
        // Following lines calculate the angle between startPos and endPos
        float angle = (Mathf.Abs (start.y - end.y) / Mathf.Abs (start.x - end.x));
        if((start.y<endPos.y && start.x>end.x) || (end.y<start.y && end.x>start.x))
        {
            angle*=-1;
        }
        angle = Mathf.Rad2Deg * Mathf.Atan (angle);
        col.transform.Rotate (0, 0, angle);
    }

    private void addLabel(Vector3 start, Vector3 end, Vector3 position)
    {
        linkLabel = Instantiate(labelPrefab,canvas.transform);
        // Set name of label to the link name + label
        linkLabel.name = this.gameObject.name + " Label";
        // Set the position of the label to the centre of the link object
        linkLabel.transform.position = position;
        // set parent of link label to the canvas
        linkLabel.transform.SetParent(canvas.transform,true);
        // Determine angle to place the text
        float angle = (Mathf.Abs (start.y - end.y) / Mathf.Abs (start.x - end.x));
        if((start.y<endPos.y && start.x>end.x) || (end.y<start.y && end.x>start.x))
        {
            angle*=-1;
        }
        angle = Mathf.Rad2Deg * Mathf.Atan (angle);
        linkLabel.transform.Rotate (0, 0, angle);
        TextMeshProUGUI textComponent = linkLabel.GetComponent<TextMeshProUGUI>();
        // Shorten the text to just L instead of Link
        textComponent.text = this.gameObject.name.Replace("Link","L");
    }

    void InitializeRigidBody ()
    {
        gameObject.AddComponent<Rigidbody2D>();
        rigidBody = gameObject.GetComponent<Rigidbody2D>();
        rigidBody.gravityScale = 0;
        // could set the body type to static if needed
    }

    void ConvertToIPString(uint ip)
    {   
        byte[] bytes= BitConverter.GetBytes(ip);
        if(BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
     // subnetIPString = BitConverter.ToString(bytes);

        for ( int i = 0; i < 4; i++)
        {
            int octet = 0xFF & bytes[i];
            subnetIPString += "." + octet;
        }
        subnetIPString = subnetIPString.Substring(1);
    }

   

    // adding the edge for this link between routers into the "global" adjacency matrix 
    void AddEdgeCost () 
    {
        GameObject routerObject1 = GameObject.Find(entity1);
        GameObject routerObject2 = GameObject.Find(entity2);
        // if both connected entities are routers then add the edge, if one is a host don't add 
        if(routerObject1.CompareTag("Router") && routerObject2.CompareTag("Router"))
        {
        // Get the graph creator component from the game object so that the adjacency matrix can be edited
        graphCreator = GameObject.Find("Graph Creator").GetComponent<GraphCreator>();
        // using i and j as the indexes
        int i , j;
        router = GameObject.Find(entity1).GetComponent<Router>();
        // find the router number of entity 1 and store as index for i
        i = router.routerNumber;
        router = GameObject.Find(entity2).GetComponent<Router>();
        // find the router number of entity 2 and store as index for j
        j = router.routerNumber;
        // Add the edges twice because it is an undirected graph.
        graphCreator.adjMatrixOSPF[i,j] = cost;
        graphCreator.adjMatrixOSPF[j,i] = cost;
        // Since hop count is used as metric, the link cost is basically 1 for all edges in RIP
        graphCreator.adjMatrixRIP[i,j] = 1;
        graphCreator.adjMatrixRIP[j,i] = 1;
        }
    }

      // adding the edge for this link between routers into the "global" adjacency matrix 
    void AddEdgeLink () 
    {
        GameObject routerObject1 = GameObject.Find(entity1);
        GameObject routerObject2 = GameObject.Find(entity2);
        // if both connected entities are routers then add the edge, if one is a host don't add 
        if(routerObject1.CompareTag("Router") && routerObject2.CompareTag("Router"))
        {
        // Get the graph creator component from the game object so that the adjacency matrix can be edited
        graphCreator = GameObject.Find("Graph Creator").GetComponent<GraphCreator>();
        // using i and j as the indexes
        int i , j;
        router = GameObject.Find(entity1).GetComponent<Router>();
        // find the router number of entity 1 and store as index for i
        i = router.routerNumber;
        router = GameObject.Find(entity2).GetComponent<Router>();
        // find the router number of entity 2 and store as index for j
        j = router.routerNumber;
        // Add the edges twice because it is an undirected graph.
        graphCreator.adjMatrixLinks[i,j] = gameObject.name;
        graphCreator.adjMatrixLinks[j,i] = gameObject.name;
        
        }
    }


    // turn update on for all routers and to the routing information component
    public void UpdateOn()
    {
        routers = GameObject.FindGameObjectsWithTag("Router");
        // j = 1 here because index results in router prefab which we are not interested in
        for( int j = 0 ; j < routers.Length; j++)
        {
        updateRouter = GameObject.Find(routers[j].name).GetComponent<Router>();
        updateRouter.update = true;
        }
        // Set the pathDetermined bool to false so that new path can be determined (For OSPF)
        while(dataTransmission.pathDetermined == true)
        {
        dataTransmission.pathDetermined = false;
        }

        // if both entities on the link were routers or hosts then send update to routing information component
        GameObject routerObject1 = GameObject.Find(entity1);
        GameObject routerObject2 = GameObject.Find(entity2);
        //if(routerObject1.CompareTag("Router") && routerObject2.CompareTag("Router"))
        //{

            GameObject RI = GameObject.Find("Routing Information");
            RIComponent = RI.GetComponent<RoutingInformation>();
            // set updateOSPF to true so that the animation for exchange of routing information is triggered
            RIComponent.updateOSPF = true;
            // reset timer to 0 for hello packet interval
            RIComponent.timeElapsedSinceChange = 0f;
            RIComponent.linksOSPF.Add(this.gameObject.name);
        //}
    }

    private void TurnLinkOff()
    {
        GameObject routerObject1 = GameObject.Find(entity1);
        GameObject routerObject2 = GameObject.Find(entity2);
        // perform null check because this may run when link is created before the link determines 
        // what entities are on the link

        // if both connected entities are routers then turn cost to 0 for both OSPF and RIP matrices
        if(routerObject1 != null && routerObject2 != null)
        {
        if(routerObject1.CompareTag("Router") && routerObject2.CompareTag("Router"))
        {
        // Get the graph creator component from the game object so that the adjacency matrix can be edited
        graphCreator = GameObject.Find("Graph Creator").GetComponent<GraphCreator>();
        // using i and j as the indexes
        int i , j;
        router = GameObject.Find(entity1).GetComponent<Router>();
        // find the router number of entity 1 and store as index for i
        i = router.routerNumber;
        router = GameObject.Find(entity2).GetComponent<Router>();
        // find the router number of entity 2 and store as index for j
        j = router.routerNumber;
        // Add the edges twice because it is an undirected graph.
        graphCreator.adjMatrixOSPF[i,j] = 0;
        graphCreator.adjMatrixOSPF[j,i] = 0;
        // For RIP matrix , turn cost to 0 as well
        graphCreator.adjMatrixRIP[i,j] = 0;
        graphCreator.adjMatrixRIP[j,i] = 0;
        }
        }
        else
        {
            Debug.Log("Null entities on links when trying to turn off the link");
        }
    }

    private void SetTransparent()
    {
        Color currentColour = linkRenderer.material.color;
        currentColour.a = 0.3f;
        linkRenderer.material.color = currentColour;
    }

    private void SetOpaque()
    {
        Color currentColour = linkRenderer.material.color;
        currentColour.a = 1f;
        linkRenderer.material.color = currentColour;
    }




}
