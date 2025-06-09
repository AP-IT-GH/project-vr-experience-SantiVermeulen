using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic; // For List<Transform>

public class RaceCarAgent : Agent
{
    // --- Referenties naar de Car Controller componenten (vergelijkbaar met jouw CarController) ---
    [Header("Car Components")]
    [SerializeField] private float motorForce = 500f; // Force applied to wheels
    [SerializeField] private float brakeForce = 800f; // Braking force
    [SerializeField] private float maxSteerAngle = 30f; // Max steering angle

    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    [Header("AI & Training Settings")]
    [SerializeField] private WaypointContainer waypointContainer; // Drag your WaypointContainer GameObject here
    [SerializeField] private LayerMask obstacleLayer; // Layer for walls, other cars, etc.
    [SerializeField] private Transform spawnPoint; // Where the car resets
    [SerializeField] private float maxTimePerCheckpoint = 10f; // Max time to reach a checkpoint before negative reward/episode end
    [SerializeField] private float timePenaltyPerSecond = 0.01f; // Small negative reward per second
    [SerializeField] private float collisionPenalty = 2f; // Penalty for hitting obstacles
    [SerializeField] private float offRoadPenalty = 0.5f; // Penalty for going off track (e.g., specific 'off-road' layer)
    [SerializeField] private float raycastLength = 20f; // Length of the observation raycasts

    private Rigidbody rBody;
    private List<Transform> waypoints;
    private int currentWaypointIndex;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float timeSinceLastCheckpoint;

    // --- Control inputs from ML-Agent Actions ---
    private float currentMotorTorque;
    private float currentSteerAngle;
    private float currentBrakeForce;

    // --- Initialization ---
    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
        if (rBody == null)
        {
            Debug.LogError("RaceCarAgent requires a Rigidbody component on the same GameObject.", this);
            enabled = false;
            return;
        }

        if (waypointContainer == null)
        {
            Debug.LogError("WaypointContainer is not assigned to RaceCarAgent.", this);
            enabled = false;
            return;
        }

        waypoints = waypointContainer.waypoints;
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogError("WaypointContainer has no waypoints.", this);
            enabled = false;
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("Spawn Point not assigned. Using current position as spawn.", this);
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }
        else
        {
            initialPosition = spawnPoint.position;
            initialRotation = spawnPoint.rotation;
        }
    }

    // --- Episode Reset ---
    public override void OnEpisodeBegin()
    {
        // Reset car's physics and position
        rBody.linearVelocity = Vector3.zero;
        rBody.angularVelocity = Vector3.zero;
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // Reset waypoints
        currentWaypointIndex = 0;
        timeSinceLastCheckpoint = 0f;

        // Reset car control inputs
        currentMotorTorque = 0;
        currentSteerAngle = 0;
        currentBrakeForce = 0;

        // Reset wheel physics and visuals
        ApplyCarControls(); // Apply zero forces/angles
        UpdateWheels();
    }

    // --- Collect Observations (What the AI 'sees') ---
    public override void CollectObservations(VectorSensor sensor)
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            // If no waypoints, provide dummy observations or end episode
            sensor.AddObservation(new float[15]); // Add dummy values
            return;
        }

        Transform nextWaypoint = waypoints[currentWaypointIndex];

        // 1. Car's current speed
        sensor.AddObservation(rBody.linearVelocity.magnitude); // 1 float

        // 2. Direction to next waypoint relative to car's forward (using local space)
        Vector3 localWaypointDir = transform.InverseTransformPoint(nextWaypoint.position);
        sensor.AddObservation(localWaypointDir.normalized.x); // x component for left/right (1 float)
        sensor.AddObservation(localWaypointDir.normalized.z); // z component for forward/backward (1 float)

        // 3. Distance to next waypoint
        sensor.AddObservation(Vector3.Distance(transform.position, nextWaypoint.position)); // 1 float

        // 4. Car's local velocity (how much it's sliding sideways)
        sensor.AddObservation(transform.InverseTransformDirection(rBody.linearVelocity).x); // sideways velocity (1 float)
        sensor.AddObservation(transform.InverseTransformDirection(rBody.linearVelocity).z); // forward velocity (1 float)

        // 5. Raycasts for obstacle detection (forward, forward-left, forward-right)
        // Helps the car avoid walls and other objects.
        Vector3[] rayDirections = {
            transform.forward, // Straight ahead
            (transform.forward + transform.right * 0.5f).normalized, // Slightly right
            (transform.forward - transform.right * 0.5f).normalized, // Slightly left
            (transform.forward + transform.right * 1f).normalized,   // More right
            (transform.forward - transform.right * 1f).normalized    // More left
        };

        foreach (Vector3 dir in rayDirections)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, out hit, raycastLength, obstacleLayer))
            {
                sensor.AddObservation(hit.distance / raycastLength); // Normalized distance to hit (0 to 1)
            }
            else
            {
                sensor.AddObservation(1f); // No hit
            }
        }
        // Total observations: 1 (speed) + 2 (localWaypointDir) + 1 (distance) + 2 (localVelocity) + 5 (raycasts) = 11 floats
    }

    // --- OnActionReceived (Apply AI's decisions to car physics) ---
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Get continuous actions from the agent's brain
        // Action 0: Accelerate/Brake (e.g., -1 for full brake/reverse, 1 for full accelerate)
        // Action 1: Steering (e.g., -1 for full left, 1 for full right)
        float accelerateBrakeInput = actions.ContinuousActions[0];
        float steerInput = actions.ContinuousActions[1];

        // Apply these inputs to the car's physics properties
        HandleMotor(accelerateBrakeInput);
        HandleSteering(steerInput);
        ApplyCarControls(); // Apply calculated motor/brake/steer torque to wheel colliders
        UpdateWheels(); // Update visual wheels

        // --- Rewards & Penalties ---
        timeSinceLastCheckpoint += Time.fixedDeltaTime;

        // Penalty for time taken (encourages speed)
        AddReward(-timePenaltyPerSecond * Time.fixedDeltaTime);

        // Penalty for going too long without hitting a checkpoint
        if (timeSinceLastCheckpoint > maxTimePerCheckpoint)
        {
            SetReward(-1.0f); // Large negative reward
            EndEpisode();
            return;
        }

        // Penalty for driving slowly or backward (if necessary, depends on game type)
        // If speed is very low and not braking, penalize
        if (rBody.linearVelocity.magnitude < 1f && Mathf.Abs(currentMotorTorque) > 0.1f && currentBrakeForce < 100f)
        {
             // AddReward(-0.005f); // Discourage getting stuck
        }
    }

    // --- Collision Detection for Penalties ---
    private void OnCollisionEnter(Collision collision)
    {
        // Penalize for hitting obstacles
        if (((1 << collision.gameObject.layer) & obstacleLayer) != 0)
        {
            AddReward(-collisionPenalty);
            // Optionally, end episode on severe collision
            // EndEpisode();
        }
        // Add a layer for "off-road" or "out of bounds" and penalize if the car enters it
        // Example: if (collision.gameObject.CompareTag("OffRoad")) { AddReward(-offRoadPenalty); }
    }


    // --- Manual Control for Debugging (Heuristic Mode) ---
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        
        // Use keyboard input for manual control in Heuristic mode
        // W/S for accelerate/brake
        float vertical = Input.GetAxis("Vertical"); 
        // A/D for steering
        float horizontal = Input.GetAxis("Horizontal");

        continuousActionsOut[0] = vertical; // Accelerate/Brake
        continuousActionsOut[1] = horizontal; // Steer
    }

    // --- Car Control Logic (Adapted from your CarController) ---

    private void HandleMotor(float accelerateBrakeInput)
    {
        // If accelerateBrakeInput > 0, apply motor torque (gas)
        // If accelerateBrakeInput < 0, check for braking or reverse
        
        if (accelerateBrakeInput > 0)
        {
            currentMotorTorque = accelerateBrakeInput * motorForce;
            currentBrakeForce = 0; // Release brake when accelerating
        }
        else if (accelerateBrakeInput < 0)
        {
            // If moving forward, apply brake
            if (Vector3.Dot(transform.forward, rBody.linearVelocity) > 0.1f) // Check if moving forward
            {
                currentBrakeForce = Mathf.Abs(accelerateBrakeInput) * brakeForce; // Apply brake force proportional to input
                currentMotorTorque = 0; // No motor torque when braking
            }
            else // If moving backward or stopped, apply reverse motor torque
            {
                currentMotorTorque = accelerateBrakeInput * motorForce; // Negative torque for reverse
                currentBrakeForce = 0; // Release brake when reversing
            }
        }
        else // No input, apply gentle brake to stop
        {
            currentMotorTorque = 0;
            currentBrakeForce = brakeForce * 0.1f; // Small constant brake to stop
        }

        frontLeftWheelCollider.motorTorque = currentMotorTorque;
        frontRightWheelCollider.motorTorque = currentMotorTorque;
        // Optionally, apply motor torque to rear wheels for AWD effect for AI
        rearLeftWheelCollider.motorTorque = currentMotorTorque;
        rearRightWheelCollider.motorTorque = currentMotorTorque;
    }

    private void HandleSteering(float steerInput)
    {
        currentSteerAngle = maxSteerAngle * steerInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void ApplyCarControls()
    {
        // Apply the determined motor torque, steer angle, and brake force
        frontLeftWheelCollider.brakeTorque = currentBrakeForce;
        frontRightWheelCollider.brakeTorque = currentBrakeForce;
        rearLeftWheelCollider.brakeTorque = currentBrakeForce;
        rearRightWheelCollider.brakeTorque = currentBrakeForce;
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

    // --- Checkpoint Logic ---
    private void OnTriggerEnter(Collider other)
    {
        // Check if the car hits the correct checkpoint
        if (waypoints != null && currentWaypointIndex < waypoints.Count && other.transform == waypoints[currentWaypointIndex])
        {
            AddReward(1.0f); // Reward for hitting a checkpoint
            Debug.Log($"Agent hit checkpoint {currentWaypointIndex}. Reward: {GetCumulativeReward()}");

            // Optionally, add a reward based on time taken for the checkpoint
            // float timeBonus = Mathf.Max(0, 1.0f - (timeSinceLastCheckpoint / maxTimePerCheckpoint));
            // AddReward(timeBonus * 0.5f);

            currentWaypointIndex++;
            timeSinceLastCheckpoint = 0f; // Reset time for the next checkpoint

            if (currentWaypointIndex >= waypoints.Count)
            {
                // Reached the end of the track (or finished a lap)
                AddReward(10.0f); // Big reward for finishing a lap
                Debug.Log($"Agent completed a lap! Final Reward: {GetCumulativeReward()}");
                EndEpisode(); // End the episode
            }
        }
        else if (other.gameObject.CompareTag("OffRoad") || other.gameObject.CompareTag("Boundary")) // Tag your off-road areas or boundaries
        {
             AddReward(-offRoadPenalty);
             // Optionally, end episode on going off track too much
             // EndEpisode();
        }
    }


    // Optional: Draw Gizmos for AI's current target waypoint in editor
    private void OnDrawGizmos()
    {
        if (waypoints != null && currentWaypointIndex >= 0 && currentWaypointIndex < waypoints.Count && waypoints[currentWaypointIndex] != null)
        {
            Gizmos.color = Color.magenta; // Highlight the current target waypoint
            Gizmos.DrawSphere(waypoints[currentWaypointIndex].position, 1.5f);
            Gizmos.DrawLine(transform.position, waypoints[currentWaypointIndex].position);
        }

        // Draw raycasts for debugging observation
        Gizmos.color = Color.red;
        Vector3[] rayDirections = {
            transform.forward,
            (transform.forward + transform.right * 0.5f).normalized,
            (transform.forward - transform.right * 0.5f).normalized,
            (transform.forward + transform.right * 1f).normalized,
            (transform.forward - transform.right * 1f).normalized
        };
        foreach (Vector3 dir in rayDirections)
        {
            Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, dir * raycastLength);
        }
    }
}