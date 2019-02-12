using System.Collections;
using System.Collections.Generic;
using BansheeGz.BGSpline.Curve;
using UnityEngine;


//this goes on the empty gameobject that gets moved by the translate arrow then sets the position of the curve point
//to this position
public class SetCurveControlToGizmoPosition : MonoBehaviour
{
    public BGCurvePointGO curvePoint;
    public GizmoTranslateScript gizmoTranslate;

    private void Start()
    {
        //curvePoint = GetComponent<BGCurvePointGO>();
        if (gizmoTranslate != null)
        {
            gizmoTranslate.UpdateTranslate += UpdateTranslateHandler;
        }
    }

    private void UpdateTranslateHandler()
    {
        curvePoint.ControlFirstWorld = transform.position;
    }
}
