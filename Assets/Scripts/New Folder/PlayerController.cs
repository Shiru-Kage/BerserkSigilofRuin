using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;

    private void Awake()
    {
        inputActions = InputManager.GetInputActions();
    }

    private void OnEnable()
    {
        InputManager.Instance.Events.OnMove.AddListener(HandleMove);
    }

    private void OnDisable()
    {
        InputManager.Instance.Events.OnMove.RemoveListener(HandleMove);
    }

    private void HandleMove(Vector2 input)
    {
        moveInput = input;
    }

    private void Update()
    {

        transform.Translate(moveInput * moveSpeed * Time.deltaTime);
    }
}
