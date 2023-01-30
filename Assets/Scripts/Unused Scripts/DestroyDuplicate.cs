using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyDuplicate : MonoBehaviour
{
    public static GameObject existingObject;
    void Awake()
    {
        if(existingObject)
        {
            Destroy(existingObject);
            existingObject = gameObject;
        }
        else 
        {
            existingObject = gameObject;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
