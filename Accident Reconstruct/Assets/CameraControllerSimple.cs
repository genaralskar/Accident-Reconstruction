using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Numerics;
using Cinemachine.Utility;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class CameraControllerSimple : MonoBehaviour
{
    public Transform cameraFollowPoint;
    
    public float moveSpeed = 1;
    public float zoomSpeed = 10;
    public float rotateSpeed = 5;

    private Vector3 dragOrigin;
    
    // Update is called once per frame
    void Update()
    {
        RotateCamera();
        
        float vert = Input.GetAxis("Vertical");
        float horiz = Input.GetAxis("Horizontal");
        float scroll = -Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        Vector3 moveDir = new Vector3(horiz, scroll, vert) * moveSpeed;
        cameraFollowPoint.transform.Translate(moveDir);
    }

    private void RotateCamera()
    {
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = Input.mousePosition;
            return;
        }
 
        if (!Input.GetMouseButton(2)) return;
 
        Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);

        Vector3 currentRot = transform.rotation.eulerAngles;
        
        Vector3 newRot = new Vector3(0, pos.x * rotateSpeed, 0);
        
        cameraFollowPoint.transform.Rotate(newRot);

        newRot.x = -pos.y * rotateSpeed;

        newRot += currentRot;
        //newRot = transform.InverseTransformDirection(newRot);
        Quaternion newQuat = Quaternion.Euler(newRot);
        transform.rotation = newQuat;
    }
}
