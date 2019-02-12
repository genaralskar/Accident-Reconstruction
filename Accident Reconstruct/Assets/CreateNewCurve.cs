using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CreateNewCurve : MonoBehaviour
{
    public GameObject carManager;
    public Camera gizmoCamera;

    public List<GameObject> managers;

    public CarSpawnManager currentManager;
    
    public static UnityAction<ControlInputs> StartCar;
    public static UnityAction reset;

    [Header("UI Stuff")]
    public GameObject currentUIPanel;
    [Tooltip("Closes the curve")]
    public Toggle closedUI;
    [Tooltip("Keep rendering the line when object is not selected")]
    public Toggle keepLineUI;
    
    public void SpawnCar()
    {
        currentManager?.DeactivateCurve();
        
        //instantiate manager
        GameObject newCurve = Instantiate(carManager);
        //get spawn manager componenet
        CarSpawnManager curveManager = newCurve.GetComponent<CarSpawnManager>();
        //set manager to this
        curveManager.curveSpawner = this;
        //set gizmo camera
        curveManager.gizmoCamera = gizmoCamera;
        //enable manager
        curveManager.ActivateCurve();
        //add to list of current managers
        managers.Add(newCurve);
    }

    public void StartCars(ControlInputs input)
    {
        StartCar?.Invoke(input);
    }

    public void SetCurrentCurveUIActive(bool active)
    {
        currentUIPanel.SetActive(active);
        if (active)
        {
            UpdateCurrentCurveUI();
        }
    }

    public void UpdateCurrentCurveUI()
    {
        print(currentManager);
        closedUI.isOn = currentManager.closedCurve;
    }

    public void SetCurrentClosed(bool closed)
    {
        if (currentManager == null) return;
        currentManager.SetCurveClosed(closed);
    }
}
