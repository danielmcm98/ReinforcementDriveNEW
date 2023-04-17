using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AdvancedCarController : MonoBehaviour
{
    [Header("Car Properties")]
    [SerializeField] private float motorForce = 1500f;
    [SerializeField] private float brakeForce = 3000f;
    [SerializeField] private float maxSteeringAngle = 30f;
    [SerializeField] private float downForceCoefficient = 10f;
    [SerializeField] private float steerAssistCoefficient = 0.2f;

    [Header("Wheels")]
    [SerializeField] private WheelCollider[] driveWheels;
    [SerializeField] private WheelCollider[] steerWheels;
    [SerializeField] private WheelCollider[] brakeWheels;

    private float horizontalInput;
    private float verticalInput;
    private bool isBraking;
    private Rigidbody carRigidbody;

    private void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();

        // Lower the center of mass
        carRigidbody.centerOfMass = new Vector3(0, -0.7f, 0);

    }

    private void FixedUpdate()
    {
        ProcessInput();
        ApplyMotorTorque();
        ApplySteering();
        ApplyBrakes();
        ApplyDownForce();
        ApplySteerAssist();
    }

    private void ProcessInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBraking = Input.GetKey(KeyCode.Space);
    }

    private void ApplyMotorTorque()
    {
        float motorTorque = verticalInput * motorForce;
        foreach (var wheel in driveWheels)
        {
            wheel.motorTorque = motorTorque;
        }
    }

    private void ApplySteering()
    {
        float steerAngle = maxSteeringAngle * horizontalInput;
        foreach (var wheel in steerWheels)
        {
            wheel.steerAngle = steerAngle;
        }
    }

    private void ApplyBrakes()
    {
        float appliedBrakeForce = isBraking ? brakeForce : 0f;
        foreach (var wheel in brakeWheels)
        {
            wheel.brakeTorque = appliedBrakeForce;
        }
    }

    private void ApplyDownForce()
    {
        carRigidbody.AddForce(-transform.up * carRigidbody.velocity.magnitude * downForceCoefficient);
    }

    private void ApplySteerAssist()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(carRigidbody.velocity);
        carRigidbody.AddForceAtPosition(transform.right * -localVelocity.x * steerAssistCoefficient, transform.position);
    }
}
