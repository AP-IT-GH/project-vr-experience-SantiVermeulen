using UnityEngine;
using UnityEngine.InputSystem;

public class VRCarController : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference accelerateAction; // Right trigger
    public InputActionReference brakeAction;      // Left trigger

    [Header("Car Settings")]
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float brakeForce = 15f;
    [SerializeField] private float turnSpeed = 100f;
    [SerializeField] private float headTiltSensitivity = 0.5f;
    
    [Header("References")]
    [SerializeField] private Transform headset;
    [SerializeField] private Rigidbody carRigidbody;
    [SerializeField] private Transform carModel;
    
    private float currentSpeed;
    private float currentSteering;
    private bool isGrounded;
    
    private void Start()
    {
        // Ensure we have all required components
        if (carRigidbody == null)
            carRigidbody = GetComponent<Rigidbody>();
            
        if (headset == null)
            headset = Camera.main.transform;
            
        // Lock rigidbody rotation to prevent flipping
        carRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | 
                                 RigidbodyConstraints.FreezeRotationZ;

        // Enable input actions
        accelerateAction.action.Enable();
        brakeAction.action.Enable();
    }
    
    private void Update()
    {
        float throttle = accelerateAction.action.ReadValue<float>();
        float brake = brakeAction.action.ReadValue<float>();
        
        // Get head tilt for steering
        float headTilt = headset.localEulerAngles.z;
        if (headTilt > 180f) headTilt -= 360f;
        currentSteering = -headTilt * headTiltSensitivity;
        
        // Apply movement
        MoveCar(throttle, brake);
        SteerCar();
    }
    
    private void MoveCar(float throttle, float brake)
    {
        // Calculate target speed
        float targetSpeed = throttle * maxSpeed;
        
        // Apply acceleration or braking
        if (brake > 0.1f)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, brake * brakeForce * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        }
        
        // Apply movement
        Vector3 movement = transform.forward * currentSpeed;
        carRigidbody.linearVelocity = movement;
    }
    
    private void SteerCar()
    {
        // Apply steering rotation
        float turnAmount = currentSteering * turnSpeed * Time.deltaTime;
        transform.Rotate(0f, turnAmount, 0f);
        
        // Tilt car model for visual feedback
        if (carModel != null)
        {
            float targetTilt = -currentSteering * 15f; // Max 15 degrees tilt
            carModel.localRotation = Quaternion.Lerp(
                carModel.localRotation,
                Quaternion.Euler(0f, 0f, targetTilt),
                Time.deltaTime * 5f
            );
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Check if we hit the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    
    private void OnCollisionExit(Collision collision)
    {
        // Check if we left the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
} 