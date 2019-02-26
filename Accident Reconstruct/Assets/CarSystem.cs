using BansheeGz.BGSpline.Components;
using BansheeGz.BGSpline.Curve;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CarSystem : MonoBehaviour
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
    //List of spawned objs along the curve
    public List<MoveAlongCurve> MACs;
    
    [HideInInspector]
    public CarSystemManager curveSpawner;
    
    public bool active = false;

    //called to set if this is the active car system
    public UnityAction<bool> ChangeActivation;
    //called to make this not the current active car system
    public static UnityAction CurveEnd;

    [Header("Stuff controlled by UI")]
    public bool keepLine = true;
    public bool closedCurve = true;

    private void Awake()
    {
        objsToDisable = new List<GameObject>();
        MACs = new List<MoveAlongCurve>();
        CarSystemManager.StartSim += StartSimHandler;
    }

    private void OnDestroy()
    {
        CarSystemManager.StartSim -= StartSimHandler;
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
                //add point to curve
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
        CarSystemManager.currentManager = this;
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
    
    public void StartSimHandler(ControlInputs newBehavior)
    {
        carController.input = newBehavior;
        //set current states
            //position
            //rotation
            //wheel speed
            //velocity
    }

    //instantiate a game object and proper components to have it move along the curve
    public void AddTranslateObjectToCurve(GameObject newObj)
    {
        MoveAlongCurve newMAC = newObj.GetComponent<MoveAlongCurve>();
        if (newMAC == null)
        {
            Debug.LogWarning("Object " + newObj + " does not have a MoveAlongCurve compnenet attached, but is trying to" +
                             " be is needed to work with SpawnObjOnCurve script");
            return;
        }

        //spawn obj to move
        GameObject obj = Instantiate(newObj);
        
        //create new cursor for obj to translate along
        BGCcCursor newCursor = CurveObj.AddComponent<BGCcCursor>();
        
        //add new CursorObjectTranslate to set translated object
        BGCcCursorObjectTranslate newTranslate = CurveObj.AddComponent<BGCcCursorObjectTranslate>();
        //add cursor objec trotate to set rotation
        BGCcCursorObjectRotate newRotate = CurveObj.AddComponent<BGCcCursorObjectRotate>();
        
        //set newTranslate's and newRotate's cursor to the new cursor
        newTranslate.Cursor = newCursor;
        newRotate.Cursor = newCursor;
        
        //set obj to be moved by newTranslate and newRotate
        newTranslate.ObjectToManipulate = obj.transform;
        newRotate.ObjectToManipulate = obj.transform;
        
        //set newMACs cursor to new cursor
        newMAC.curvePosition = newCursor;
        newMAC.UpdatePos(0);
        
        MACs.Add(newMAC);
        
        curveSpawner.UpdateCurveCurveMovers();
        
        print("all the stuff is setup");
    }
    
    
}
