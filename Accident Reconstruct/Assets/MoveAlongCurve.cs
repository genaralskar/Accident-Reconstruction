using System.Collections;
using System.Collections.Generic;
using BansheeGz.BGSpline.Components;
using UnityEngine;
using UnityEngine.UI;

public class MoveAlongCurve : MonoBehaviour
{
    public BGCcCursor curvePosition;
    public float currentPosition;

    public Slider positionSlider;

    private void OnMouseDown()
    {
        //set position slider ui to this
    }

    //set the position of the object to a new value
    public void UpdatePos(float newPos)
    {
        curvePosition.DistanceRatio = newPos;
        currentPosition = curvePosition.DistanceRatio;
    }
}
