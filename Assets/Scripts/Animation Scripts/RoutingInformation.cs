using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoutingInformation : MonoBehaviour
{
// This is to access the number of routers that created on the network so
// the GetSourceRouter() function knows when to stop checking
private BuildingSystem buildingSystem;
// glow material is for displaying routing information being transmitted
// nonglow material is for the regular link material (white)
public Material glow, nonglow;

// to access the linerenderer properties
private LineRenderer lr;

// OSPF : receives update signal from link connections (triggers with routers only)
// OSPF : also needs to send hello packets every 10 seconds to know if links are functional
// RIP : updates are on periodically (using 5 seconds but normally 30 seconds)
public bool updateOSPF;
public bool updateRIP;

// The starting router for which the level order traversal starts 
private Router source; 

// These links keep track of the links that have a connection in OSPF mode
public List<string> linksOSPF = new List<string>();
// These links keep track of the current level of links being traversed in RIP mode
public List<string> currentLevelLinks = new List<string>();
// For the animation of links
public float timeElapsed, timeDelay;
// For sending hello packets in OSPF mode
public float timeElapsedSinceChange, helloTimer;
private Color blue = Color.blue;
private Color white = Color.white;

// To access the graphCreator component which stores the status of the update signal for RIP and the selected 
// mode of operation as in OSPF or RIP.
private GraphCreator graphCreator;
private DataTransmission dataTransmission;

    // Start is called before the first frame update
    void Start()
    {
        buildingSystem = GameObject.Find("Grid").GetComponent<BuildingSystem>();
        graphCreator = GameObject.Find("Graph Creator").GetComponent<GraphCreator>();
        dataTransmission = GameObject.Find("Data Transmission").GetComponent<DataTransmission>();
        helloTimer = 10f;
        timeElapsed = 0f;
        timeDelay = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        if(graphCreator.OSPF == true)
        {
            timeElapsedSinceChange = timeElapsedSinceChange + 1f * Time.deltaTime;
            if(timeElapsedSinceChange >= helloTimer)
            {
                updateOSPF = true;
                // Tell routers to update its forwarding tables
                GameObject [] routers = GameObject.FindGameObjectsWithTag("Router");
                // j = 1 here because index results in router prefab which we are not interested in
                for(int j = 1 ; j < routers.Length; j++)
                {
                Router routerComponent = GameObject.Find(routers[j].name).GetComponent<Router>();
                routerComponent.update = true;
                }
                // obtain new routing path is data transmission is occurring
                dataTransmission.pathDetermined = false;
                timeElapsedSinceChange = 0f;
            }
        }
        // For OSPF mode and whenever there is a link connection or the OSPF mode was selected
        // This will also run after every hello packet time interval
        if( graphCreator.OSPF == true && updateOSPF == true)
        {
            GlowLinks(linksOSPF);
            timeElapsed = timeElapsed + 1f * Time.deltaTime;
            if( timeElapsed >= timeDelay)
            {
                DisableGlow(linksOSPF);
                timeElapsed = 0f;
                updateOSPF = false;
            }
        }
        // Whenever RIP is selected as the mode and the periodic interval has elapsed, this will run 
        else if (graphCreator.RIP == true && updateRIP == true)
        {
            /*
            GlowLinks(linksOSPF);
            timeElapsed = timeElapsed + 1f * Time.deltaTime;
            if( timeElapsed >= timeDelay)
            {
                DisableGlow(linksOSPF);
                timeElapsed = 0f;
                updateRIP = false;
            } */

            Router source = GetSourceRouter();
            LevelOrderTraversal(source);
            updateRIP = false;
        }
    }

    // Lights up all links that are connected to routers as blue
    // Make routing information updates(blue) show in front of the data transmission lines
    private void GlowLinks(List<string> links)
    {
        foreach(var name in links)
        {
            lr = GameObject.Find(name).GetComponent<LineRenderer>();
            lr.sortingOrder = 3;
            SetColourGradient(blue,lr);
        }
    }

    // Returns the links back to white colour
    // Set back to lowest sorting order when it returns to white
    private void DisableGlow(List<string> links)
    {
        foreach(var name in links)
        {
            lr = GameObject.Find(name).GetComponent<LineRenderer>();
            lr.sortingOrder = 0;
            SetColourGradient(white,lr);
        }
    }

    // Sets colour of line
    private void SetColourGradient(Color colour, LineRenderer lr) 
    {
        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(colour, 0.0f), new GradientColorKey(colour, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        lr.colorGradient = gradient;
    }

    // Traverses all routers from source to children until all children are processed
    // Highlights the neighbour links of each router processed
    // Something about this is infinitely looping
    private void LevelOrderTraversal (Router source)
    {   
        if (source == null)
        { 
            return;
        }
        // the tree level is used to invoke delays
        int treeLevel = 0; // base level
        bool processed = false;
        Queue<Router> routerQueue = new Queue<Router>();
        List<Router> previousLevelRouters = new List<Router>();
        routerQueue.Enqueue(source);

        while(routerQueue.Count != 0)
        {
            int n = routerQueue.Count;
            while (n > 0)
            {
                // assign the next router in queue to be processed
                Router currentRouter = routerQueue.Peek();
                // Remove from the queue after
                routerQueue.Dequeue();
                previousLevelRouters.Add(currentRouter);
                // Get the neighbour routers and its link interfaces
                List<string> currentLevelLinks = currentRouter.linkInterface;
                List <string> neighbours = currentRouter.neighbourRouters;
                // Highlight the links to show that router information is being exchanged
                // Add the glow link component to the links 
                foreach(var name in currentLevelLinks)
                {
                GlowLinkRIP glowComponent = GameObject.Find(name).AddComponent<GlowLinkRIP>();
                glowComponent.level = treeLevel;
                }

                for(int i = 0; i < neighbours.Count; i++)
                {
                    // Getting neighbour routers which are children of current router and
                    // adding into the queue
                    Router neighbour = GameObject.Find(neighbours[i]).GetComponent<Router>();
                    // Since the neighbours could contain the parent router, this must be included
                    // otherwise it will infinitely loop
                    for(int j = 0; j < previousLevelRouters.Count; j++)
                    {
                        // If the neighbour was already processed, then it was a parent router
                        // set processed to true so that it does not enqueue it to avoid infinite looping
                        if(neighbour == previousLevelRouters[j])
                        {
                            processed = true;
                        }
                    }
                    if(processed == false)
                    {
                    routerQueue.Enqueue(neighbour);
                    }
                    // set processed back to false at end of loop
                    processed = false;
                }
                n--;
            }
            // increase the level
            treeLevel++;
        }
    }

    private Router GetSourceRouter()
    {
        GameObject routerObj;
        int cnt = 0;
        string sourceRouterName;
        Router sourceRouter = null;
        while(sourceRouter == null)
        {
            // If all routers are checked then exit while loop and return the null router
            if (cnt >= buildingSystem.routerNum)
            {
                break;
            }
            sourceRouterName = "Router" + cnt;
            routerObj = GameObject.Find(sourceRouterName);
            sourceRouter = GameObject.Find(sourceRouterName).GetComponent<Router>();
            // if router is down then find next router as the source router
            if (sourceRouter.routerStatus == false)
            {
                sourceRouter = null;
                cnt++;
            }
        }
        return sourceRouter;
    }

    private void AnimateRIP()
    {
        int i = 0;
        Router currentRouter;
        string currentRouterName;
        while(i < 10)
        {
        currentRouterName = "Router" + i;
        currentRouter = GameObject.Find(currentRouterName).GetComponent<Router>();
        List<string> currentLevelLinks = currentRouter.linkInterface;
        GlowLinks(currentLevelLinks);
        DisableGlow(currentLevelLinks);
        }
    }


}
