using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InfoButtonMultiple : MonoBehaviour
{
    [System.Serializable]
    public class InfoSet
    {
        public GameObject infoPanel;       // Panel GameObject
        public GameObject arrowPanel;      // Arrow pointing to the model
        public Transform model;            // 3D model to point to

        [HideInInspector] public bool isVisible = false;
    }

    [Header("List of info sets")]
    public List<InfoSet> infoSets = new List<InfoSet>();

    [Header("Camera Settings")]
    public Camera mainCamera;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        foreach (var set in infoSets)
        {
            if (set.infoPanel != null) set.infoPanel.SetActive(false);
            if (set.arrowPanel != null) set.arrowPanel.SetActive(false);
            set.isVisible = false;
        }
    }

    void Update()
    {
        if (mainCamera == null) return;

        foreach (var set in infoSets)
        {
            if (!set.isVisible || set.infoPanel == null || set.model == null)
                continue;

            // Make panel face camera (keep upright)
            Vector3 lookPos = mainCamera.transform.position;
            lookPos.y = set.infoPanel.transform.position.y;
            set.infoPanel.transform.LookAt(lookPos);
            set.infoPanel.transform.Rotate(0, 180, 0);
        }
    }

    // Toggle all info sets ON or OFF
    public void ToggleAllInfo()
    {
        bool newState = infoSets.Count > 0 && !infoSets[0].isVisible;

        foreach (var set in infoSets)
        {
            ToggleInfoSet(set, newState);
        }
    }

    // Toggle a specific info set by index
    public void ToggleInfoSet(int index)
    {
        if (index < 0 || index >= infoSets.Count) return;

        var set = infoSets[index];
        ToggleInfoSet(set, !set.isVisible);
    }

    // Internal toggle handler
    private void ToggleInfoSet(InfoSet set, bool state)
    {
        set.isVisible = state;

        if (set.infoPanel != null)
            set.infoPanel.SetActive(state);

        if (set.arrowPanel != null)
            set.arrowPanel.SetActive(state);
    }
}
