using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Host : MonoBehaviour
{
    // store the host name
    public string hostName;
    // store number of the host
    public int hostNumber;
    // store host IP address
    public uint hostIPAddress;
    // store link interface it is associated with
    public string hostIPAddressString;
    public string linkInterface;
    // Start is called before the first frame update
    public string connectedRouter;
    private Collider2D temp;
    private Link link;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D (Collider2D other)
    {
        temp = other;
        Collider2D [] myCollider = new Collider2D [1];
        ContactFilter2D contactFilter = new ContactFilter2D().NoFilter();
        int colliderNum = other.OverlapCollider(contactFilter,myCollider);
        
        // host knows which link game object it is connected to
        linkInterface = other.transform.parent.name;
        Invoke("AddConnectedRouter",0.5f);
        Invoke("AssignIPAddress",0.5f);
    }

    void AddConnectedRouter()
    {
        link = temp.transform.parent.GetComponent<Link>();
        // the router adds the other router if it is a router into its neighbouring routers
        Debug.Log("The entities are " + link.entity1 + "and " + link.entity2);
        if(hostName != link.entity1 && link.entity1.Contains("Router")) 
        {
            connectedRouter = link.entity1;
            Debug.Log("Connected Router is : " + connectedRouter);
        }
        else if (hostName != link.entity2 && link.entity2.Contains("Router"))
        {
            connectedRouter = link.entity2;
            Debug.Log("Connected Router is : " + connectedRouter);
        }
    }
    //Assign host IP address, takes the network portion from the link it is connected to,
    // depending on whether it is stored as entity1 or entity2, it will take the host ID 1 or 2 and add
    // it to the subnet IP address , creating an IP address for the host.
    void AssignIPAddress ()
    {
        link = temp.transform.parent.GetComponent<Link>();
        if(hostName == link.entity1)
        {
            hostIPAddress= link.subnetIP + (uint)1;
        }
        else
        {
            hostIPAddress = link.subnetIP + (uint)2;
        }
        ConvertToIPString(hostIPAddress);
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
            hostIPAddressString += "." + octet;
        }
        hostIPAddressString = hostIPAddressString.Substring(1);
    }
}
