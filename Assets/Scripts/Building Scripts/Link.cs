using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

public class Link : MonoBehaviour
{
    // on is true, off is false
    public bool status;
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
  
    


    // Start is called before the first frame update
    void Start()
    {
        lr = transform.GetComponent<LineRenderer>();
        this.gameObject.tag = "Link";
        dataTransmission = GameObject.Find("Data Transmission").GetComponent<DataTransmission>();
        InitializeRigidBody();
        CreateCollider();
        ConvertToIPString(subnetIP);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D (Collider2D other)
    {
        graphCreator = GameObject.Find("Graph Creator").GetComponent<GraphCreator>();
        Collider2D [] myCollider = new Collider2D [1];
        ContactFilter2D contactFilter = new ContactFilter2D().NoFilter();
        int colliderNum = other.OverlapCollider(contactFilter,myCollider);
        Debug.Log("Collider detected is :" + myCollider[0].gameObject.name);

        //This is the beginning point of the line, store it in entity1
        if(myCollider[0].gameObject.name == "Collider1")
        {
            Debug.Log("Collider is Collider1");
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
            Debug.Log("Collider is Collider2");
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
        if(entity2 != null)
        {
        Invoke("AddEdgeCost",0f);
        Invoke("AddEdgeLink",0f);
        // Let routers know there was a change in adjacency matrices
        Invoke("UpdateOn",0f);
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
        startCollider.tag = "Link";
        startCollider.transform.position = startPos;
        startCollider.transform.SetParent(transform);
        startCollider.AddComponent<BoxCollider2D>();
        boxCollider = startCollider.GetComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(2*width,2*width);
        boxCollider.isTrigger = true;

        // create the collider on other end of the line (end position) 
        endCollider = new GameObject("Collider2");
        endCollider.tag = "Link";
        endCollider.transform.position = endPos;
        endCollider.transform.SetParent(transform);
        endCollider.AddComponent<BoxCollider2D>();
        boxCollider = endCollider.GetComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(2*width,2*width);
        boxCollider.isTrigger = true;
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
        for( int j = 1 ; j < routers.Length; j++)
        {
        updateRouter = GameObject.Find(routers[j].name).GetComponent<Router>();
        updateRouter.update = true;
        }
        // Set the pathDetermined bool to false so that new path can be determined (For OSPF)
        while(dataTransmission.pathDetermined == true)
        {
        dataTransmission.pathDetermined = false;
        }

        // if both entities on the link were routers then send update to routing information component
        GameObject routerObject1 = GameObject.Find(entity1);
        GameObject routerObject2 = GameObject.Find(entity2);
        if(routerObject1.CompareTag("Router") && routerObject2.CompareTag("Router"))
        {

            GameObject RI = GameObject.Find("Routing Information");
            RIComponent = RI.GetComponent<RoutingInformation>();
            // set updateOSPF to true so that the animation for exchange of routing information is triggered
            RIComponent.updateOSPF = true;
            // reset timer to 0 for hello packet interval
            RIComponent.timeElapsedSinceChange = 0f;
            RIComponent.linksOSPF.Add(this.gameObject.name);
        }
    }




}
