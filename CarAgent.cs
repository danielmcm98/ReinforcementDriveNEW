using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Assets
{
    public class CarAgent : Agent
    {

        [Header("Car Properties")]
        [SerializeField] private float motorForce = 1000f;
        [SerializeField] private float brakeForce = 2000f;
        [SerializeField] private float maxSteeringAngle = 35f;
        [SerializeField] private float downForceCoefficient = 200f;
        [SerializeField] private float steerAssistCoefficient = 1f;

        [Header("Wheels")]
        [SerializeField] private WheelCollider[] driveWheels;
        [SerializeField] private WheelCollider[] steerWheels;
        [SerializeField] private WheelCollider[] brakeWheels;


        [Header("Training")]
        [Tooltip("Number of steps to time out after in training")]
        public int stepTimeout = 3000;

        public int NewCheckpointIndex { get; set; }

        // When the next step timeout will be during training
        private float nextStepTimeout;

        //Components to keep track of 
        private CarArea area;
        new private Rigidbody rigidbody;

        //CHeck if frozen
        private bool frozen = false;

        public bool useHeuristic;


        //Controls
        private float steerChange = 0f;
        private float driveChange = 0f;
        private float brakeChange = 0f;

        //Called when agent is first 
        public override void Initialize()
        {
            area = GetComponentInParent<CarArea>();
            rigidbody = GetComponent<Rigidbody>();
            rigidbody.centerOfMass = new Vector3(0, -0.7f, 0);


            //Training and racing steps
            MaxStep = area.trainingMode ? 5000 : 0;
        }

        //Called when new start
        public override void OnEpisodeBegin()
        {
            //Reset  position, speed, orientation
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            area.ResetAgentPosition(agent: this, randomise: area.trainingMode); //Resets to random checkpoint

            //Update steo timeout if training
            if (area.trainingMode) nextStepTimeout = StepCount + stepTimeout;
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            if (useHeuristic)
            {
                var discreteActions = actionsOut.DiscreteActions;

                // Steering
                if (Input.GetKey(KeyCode.A))
                {
                    discreteActions[0] = 2;
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    discreteActions[0] = 1;
                }
                else
                {
                    discreteActions[0] = 0;
                }

                // Throttle
                if (Input.GetKey(KeyCode.W))
                {
                    discreteActions[1] = 1;
                }
                else
                {
                    discreteActions[1] = 0;
                }

                // Brake
                if (Input.GetKey(KeyCode.Space))
                {
                    discreteActions[2] = 1;
                }
                else
                {
                    discreteActions[2] = 0;
                }
            }
        }


        public override void OnActionReceived(ActionBuffers actions)
        {
            if (frozen) return;

            switch (actions.DiscreteActions[0])
            {
                case 0: steerChange = 0f; break;
                case 1: steerChange = +1f; break;
                case 2: steerChange = -1f; break;
            }
            switch (actions.DiscreteActions[1])
            {
                case 0: driveChange = 0f; break;
                case 1: driveChange = +1f; break;
            }
            switch (actions.DiscreteActions[2])
            {
                case 0: brakeChange = 0f; break;
                case 1: brakeChange = +1f; break;
            }


            FixedUpdate();

            if (area.trainingMode)
            {   // Small reward every step
                AddReward(-1f / MaxStep);

                //make sure trainign time 
                if (StepCount > nextStepTimeout)
                {

                    Debug.Log("StepTimeout");
                    AddReward(-2.5f);
                    EndEpisode();
                }

                Vector3 lCheckpointDir = VectorNextCheckpoint();
                if (lCheckpointDir.magnitude < Academy.Instance.EnvironmentParameters.GetWithDefault("checkpoint_rad", 0f))
                {
                    CheckpointRec();
                }
            }
        }

        //Collect observations used by agents
        public override void CollectObservations(VectorSensor sensor)
        {
            // Obsereve car speed 1vec3, 3 vals
            sensor.AddObservation(transform.InverseTransformDirection(rigidbody.velocity));

            //Find next checkpoint 1vec3, 3 vals
            sensor.AddObservation(VectorNextCheckpoint());

            //Direction of next checkpoint 1vec3, 3 vals
            Vector3 nextCheckpointFor = area.Checkpoints[NewCheckpointIndex].transform.forward;
            sensor.AddObservation(transform.InverseTransformDirection(nextCheckpointFor));

            //9 total observations
        }


            //Stops agent moving and taking actions
            public void FreezeAgent()
        {
            Debug.Assert(area.trainingMode == false, "Freeze/unfreeze unsuported");
            frozen = true;
            rigidbody.Sleep();
        }
        public void UnFreezeAgent()
        {
            Debug.Assert(area.trainingMode == false, "Freeze/unfreeze unsuported");
            frozen = false;
            rigidbody.WakeUp();
        }

        //Gets vector to next checkpoint agent needs to drive through
        private Vector3 VectorNextCheckpoint()
        {
            Vector3 newCheckpointDir = area.Checkpoints[NewCheckpointIndex].transform.position - transform.position;
            Vector3 lCheckpointDir = transform.InverseTransformDirection(newCheckpointDir);
            return lCheckpointDir;
        }

        //Caled when agent drives through checkpoint
        private void CheckpointRec()
        {
            //Next checkpoint reached updatw
            NewCheckpointIndex = (NewCheckpointIndex + 1) % area.Checkpoints.Count; //so doesnt go over total num of checkpoints %

            if (area.trainingMode)
            {
                Debug.Log("Checkpoint");
                AddReward(1f);
                nextStepTimeout = StepCount + stepTimeout; 
            }
        }


        private void FixedUpdate()
        {
            if (!frozen)
            {
                ApplyMotorTorque();
                ApplySteering();
                ApplyBrakes();
                ApplyDownForce();
                ApplySteerAssist();
            }
        }

        private void ApplyMotorTorque()
        {
            float motorTorque = driveChange * motorForce;
            foreach (var wheel in driveWheels)
            {
                wheel.motorTorque = motorTorque;
            }

            if (area.trainingMode && driveChange < 1f)
            {
                AddReward(-0.005f);
            }
        }

        private void ApplySteering()
        {
            float steerAngle = maxSteeringAngle * steerChange;
            foreach (var wheel in steerWheels)
            {
                wheel.steerAngle = steerAngle;
            }
        }

        private void ApplyBrakes()
        {
            float appliedBrakeForce = brakeChange * brakeForce;
            foreach (var wheel in brakeWheels)
            {
                wheel.brakeTorque = appliedBrakeForce;
            }

            // Apply a small negative reward for braking during training
            if (area.trainingMode && brakeChange > 0f)
            {
                AddReward(-0.005f);
            }
        }


        private void ApplyDownForce()
        {
            rigidbody.AddForce(-transform.up * rigidbody.velocity.magnitude * downForceCoefficient);
        }

        private void ApplySteerAssist()
        {
            Vector3 localVelocity = transform.InverseTransformDirection(rigidbody.velocity);
            rigidbody.AddForceAtPosition(transform.right * -localVelocity.x * steerAssistCoefficient, transform.position);
        }


        //What happens when hit trigger, other = colider entered
        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.CompareTag("Checkpoint") &&
                other.gameObject == area.Checkpoints[NewCheckpointIndex])
            {
                CheckpointRec();
            }

            if (other.transform.CompareTag("Walls"))
            {
                if (area.trainingMode)
                {
                    AddReward(-0.75f);
                    Debug.Log("Wall Hit");
                }
            }

        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.transform.CompareTag("Agent"))
            {
                if (area.trainingMode)
                {
                    AddReward(-.25f);
                    Debug.Log("Crash");
                }

            }

            if (collision.transform.CompareTag("OffTrack"))
            {
                if (area.trainingMode)
                {
                    Debug.Log("Offtrack");
                    AddReward(-2f);
                    EndEpisode();
                }
                else
                {
                    StartCoroutine(FailReset());
                }
            }
        }



        private IEnumerator FailReset()
        {
            FreezeAgent();

            yield return new WaitForSeconds(2f);

            //Reset pos
            area.ResetAgentPosition(agent: this);
            yield return new WaitForSeconds(1f);

            UnFreezeAgent();

        }
    }
}
