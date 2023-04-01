using System.Collections;
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
  
        
        [Header("Movement Parameters")]
        public float driveForce = 1000f;
        public float breakingForce = 5000f;
        public float steerSpeed = 100f;

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

        //Controls
        private float steerChange = 0f;
        private float steerAngleNow;
        private float maxSteerAngle = 30f;
        private float driveChange = 0f;
        private float brakeChange = 0f;
        private float breakingForceNow = 0f;

        //Called when agent is first 
        public override void Initialize()
        {
            area = GetComponentInParent<CarArea>();
            rigidbody = GetComponent<Rigidbody>();

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
                case 2: driveChange = -1f; break;
            }
/*            switch (actions.DiscreteActions[2])
            {
                case 0: brakeChange = 0f; break;
                case 1: brakeChange = +1f; AddReward(-0.05f); break;
            }*/

/*            Debug.Log(actions.DiscreteActions[0]);
            Debug.Log(actions.DiscreteActions[1]);
            Debug.Log(actions.DiscreteActions[2]);*/

            MakeMoves();

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
                if(lCheckpointDir.magnitude < Academy.Instance.EnvironmentParameters.GetWithDefault("checkpoint_rad", 0f))
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

            //9 total obs
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
                AddReward(.5f);
                nextStepTimeout = StepCount + stepTimeout; //.5 for each checkpoint then increases timeout
            }
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
            steerAngleNow = maxSteerAngle * steerChange;
            FrontLeftWheelCollider.steerAngle = steerAngleNow;
            FrontRightWheelCollider.steerAngle = steerAngleNow;

            //Update wheels
            ChangeOneWheel(FrontLeftWheelCollider, FrontLeftWheelTransform);
            ChangeOneWheel(FrontRightWheelCollider, FrontRightWheelTransform);
            ChangeOneWheel(RearRightWheelCollider, RearRightWheelTransform);
            ChangeOneWheel(RearLeftWheelCollider, RearLeftWheelTransform);
        }
        private void ChangeOneWheel(WheelCollider wheelCollider, Transform wheelTransform)
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

        //What happens when hit trigger, other = colider entered
        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.CompareTag("Checkpoint") &&
                other.gameObject == area.Checkpoints[NewCheckpointIndex])
            {
                CheckpointRec();
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

            if(collision.gameObject.CompareTag("Walls"))
            {
                AddReward(-0.5f);
            }

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
                  //  AddReward(-2f);
                  //  EndEpisode();
                    Debug.Log("Offtrack");
                }
                else
                {
                    StartCoroutine(FailReset());
                }
            }
        }


        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.CompareTag("Walls"))
            {
                AddReward(-0.1f);
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