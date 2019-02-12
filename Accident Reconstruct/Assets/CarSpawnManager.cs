﻿using BansheeGz.BGSpline.Components;
using BansheeGz.BGSpline.Curve;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CarSpawnManager : MonoBehaviour
{
    [Header("Car Stuff")]
    public GameObject carPrefab;
    public GameObject carWaypointPrefab;
    
    [Header("Curve Stuff")]
    public BGCurvePoint.ControlTypeEnum curveType;
    public Material lineRendererMaterial;

    [Header("Translate Gizmo Stuff")]
    public GameObject translateArrowPrefab;
    public Camera gizmoCamera;
    public LayerMask gizmoLayer;
    
    private GameObject car;
    private GameObject CurveObj;
    private CarController carController;
    private MoveCarWaypoint carWayPoint;
    private bool carSpawned = false;
    private BGCurve curve;
    private LineRenderer lineRender;
    private List<GameObject> objsToDisable;
    [HideInInspector]
    public CreateNewCurve curveSpawner;
    
    public bool active = false;

    public UnityAction<bool> ChangeActivation;
    public static UnityAction CurveEnd;

    [Header("Stuff controlled by UI")]
    public bool keepLine = true;
    public bool closedCurve = true;

    private void Awake()
    {
        objsToDisable = new List<GameObject>();
        CreateNewCurve.StartCar += SetCarBehavior;
    }

    private void OnDestroy()
    {
        CreateNewCurve.StartCar -= SetCarBehavior;
    }

    private void Update()
    {
        if (!active) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0) && Input.GetButton("PlaceCurveModifier"))
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && !carSpawned)
            {
                Spawn(hit.point);
            }
            else if (Physics.Raycast(ray, out hit))
            {
                Vector3 controlPoint1 = Vector3.right * 3;
                curve.AddPoint(new BGCurvePoint(curve, hit.point, curveType, controlPoint1, -controlPoint1));
                SetupTranslateArrow(CurveObj);
                SetupControlTranslateArrow(CurveObj.transform.GetChild(CurveObj.transform.childCount - 1).gameObject);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            DeactivateCurve();
        }
    }

    public void ActivateCurve()
    {
        CurveEnd?.Invoke();
        CurveEnd = null;
        
        active = true;
        if (lineRender != null)
        {
            lineRender.enabled = true;
        }
        SetActiveGOs(true);
        curveSpawner.currentManager = this;
        curveSpawner.SetCurrentCurveUIActive(true);
        
        ChangeActivation?.Invoke(active);
        CurveEnd += DeactivateCurve;
    }

    public void DeactivateCurve()
    {
        active = false;
        if (lineRender != null && !keepLine)
        {
            lineRender.enabled = false;
        }
        SetActiveGOs(false);
        
        ChangeActivation?.Invoke(active);
        curveSpawner.SetCurrentCurveUIActive(false);
    }

    private void SetActiveGOs(bool state)
    {
        foreach (var obj in objsToDisable)
        {
            obj.SetActive(state);
        }
    }

    public void Spawn(Vector3 position)
    {
        carSpawned = true;
        //spawn car
        Vector3 spawnPos = position + Vector3.up;
        car = Instantiate(carPrefab, spawnPos, Quaternion.identity);
        car.GetComponent<ActivateCurve>().manager = this;
        carController = car.GetComponent<CarController>();
        
        //spawn waypoint prefab
        GameObject waypoint = Instantiate(carWaypointPrefab);
        carController.carWaypoint = waypoint.transform.GetChild(0);
        //set waypoint to only react to this car
        carWayPoint = waypoint.GetComponent<MoveCarWaypoint>();
        carWayPoint.car = this.car;
        carWayPoint.loop = closedCurve;
        
        //create curve
        CurveObj = new GameObject();
        curve = CurveObj.AddComponent<BGCurve>();
        curve.PointsMode = BGCurve.PointsModeEnum.GameObjectsTransform;
        curve.Closed = closedCurve;

        Vector3 controlPoint1 = Vector3.right * 3;
        //figure how to make these relative to the tangent(?) of the current point
        curve.AddPoint(new BGCurvePoint(curve, position, curveType, controlPoint1, -controlPoint1));

        var lineRend = CurveObj.AddComponent<BGCcVisualizationLineRenderer>();
        CurveObj.GetComponent<LineRenderer>().materials[0] = lineRendererMaterial;

        lineRender = CurveObj.GetComponent<LineRenderer>();

        var cursor = CurveObj.AddComponent<BGCcCursor>();
        waypoint.GetComponent<MoveCarWaypoint>().cursor = cursor;

        var translate = CurveObj.AddComponent<BGCcCursorObjectTranslate>();
        translate.ObjectToManipulate = waypoint.transform;

        var rotate = CurveObj.AddComponent<BGCcCursorObjectRotate>();
        rotate.ObjectToManipulate = waypoint.transform;

        var math = CurveObj.GetComponent<BGCcMath>();
        math.Fields = BGCurveBaseMath.Fields.PositionAndTangent;

        SetupTranslateArrow(CurveObj);
        SetupControlTranslateArrow(CurveObj.transform.GetChild(CurveObj.transform.childCount - 1).gameObject);
        
    }

    private void SetupTranslateArrow(GameObject newCurve)
    {
        GameObject newPoint = newCurve.transform.GetChild(newCurve.transform.childCount - 1).gameObject;
        
        GameObject arrow = Instantiate(translateArrowPrefab);
        objsToDisable.Add(arrow);
        GizmoTranslateScript arrowTranslate = arrow.GetComponent<GizmoTranslateScript>();
        arrowTranslate.translateTarget = newPoint;
        arrowTranslate.transform.position = newPoint.transform.position;

        GizmoClickDetection[] detectors = arrow.GetComponentsInChildren<GizmoClickDetection>();
        foreach (var detector in detectors)
        {
            detector.gizmoCamera = gizmoCamera;
        }
        
        arrow.transform.SetParent(newCurve.transform.GetChild(newCurve.transform.childCount - 1));
    }

    public void SetCarBehavior(ControlInputs newBehavior)
    {
        carController.input = newBehavior;
    }

    private void SetupControlTranslateArrow(GameObject controlPoint)
    {
        //get the current curvePoint
        BGCurvePointGO curvePoint = controlPoint.GetComponent<BGCurvePointGO>();
        
        //spawn empty gameobject for arrow to move
        GameObject controlObj = new GameObject();
        //sets gos position 
        controlObj.transform.position = curvePoint.ControlFirstWorld;
        controlObj.transform.SetParent(controlPoint.transform);
        //add setcurvecontrol... script to new gameobject
        SetCurveControlToGizmoPosition curveControl = controlObj.AddComponent<SetCurveControlToGizmoPosition>();
        //set new setcuvecontrol... curve point to proper curve point so it can move it
//        print(curvePoint);
        curveControl.curvePoint = curvePoint;
//        print(curveControl.curvePoint);
        //set the control vector to the new game object's position(probs in curvecontroltogizmo script)
        
        //spawn arrow gizmo
        GameObject newArrow = Instantiate(translateArrowPrefab);
        objsToDisable.Add(newArrow);
        //parent arrow to controlPoint
        newArrow.transform.SetParent(controlPoint.transform);
        //get the gizmotranslate componenet from the arrow
        GizmoTranslateScript gizmoTranslate = newArrow.GetComponent<GizmoTranslateScript>();
        //set arrows position to curvePoint location
        newArrow.transform.position = curvePoint.ControlFirstWorld;
        //set arrows translateTarget to the new gameobject
        gizmoTranslate.translateTarget = controlObj;

        //set the gizmo cameras on the parts for the arrow
        GizmoClickDetection[] detectors = newArrow.GetComponentsInChildren<GizmoClickDetection>();
        foreach (var detector in detectors)
        {
            detector.gizmoCamera = gizmoCamera;
        }

        curveControl.gizmoTranslate = gizmoTranslate;
    }

    public void SetCurveClosed(bool closed)
    {
        curve.Closed = closed;
        closedCurve = closed;
        carWayPoint.loop = closed;
    }
}