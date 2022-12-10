using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Router : MonoBehaviour
{   
    // port interface number from 1 to 8
    public int [] portNum = new int[8];
    // to increment the portNum by 1 every time the port is assigned to a connection, used for indexing
    private int portIncrement = 0;
    // store the IP address of the ports 
    public uint [] portIPAddress = new uint [8];
    public string [] portIPAddressString = new string [8];
    // store the link interface associated with the ports, maximum 8 
    public List<string> linkInterface = new List<string>();
    // store router name 
    public string routerName;
    // store router name of neighbouring routers, maximum 7 
    public List <string> neighbourRouters = new List<string>();
    //store host names connected to router , maximum of 7
    public List<string> connectedHosts = new List<string>();
    //store the connected entity(routers and hosts in order) and its cost
    public Dictionary<string,int> connections = new Dictionary<string,int>();
    // to access the entities connected on the link
    private Link link;
    // temporary collider2d to be able to access the link via the collider child
    private Collider2D temp;



    // for debug purposes to see if components are entered in order 
    public List <string> connectionsList = new List<string>();
    private WaitForSeconds delay = new WaitForSeconds(5);
    
    
    // Start is called before the first frame update
    void Start()
    {
        routerName = gameObject.name;
        StartCoroutine(DebugMessage());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D (Collider2D other)
    {
        
        Collider2D [] myCollider = new Collider2D [1];
        ContactFilter2D contactFilter = new ContactFilter2D().NoFilter();
        int colliderNum = other.OverlapCollider(contactFilter,myCollider);
        temp = other;
        // adds all the link game objects the router is connected to
        linkInterface.Add(other.transform.parent.name);
        // adds all routers this router is connected to into the router's list of neighbour routers
        Invoke("AddNeighbourRouter",0.5f);
        // adds all the hosts connected to this router into the router's list of hosts
        Invoke("AddHost",0.5f);
        // adds all routers and hosts connected to this router in a dictionary wiht its costs
        // cost for router to host is 0
        Invoke("AddConnections",0.5f);
    }

    void AddNeighbourRouter()
    {
        link = temp.transform.parent.GetComponent<Link>();
        // the router adds the other router if it is a router into its neighbouring routers
        Debug.Log("The entities are " + link.entity1 + " and " + link.entity2);
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
            Debug.Log("Entity 1 is " + link.entity1 );
        }
        else if (routerName != link.entity2 && link.entity2.Contains("Router"))
        {
            connections.Add(link.entity2,link.cost);
            connectionsList.Add(link.entity2);
            AssignPortNumAndIP();
            Debug.Log("Entity 2 is " + link.entity2 );
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

    private IEnumerator DebugMessage()
    {
        while(true)
        {
            Debug.Log ("Number of Connections for " + routerName + ": " + connections.Count);
            yield return delay;
        }
    }
}
