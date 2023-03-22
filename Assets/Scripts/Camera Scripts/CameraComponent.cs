using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraComponent : MonoBehaviour
{
    public Camera mainCamera;
    public Transform cameraTransform;
    public float movementSpeed;
    public float movementTime;
    public Vector3 newPosition;
    public float minX,maxX,minY,maxY;

    public float zoomIncrement;
    public float zoomAmount;
    public float maxZoom, minZoom;
    // Start is called before the first frame update
    void Start()
    {
        newPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        GetMovementInput();
    }

    void GetMovementInput()
    {
        // For up down left right movement of camera
        // World is bounded to max and min X and Y coordinates. 
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            newPosition += (transform.up * movementSpeed);
            newPosition.y = Mathf.Clamp(newPosition.y,minY,maxY);
        }
        if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            newPosition += (transform.up * -movementSpeed);
            newPosition.y = Mathf.Clamp(newPosition.y,minY,maxY);
        }
        if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            newPosition += (transform.right * movementSpeed);
            newPosition.x = Mathf.Clamp(newPosition.x,minX,maxX);
        }
        if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            newPosition += (transform.right * -movementSpeed);
            newPosition.x = Mathf.Clamp(newPosition.x,minX,maxX);
        }

        // For zooming in, with zoom limits
        if(Input.mouseScrollDelta.y > 0)
        {
            zoomAmount = mainCamera.orthographicSize - zoomIncrement;
            mainCamera.orthographicSize = Mathf.Clamp(zoomAmount,minZoom,maxZoom);
    
        }
        //For zooming out, with zoom limits
        else if (Input.mouseScrollDelta.y < 0)
        {
            zoomAmount = mainCamera.orthographicSize + zoomIncrement;
            mainCamera.orthographicSize = Mathf.Clamp(zoomAmount,minZoom,maxZoom);
        }
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);

    }
}
