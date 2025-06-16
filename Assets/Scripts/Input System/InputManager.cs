using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    [SerializeField] private InputEvents events;
    public InputEvents Events => events;
    private static InputSystem_Actions inputActions;
    [System.Serializable]
	public struct InputEvents {
		public UnityEvent<Vector2> OnTap;
        public UnityEvent<Vector2> OnMove;
	}
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
            inputActions.Enable();
        }
    }
    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    public static InputSystem_Actions GetInputActions()
    {
        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
            inputActions.Enable();
        }
        return inputActions;
    }
    private void Start() {
		inputActions.UI.LocationTap.started += _ => OnClick();

        inputActions.Player.Move.performed += ctx => events.OnMove.Invoke(ctx.ReadValue<Vector2>());
        inputActions.Player.Move.canceled += ctx => events.OnMove.Invoke(Vector2.zero);
	}

	private void OnClick() {
		Vector2 screenPosition = inputActions.UI.LocationTap.ReadValue<Vector2>();
		events.OnTap.Invoke(screenPosition);
	}
}
