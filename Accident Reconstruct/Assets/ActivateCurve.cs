using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateCurve : MonoBehaviour
{
    [HideInInspector] public CarSpawnManager manager;
    private void OnMouseDown()
    {
        if (!manager.active)
        {
            manager.ActivateCurve();
        }
    }
}
