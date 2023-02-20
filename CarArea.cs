using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets //So no naming conflicts
{
    public class CarArea : MonoBehaviour
    {
        [Tooltip("The path the race will take")]
        public CinemachineSmoothPath racePath;

        [Tooltip("The prefab for checkpoints")]
        public GameObject checkpointPrefab;

        [Tooltip("The prefab for start")]
        public GameObject startLinePrefab;

        [Tooltip("The prefab for finish")]
        public GameObject finishLinePrefab;

        [Tooltip("Enable training using true")]
        public bool trainingMode;

        public List<CarAgent> CarAgents { get; private set; } //Anything public can access but can only be set inside of class
        public List<GameObject> Checkpoints { get; private set; }

        //Actions to preform when scripts wakens
        private void Awake()
        {
            //Find all aircraft Agents in area.
            CarAgents = transform.GetComponentsInChildren<CarAgent>().ToList(); //Returns list instead of array as default
            Debug.Assert(CarAgents.Count > 0, "No CarAgents found");
        }

        //Setup the area
        private void Start()
        {
            //Create checkpoints on racepath
            Debug.Assert(racePath != null, "Racepath not set");
            Checkpoints = new List<GameObject>();
            int numCheckpoints = (int)racePath.MaxUnit(CinemachinePathBase.PositionUnits.PathUnits); //Figures out how many points on racepath
            for (int i = 0; i < numCheckpoints; i++){

                //Either create a nromal checkpoint, start or finish line
                GameObject checkpoint;
                if (i == 0) checkpoint = Instantiate<GameObject>(startLinePrefab);
                else if (i == numCheckpoints - 1) checkpoint = Instantiate<GameObject>(finishLinePrefab);
                else checkpoint = Instantiate<GameObject>(checkpointPrefab);

                //Set the parent, position and rotation
                checkpoint.transform.SetParent(racePath.transform);
                checkpoint.transform.localPosition = racePath.m_Waypoints[i].position;
                checkpoint.transform.rotation = racePath.EvaluateOrientationAtUnit(i, CinemachinePathBase.PositionUnits.PathUnits);

                // Add the checkpoint to the list
                Checkpoints.Add(checkpoint);

            }

        }

        //Reset the position of agent using its current new checkpoint index unless randomise true
        public void ResetAgentPosition(CarAgent agent, bool randomise = false)
        {
            if (randomise)
            {
                //Pick next checkpoint at random
                agent.NewCheckpointIndex = Random.Range(0, Checkpoints.Count);
            }

            //Set start pos to last checkpoint
            int lastCheckpointIndex = agent.NewCheckpointIndex - 1;
            if (lastCheckpointIndex == -1) lastCheckpointIndex = Checkpoints.Count - 1;

            float startPos = racePath.FromPathNativeUnits(lastCheckpointIndex, CinemachinePathBase.PositionUnits.PathUnits);

            //Make pos on racepath to 3Dspace pos
            Vector3 basePos = racePath.EvaluatePosition(startPos);

            // Get the orientation at that position on the race path
            Quaternion orientation = racePath.EvaluateOrientation(startPos);

            // Calculate a horizontal offset so that agents are spread out
            Vector3 posOffset = Vector3.right * (CarAgents.IndexOf(agent) - CarAgents.Count / 2f)
                * Random.Range(4f, 5f); //Random so it doesnt take same path

            // Set the car pos and rotation
            agent.transform.position = basePos + orientation * posOffset;
            agent.transform.rotation = orientation;
        }
    }
}