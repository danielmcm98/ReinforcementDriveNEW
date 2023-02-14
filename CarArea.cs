using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Car //So no naming conflicts
{
    public class CarArea : MonoBehaviour
    {
        [Tooltip("The path the race will take")]
        public CinemachineSmoothPath racePath;

        [Tooltip("The prefab for checkpoints")]
        public GameObject checkpointPrefab;

        [Tooltip("The prefab for start")]
        public GameObject startPrefab;

        [Tooltip("The prefab for finish")]
        public GameObject endPrefab;

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
            int numCheckpoints = (int)racePath.MaxUnit(CinemachinePathBase.Position.PathUnits); //Figures out how many points on racepath
            for (int i = 0; i < numCheckpoints; i++){

                //Either create a nromal checkpoint, start or finish line
                GameObject checkpoint;
                if (i == 0) checkpoint = Instantiate<GameObject>(startLine);
                else if (i == numCheckpoints - 1) checkpoint = Instantiate<GameObject>(finishLine);
                else checkpoint = Instantiate<GameObject>(checkpointPrefab);

                //Set the parent, position and rotation
                checkpointPrefab.trasnform.SetParent(racePath.transform);
                checkpointPrefab.transform.localPosition = racePath.m_Waypoints[i].position;
                checkpoint.transform.rotation = racePath.EvaluateOrientationAtUnit(i, CinemachinePathBase.PositionUnits.PathUnits);

                // Add the checkpoint to the list
                Checkpoints.Add(checkpoint);

            }

        }
    }
}