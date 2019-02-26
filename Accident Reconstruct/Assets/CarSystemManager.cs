using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CarSystemManager : MonoBehaviour
{
    public GameObject carManager;
    public Camera gizmoCamera;

    public List<GameObject> managers;

    public CarSpawnManager currentManager;

    public List<GameObject> cars;
    
    public static UnityAction<ControlInputs> StartSim;
    public static UnityAction ResetSim;

    [Header("UI Stuff")]
    public Dropdown carSelectionDropdownUI;
    public GameObject currentUIPanel;
    [Tooltip("Closes the curve")]
    public Toggle closedCurveToggleUI;
    [Tooltip("Keep rendering the line when object is not selected")]
    public Toggle keepLineToggleUI;
    
    public void SpawnCar()
    {
        currentManager?.DeactivateCurve();
        
        //instantiate manager
        GameObject newCurve = Instantiate(carManager);
        //get spawn manager componenet
        CarSpawnManager curveManager = newCurve.GetComponent<CarSpawnManager>();
        //set car prefab
        curveManager.carPrefab = cars[carSelectionDropdownUI.value];
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
        StartSim?.Invoke(input);
    }

    public void ResetCars()
    {
        ResetSim?.Invoke();
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
        closedCurveToggleUI.isOn = currentManager.closedCurve;
    }

    public void SetCurrentClosed(bool closed)
    {
        if (currentManager == null) return;
        currentManager.SetCurveClosed(closed);
    }
}
