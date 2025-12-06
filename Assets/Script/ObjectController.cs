using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ObjectController : MonoBehaviour
{
    public float zoomSpeed = 0.001f;
    public float rotateSpeed = 0.2f;
    public float minScale = 0.1f;
    public float maxScale = 3f;

    public float minXRotation = -45f;
    public float maxXRotation = 45f;

    private bool isTargetTracked = false;
    private ObserverBehaviour observerBehaviour;

    private Vector2 initialTouchPos;
    private float initialXRotation;
    private float initialYRotation;

    private Vector3 initialPosition;
    private float currentXRotation = 0f;

    private float lastTapTime = 0f;
    private float doubleTapThreshold = 0.3f;

    void Start()
    {
        observerBehaviour = GetComponentInParent<ObserverBehaviour>();
        if (observerBehaviour)
        {
            observerBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
        }

        initialPosition = transform.localPosition;
        currentXRotation = NormalizeAngle(transform.localEulerAngles.x);
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

        transform.localPosition = initialPosition;

        // === Double-Tap Reset ===
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (Time.time - lastTapTime < doubleTapThreshold)
                {
                    ResetObjectView();
                }
                lastTapTime = Time.time;
            }
        }

        // === Pinch to Zoom ===
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

        // === One Finger Rotation (Y additive, X follows finger) ===
        else if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                initialTouchPos = touch.position;
                initialXRotation = currentXRotation;
                initialYRotation = transform.localEulerAngles.y;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.position - initialTouchPos;

                // X: follows finger (absolute)
                float deltaX = delta.y * rotateSpeed;
                currentXRotation = NormalizeAngle(initialXRotation + deltaX);
                currentXRotation = Mathf.Clamp(currentXRotation, minXRotation, maxXRotation);

                // Y: accumulates
                float deltaY = -delta.x * rotateSpeed;
                float newY = initialYRotation + deltaY;

                transform.localEulerAngles = new Vector3(currentXRotation, newY, transform.localEulerAngles.z);
            }
        }
    }

    void ResetObjectView()
    {
        transform.localScale = Vector3.one;
        transform.localEulerAngles = Vector3.zero;
        currentXRotation = 0f;
    }

    float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
