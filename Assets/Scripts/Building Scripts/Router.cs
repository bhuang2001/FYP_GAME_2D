using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using PriorityQueue;

public class Router : MonoBehaviour
{   
    // this lets the router know if it needs to update its routing tables or not
    public bool update, updateRIP;
    // status of the router, true by default
    public bool prevRouterStatus,routerStatus ;
    //  to store previuos status of global power of network
    public bool prevGlobalPower;
    // Using maximum routers as 25 
    public const int maxRouters = 25;
    // Adjacency matrixes for each OSPF, RIP and the one that stores which one is being used
    public int[,] globalAdjacencyMatrix = new int[maxRouters,maxRouters];
    //public int[,] globalAdjacencyMatrixOSPF = new int[maxRouters,maxRouters];
    //public int[,] globalAdjacencyMatrixRIP = new int[maxRouters,maxRouters];
    // The output array. dist[i] will hold the shortest distance from src to i
    public int [] distanceOSPF = new int[maxRouters];
    public int [] distanceRIP = new int[maxRouters];

    // The number stored in this parent array is the router number of the parent router of router #index
    public int [] parent = new int[maxRouters];
    // The link stored in this array is the link interface which connects the parent router and the router 
    // #index
    public string [] parentLinks = new string[maxRouters];
    // The link stored in this array is the outgoing interface link which the source router must send
    // the packet to in order to reach router #index
    public string [] outgoingLinks = new string [maxRouters];

    // The indexes here coincide from here...
    // port interface number from 1 to 8
    public int [] portNum = new int[8];
    // to increment the portNum by 1 every time the port is assigned to a connection, used for indexing
    private int portIncrement = 0;
    // store the IP address of the ports 
    public uint [] portIPAddress = new uint [8];
    public string [] portIPAddressString = new string [8];
    // store the link interface associated with the ports, maximum 8 
    public List<string> linkInterface = new List<string>();
    // ... to here 

    // store router name 
    public string routerName;
    // store the router number associated with this router
    public int routerNumber;
    // store router name of neighbouring routers, maximum 7 
    public List <string> neighbourRouters = new List<string>();
    //store host names connected to router , maximum of 7
    public List<string> connectedHosts = new List<string>();
    //store the connected entity(routers and hosts in order) and its cost
    public Dictionary<string,int> connections = new Dictionary<string,int>();
    // to access the entities connected on the link
    public Link link;
    // temporary collider2d to be able to access the link via the collider child
    private Collider2D temp;
    // to access the adjacency matrices for weights and link interfaces 
    private GraphCreator graphCreator;
    // To edit the rigidbody components
    private Rigidbody2D rigidBody2D;
    // To access the RoutingInfomration Component
    private RoutingInformation RIComponent;

    // for debug purposes to see if components are entered in order 
    public List <string> connectionsList = new List<string>();
    private WaitForSeconds delay = new WaitForSeconds(5);
    // To access the sprite rendere component to make it transparent if it is off
    private SpriteRenderer spriteRenderer;
    
    
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        graphCreator = GameObject.Find("Graph Creator").GetComponent<GraphCreator>();
        // Make router status and previous global power 
        // true at start so that the update takes care of the actual status
        routerStatus = true;
        prevRouterStatus = routerStatus;
        prevGlobalPower = true;
        routerName = gameObject.name;
        //gameObject.layer = LayerMask.NameToLayer("Buildings");
        //collisionMask = Physics2D.GetLayerCollisionMask(gameObject.layer);
        //InitializeRigidBody();

    //    StartCoroutine(DebugMessage());
    }

    // Update is called once per frame
    void Update()
    {
        
        // Check to see if global power was toggled
        if(prevGlobalPower != graphCreator.globalPower)
        {
            // power is on, then router is on
            if(graphCreator.globalPower == true)
            {
                routerStatus = true;
            }
            // power is off, then router is off
            else if (graphCreator.globalPower == false)
            {
                routerStatus = false;
            }
        }
        // update the previous state to the current state
        prevGlobalPower = graphCreator.globalPower;
        
        if(prevRouterStatus != routerStatus)
        {
            Debug.Log("Change in router status");
            if(routerStatus == false)
            {
                // Make router transparent if it is off
                SetTransparent();
                // Send update to all links that this router is connected on
                foreach(var links in linkInterface)
                {
                    Link linkComponent = GameObject.Find(links).GetComponent<Link>();
                    GameObject object1 = GameObject.Find(linkComponent.entity1);
                    GameObject object2 = GameObject.Find(linkComponent.entity2);
                    if(object1.CompareTag("Router") && object2.CompareTag("Router"))
                    {
                        Router router1 = object1.GetComponent<Router>();
                        Router router2 = object2.GetComponent<Router>();
                        if(router1.routerStatus == false || router2.routerStatus == false) 
                        {
                            linkComponent.indirectOn = false;
                        }
                    }
                    else 
                    {
                        linkComponent.indirectOn = false;
                    }
                }
            }
            else if (routerStatus == true)
            {
                SetOpaque();
                foreach(var links in linkInterface)
                {
                    Link linkComponent = GameObject.Find(links).GetComponent<Link>();
                    GameObject object1 = GameObject.Find(linkComponent.entity1);
                    GameObject object2 = GameObject.Find(linkComponent.entity2);
                    // Checking to see if the link connects two routers
                    if(object1.CompareTag("Router") && object2.CompareTag("Router"))
                    {
                        Router router1 = object1.GetComponent<Router>();
                        Router router2 = object2.GetComponent<Router>();
                        // only if both routers are on , then this link is enabled
                        if(router1.routerStatus == true && router2.routerStatus == true) 
                        {
                            linkComponent.indirectOn = true;
                        }
                    }
                    // If the link is between a host and router then just enable the link
                    else 
                    {
                        linkComponent.indirectOn = true;
                    }
                }
            }
        }
        // update previous router status to the current router status after every check
        prevRouterStatus = routerStatus;

        // set the updateRIP status equal to the updateSignal status 
        updateRIP = graphCreator.updateSignalRIP;
        // update is provided from any link additions
        if(graphCreator.OSPF == true && update == true)
        {
            globalAdjacencyMatrix = graphCreator.adjMatrixOSPF.Clone() as int[,];
            // fill distance array with max value of int
            distanceOSPF = Enumerable.Repeat(int.MaxValue, maxRouters).ToArray();
            // fill parent array with -1
            parent = Enumerable.Repeat(-1, maxRouters).ToArray();
            // fill parent link array with "None" , used to animate the links when clicked on
            parentLinks = Enumerable.Repeat("None", maxRouters).ToArray();
            // fill outgoing links array with "None"
            outgoingLinks = Enumerable.Repeat("None", maxRouters).ToArray();

            dijkstra(globalAdjacencyMatrix,routerNumber);
            GetParentLinks(parent,parentLinks);
            //PrintPath(routerNumber,2,distance,parent,parentLinks);
            for (int final = 0; final < maxRouters; final++)
            {
                int v = final; 
                GetOutgoingLink(routerNumber, v, final, parent, parentLinks);
            }
            update = false;
        }

        if(graphCreator.RIP == true && updateRIP == true)
        {
            globalAdjacencyMatrix = graphCreator.adjMatrixRIP.Clone() as int[,];
            // fill distance array with max value of int
            distanceRIP = Enumerable.Repeat(int.MaxValue, maxRouters).ToArray();
            // fill parent array with -1
            parent = Enumerable.Repeat(-1, maxRouters).ToArray();
            // fill parent link array with "None" , used to animate the links when clicked on
            parentLinks = Enumerable.Repeat("None", maxRouters).ToArray();
            // fill outgoing links array with "None"
            outgoingLinks = Enumerable.Repeat("None", maxRouters).ToArray();

            BellmanFord(globalAdjacencyMatrix,routerNumber);
            GetParentLinks(parent,parentLinks);
            //PrintPath(routerNumber,2,distance,parent,parentLinks);
            for (int final = 0; final < maxRouters; final++)
            {
                int v = final; 
                GetOutgoingLink(routerNumber, v, final, parent, parentLinks);
            }
            
            updateRIP = false;
        }
    }

    void OnTriggerEnter2D (Collider2D other)
    {
        // I think i need to find a way to detect if the other collider is a link in the future
       // if(other.gameObject.CompareTag("Link"))
       // {
        Collider2D [] myCollider = new Collider2D [1];
        ContactFilter2D contactFilter = new ContactFilter2D().NoFilter();
        
        int colliderNum = other.OverlapCollider(contactFilter,myCollider);
        temp = other;
        Debug.Log("Collider detected is :" + other.gameObject.name);
        if(other.gameObject.CompareTag("LinkCollider"))
        {
            // adds all the link game objects the router is connected to
            linkInterface.Add(other.transform.parent.name);
            // adds all routers this router is connected to into the router's list of neighbour routers
            Invoke("AddNeighbourRouter",0f);
            // adds all the hosts connected to this router into the router's list of hosts
            Invoke("AddHost",0f);
            // adds all routers and hosts connected to this router in a dictionary wiht its costs
            // cost for router to host is 0
            Invoke("AddConnections",0f);
        }
      //  }
        
    }

    void InitializeRigidBody ()
    {
        gameObject.AddComponent<Rigidbody2D>();
        rigidBody2D = gameObject.GetComponent<Rigidbody2D>();
        rigidBody2D.gravityScale = 0;
        // could set the body type to static if needed
    }
    void AddNeighbourRouter()
    {
        link = temp.transform.parent.GetComponent<Link>();
        // the router adds the other router if it is a router into its neighbouring routers
         Debug.Log("The entities are " + link.entity1 + " and " + link.entity2);
        if(routerName != link.entity1 && link.entity1.Contains("Router")) 
        {
            neighbourRouters.Add(link.entity1);
        //   Debug.Log("Entity 1 is " + link.entity1 );
        }
        else if (routerName != link.entity2 && link.entity2.Contains("Router"))
        {
            neighbourRouters.Add(link.entity2);
        //    Debug.Log("Entity 2 is " + link.entity2 );
        }
    }

 
   /* void AddNeighbourRouter()
    {
        foreach (string name in linkInterface)
        {
            temp = GameObject.Find(name);
            link = temp.GetComponent<Link>();
            if(routerName != link.entity1 && link.entity1.Contains("Router")) 
            {
                neighbourRouters.Add(link.entity1);
                Debug.Log("Entity 1 is " + link.entity1 );
            }
            else if (routerName != link.entity2 && link.entity2.Contains("Router"))
            {
                neighbourRouters.Add(link.entity2);
                Debug.Log("Entity 2 is " + link.entity2 );
            
            }
        }
        
    }*/

    void AddHost()
    {
        link = temp.transform.GetComponentInParent<Link>();
        if(link.entity1.Contains("Host"))
        {
            connectedHosts.Add(link.entity1);
        }
        else if (link.entity2.Contains("Host"))
        {
            connectedHosts.Add(link.entity2);
        }
    }

    void AddConnections()
    {
        link = temp.transform.parent.GetComponent<Link>();
        // Add router to list of connections along with the link cost 
        if(routerName != link.entity1 && link.entity1.Contains("Router")) 
        {
            connections.Add(link.entity1,link.cost);
            connectionsList.Add(link.entity1);
            AssignPortNumAndIP();
        //    Debug.Log("Entity 1 is " + link.entity1 );
        }
        else if (routerName != link.entity2 && link.entity2.Contains("Router"))
        {
            connections.Add(link.entity2,link.cost);
            connectionsList.Add(link.entity2);
            AssignPortNumAndIP();
        //    Debug.Log("Entity 2 is " + link.entity2 );
        }

        // Add Host to dictionary with cost of 0, cost is not relevant for host to router 
        else if (link.entity1.Contains("Host"))
        {
            connections.Add(link.entity1,0);
            connectionsList.Add(link.entity1);
            AssignPortNumAndIP();
        }
        else if (link.entity2.Contains("Host"))
        {
            connectionsList.Add(link.entity2);
            connections.Add(link.entity2,0);
            AssignPortNumAndIP();
        }
    }
    //Assign a number to the ports on the router, the index of the array is equal to the port number
    //Then assign an ip address to the port.
    void AssignPortNumAndIP()
    {
        portNum[portIncrement] = portIncrement;
        AssignPortIPaddress(portNum[portIncrement]);
        ConvertToIPString(portIPAddress[portIncrement]);
        portIncrement++;
        
    }
    void AssignPortIPaddress (int portIndex)
    {
        // use portIncrement number
        //traverse the list of the link interfaces the router is connected on
        //take up to the subnet portion of the link "IP address" e.g 128.192.subnet and then add the host portion
        //maybe 1 for router and 2 for host so all octet of port IP address ends in 1?
        if(routerName == link.entity1)
        {
            portIPAddress[portIndex] = link.subnetIP + (uint)1;
        }
        else
        {
            portIPAddress[portIndex] = link.subnetIP + (uint)2;
        }

    }
    // Converting the port IP addresses to string for display purposes
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
            portIPAddressString[portIncrement] += "." + octet;
        }
        portIPAddressString[portIncrement] = portIPAddressString[portIncrement].Substring(1);
    }

// Methods for OSPF Routing Algorithm


// Method to print the path from the source node to another node (router to another router)
    public static void PrintPath(int u, int v, int[] distance, int[] parent, string [] parentLinks)
    {
        if (v < 0 || u < 0)
        {
            return;
        }
        if (v != u)
        {
            PrintPath(u, parent[v], distance, parent, parentLinks);
            Debug.Log("Path : Router " + v + " on " + parentLinks[v] + " with  Accumulated Link Cost: " + distance[v]);
        }
        else
            Debug.Log("Path : Router " + v + " Link Cost: " + distance[v]);
    }

// Method to update parentLinks array to know which link interface connects to the parent router
    public void GetParentLinks(int[] parent, string[] parentLinks)
    {
        graphCreator = GameObject.Find("Graph Creator").GetComponent<GraphCreator>();
        int parentRouter;
        
        // iterate through the list of parent routers to determine the link interface with its child router
        // index i 
        for(int i = 0; i < parentLinks.Length; i++)
        {
            // determine the router number of the parent router
            parentRouter = parent[i];
            // if there is a parent router then find the link interface between the parent and child router and 
            //store in parentLinks where i is the index of the end destination router/ child router
            if(parentRouter != -1)
            {
            parentLinks[i] = graphCreator.adjMatrixLinks[parentRouter,i];
            }
        }
    }

    // for all possible end destinations, this determines what link interface it must go through to reach there
    public void GetOutgoingLink(int u , int v, int final, int [] parent, string [] parentLinks) 
    {

        if (v < 0 || u < 0)
        {
            return;
        }
        if (v != u)
        {
            if(neighbourRouters.Contains("Router" + v))
            {
                
                outgoingLinks[final] = parentLinks[v];
                Debug.Log("From Router " + routerNumber + " to Router " + final + ", the outgoing link is " 
                + parentLinks[v]);
            }
                GetOutgoingLink(u, parent[v], final, parent, parentLinks);
        }
           

    }

// Debug Message Coroutine
    private IEnumerator DebugMessage()
    {
        while(true)
        {
            Debug.Log ("Number of Connections for " + routerName + ": " + connections.Count);
            yield return delay;
        }
    }

//Debug to see if adjacency matrix is updated for all routers
    private void DisplayAdjMatrix()
    {
        graphCreator = GameObject.Find("Graph Creator").GetComponent<GraphCreator>();
        for(int i = 0; i < 5; i++)
        {
            Debug.Log("Adjacency Matrix for Router" + i + " : ");
            for(int j = 0; j < 5; j++)
            {
                Debug.Log("global, Link Cost to Router" + j + " is " + globalAdjacencyMatrix[i,j]);
                Debug.Log("local, Link Cost to Router" + j + " is " + graphCreator.adjMatrixOSPF[i,j]);
            }
        }
        Debug.Log("THE END");
    }

// Method for determining the next vertex to be traversed
    private int minDistance(int[] dist,
                    bool[] sptSet)
    {
        // Initialize min value
        int min = int.MaxValue, min_index = -1;
  
        for (int v = 0; v < maxRouters; v++)
            if (sptSet[v] == false && dist[v] <= min) {
                min = dist[v];
                min_index = v;
            }
  
        return min_index;
    }
  
    // A utility function to print
    // the constructed distance array
    private void printSolution(int[] dist, int n)
    {
        Debug.Log("Vertex     Distance "
                      + "from Source\n");
        for (int i = 0; i < maxRouters; i++)
            Debug.Log(i + " \t\t " + dist[i] + "\n");
    }
  
    // Function that implements Dijkstra's single source shortest path algorithm
    // for a graph represented using adjacency matrix representation
    private void dijkstra(int[, ] graph, int src)
    {
  
        // sptSet[i] will true if vertex
        // i is included in shortest path
        // tree or shortest distance from
        // src to i is finalized
        bool[] sptSet = new bool[maxRouters];
  
        // Initialize all distances as
        // INFINITE and stpSet[] as false
        for (int i = 0; i < maxRouters; i++) {
            distanceOSPF[i] = int.MaxValue;
            sptSet[i] = false;
        }
  
        // set the distance to 0 for the source router (the router dijsktra's algorithm starts from)
        distanceOSPF[src] = 0;
  
        // Find shortest path for all vertices
        for (int count = 0; count < maxRouters - 1; count++) {
            // Pick the minimum distance vertex
            // from the set of vertices not yet
            // processed. u is always equal to
            // src in first iteration.
            int u = minDistance(distanceOSPF, sptSet);
  
            // Mark the picked vertex as processed
            sptSet[u] = true;
  
            // Update dist value of the adjacent
            // vertices of the picked vertex.
            for (int v = 0; v < maxRouters; v++)
  
                // Update dist[v] only if is not in
                // sptSet, there is an edge from u
                // to v, and total weight of path
                // from src to v through u is smaller
                // than current value of dist[v]
                if (!sptSet[v] && graph[u, v] != 0 && 
                     distanceOSPF[u] != int.MaxValue && distanceOSPF[u] + graph[u, v] < distanceOSPF[v])
                {
                    distanceOSPF[v] = distanceOSPF[u] + graph[u, v];
                    parent[v] = u;
                }
        }
  
        // print the constructed distance array
        printSolution(distanceOSPF, maxRouters);
    }

    void BellmanFord(int[,] graph, int src)
    {

        // Step 1: Initialize distances from src to all
        // other vertices as INFINITE
        for (int i = 0; i < maxRouters; ++i)
        {
            distanceRIP[i] = int.MaxValue;
        }

        distanceRIP[src] = 0;
 
        // Step 2: Relax all edges |V| - 1 times. A simple
        // shortest path from src to any other vertex can
        // have at-most |V| - 1 edges
        for( int i = 1; i < maxRouters; i++)
        {
            for (int u = 0; u < maxRouters; u++) {
                for (int v = 0; v < maxRouters; v++) {
                    if (distanceRIP[u] != int.MaxValue && graph[u,v] != 0 && distanceRIP[u]
                    + graph[u,v] < distanceRIP[v])
                    {
                        distanceRIP[v] = distanceRIP[u] + graph[u,v];
                        parent[v] = u;
                    }
                }
            }
        }
 
        // Step 3: check for negative-weight cycles. The
        // above step guarantees shortest distances if graph
        // doesn't contain negative weight cycle. If we get
        // a shorter path, then there is a cycle.

        /*for (int j = 0; j < E; ++j) {
            int u = graph.edge[j].src;
            int v = graph.edge[j].dest;
            int weight = graph.edge[j].weight;
            if (dist[u] != int.MaxValue
                && dist[u] + weight < dist[v]) {
                Console.WriteLine(
                    "Graph contains negative weight cycle");
                return;
            }
        }*/
        printSolution(distanceRIP,maxRouters);
    }

    private void SetTransparent()
    {
        Color currentColour = spriteRenderer.material.color;
        currentColour.a = 0.5f;
        spriteRenderer.material.color = currentColour;
    }

    private void SetOpaque()
    {
        Color currentColour = spriteRenderer.material.color;
        currentColour.a = 1f;
        spriteRenderer.material.color = currentColour;
    }
}
