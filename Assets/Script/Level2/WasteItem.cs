using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class WasteItem : MonoBehaviour
{
    // Public Variables
    public WasteType wasteType;
    public string wasteName;
    public LineRenderer aimLine;

    [Header("Throw Settings")]
    public float throwForce = 12f;
    public float throwArcHeight = 2.5f;
    public float maxDragDistance = 2.5f;
    public int linePoints = 25;
    public float lineDuration = 1.5f;

    [Header("Line Appearance")]
    public float lineStartWidth = 0.06f; // Thinner line
    public float lineEndWidth = 0.01f;   // Tapered tip


    // Private Variables
    private Vector3 startPosition;
    private bool isDragging = false;
    private Camera mainCamera;
    private Level2Manager levelManager;
    private Rigidbody rb;

    public bool canBeThrown = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        if (aimLine != null)
        {
            aimLine.positionCount = linePoints;
            aimLine.enabled = false;
            aimLine.startWidth = lineStartWidth; // Set width here
            aimLine.endWidth = lineEndWidth;


        }
    }

    void Start()
    {
        mainCamera = Camera.main;
        levelManager = FindObjectOfType<Level2Manager>();
        startPosition = transform.position;
    }

    void Update()
    {
        if (isDragging)
        {
            UpdateDragPosition();
            DrawTrajectory();
        }
    }

    void UpdateDragPosition()
    {
        Vector3 inputPos = Input.touchCount > 0 ?
            (Vector3)Input.GetTouch(0).position :
            Input.mousePosition;

        Ray ray = mainCamera.ScreenPointToRay(inputPos);
        Plane plane = new Plane(Vector3.up, startPosition);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 dragOffset = ray.GetPoint(distance) - startPosition;
            dragOffset = Vector3.ClampMagnitude(dragOffset, maxDragDistance);
            transform.position = startPosition + dragOffset;
        }
    }

    void DrawTrajectory()
    {
        if (aimLine == null) return;

        Vector3 throwDirection = (startPosition - transform.position).normalized;
        Vector3 throwVelocity = throwDirection * throwForce + Vector3.up * throwArcHeight;

        for (int i = 0; i < linePoints; i++)
        {
            float simulationTime = i / (float)linePoints * lineDuration;
            Vector3 displacement = throwVelocity * simulationTime +
                                  Physics.gravity * simulationTime * simulationTime * 0.5f;

            if (displacement.y < -0.5f)
            {
                aimLine.positionCount = i;
                break;
            }

            aimLine.SetPosition(i, transform.position + displacement);
        }
    }

    // Input methods (same as before)
    void OnMouseDown()
    {
        if (Input.touchCount <= 1 && canBeThrown)
            StartDrag();
    }

    void OnMouseUp() { if (isDragging) EndDrag(); }
    public void OnTouchBegan(Touch touch) { if (!isDragging) StartDrag(); }
    public void OnTouchEnded(Touch touch) { if (isDragging) EndDrag(); }

    void StartDrag()
    {
        if (!canBeThrown) return;

        isDragging = true;
        rb.isKinematic = true;
        if (aimLine != null) aimLine.enabled = true;
        levelManager?.OnWasteGrabbed(this);
    }

    void EndDrag()
    {
        isDragging = false;
        if (aimLine != null) aimLine.enabled = false;
        ThrowWaste();
    }

    void ThrowWaste()
    {
        rb.isKinematic = false;
        Vector3 throwDirection = (startPosition - transform.position).normalized;
        Vector3 throwVelocity = throwDirection * throwForce + Vector3.up * throwArcHeight;
        rb.velocity = throwVelocity;
        AudioManager.instance?.PlaySFX("Throw");
        StartCoroutine(CheckForBinCollision());
    }

    IEnumerator CheckForBinCollision()
    {
        yield return new WaitForSeconds(0.2f);
        float checkDuration = 3f;
        float elapsed = 0f;

        while (elapsed < checkDuration && rb.velocity.sqrMagnitude > 0.1f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!rb.IsSleeping()) // If still moving but didn't hit anything
        {
            levelManager?.OnWasteSorted(false);
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {

        AudioManager.instance?.PlaySFX("Hitting");

        if (collision.gameObject.TryGetComponent<BinType>(out var bin))
        {
            bool correct = bin.binType == wasteType;
            levelManager?.OnWasteSorted(correct);
            Destroy(gameObject);
        }
    }

    public void Initialize(Vector3 spawnPos)
    {
        startPosition = spawnPos;
        transform.position = spawnPos;

    }
}