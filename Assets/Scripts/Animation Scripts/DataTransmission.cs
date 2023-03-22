using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataTransmission : MonoBehaviour
{
    // GameObjects to access the host and router objects
    private GameObject thisGameObject;
    public GameObject host;
    public GameObject router;
    public GameObject linkObject;

    // stores the router numbers of the start,next and last router. Last router is used as the index for each
    // router's outgoing link array
    public int start;
    public int next;
    public int last;
    // stores the link name of the outgoing link interface
    public string outgoingLinkName = "None";

    // this is to determine which hosts were selected
    public SelectionManager selectionManager;
    // this is to accessing the transmitting host information
    public Host transmitter;
    // this is for accessing the receiving host information
    public Host receiver;
    // this is for accessing the first router the data is transmitted onto
    public Router startRouter;
    //this is for accessing the next router the data is transmitted onto
    public Router currentRouter;
    // this is for accessing the last router the data is transmitted onto
    public Router lastRouter;
    // this is for accessing the link it is being transmitted on
    public Link link;

    // This is true when the link interfaces are determined for data transmission
    public bool pathDetermined;

    // List of the links on the path to be animated
    public List<GameObject> animatedLinks;

    // Start is called before the first frame update
    void Start()
    {
        thisGameObject = GameObject.Find("Data Transmission");
        selectionManager = GameObject.Find("SelectObjectsFunc").GetComponent<SelectionManager>();
        pathDetermined = false;
    }

    // Update is called once per frame
    void Update()
    {
        // If user does not want to send data, turn off the animation if it is running
        if(selectionManager.sendData == false)
        {
            if (this.gameObject.GetComponent<AnimationController>() != null)
                {
                    Destroy(GetComponent<AnimationController>());
                    // Clears all the links in the previous transmission
                    animatedLinks.Clear();
                    
                    // deletes the previous set of animated links if it exists
                    if (transform.childCount > 0)
                    {
                        foreach(Transform child in transform)
                        {
                            Destroy(child.gameObject);
                        }
                    }
                    
                    Debug.Log ("Previous Animation Controller Component Destroyed");
                    Debug.Log ("Links Cleared");
                }
        }

        // If user is not sending data (when button is not pressed), then set the path determined to be false
        if(selectionManager.done == false)
        {
            pathDetermined = false;
        }
        // get the sending machine/host
        else if(selectionManager.done == true)
        {   
            
            host = selectionManager.sendingHost;
            transmitter = host.GetComponent<Host>();
            // get the receiving machine/host
            host = selectionManager.receivingHost;
            receiver = host.GetComponent<Host>();

        

            if(transmitter != null && receiver != null && pathDetermined == false)
            {
            // If there is already an animation controller component , then destroy it first 
            // This would only happen if user decides to select two hosts to transmit/receive data again
                if (this.gameObject.GetComponent<AnimationController>() != null)
                {
                    Destroy(GetComponent<AnimationController>());
                    // Clears all the links in the previous transmission
                    animatedLinks.Clear();
                    /*
                    // deletes the previous set of animated links if it exists
                    if (transform.childCount > 0)
                    {
                        foreach(Transform child in transform)
                        {
                            Destroy(child.gameObject);
                        }
                    }
                    */
                    Debug.Log ("Previous Animation Controller Component Destroyed");
                    Debug.Log ("Links Cleared");
                }
                DetermineConnectedRouter();
                GetRouterNum();
                DetermineNextRouter();
                ForwardToLink(currentRouter,lastRouter);
            }
        }
        
        
    }

// Determine the connected routers for the transmitting and receiving hosts
    private void DetermineConnectedRouter()
    {
        // determine the router that the transmitting host is connected onto
        router = GameObject.Find(transmitter.connectedRouter);
        startRouter = router.GetComponent<Router>();
        router = GameObject.Find(receiver.connectedRouter);
        lastRouter = router.GetComponent<Router>();
    }

// Obtain the router numbers of the connected routers for the start and last router 
    private void GetRouterNum()
    {
        start = startRouter.routerNumber;
        last = lastRouter.routerNumber;
    }

    private void DetermineNextRouter()
    {
        // Initial transmission state where the current outgoing link doesn't exist as yet
        // The current router is the starting router
        if (outgoingLinkName == "None")
        {
        // Transmit on the link connecting the sending host to the start router then set it to the start router
            outgoingLinkName = transmitter.linkInterface;
            linkObject = GameObject.Find(outgoingLinkName);
        // Add to animatedLinks list 
            animatedLinks.Add(linkObject);
            Debug.Log("Data is being forwarded on " + outgoingLinkName + " right now ");
            currentRouter = startRouter;
        }
        // The next router is the other entity connected to the outgoing link which is not the current router
        else
        {
            // Get the Link component of the link that was last transmitted on
            link = GameObject.Find(outgoingLinkName).GetComponent<Link>();
            // Whichever of the two entities connected to the link isn't the current router,
            // assign the other entity as the current router because the next router's forwarding table is what
            // we want access to so that it can be forwarded on the right link towards the receiving host
            if(currentRouter.routerName == link.entity1)
            {
                router = GameObject.Find(link.entity2);
                currentRouter = router.GetComponent<Router>();
            }
            else if (currentRouter.routerName == link.entity2)
            {
                router = GameObject.Find(link.entity1);
                currentRouter = router.GetComponent<Router>();               
            }
        }
    }

    private void ForwardToLink(Router currRouter, Router endRouter)
    {   
        // If link is down or the sender/receiver is not connected,
        // then cancel the animation since it is not possible 
        // and stop it from trying to calculate a path 
        link = GameObject.Find(outgoingLinkName).GetComponent<Link>();
        if(link.status == false || transmitter.connected == false || receiver.connected == false)
        {
            // Destroy the previous animated links and message sprite
            if (transform.childCount > 0)
            {
                foreach(Transform child in transform)
                {
                    Destroy(child.gameObject);
                }
            }
            Destroy(GetComponent<AnimationController>());
            // Clears all the links in the previous transmission
            animatedLinks.Clear();
            Debug.Log ("Links connected to the hosts are down ");
            Debug.Log ("Links Cleared");
            // set pathDetermined to true so that it stops calculating a path
            pathDetermined = true;
            // This resets the algorithm when it calculates the path again
            outgoingLinkName = "None";
        }
    // If the current router it has reached is not the last router then keep transmitting on the links
    // until it is reached
        else if(currRouter != endRouter)
        {
        // Find the link game object it must be forwarded to 
            outgoingLinkName = currRouter.outgoingLinks[last];
            // outgoingLinkName is None whenever the link is down , hence it appears as none in a forwarding table
            if(outgoingLinkName == "None")
            {
                // Destroy the previous animated links and message sprite
                if (transform.childCount > 0)
                {
                    foreach(Transform child in transform)
                    {
                        Destroy(child.gameObject);
                    }
                }
                Destroy(GetComponent<AnimationController>());
            // Clears all the links in the previous transmission
                animatedLinks.Clear();
                Debug.Log ("Links connected to the hosts are down ");
                Debug.Log ("Links Cleared");
            // set pathDetermined to true so that it stops calculating a path
                pathDetermined = true;
                outgoingLinkName = "None";
            }
            else
            {
            linkObject = GameObject.Find(outgoingLinkName);
        // Add to animatedLinks list
            animatedLinks.Add(linkObject);
        //This is where I use some sort of visual cue and/or audio cue to show the data being transmitted
            Debug.Log("Data is being forwarded on " + outgoingLinkName + " right now ");
            }
        }
    // If it is the end router then the last link to be transmitted onto is the one connecting to the receiver 
    // host, and then it should reset current router back to the start router so the animation can be looped
        else if (currRouter == endRouter)
        {
            outgoingLinkName = receiver.linkInterface;
            linkObject = GameObject.Find(outgoingLinkName);
            // Add to animatedLinks list
            animatedLinks.Add(linkObject);
            Debug.Log("Data is being forwarded on " + outgoingLinkName + " right now ");

            // reset current router to start router by setting outgoinglink name to none
            // this will make DetermineNextRouter reset the current router back to start router
            outgoingLinkName = "None";
            pathDetermined = true;
            
            // This will only run when the path is determined 
            thisGameObject.AddComponent<AnimationController>();
        }
    }

    
}
