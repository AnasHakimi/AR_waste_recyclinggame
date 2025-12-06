using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    [Header("Auto-animated waste objects")]
    public List<GameObject> animatedObjects = new List<GameObject>();

    [Header("Spin Settings")]
    public Vector3 rotationSpeed = new Vector3(0, 40f, 0); // Slower spin

    public void RegisterObject(GameObject obj)
    {
        if (obj == null || animatedObjects.Contains(obj)) return;

        animatedObjects.Add(obj);
    }

    void Update()
    {
        foreach (GameObject obj in animatedObjects)
        {
            if (obj == null) continue;

            // Spin only
            obj.transform.Rotate(rotationSpeed * Time.deltaTime);
        }
    }
}
