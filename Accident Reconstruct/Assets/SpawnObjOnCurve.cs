using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpawnObjOnCurve : MonoBehaviour
{
    public ControlInputs newBehavior;
    public TMP_Dropdown dropdown;
    public List<GameObject> objsToSpawn;

    public void SpawnChanger()
    {
        if (CarSystemManager.currentManager == null) return;
        
        print("Starting Spawn");
        GameObject newObj = objsToSpawn[dropdown.value];
        CarSystemManager.currentManager.AddTranslateObjectToCurve(newObj);
    }
}
