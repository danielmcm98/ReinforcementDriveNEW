using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        }
    }
}