using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    // Deze variabele bepaalt of de auto kan bewegen.
    public bool canControl = false;

    // --- Instellingen ---
    [Header("Car Settings")]
    [SerializeField] private float motorForce = 500f;
    [SerializeField] private float breakForce = 800f;
    [SerializeField] private float maxSteerAngle = 30f;

    // --- Wiel Colliders ---
    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    // --- Visuele Wielen ---
    [Header("Wheel Visuals")]
    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    // --- Input Acties ---
    [Header("Input Actions")]
    [SerializeField] private InputActionReference steerAction;
    [SerializeField] private InputActionReference accelerateAction;
    [SerializeField] private InputActionReference brakeAction;

    // --- Private Variabelen ---
    private float horizontalInput, verticalInput;
    private float currentSteerAngle, currentbreakForce;

    private void FixedUpdate()
    {
        // Als we geen controle hebben, doe dan niets.
        if (!canControl)
        {
            // Zorg ervoor dat de auto stopt als de controle wegvalt.
            frontLeftWheelCollider.motorTorque = 0;
            frontRightWheelCollider.motorTorque = 0;
            return;
        }

        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    private void GetInput()
    {
        horizontalInput = steerAction.action.ReadValue<Vector2>().x;
        float gasValue = accelerateAction.action.ReadValue<float>();
        float brakeReverseValue = brakeAction.action.ReadValue<float>();
        verticalInput = gasValue - brakeReverseValue;
        currentbreakForce = brakeReverseValue * breakForce;
    }

    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;
        ApplyBreaking();
    }

    private void ApplyBreaking()
    {
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