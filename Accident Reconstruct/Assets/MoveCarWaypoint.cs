using System.Collections;
using System.Collections.Generic;
using BansheeGz.BGSpline.Components;
using UnityEngine;

public class MoveCarWaypoint : MonoBehaviour
{
    public BGCcCursor cursor;
    public float moveAmount = 0;
    public bool loop;
    public GameObject car;

    public ControlInputs stopInput;

    private bool atEnd = false;

    private void Update()
    {
        if (cursor != null)
        {
            if (cursor.DistanceRatio >= 1)
            {
                if (loop)
                {
                    cursor.DistanceRatio = 0;
                }
                else
                {
                    atEnd = true;
                    if(stopInput != null)
                        car.GetComponent<CarController>().input = stopInput;
                    
                    GetComponent<BoxCollider>().enabled = false;
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject != car) return;
        
        if (cursor != null)
        {
            cursor.Distance += moveAmount * Time.deltaTime;
        }
    }
}
