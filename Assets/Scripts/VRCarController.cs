using UnityEngine;
using UnityEngine.InputSystem; // NEW: Import the new Input System namespace

public class CarController : MonoBehaviour
{
    private float horizontalInput, verticalInput;
    private float currentSteerAngle, currentbreakForce;
    // 'isBreaking' is no longer needed as we can read brake force directly

    // --- SETTINGS ---
    [Header("Car Settings")]
    [SerializeField] private float motorForce = 500f;
    [SerializeField] private float breakForce = 800f;
    [SerializeField] private float maxSteerAngle = 30f;

    // --- WHEEL COLLIDERS ---
    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    // --- WHEEL TRANSFORMS (for visual updates) ---
    [Header("Wheel Visuals")]
    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    // --- NEW: INPUT ACTIONS ---
    [Header("Input Actions")]
    [SerializeField] private InputActionReference steerAction;
    [SerializeField] private InputActionReference accelerateAction; // For gas (right trigger)
    [SerializeField] private InputActionReference brakeAction;      // For brake/reverse (left trigger)


    private void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    private void GetInput()
    {
        // This method now reads from the new Input System Actions

        // Steering: Read the X-axis from a Vector2 action (like a thumbstick)
        horizontalInput = steerAction.action.ReadValue<Vector2>().x;

        // Acceleration & Reverse: 
        // Read the float value from the gas trigger (0 to 1)
        float gasValue = accelerateAction.action.ReadValue<float>();
        // Read the float value from the brake/reverse trigger (0 to 1)
        float brakeReverseValue = brakeAction.action.ReadValue<float>();

        // Combine them: Right trigger adds torque, Left trigger subtracts it (for reverse)
        verticalInput = gasValue - brakeReverseValue;

        // Braking: Apply brake force proportional to how much the left trigger is pressed
        currentbreakForce = brakeReverseValue * breakForce;
    }

    private void HandleMotor()
    {
        // Apply torque for acceleration/reversing. Front-wheel drive.
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;

        // Apply the calculated brake force
        ApplyBreaking();
    }

    private void ApplyBreaking()
    {
        // Apply brake force to all four wheels
        frontRightWheelCollider.brakeTorque = currentbreakForce;
        frontLeftWheelCollider.brakeTorque = currentbreakForce;
        rearLeftWheelCollider.brakeTorque = currentbreakForce;
        rearRightWheelCollider.brakeTorque = currentbreakForce;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    // --- This part remains unchanged, it's perfect ---
    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }
}