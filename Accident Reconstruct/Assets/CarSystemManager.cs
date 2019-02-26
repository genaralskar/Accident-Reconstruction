using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CarSystemManager : MonoBehaviour
{
    public GameObject carManager;
    public Camera gizmoCamera;

    
    //list of all car systems in the game
    public List<GameObject> managers;

    //currently selected car system
    public static CarSystem currentManager;

    //list of cars able to be spawned
    public List<GameObject> cars;
    
    //set all cars in scene with specific behavior
    public static UnityAction<ControlInputs> StartSim;
    //called to reset all dynamic objects in scene
    public static UnityAction ResetSim;

    [Header("UI Stuff")]
    public Dropdown carSelectionDropdownUI;
    public GameObject currentUIPanel;
    public TMP_Dropdown currentCurveMoversUI;
    public Slider currentMoverPositionUI;
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
        CarSystem curveManager = newCurve.GetComponent<CarSystem>();
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
            UpdateCurveCurveMovers();
        }
    }

    public void UpdateCurveCurveMovers()
    {
        List<TMP_Dropdown.OptionData> newOptions = new List<TMP_Dropdown.OptionData>();
        var i = 0;
        foreach (var mac in currentManager.MACs)
        {
            newOptions.Add(new TMP_Dropdown.OptionData {text = i.ToString()});
            i++;
        }
        currentCurveMoversUI.ClearOptions();
        currentCurveMoversUI.AddOptions(newOptions);
        
        UpdateCurrentMoverPositionUI(currentCurveMoversUI.value);
    }

    public void UpdateCurrentMoverPositionUI(int moverSelection)
    {
        print("mover selection: " + moverSelection);
        if (currentManager.MACs.Count == 0) return;
        currentMoverPositionUI.onValueChanged.RemoveAllListeners();
        
        if (currentManager.MACs[moverSelection] == null) return;
        currentMoverPositionUI.value = currentManager.MACs[moverSelection].currentPosition;
        print("mover position: " + currentManager.MACs[moverSelection].currentPosition);
        
        currentMoverPositionUI.onValueChanged?.AddListener(currentManager.MACs[moverSelection].UpdatePos);
        print(currentManager.MACs);
    }

//    public void UpdateCurveMoverPoition(float newPos)
//    {
//        currentManager.MACs[currentCurveMoversUI.value].UpdatePos(newPos);
//    }

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
