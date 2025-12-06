using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotCollector : MonoBehaviour
{
    public float moveSpeed = 2f;
    private GameObject targetWaste;
    private bool isMoving = false;
    private Animator animator; // Reference to the Animator

    void Start()
    {
        animator = GetComponent<Animator>(); // Get the Animator component
    }

    public void MoveToWaste(GameObject waste)
    {
        if (isMoving) return;
        targetWaste = waste;
        StartCoroutine(MoveAndCollect());
    }

    IEnumerator MoveAndCollect()
    {
        isMoving = true;
        animator.SetBool("IsWalking", true); // Start walking animation

        // Rotation logic (same as before)
        Vector3 direction = (targetWaste.transform.position - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation *= Quaternion.Euler(0, -90, 0); // adjust based on rig orientation
            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.5f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 4f * Time.deltaTime);
                yield return null;
            }
        }

        // Movement logic (same as before)
        while (Vector3.Distance(transform.position, targetWaste.transform.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetWaste.transform.position,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        animator.SetBool("IsWalking", false); // Stop walking animation
        Destroy(targetWaste);
        FindObjectOfType<Level1Manager>().OnWasteCollected();
        isMoving = false;
    }

    public void DisableControl()
    {
        StopAllCoroutines();
        animator.SetBool("IsWalking", false); // Ensure animation stops
        isMoving = false;
        enabled = false;
    }
}
