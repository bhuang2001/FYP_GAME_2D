using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    // For instantiating the message prefab
    public GameObject message;
    // To hold a reference to the first instance of the message
    private static GameObject oldMessage;
    // To access the sprite renderer properties of the message
    private SpriteRenderer spriteRendrer;
    // This is to store the transform of the parent object which is Data Transmission so all the links can 
    // fall under this game object as child objects
    private Transform parent;
    // This is to create game objects for the animated links
    private GameObject temp;
    // This is to add line renderer components to animated links
    [SerializeField] private LineRenderer lr;
    //This is for accessing the list of links to be animated 
    private DataTransmission dataTransmission;
    // This is to access the link properties of each link
    private Link currLink,nextLink;
    // Get number of links in the animatedLinks list
    private int linkQuantity;
    // This is to store all points in order to animate the link
    public Vector3[] pointCoordinates;
    // This is to store the 3 points of each line
    private Vector3[] lineCoordinates;
    // This is for storing the coordinates of the start, mid, end coordinates of consecutive links
    private Vector3 currLinkStart, currLinkMid, currLinkEnd, nextLinkStart, nextLinkMid, nextLinkEnd;
    // Distances from start/end coordinate from previous link to start/end coordinate on the next link
    // SS is for start to start coordinate
    // SE is for start to end coordinate 
    // ES is for end to start coordinate
    // EE is for end to end coordinate
    private float distanceSS,distanceSE,distanceES,distanceEE, result;

    // Animation duration length per link 
    [SerializeField] private float animationDuration = 0.2f;
    // Animation startPos and endPos
    private Vector3 startPosition,endPosition;
    // startTime of animation sequence
    private float startTime;
    // time between each transition
    private float t;
    // position tracker
    private int index;


    

    // Destroy the previous message game object if it exists already
    
   
    // Start is called before the first frame update
    // This will only be run whenever a path is determined
    void Start()
    {

        
        message = Instantiate((GameObject)Resources.Load("Prefab/Message prefab"));
        message.name = "Message";
        message.tag = "Message";
        spriteRendrer = message.GetComponent<SpriteRenderer>();
        spriteRendrer.sortingOrder = 2;
        oldMessage = message;

        parent = GameObject.Find("Data Transmission").transform;
         // deletes the previous set of animated links if it exists
        if (transform.childCount > 0)
        {
            foreach(Transform child in parent)
            {
                Destroy(child.gameObject);
            }
        }
        dataTransmission = GameObject.Find("Data Transmission").GetComponent<DataTransmission>();
        linkQuantity = dataTransmission.animatedLinks.Count;
        // every link has a start,mid,end coordinate so there are 3 coordinates per link
        pointCoordinates = new Vector3[3*linkQuantity]; 
        GetPointCoordinates(pointCoordinates);
        CreateAnimationLines();
        

        // Start of animation seqeunce
        message.transform.position = pointCoordinates[0];
        startPosition = message.transform.position;
        startTime = Time.time;
        // index is one because the position of the message is already at index 0 
        index = 1;
        endPosition = pointCoordinates[index];

    
        
    }

    // Update is called once per frame
    void Update()
    {
        t = (Time.time - startTime)/animationDuration;
        if ( index != 0 )
        {
        message.transform.position = Vector3.Lerp(startPosition,endPosition, t);
        }
        else
        {
            message.transform.position = pointCoordinates[0];
        }
        if( t >= 1)
        {
            
            startTime = Time.time;
            index++;
            index = index % pointCoordinates.Length;
            startPosition = endPosition;
            endPosition = pointCoordinates[index];
        }
    }

    private void GetPointCoordinates(Vector3[] pointCoordinates)
    {
        
        for(int i = 0, j = 0; i < linkQuantity - 1; i++, j = j+3)
        {   
            // Determining the direction to animate from
            // We have the start and end coordinates of two consecutive links
            currLink = dataTransmission.animatedLinks[i].GetComponent<Link>();
            nextLink = dataTransmission.animatedLinks[i + 1].GetComponent<Link>();
            // Assigning the start and end coordinates to the respective links
            currLinkStart = currLink.startPos;
            currLinkMid = currLink.midPos;
            currLinkEnd = currLink.endPos;
            nextLinkStart = nextLink.startPos;
            nextLinkMid = nextLink.midPos;
            nextLinkEnd = nextLink.endPos;
            distanceSS = Vector3.Distance(currLinkStart,nextLinkStart);
            distanceSE = Vector3.Distance(currLinkStart,nextLinkEnd);
            distanceES = Vector3.Distance(currLinkEnd,nextLinkStart);
            distanceEE = Vector3.Distance(currLinkEnd,nextLinkEnd);
            
            // If the 2 closest coordinates are the start coord of prev Link to start coord of next Link
            // Assign the order of coordinates appropriately
                // If the last two links are reached, then assign the last set of coordinates from the 
                // accordingly last link 
            if(distanceSS == Mathf.Min(distanceSS,distanceSE,distanceES,distanceEE))
            {
                pointCoordinates[j] = currLinkEnd;
                pointCoordinates[j + 1] = currLinkMid;
                pointCoordinates[j + 2] = currLinkStart;

                if(i == linkQuantity - 2)
                {
                    pointCoordinates[j + 3] = nextLinkStart;
                    pointCoordinates[j + 4] = nextLinkMid;
                    pointCoordinates[j + 5] = nextLinkEnd;
                }
            }
            else if(distanceSE == Mathf.Min(distanceSS,distanceSE,distanceES,distanceEE))
            {
                pointCoordinates[j] = currLinkEnd;
                pointCoordinates[j + 1] = currLinkMid;
                pointCoordinates[j + 2] = currLinkStart;

                if(i == linkQuantity - 2)
                {
                    pointCoordinates[j + 3] = nextLinkEnd;
                    pointCoordinates[j + 4] = nextLinkMid;
                    pointCoordinates[j + 5] = nextLinkStart;
                }
            }
            else if(distanceES == Mathf.Min(distanceSS,distanceSE,distanceES,distanceEE))
            {
                pointCoordinates[j] = currLinkStart;
                pointCoordinates[j + 1] = currLinkMid;
                pointCoordinates[j + 2] = currLinkEnd;
                if(i == linkQuantity - 2)
                {
                    pointCoordinates[j + 3] = nextLinkStart;
                    pointCoordinates[j + 4] = nextLinkMid;
                    pointCoordinates[j + 5] = nextLinkEnd;
                }
            }
            else if (distanceEE == Mathf.Min(distanceSS,distanceSE,distanceES,distanceEE))
            {
                pointCoordinates[j] = currLinkStart;
                pointCoordinates[j + 1] = currLinkMid;
                pointCoordinates[j + 2] = currLinkEnd;

                if(i == linkQuantity - 2)
                {
                    pointCoordinates[j + 3] = nextLinkEnd;
                    pointCoordinates[j + 4] = nextLinkMid;
                    pointCoordinates[j + 5] = nextLinkStart;
                }
            }
        }

    }

    // Might remove this 
    
    private void CreateAnimationLines()
    {
        for(int i = 0; i < linkQuantity; i++)
        {
            parent = GameObject.Find("Data Transmission").transform;
            temp = new GameObject();
            temp.name = "AnimatedLink" + i;
            temp.AddComponent<LineRenderer>();
            lr = temp.GetComponent<LineRenderer>();
            temp.transform.SetParent(parent);
            lr.material = Resources.Load("Material/link_material", typeof(Material)) as Material;
            lr.positionCount = 3;
            lr.startWidth = 0.15f;
            lr.endWidth = 0.15f;
            lr.useWorldSpace = false;
            lr.numCapVertices = 50;
            lr.sortingOrder = 1;
            SetColourGradient();
            // Setting the start, mid and end points of each line
            Debug.Log("Value of i is : " + i + " end coordinate is " + pointCoordinates[(3*i)+2]);
            lr.SetPosition(0,pointCoordinates[3*i]);
            lr.SetPosition(1,pointCoordinates[(3*i) + 1]);
            lr.SetPosition(2,pointCoordinates[(3*i)+2]);
        }
    }

    private void SetColourGradient() 
    {
        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.blue, 0.0f), new GradientColorKey(Color.red, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        lr.colorGradient = gradient;
    }



   
}
