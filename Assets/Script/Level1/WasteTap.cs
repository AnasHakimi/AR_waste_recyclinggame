using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WasteTap : MonoBehaviour
{
    private void OnMouseDown()
    {
        // Tell the robot to come collect this waste
        RobotCollector robot = FindObjectOfType<RobotCollector>();
        if (robot != null)
        {
            robot.MoveToWaste(gameObject);
        }
    }
}


