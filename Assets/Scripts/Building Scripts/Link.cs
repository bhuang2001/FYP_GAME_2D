using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

public class Link : MonoBehaviour
{
    // store the name of the two entities it connects

    public string entity1;
    public string entity2;
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
    
    // Start is called before the first frame update
    void Start()
    {
        lr = transform.GetComponent<LineRenderer>();
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
        Collider2D [] myCollider = new Collider2D [1];
        ContactFilter2D contactFilter = new ContactFilter2D().NoFilter();
        int colliderNum = other.OverlapCollider(contactFilter,myCollider);
        
        //This is the beginning point of the line, store it in entity1
        if(myCollider[0].gameObject.name == "Collider1")
        {
            // a line could trigger a collision with another line and place the name of the collider instead
            if (other.gameObject.CompareTag("Router") || other.gameObject.CompareTag("Host"))
            {
                entity1 = other.gameObject.name;
            }
        }
        else if (myCollider[0].gameObject.name == "Collider2")
        {
            if (other.gameObject.CompareTag("Router") || other.gameObject.CompareTag("Host"))
            {
                entity2 = other.gameObject.name;
            
            }
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
        startCollider.transform.position = startPos;
        startCollider.transform.SetParent(transform);
        startCollider.AddComponent<BoxCollider2D>();
        boxCollider = startCollider.GetComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(2*width,2*width);
        boxCollider.isTrigger = true;

        // create the collider on other end of the line (end position) 
        endCollider = new GameObject("Collider2");
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
}
