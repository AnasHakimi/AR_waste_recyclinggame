using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class PinchToZoomWithTracking : MonoBehaviour
{
    public float zoomSpeed = 0.01f;
    public float minScale = 0.1f;
    public float maxScale = 3f;

    private bool isTargetTracked = false;
    private ObserverBehaviour observerBehaviour;

    void Start()
    {
        observerBehaviour = GetComponentInParent<ObserverBehaviour>();
        if (observerBehaviour)
        {
            observerBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
        }
    }

    void OnDestroy()
    {
        if (observerBehaviour)
        {
            observerBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        isTargetTracked = targetStatus.Status == Status.TRACKED || targetStatus.Status == Status.EXTENDED_TRACKED;
    }

    void Update()
    {
        if (!isTargetTracked)
            return;

        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

            float prevTouchDeltaMag = (touch0PrevPos - touch1PrevPos).magnitude;
            float currentTouchDeltaMag = (touch0.position - touch1.position).magnitude;

            float deltaMagnitudeDiff = currentTouchDeltaMag - prevTouchDeltaMag;

            float newScale = Mathf.Clamp(transform.localScale.x + deltaMagnitudeDiff * zoomSpeed, minScale, maxScale);
            transform.localScale = new Vector3(newScale, newScale, newScale);
        }
    }
}