using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DemoMode : MonoBehaviour
{   
    // This is to keep track of spawner routers, max of 6 routers should be spawned so that 2 hosts can always be spawned if needed
    // since there are 8 positions available
    private int spawnCountRouter ;
    // This is the randomly generated number to decide what the outcome/option is for the demo mode
    private int option;
    // keeps a list of the link gameobjects that are on currently on
    public List<GameObject> linksOff = new List<GameObject>();
    // keeps a list of the link gameobjects that are on currently on
    public List<GameObject> linksOn = new List<GameObject>();
    // list of available positions to spawn routers
    public List<Vector3> routerPositions = new List<Vector3>();
    // reference to the list of closest routers that can be connected to
    public List<GameObject> nearestRouters = new List<GameObject>();
    // storing the upper and lower limits of the x and y axis of the existing network
    private float maxY, maxX, minY, minX;

    // storing the vertexes of the boundary of the existing network
    private Vector3 topLeft, topRight, bottomLeft, bottomRight;

    private GameObject[] existingRouters;


    // To access the existing building system component to make changes
    private BuildingSystem buildingSystem;
    // To be able to place the router
    private Building building;
    // The grid gameobject is responsible for the operation of the canvas.
    private GameObject grid;
    // stores router prefab so it can be instantiated
    public GameObject routerPrefab;
    // stores host prefab so it can be instantiated
    public GameObject hostPrefab;
    // reference to the most recently spawned router and host's game object
    public GameObject spawnedRouter, spawnedHost;
    // To access the link manager to create link connections between routers
    private LinkManager linkManager;
    private GameObject linkManagerObject;
    // To access the data transmission object to send data between hosts
    private DataTransmission dataTransmission;
    // To access the selection manager component so that data transmission can be done
    private SelectionManager selectionManager;
    // material for link
    public Material material;
    // to access line renderer component of links created
    private LineRenderer lineRenderer;

    // Timer variables
    public float timeElapsed, timeDelay;
    

    // Start is called before the first frame update
    void Start()
    {
        spawnCountRouter = 0;
        maxY = 0f;
        maxX = 0f;
        minY = 0f;
        minX = 0f;
        timeElapsed = 0f;
        // timeDelay controls the time interval per random network change
        // 5 seconds is used
        timeDelay = 5f;
        grid = GameObject.Find("Grid");
        buildingSystem = grid.GetComponent<BuildingSystem>();
        dataTransmission = GameObject.Find("Data Transmission").GetComponent<DataTransmission>();
        selectionManager = GameObject.Find("SelectObjectsFunc").GetComponent<SelectionManager>();
        linkManagerObject = GameObject.Find("Link Creator");
        linkManager = linkManagerObject.GetComponent<LinkManager>();
        routerPrefab = (GameObject) Resources.Load("Router prefab");
        hostPrefab = (GameObject) Resources.Load("Computer prefab");
        material = (Material) Resources.Load("Material/link_material");

        
        // GetBoundaries
        GetBoundaries();
        // DeterminePositions - If all positions are taken up then, no more routers can be added
        DeterminePositions();
        // Get all links into the list that keeps track of links that are turned on and off
        GetAllLinksInList();
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed = timeElapsed + 1f * Time.deltaTime;
        if( timeElapsed >= timeDelay)
        {
            // Generate a random int from 1 to 4 inclusive as there are 4 options
            option = GenerateRandomInt(1,5);
            // Do something every timeDelay seconds.
            // If the outcome is to spawn router then
            // Only if there are still available router positions and there are less than 6 spawned routers 
            if(option == 1 && routerPositions.Count != 0 && spawnCountRouter < 6)
            {  
            // GetPosition 
            Vector3 spawnPos = GetPosition();    
            // SpawnRouter (at position obtained from GetPosition)
            SpawnRouter(spawnPos);
            // Find the two closest routers that can be connected to
            // FindNearestRouters 
            nearestRouters = FindNearestRouters(spawnedRouter,1);
            // Connect the spawned router to the nearest router by creating a link between both
            // ConnectEntities
            ConnectEntities(nearestRouters,spawnedRouter);
            // increment 
            spawnCountRouter++;
            }
            // If the rng decides option 2 and the list of links that are on isn't empty then turn off a 
            // randomly selectedlink
            else if ( option == 2 && linksOn.Count != 0)
            {
                TurnOffLink();
            }
            // If the rng decides option 3 and the list of links that are off isn't empty then turn on a 
            // randomly selectedlink
            else if ( option == 3 && linksOff.Count != 0)
            {
                TurnOnLink();
            }
            // If the rng decides option 4 , we should select two hosts to transmit and receive data
            // If there are less than two hosts on the system then spawn host instead
            else if(option == 4)
            {
                if(buildingSystem.hostNum >= 2)
                {
                    TransmitData();
                }
                else 
                {
                    // spawn host
                    // get available position to spawn a host
                    Vector3 spawnPos = GetPosition();
                    SpawnHost(spawnPos); 
                    // Find the closest router that it can be connected to
                    // FindNearestRouters 
                    nearestRouters = FindNearestRouters(spawnedHost,0);
                    // Connect the spawned router to the nearest router by creating a link between both
                    // Connect
                    ConnectEntities(nearestRouters,spawnedHost);
                }
            }
            timeElapsed = 0f;
        }
    }

    //Functions that should go in Start (), Start() will run when non-interactive mode is on

    // GetBoundaries - This function is run whenever the non-interactive mode is toggled on
    // It will get the boundaries of the existing network.

    private void GetBoundaries()
    {
        // Get a list of all the routers in the existing network
        if(existingRouters == null)
        {
            existingRouters = GameObject.FindGameObjectsWithTag("Router");
        }

        // Remember to change to i = 0 after the prefab is removed from the hierarchy

        // Update maxY, maxX, minY, minX if a new limit is found after traversing through each
        // of the existing routers

        // A network can only exist if there are two or more routers connected 
        // If no routers exist on the canvas then create a new network
        if(existingRouters.Length < 2)
        {
            CreateNewNetwork();
            // Update the existingRouters array 
            existingRouters = GameObject.FindGameObjectsWithTag("Router");
        }

        // changed i to 0 after removing prefab router from hierarchy
        for(int i = 0 ; i < existingRouters.Length; i++)
        {
            // If the x component of the router is greater than the previous max X value, then update it
            if(existingRouters[i].transform.position.x > maxX )
            {
                maxX = existingRouters[i].transform.position.x;
            }

            // If the y component of the router is greater than the previous max Y value , then update it
            if(existingRouters[i].transform.position.y > maxY )
            {
                maxY = existingRouters[i].transform.position.y;
            }

            // If the x component of the router is less than the previous min X value, then update it
            if(existingRouters[i].transform.position.x < minX )
            {
                minX = existingRouters[i].transform.position.x;
            }

            // If the y component of the router is less than the previous min Y value , then update it
            if(existingRouters[i].transform.position.y < minY )
            {
                minY = existingRouters[i].transform.position.y;
            }
        }
        // Set positions of vertices of the boundary of the existing network into the corresponding vector3.
        SetVertexPositions(minX,maxX, minY, maxY);
        
    }

    // SetVertexPositions - Sets the vector3 positions of the four vertices of the boundary box.
    private void SetVertexPositions(float xmin, float xmax, float ymin, float ymax)
    {
        topLeft.Set(xmin,ymax,0);
        topRight.Set(xmax,ymax,0);
        bottomLeft.Set(xmin,ymin,0);
        bottomRight.Set(xmax,ymax,0);
    }

    // CreateNewNetwork - If there isn't one, then two routers should be instantiated 
    // at set positions and connected to each other

    private void CreateNewNetwork()
    {
        Vector3 pos1 = new Vector3();
        pos1.Set(5f,5f,0f);
        Vector3 pos2 = new Vector3();
        pos2.Set(-5f,-5f,0f);
        SpawnRouter(pos1);
        SpawnRouter(pos2);

        //Connect routers using a link , will need to create function to do that and add it here
        CreateLink(pos1,pos2);
    }

    // DeterminePositions - This function will create another boundary box around the boundary of the existing 
    // network and add positions to a list that routers can be spawned at. 
    // On each side of the box, there should be 2 locations where routers can be placed, giving a total of 8.
    // The outer box should be from maxX + 10 to minX - 10 and maxY + 10 to minY - 10. This will ensure
    // there is enough spacing for routers to be spawned.
    private void DeterminePositions()
    {
        // temp variables to calculate and store the position
        Vector3 newPos = new Vector3();
        float xvar;
        float yvar;
        //  Divide the spacing into 3 sections, so that there can be two usable positions
        // 2nd and 3rd position will be used as spawn points, ----> | | | |
        int sections = 3;
        float outerMaxX = maxX + 10f;
        float outerMinX = minX - 10f;
        float outerMaxY = maxY + 10f;
        float outerMinY = minY - 10f;

        // This should be 20
        float xDifference = outerMaxX - outerMinX;
        float yDifference = outerMaxY - outerMinY;

        // Positions for top and bottom side
        // Top side
        // X position would require divisions of the x axis, Y position is in the middle between 
        // maxY and outerMaxY , i.e (maxY + outerMaxY)/2
        // For bottom side, Y position is in the middle between 
        // minY and outerMinY , i.e (minY + outerMinY)/2
        for (int cnt = 1; cnt < sections; cnt++ )
        {
            // minimum x value + offset
            xvar = outerMinX + ((xDifference/sections)*cnt);
            // y position for top side
            yvar = (maxY + outerMaxY)/2;
            // create the vector3 to store the position
            newPos.Set(xvar,yvar,0);
            // add the vector3 to the list of available router positions
            routerPositions.Add(newPos);
            // y position for bottom side
            yvar = (minY + outerMinY)/2;
            // create the vector3 to store the position
            newPos.Set(xvar,yvar,0);
            // add the vector3 to the list of available router positions
            routerPositions.Add(newPos);
        }

        // Positions for left and right side
        // Left side
        // Y position would require divisions of the y axis, X position is in the middle between 
        // minX and outerMinX , i.e (minX + outerMinX)/2
        // For right side, X position is in the middle between 
        // maxX and outerMaxX , i.e (maxX + outerMaxX)/2
        for (int cnt = 1; cnt < sections; cnt++ )
        {
            // minimum y value + offset
            yvar = outerMinY + ((yDifference/sections)*cnt);
            // x position for left side
            xvar = (minX + outerMinX)/2;
            // create the vector3 to store the position
            newPos.Set(xvar,yvar,0);
            // add the vector3 to the list of available router positions
            routerPositions.Add(newPos);
            // x position for right side
            xvar = (maxX + outerMaxX)/2;
            // create the vector3 to store the position
            newPos.Set(xvar,yvar,0);
            // add the vector3 to the list of available router positions
            routerPositions.Add(newPos);
        }
        
    }
    
    // GetAllLinksInList - This function obtains all link game objects that are turned on directly and turned off
    // directly into their corresponding lists
    
    private void GetAllLinksInList()
    {
        // Find all objects with the link script
        Link [] links =  GameObject.FindObjectsOfType<Link>();
        Link temp;
        GameObject [] goArray  = new GameObject[links.Length];
        // Store each gameobject into goArray and add to goList
        for( int i = 0 ; i < goArray.Length ; i++)
        {
            goArray[i] = links[i].gameObject;
            // Get access to the link script of the link game object
            temp = goArray[i].GetComponent<Link>();
            // If the link is on then, add it to the list of links that are on 
            if(temp.directOn == true)
            {
                linksOn.Add(goArray[i]);
            }
            // If the link is directly turned off, then add it to the list of links that are off
            else if(temp.directOn == false)
            {
                linksOff.Add(goArray[i]);
            }
        }
    }

    // GetAllLinksOff - This function obtains all link game objects that are turned on directly

    // GetPosition - This function retrieves a random position from the list of available router positions
    private Vector3 GetPosition()
    {
        Vector3 result = new Vector3();
        // Random.Range with int is min inclusive and max exclusive
        // Get a random index/position from 0 to the number of list items + 1 because it is max exclusive
        int index = Random.Range(0,routerPositions.Count);
        Debug.Log("Router positions left : " + routerPositions.Count);
        result = routerPositions[index];
        // remove position from the list of router positions as it is taken
        routerPositions.RemoveAt(index);
        return result;
    }

    // Functions that would be used in coroutine/update function every 5 seconds

    // SpawnRouter - This function will spawn the router at given location and
    //  utilize the existing BuildingSystem component.

    private void SpawnRouter(Vector3 position)
    {
        
        // Instantiate at the given position
        spawnedRouter = Instantiate(routerPrefab, position, Quaternion.identity);
        // Get the building component
        building = spawnedRouter.GetComponent<Building>();
        
        // This implements grid snapping, but could possibly leave it out if it affects linking routers
        // Will need to use the localPosition instead to connect two routers
        //building.transform.localPosition = buildingSystem.gridlayout.CellToLocalInterpolated(position 
        //                                    + new Vector3(0.5f,0.5f,0f));
        // Add router component
        building.gameObject.AddComponent<Router>();
        // assign its name
        building.gameObject.name = "Router" + buildingSystem.routerNum;
        building.gameObject.GetComponent<Router>().routerNumber = buildingSystem.routerNum;
        buildingSystem.routerNum++;
        // If this if statement does not work i can remove the if condition
        // and just place the building because I am spawning routers in positions that I know is available.
        if(building.CanBePlaced())
        {
            Debug.Log("Router was spawned");
            building.Place();
        }
    }

    // SpawnHost - This function will spawn a host at the given location and utilzie the existing BuildingSystem component.
    private void SpawnHost(Vector3 position)
    {
        
        // Instantiate at the given position
        spawnedHost = Instantiate(hostPrefab, position, Quaternion.identity);
        // Get the building component
        building = spawnedHost.GetComponent<Building>();
        
        // This implements grid snapping, but could possibly leave it out if it affects linking routers
        // Will need to use the localPosition instead to connect two routers
        //building.transform.localPosition = buildingSystem.gridlayout.CellToLocalInterpolated(position 
        //                                    + new Vector3(0.5f,0.5f,0f));
        // Add router component
        building.gameObject.AddComponent<Host>();
        // assign its name
        building.gameObject.name = "Host" + buildingSystem.hostNum;
        building.gameObject.GetComponent<Host>().hostNumber = buildingSystem.hostNum;
        buildingSystem.hostNum++;
        // If this if statement does not work i can remove the if condition
        // and just place the building because I am spawning routers in positions that I know is available.
        if(building.CanBePlaced())
        {
            Debug.Log("Host was spawned");
            building.Place();
        }
    }

    // FindNearestRouters - Determines the nearest router that the spawned router can be connected to
    // index = 1 for spawning a router, index = 0 for host because the 0th index is the spawned router itself in the routers array
    private List<GameObject> FindNearestRouters(GameObject obj, int index)
    {
        // cnt always inititalized as 2 which keeps track of the number of routers added to list
        int cnt = 0;
        // List to store the closest routers 
        List<GameObject> closestRouters = new List<GameObject>();
        // To store routers that are on the network
        GameObject[] routers; 
        // Get all of the routers that are on the canvas
        routers = GameObject.FindGameObjectsWithTag("Router");
        // Remember to remove the line below when the prefab is removed in hierarchy
        // Removes the first element in the array due to the prefab in hierarchy
        //routers = routers.Skip(1).ToArray();
        // Sorting the array by highest distance to the spawned router
        //alternatively can convert array into list and then sort if this does not work
        SortByDistance(routers,obj.transform.position);
        // Reverse the routers array so that the objects are sorted by lowest distance first
        routers = Enumerable.Reverse(routers).ToArray();
        for(int i = 0; i < routers.Length; i++)
        {
            Debug.Log(i + ": " + routers[i].name);
        }
        // Add the closest router into the list
        // i = 1 here because the 0 index game object is always the spawned object as it has 0 distance
        // to itself so it must be ignored.
        for(int i = index; i < routers.Length ; i++)
        {
            // Check if the router has an available port
            if(CanBeConnected(routers[i]))
            {
                // Add router to the list
                closestRouters.Add(routers[i]);
                // Increment the count
                cnt++;
            }
            // when the closest router is added into the list, break from the for loop 
            if( cnt == 1)
            {
                break; 
            }
        }
        return closestRouters;
    }

    // SortByDistance - This sorts the gameobject array by the distance to a given point (spawned router)
    private void SortByDistance ( GameObject[] array , Vector3 point )
    {
     System.Array.Sort( array, (a, b) => (int)Mathf.Sign(Vector3.Distance(point, 
                    b.transform.position) - Vector3.Distance(point, a.transform.position)));
    }
    
     // If above sort does not work , use this 
    // https://answers.unity.com/questions/598323/how-to-find-closest-object-with-tag-one-object.html
    private int ByDistance(GameObject a, GameObject b) 
    {
     var dstToA = Vector3.Distance(transform.position, a.transform.position);
     var dstToB = Vector3.Distance(transform.position, b.transform.position);
     return dstToA.CompareTo(dstToB);
    }

    // CanBeConnected - This function checks if the router has space , i.e if it has less than 8 connections
    private bool CanBeConnected(GameObject routerObject)
    {
        Router routerComponent;
        routerComponent = routerObject.GetComponent<Router>();
        if(routerComponent.connectionsList.Count < 8)
        {
            return true;
        }
        else 
            return false;
    }
    // ConnectEntities - This function will connect the spawned object to the closest router. 
    private void ConnectEntities(List<GameObject> routersList, GameObject obj)
    {
        Vector3 routerPos = new Vector3();
        
        // routersList should have a count of 1 since we only want to connect spawned router to the closest
        // router
        for(int i = 0 ; i < routersList.Count; i++)
        {
            routerPos = routersList[i].transform.position;
            CreateLink(obj.transform.position,routerPos);
        }
    }
    // CreateLink - This function should create a link utilizing the link manager
    // The input paramaters should be the two routers it will connect hence it will need
    // two vector3 inputs.
    private void CreateLink(Vector3 routerPos1, Vector3 routerPos2)
    {
        // temporary variable to get link cost
        int linkCost;
        Link linkComponent;
        GameObject linkObject = new GameObject("Link" + linkManager.currLines);
        // same function from linkManager 
        // if this does not work, just copy paste the function and make variables local to this function
        lineRenderer = linkObject.AddComponent<LineRenderer>();
       lineRenderer.material = material;
       lineRenderer.positionCount = 3;
       lineRenderer.startWidth = 0.15f;
       lineRenderer.endWidth = 0.15f;
       lineRenderer.useWorldSpace = false;
       lineRenderer.numCapVertices = 50;
       // get the midpoint of the link
       Vector3 middlePos = Vector3.Lerp(routerPos1,routerPos2,0.5f);
       // Set the positions of the link 
       lineRenderer.SetPosition(0,routerPos1);
       lineRenderer.SetPosition(1,middlePos);
       lineRenderer.SetPosition(2,routerPos2);
       // Creating The Link Component
       // Get a random link cost from 1 to 9 inclusive
       linkCost = GenerateRandomInt(1,10);
        linkObject.AddComponent<Link>();
        linkComponent = linkObject.GetComponent<Link>();
        // Let the link script know the position of the link
        linkComponent.startPos = routerPos1;
        linkComponent.midPos = middlePos;
        linkComponent.endPos = routerPos2;
        // Assign the cost to the link script
        linkComponent.cost = linkCost;
        // Cost entered must be set to true so that link can be selected and routes can be highlighted
        linkManager.costEntered = true;
        // Assign an IP address to the link created
       assignSubnetIP(linkComponent);
       // Add it to the links on list since all links are on by default
       linksOn.Add(linkObject);
       // Finally, increment the currLines variable in the linkManager component
       linkManager.currLines++;
    }

    // GenerateRandomInt - This function takes in a range of integers and outputs a random result

    private int GenerateRandomInt(int lowerInclusive, int upperExclusive)
    {
        int result;
        // generates a random integer from the lower limit to the upper limit (inclusive,exclusive)
        // e.g Random.Range(1,10) will give a number from 1 to 9 with equal probability
        result = Random.Range(lowerInclusive,upperExclusive);
        return result;
    }

    // SAME FUNCTION FROM LINK MANAGER
    // ONLY DIFFERENCE IS THAT IT MUST UPDATE THE LINK MANAGER PROPERTIES SUCH AS SUBNET NUM
    // Helper function which assign an IP address to the link created so that the router and host 
    // or router and router connected can take 
    // the IP address and manipulate the host portion (either 1 or 2)
    // 128.128.0.0 = 0x80800000   (QUADRANT 1)
    // 128.192.0.0 = 0x80c00000   (QUADRANT 2)
    // 128.224.0.0 = 0x80e00000   (QUADRANT 3)
    // 128.240.0.0 = 0x80f00000   (QUADRANT 4)
    public void assignSubnetIP(Link currentLink)
    {
        // this checks to see if the starting point of the link is in quadrant 1
        if(currentLink.startPos.y > ((-0.5*currentLink.startPos.x) + 4.27)  
        && currentLink.startPos.y > ((0.5 * currentLink.startPos.x) - 2.3))
        {
            // this increments the subnet portion by 1 e.g 128.128.1.0
            currentLink.subnetIP = 0x80800000 + (uint)((linkManager.subnetNum1 + 1)*256);
            linkManager.subnetNum1++;
        }
        // this checks to see if the starting point of the link is in quadrant 2
        else if(currentLink.startPos.y < ((-0.5*currentLink.startPos.x) + 4.27)  
        && currentLink.startPos.y > ((0.5 * currentLink.startPos.x) - 2.3))
        {
            // this increments the subnet portion by 1 e.g 128.192.1.0
            currentLink.subnetIP = 0x80c00000  + (uint)((linkManager.subnetNum2 + 1)*256);
            linkManager.subnetNum2++;
        }
        // this checks to see if the starting point of the link is in quadrant 3
        else if(currentLink.startPos.y < ((-0.5*currentLink.startPos.x) + 4.27)  
        && currentLink.startPos.y < ((0.5 * currentLink.startPos.x) - 2.3))
        {
            // this increments the subnet portion by 1 e.g 128.224.1.0
            currentLink.subnetIP = 0x80e00000  + (uint)((linkManager.subnetNum3 + 1)*256);
            linkManager.subnetNum3++; 
        }
        // lastly, it must fall in quadrant 4 , if code reaches here
        else 
        {
            currentLink.subnetIP = 0x80f00000  + (uint)((linkManager.subnetNum4 + 1)*256);
            linkManager.subnetNum4++; 
        }
    }

    // TurnOffLink - Turns off the link, removes from the linksOn list and adds to the linksOff list
    private void TurnOffLink()
    {
        // Get a random link to turn off
        int index = GenerateRandomInt(0,linksOn.Count);
        GameObject linkObject = linksOn[index];
        // Get the link component of the game object
        Link linkComponent = linkObject.GetComponent<Link>();
        //Turn off the link
        linkComponent.directOn = false;
        //Remove it from the linksOn list
        linksOn.RemoveAt(index);
        // Add it to the linksOff list
        linksOff.Add(linkObject);
    }

    // TurnOnLink - Turns on the link, removes from the linksOff list and adds to the linksOn list
    private void TurnOnLink()
    {
        // Get a random link to turn off
        int index = GenerateRandomInt(0,linksOff.Count);
        GameObject linkObject = linksOff[index];
        // Get the link component of the game object
        Link linkComponent = linkObject.GetComponent<Link>();
        //Turn on the link
        linkComponent.directOn = true;
        //Remove it from the linksOff list
        linksOff.RemoveAt(index);
        // Add it to the linksOn list
        linksOn.Add(linkObject);
    }

    // TransmitData - Transmits data between two randomly selected hosts.

    private void TransmitData()
    {
        // Get all host game objects
        GameObject[] hosts = GameObject.FindGameObjectsWithTag("Host");
        // changed to >= 2 after removing computer prefab from hierarchy
        // as a result, changed index to 0 and 1 after removing computer prefab from hierarchy
        if(hosts.Count() >= 2)
        {
            selectionManager.sendData = true;
            // Put first host as transmitter
            selectionManager.sendingHost = hosts[0];
            // Put second host as receiver
            selectionManager.receivingHost = hosts[1];
            // Set done to true
            selectionManager.done = true;
        }
    }
}
