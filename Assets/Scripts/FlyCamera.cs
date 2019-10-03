using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FlyCamera : MonoBehaviour
{
	#region UI	

	[SerializeField]
	private bool active = true;

	[Space]

	[SerializeField]
	private bool enableRotation = true;

	[SerializeField]
	private float mouseSense = 1.8f;

	[Space]

	[SerializeField]
	private bool enableMovement = true;

	[SerializeField]
	private float movementSpeed = 10f;

	[SerializeField]
	private float boostedSpeed = 50f;

	[Space]

	[SerializeField]
	private bool enableSpeedAcceleration = true;

	[SerializeField]
	private float speedAccelerationFactor = 1.5f;

	[Space]

	[SerializeField]
	private KeyCode initPositonButton = KeyCode.R;

	#endregion UI

	private CursorLockMode wantedMode;

	private float _currentIncrease = 1;
	private float _currentIncreaseMem = 0;

	private Vector3 _initPosition;
	private Vector3 _initRotation;

#if UNITY_EDITOR
	private void OnValidate()
	{
		if (boostedSpeed < movementSpeed)
			boostedSpeed = movementSpeed;
	}
#endif


	private void Start()
	{
		_initPosition = transform.position;
		_initRotation = transform.eulerAngles;
	}

	private void OnEnable()
	{
		if (active)
			wantedMode = CursorLockMode.Locked;
	}

	// Apply requested cursor state
	private void SetCursorState()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Cursor.lockState = wantedMode = CursorLockMode.None;
		}

		if (Input.GetMouseButtonDown(0))
		{
			wantedMode = CursorLockMode.Locked;
		}

		// Apply cursor state
		Cursor.lockState = wantedMode;
		// Hide cursor when locking
		Cursor.visible = (CursorLockMode.Locked != wantedMode);
	}

	private void CalculateCurrentIncrease(bool moving)
	{
		_currentIncrease = Time.deltaTime;

		if (!enableSpeedAcceleration || enableSpeedAcceleration && !moving)
		{
			_currentIncreaseMem = 0;
			return;
		}

		_currentIncreaseMem += Time.deltaTime * (speedAccelerationFactor - 1);
		_currentIncrease = Time.deltaTime + Mathf.Pow(_currentIncreaseMem, 3) * Time.deltaTime;
	}

	private void Update()
	{
		if (!active)
			return;

		SetCursorState();

		if (Cursor.visible)
			return;

		// Movement
		if (enableMovement)
		{
			Vector3 deltaPosition = Vector3.zero;
			float currentSpeed = movementSpeed;

			if (Input.GetKey(KeyCode.LeftShift))
				currentSpeed = boostedSpeed;

			if (Input.GetKey(KeyCode.W))
				deltaPosition += transform.forward;

			if (Input.GetKey(KeyCode.S))
				deltaPosition -= transform.forward;

			if (Input.GetKey(KeyCode.A))
				deltaPosition -= transform.right;

			if (Input.GetKey(KeyCode.D))
				deltaPosition += transform.right;

			// Calc acceleration
			CalculateCurrentIncrease(deltaPosition != Vector3.zero);

			transform.position += deltaPosition * currentSpeed * _currentIncrease;
		}

		// Rotation
		if (enableRotation)
		{
			// Pitch
			transform.rotation *= Quaternion.AngleAxis(
				-Input.GetAxis("Mouse Y") * mouseSense,
				Vector3.right
			);

			// Paw
			transform.rotation = Quaternion.Euler(
				transform.eulerAngles.x,
				transform.eulerAngles.y + Input.GetAxis("Mouse X") * mouseSense,
				transform.eulerAngles.z
			);
		}

		// Return to init position
		if (Input.GetKeyDown(initPositonButton))
		{
			transform.position = _initPosition;
			transform.eulerAngles = _initRotation;
		}
	}
}
