using System.Collections;
using System.Collections.Generic;
using BansheeGz.BGSpline.Components;
using UnityEngine;
using UnityEngine.UI;

public class MoveAlongCurve : MonoBehaviour
{
    public BGCcCursor cursor;
    public float currentPosition;

    public Slider positionSlider;

    private void Start()
    {
        CarSystemManager.StartSim += StartSimHandler;
        CarSystemManager.ResetSim += EndSimHandler;
    }

    private void OnDestroy()
    {
        CarSystemManager.StartSim -= StartSimHandler;
        CarSystemManager.ResetSim -= EndSimHandler;
    }

    private void OnMouseDown()
    {
        //set position slider ui to this
    }

    //set the position of the object to a new value
    public void UpdatePos(float newPos)
    {
        //print("Updating Position " + newPos + ", " + cursor);
        cursor.DistanceRatio = newPos;
        currentPosition = cursor.DistanceRatio;
    }

    public void StartSimHandler(ControlInputs inputs)
    {
        GetComponent<Collider>().enabled = false;
    }

    public void EndSimHandler()
    {
        GetComponent<Collider>().enabled = true;
    }
}
