using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;

public class LinkManager : MonoBehaviour
{
    private LineRenderer line;
    private Vector3 startMousePos;
    private Vector3[] connectorPos = new [] {new Vector3(0f,0f,0f),new Vector3(0f,0f,0f),new Vector3(0f,0f,0f)};
    private Vector3 endMousePos;
    public Material material;
    // to keep track of the number of links, for naming purposes
    private int currLines = 0;
    // when a button is pressed to add link, this turns true
    private bool addLink = false;
    public bool costEntered;
    // number to keep track of how many times the orientation of the link is pressed
    private int toggleCount = 0;
    // subnetNum keeps track of the number of subnets in each quadrant 
    private int subnetNum1 = 0;
    private int subnetNum2 = 0;
    private int subnetNum3 = 0;
    private int subnetNum4 = 0;
    public GameObject enterCostUI;
    public TMP_InputField inputField;
    private Link currentLink;
    
    // Start is called before the first frame update
    void Start()
    {

       // enterCostUI.SetActive(false);
       // inputField = GameObject.Find("UI/EnterCost/Canvas/InputWindow/InputField").GetComponent<TMP_InputField>();
    }

    // Update is called once per frame
    void Update()
    {
        placeLink();
    }

    void createLine() 
    {
        line = new GameObject("Link" + currLines).AddComponent<LineRenderer>();
       line.material = material;
       line.positionCount = 3;
       line.startWidth = 0.15f;
       line.endWidth = 0.15f;
       line.useWorldSpace = false;
       line.numCapVertices = 50;
    }

    private void placeLink () {
        if (addLink == true) {
            // Whenever link is added, the cost is not entered yet so set state to false
            costEntered = false;
            if (Input.GetMouseButtonDown(0)) 
            {
                if (line == null)
                {
                    createLine();
                }

                startMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                startMousePos.z = 0;
                line.SetPosition(0,startMousePos);
                line.SetPosition(1,startMousePos);
                line.SetPosition(2,startMousePos);
            }
            else if (Input.GetMouseButton(0) && line)
            {
                endMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                endMousePos.z = 0;
                connectorPos[0] = Vector3.Lerp(startMousePos,endMousePos,0.5f);
                Debug.Log("This works");
                line.SetPosition(1, connectorPos[0]);
               
                line.SetPosition(2, endMousePos);
            }
            else if (Input.GetMouseButtonUp(0) && line) 
            {
                endMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                endMousePos.z = 0;
                // straight line
                connectorPos[0] = Vector3.Lerp(startMousePos,endMousePos,0.5f);
               // connectorPos[0].x = (startMousePos.x + endMousePos.x)/2; 
              //  connectorPos[0].y = (startMousePos.y + endMousePos.y)/2; 
               // connectorPos[0].z = 0;
               Debug.Log("This also worked");
                //horizontal then vertical
                connectorPos[1].z = 0 ;
                connectorPos[1].x = endMousePos.x;
                connectorPos[1].y = startMousePos.y;

                //vertical then horizontal
                connectorPos[2].z = 0;
                connectorPos[2].x = startMousePos.x;
                connectorPos[2].y = endMousePos.y;

                line.SetPosition(1, connectorPos[0]);
                line.SetPosition(2, endMousePos);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                toggleCount = (toggleCount + 1) % 3;
                line.SetPosition(1,connectorPos[toggleCount]);
                line.SetPosition(2,endMousePos);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                
                enterCostUI.SetActive(true);
                currLines++;
                addLink = false;

            }

        }
    }
    // For pressing on "Link" button to create a link
    public void addLinkFunction () 
    {
        addLink = true;
    }

    // When pressing "Enter" button in the enterCost UI
    // Only after link cost is entered, then colliders initiated so the parameters can be transferred to the 
    // host and router
    // Also assigns the positions of the start, middle and end coordinates
    public void getCost()
    {
        line.gameObject.AddComponent<Link>();
        currentLink = GameObject.Find("Link" + (currLines-1)).GetComponent<Link>();
        currentLink.startPos = startMousePos;
        currentLink.midPos = connectorPos[toggleCount];
        currentLink.endPos = endMousePos;
        currentLink.cost = int.Parse(inputField.text);
        assignSubnetIP();
        enterCostUI.SetActive(false);
        line = null;
        costEntered = true;
        // After cost is entered , then set the addLink to false so it can only be pressed again afterwards
    
    }
    // Helper function which assign an IP address to the link created so that the router and host 
    // or router and router connected can take 
    // the IP address and manipulate the host portion (either 1 or 2)
    // 128.128.0.0 = 0x80800000   (QUADRANT 1)
    // 128.192.0.0 = 0x80c00000   (QUADRANT 2)
    // 128.224.0.0 = 0x80e00000   (QUADRANT 3)
    // 128.240.0.0 = 0x80f00000   (QUADRANT 4)
    public void assignSubnetIP()
    {
        // this checks to see if the starting point of the link is in quadrant 1
        if(currentLink.startPos.y > ((-0.5*currentLink.startPos.x) + 4.27)  
        && currentLink.startPos.y > ((0.5 * currentLink.startPos.x) - 2.3))
        {
            // this increments the subnet portion by 1 e.g 128.128.1.0
            currentLink.subnetIP = 0x80800000 + (uint)((subnetNum1 + 1)*256);
            subnetNum1++;
        }
        // this checks to see if the starting point of the link is in quadrant 2
        else if(currentLink.startPos.y < ((-0.5*currentLink.startPos.x) + 4.27)  
        && currentLink.startPos.y > ((0.5 * currentLink.startPos.x) - 2.3))
        {
            // this increments the subnet portion by 1 e.g 128.192.1.0
            currentLink.subnetIP = 0x80c00000  + (uint)((subnetNum2 + 1)*256);
            subnetNum2++;
        }
        // this checks to see if the starting point of the link is in quadrant 3
        else if(currentLink.startPos.y < ((-0.5*currentLink.startPos.x) + 4.27)  
        && currentLink.startPos.y < ((0.5 * currentLink.startPos.x) - 2.3))
        {
            // this increments the subnet portion by 1 e.g 128.224.1.0
            currentLink.subnetIP = 0x80e00000  + (uint)((subnetNum3 + 1)*256);
            subnetNum3++; 
        }
        // lastly, it must fall in quadrant 4 , if code reaches here
        else 
        {
            currentLink.subnetIP = 0x80f00000  + (uint)((subnetNum4 + 1)*256);
            subnetNum4++; 
        }
    }

    
}
