using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Assets
{ 
    public class CarAgent : Agent
    {

        [SerializeField] private WheelCollider FrontLeftWheelCollider;
        [SerializeField] private WheelCollider FrontRightWheelCollider;
        [SerializeField] private WheelCollider RearLeftWheelCollider;
        [SerializeField] private WheelCollider RearRightWheelCollider;

        [SerializeField] private Transform FrontLeftWheelTransform;
        [SerializeField] private Transform FrontRightWheelTransform;
        [SerializeField] private Transform RearLeftWheelTransform;
        [SerializeField] private Transform RearRightWheelTransform;
        public int NewCheckpointIndex { get; set; }
        
        [Header("Movement Parameters")]
        public float driveForce = 1000f;
        public float breakingForce = 5000f;
        public float steerSpeed = 100f;



        //Components to keep track of 
        private CarArea area;
        new private Rigidbody rigidbody;

        //Controls
        private float steerChange = 0f;
        private float SmoothSteerChange = 0f;
        private float maxSteerAngle = 30f;
        private float driveChange = 0f;
        private float brakeChange = 0f;
        private float breakingForceNow = 0f;

        //Called when agent is first 
        public override void Initialize()
        {
            area = GetComponentInParent<CarArea>();
            rigidbody = GetComponent<Rigidbody>();
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            switch (actions.DiscreteActions[0])
            {
                case 0: steerChange = 0f; break;
                case 1: steerChange = 1f; break;
                case 2: steerChange = -1f; break;
            }
            switch (actions.DiscreteActions[1])
            {
                case 0: driveChange = 0f; break;
                case 1: driveChange = 1f; break;
                case 2: driveChange = -1f; break;
            }
            switch (actions.DiscreteActions[2])
            {
                case 0: brakeChange = 0f; break;
                case 1: brakeChange = 1f; break;
            }

            MakeMoves();
        }
        
        //Do movement
        private void MakeMoves()
        {
            //Driving forward and back
            FrontLeftWheelCollider.motorTorque = driveChange * driveForce;
            FrontRightWheelCollider.motorTorque = driveChange * driveForce;
            breakingForceNow = brakeChange * breakingForce;
            UseBreak();

            //Turning
            SmoothSteerChange = maxSteerAngle * steerChange;
            FrontLeftWheelCollider.steerAngle = SmoothSteerChange;
            FrontRightWheelCollider.steerAngle = SmoothSteerChange;

            //Update wheels
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

        private void UseBreak()
        {
            FrontRightWheelCollider.brakeTorque = breakingForceNow;
            FrontLeftWheelCollider.brakeTorque = breakingForceNow;
            RearLeftWheelCollider.brakeTorque = breakingForceNow;
            RearRightWheelCollider.brakeTorque = breakingForceNow;
        }

    }

} 
/*
            [SerializeField] private Transform spawnPos;

            private CarController carController;    

            private void Awake()
            {
                carController = GetComponent<CarController>(); 
            }

            public override void OnEpisodeBegin() {
                transform.position = spawnPos.position + new Vector3(Random.Range(-5f, +5f), 1, Random.Range(-5f, +5f));
                transform.forward = spawnPos.forward;
               // carController.StopCompletely();
            }

            /*public override void CollectObservations(VectorSensor sensor){
                Vector3 
                }

            public override void OnActionReceived(ActionBuffers actions)
            {
                float vertInput = 0f;
                float horizInput = 0f;
                float isBreak = 0f;

                switch (actions.DiscreteActions[0]){
                    case 0: vertInput = 0f; break;
                    case 1: vertInput = 1f; break;
                    case 2: vertInput = -1f; break;
                }
                switch (actions.DiscreteActions[1]){
                    case 0: horizInput = 0f; break;
                    case 1: horizInput = 1f; break;
                    case 2: horizInput = -1f; break;
                }
                switch (actions.DiscreteActions[2]){
                    case 0: isBreak = 0f; break;
                    case 1: isBreak = 1f; break;
                    case 2: isBreak = -1f; break;
                }

                carController.GetInput(horizInput, vertInput, isBreak);


                }
            */