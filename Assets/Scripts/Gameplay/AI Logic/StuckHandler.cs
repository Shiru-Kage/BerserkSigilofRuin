using UnityEngine;
using System;

public class StuckHandler
{
    private Vector2 lastPosition;
    private float stuckTimer = 0f;
    private EnvironmentDetector detector;

    public StuckHandler(EnvironmentDetector d)
    {
        detector = d;
    }

    public void Update(Vector2 currentPosition, bool isGrounded, Vector2 forwardDirection, Vector2 feetPos, bool canJump, Action jumpAction)
    {
        if (!detector.IsObstacleInFront(forwardDirection, feetPos))
        {
            stuckTimer = 0f;
            lastPosition = currentPosition;
            return;
        }

        float distanceMoved = Vector2.Distance(currentPosition, lastPosition);
        if (distanceMoved < 0.05f && isGrounded)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 0.5f && canJump)
            {
                Debug.Log("Enemy is stuck, trying to jump.");
                jumpAction.Invoke();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = currentPosition;
    }
}
