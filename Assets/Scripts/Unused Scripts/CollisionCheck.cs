using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCheck : MonoBehaviour
{
    public string entity;

    void Start() 
    {

    }
    
    void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("Name of object : " + collider.gameObject.name);
        Collider2D[] otherColliders = new Collider2D[10];
        ContactFilter2D contactFilter = new ContactFilter2D().NoFilter();
        int colliderNum = collider.OverlapCollider(contactFilter,otherColliders);
        Debug.Log("Number of Colliders overlapping : " + colliderNum );
    }
}
