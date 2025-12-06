using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class LevelMarkerHandler : MonoBehaviour
{
    public MonoBehaviour levelManagerScript; // assign Level1Manager, Level2Manager, etc.
    public GameObject levelRoot;
    private ILevelManager levelManager;

    private ObserverBehaviour observerBehaviour;

    void Start()
    {
        observerBehaviour = GetComponent<ObserverBehaviour>();

        if (observerBehaviour)
        {
            observerBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
        }

        levelManager = levelManagerScript as ILevelManager;

        if (levelManager == null)
        {
            Debug.LogError("Assigned script does not implement ILevelManager interface!");
        }
    }

    private void OnDestroy()
    {
        if (observerBehaviour)
        {
            observerBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        if (targetStatus.Status == Status.TRACKED || targetStatus.Status == Status.EXTENDED_TRACKED)
        {
            Debug.Log("Marker detected!");
            levelManager?.StartLevel();
            SetLevelObjectsActive(true);
        }
        else
        {
            Debug.Log("Marker lost!");
            SetLevelObjectsActive(false);
        }
    }

    void SetLevelObjectsActive(bool isActive)
    {
        if (levelRoot != null)
            levelRoot.SetActive(isActive);
    }

}
