using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";

    private float horizInput;
    private float vertInput;
    private float steerAngleNow;
    private float breakingForceNow;
    private bool isBreak;

    public float driveForce = 1000f;
    public float breakingForce = 5000f;
    public float maxSteerAngle = 30f;

    [SerializeField] private WheelCollider FrontLeftWheelCollider;
    [SerializeField] private WheelCollider FrontRightWheelCollider;
    [SerializeField] private WheelCollider RearLeftWheelCollider;
    [SerializeField] private WheelCollider RearRightWheelCollider;

    [SerializeField] private Transform FrontLeftWheelTransform;
    [SerializeField] private Transform FrontRightWheelTransform;
    [SerializeField] private Transform RearLeftWheelTransform;
    [SerializeField] private Transform RearRightWheelTransform;

    private void FixedUpdate()
    {
        GetInput();
        HandleDrive();
        HandleSteer();
        MoveWheels();
    }


    private void GetInput()
    {
        horizInput = Input.GetAxis(HORIZONTAL);
        vertInput = Input.GetAxis(VERTICAL);
        isBreak = Input.GetKey(KeyCode.Space);
    }

    private void HandleDrive()
    {
        FrontLeftWheelCollider.motorTorque = vertInput * driveForce;
        FrontRightWheelCollider.motorTorque = vertInput * driveForce;
        breakingForceNow = isBreak ? breakingForce : 0f;
        UseBreak();
    }

    private void UseBreak()
    {
        FrontRightWheelCollider.brakeTorque = breakingForceNow;
        FrontLeftWheelCollider.brakeTorque = breakingForceNow;
        RearLeftWheelCollider.brakeTorque = breakingForceNow;
        RearRightWheelCollider.brakeTorque = breakingForceNow;
    }

    private void HandleSteer()
    {
        steerAngleNow = maxSteerAngle * horizInput;
        FrontLeftWheelCollider.steerAngle = steerAngleNow;
        FrontRightWheelCollider.steerAngle = steerAngleNow;
    }

    private void MoveWheels()
    {
        UpdateSingleWheel(FrontLeftWheelCollider, FrontLeftWheelTransform);
        UpdateSingleWheel(FrontRightWheelCollider, FrontRightWheelTransform);
        UpdateSingleWheel(RearRightWheelCollider, RearRightWheelTransform);
        UpdateSingleWheel(RearLeftWheelCollider, RearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 position;
        Quaternion rotation;
        wheelCollider.GetWorldPose(out position, out rotation);
        wheelTransform.rotation = rotation;
        wheelTransform.position = position;
    }
}
