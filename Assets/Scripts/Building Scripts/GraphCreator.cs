

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PriorityQueue;



public class GraphCreator : MonoBehaviour
{
// stores the status of the network power 
public bool globalPower;
// the boolean variables keep track whether or not OSPF or RIP protocol is on
// by default OSPF is on and RIP is off
// updateSignal is off, when it is on it tells all routers to update its routing information if it is using RIP
public bool updateSignalRIP = false;
public bool OSPF = true, RIP = false;
public GameObject RI;
// gameobject array to turn updates for all routers back on after switching from RIP to OSPF
private GameObject [] routers;
private Router routerComponent;
// PERIODIC interval set for RIP, default is 30 seconds and timeElapsed keeps track of the amount of time passed
public float periodicInterval, timeElapsed;
public const int maxRouters = 10;
public int currentRouterNum = -1;
public int prevRouterNum = 0;
// Using a 10 by 10 adj matrix for now meaning there is a limitation of 10 routers only
public int[,] adjMatrixOSPF = new int[maxRouters,maxRouters];
public int[,] adjMatrixRIP = new int[maxRouters,maxRouters];
// adjMatrix to store the link interfaces for each connection from router to router
public string[,] adjMatrixLinks = new string[maxRouters,maxRouters];
// To set the updateOSPF signal to true whenever OSPF is turned on 
private RoutingInformation RIComponent;
// To access the DataTransmission properties 
public DataTransmission dataTransmission;

//for debug purposes
private WaitForSeconds delay = new WaitForSeconds(5);

// Method to initialize adjacency matrix with weights/link costs by filling with 0
public int[,] InitAdjMatrix(int[,] AdjMatrix)
{
    int i,j;
    for(i = 0; i < maxRouters ; i++ )
    {
        for(j = 0; j < maxRouters ; j++)
        {
            AdjMatrix [i,j] = 0;
        }
    }
    return AdjMatrix;
}

// Method to initialize adjacency matrix for the links connecting routers by filling it with "None"
public string[,] InitAdjMatrixLinks(string[,] AdjMatrix)
{
    int i,j;
    for(i = 0; i < maxRouters ; i++ )
    {
        for(j = 0; j < maxRouters ; j++)
        {
            AdjMatrix [i,j] = "None";
        }
    }
    return AdjMatrix;
}


    private IEnumerator DebugMessage()
    {
        while(true)
        {
            Debug.Log("Cost from Router 0 to Router 1 is " + adjMatrixOSPF[0,1]);
            Debug.Log("Cost from Router 1 to Router 0 is " + adjMatrixOSPF[1,0]);
            Debug.Log("Link from Router 0 to Router 1 is " + adjMatrixLinks[0,1]);
            Debug.Log("Link from Router 1 to Router 0 is " + adjMatrixLinks[1,0]);
            yield return delay;
        }
    }
        
    // Start is called before the first frame update
    void Start()
    {   
        globalPower = true;
        RI = GameObject.Find("Routing Information");
        RIComponent = GameObject.Find("Routing Information").GetComponent<RoutingInformation>();
        dataTransmission = GameObject.Find("Data Transmission").GetComponent<DataTransmission>();
        InitAdjMatrix(adjMatrixOSPF);
        InitAdjMatrix(adjMatrixRIP);
        InitAdjMatrixLinks(adjMatrixLinks);
        timeElapsed = 0;
        // Should be 30, using 5 because it is less time-consuming to see updates
        periodicInterval = 5;
        /*
        // Some test code to test the priority queue
        PriorityQueue<int> queue = new PriorityQueue<int>();
        //enqueue
        for (int i = 0; i < 10; i++)
        {
            int x = i;
            queue.Enqueue(x, x);
        }
        //dequeue
        while (queue.Count > 0)
        {
            Debug.Log(queue.Dequeue());
        }  */

        /*
        // Some test code to test dijkstra's algorithm and to see if it can print the path
        int source = 0;
        int[,] adjacencyMatrix = new int[,] { { 0,5,0,0,0 },
                                              { 5,0,5,0,0 },
                                              { 0,5,0,3,0 },
                                              { 0,0,3,0,0 },
                                              { 0,0,0,0,0 } };

        int numberOfVertex = adjacencyMatrix.GetLength(0);
        int[] distance = Enumerable.Repeat(int.MaxValue, numberOfVertex).ToArray();
        int[] parent = Enumerable.Repeat(-1, numberOfVertex).ToArray();
        distance[source] = 0;
        //calling dijkstra  algorithm
        Dijkstra(adjacencyMatrix, numberOfVertex, distance, parent);
        //printing distance
        PrintPath(0, 3, distance, parent);
        
        
        
        adjMatrix = InitAdjMatrix(adjMatrix);
        adjMatrixLinks = InitAdjMatrixLinks(adjMatrixLinks);
        // StartCoroutine(DebugMessage());
        */
    }

    // Update is called once per frame
    void Update()
    {
        // This is for sending the signal to update the animation in RoutingInfo for RIP
        if(RIP == true)
        {
            // Set pathDetermined to be true once it is in RIP mode
            // Only when the periodic interval has elapsed then the pathDetermined will be false
            // which will compute the updated routing path for data transmission
            // dataTransmission.pathDetermined = true;
            updateSignalRIP = false;
            timeElapsed = timeElapsed + 1f * Time.deltaTime;
            if(timeElapsed >= periodicInterval)
            {
                // This is for the routers
                updateSignalRIP = true;
                // This is for the animation
                RIComponent.updateRIP = true;
                //This triggers the system to determine the new path for data transmission 
                // each time router information is exchanged
                dataTransmission.pathDetermined = false;
                timeElapsed = 0f;
            }
        }
        /*
        currentRouterNum = GameObject.FindGameObjectsWithTag("Router").Length;
        if ( currentRouterNum != prevRouterNum ) 
        {

        }
        */
    }

    public void EnableOSPF()
    {
        // turn on OSPF
        OSPF = true;
        // turns on updateOSPF which controls the animation of links
        // for showing the exchange of routing information occurring
        RIComponent.updateOSPF = true;
        // reset the timer for hello packet interval since change occurred
        RIComponent.timeElapsedSinceChange = 0f;
        // Send update notifications to all routers so it can run Dijkstra's algorithm to update its routing
        // information.
        routers = GameObject.FindGameObjectsWithTag("Router");
        // j = 1 here because index results in router prefab which we are not interested in
        for( int j = 1 ; j < routers.Length; j++)
        {
        routerComponent = GameObject.Find(routers[j].name).GetComponent<Router>();
        routerComponent.update = true;
        }
        // When switching to OSPF, set path determined to false so it can compute the correct
        // routing path for data transmission
        RIP = false;
        dataTransmission.pathDetermined = false;
    }

    public void EnableRIP()
    {
        OSPF = false;
        RIP = true;
        // update RIP should be true as well so that the routing information updates as it is turned on
        // then it waits the periodic interval 
        updateSignalRIP = true;
        timeElapsed = 0f;
    }

    public void TogglePower()
    {
        if(globalPower == true)
        {
            globalPower = false;
        }

        else if(globalPower == false)
        {
            globalPower = true;
        }
    }
}
