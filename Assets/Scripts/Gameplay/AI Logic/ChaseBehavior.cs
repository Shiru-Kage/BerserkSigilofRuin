using UnityEngine;
using System;

public class ChaseBehavior
{
    private EnvironmentDetector detector;
    private CharacterAttack characterAttack;
    private float stoppingDistance;

    public event Action OnAttack;

    public ChaseBehavior(EnvironmentDetector d, CharacterAttack attack, float stopDist = 0.5f)
    {
        detector = d;
        characterAttack = attack;
        stoppingDistance = stopDist;
    }

    public Vector2 UpdateBehavior(Vector2 targetPos, Vector2 selfPos, bool isGrounded, bool canJump, ref bool hasJumped, Action jumpAction)
    {
        Vector2 toTarget = targetPos - selfPos;
        float distance = toTarget.magnitude;

        if (distance <= stoppingDistance)
        {
            if (characterAttack != null && Time.time - characterAttack.LastAttackTime >= characterAttack.AttackCooldown)
                OnAttack?.Invoke();
            return Vector2.zero;
        }

        Vector2 direction = toTarget.normalized;

        bool shouldJump = detector.ShouldJump(direction, selfPos + Vector2.down * 0.5f);
        if (isGrounded && shouldJump && !hasJumped && canJump)
        {
            jumpAction.Invoke();
            hasJumped = true;
            return Vector2.zero;
        }

        if (isGrounded && !shouldJump)
            hasJumped = false;

        return direction;
    }
}
