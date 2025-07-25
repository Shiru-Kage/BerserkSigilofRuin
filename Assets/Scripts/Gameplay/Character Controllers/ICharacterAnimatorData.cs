using System;
using UnityEngine;

public interface ICharacterAnimatorData
{
    Vector2 MoveInput { get; }
    Vector2 Velocity { get; }
    bool IsGrounded { get; }
    event Action OnAttack;
}
