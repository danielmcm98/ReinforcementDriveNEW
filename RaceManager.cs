using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

namespace Assets
{
    public class RaceManager : MonoBehaviour
    {
        [Tooltip("Number of laps for this race")]
        public int numLaps = 2;

        [Tooltip("Bonus seconds on reaching checkpoint")]
        public float checkpointBonusTime = 10f;

        [Serializable]
        public struct DifficultyModel
        {
            public GameDifficulty difficulty;
            public NNModel model;
        }

        public List<DifficultyModel> difficultyModels;

        //Agent being followed by camera
        public CarAgent ObservedAgent { get; private set; }


        private TimerUIController timerUI;
        private DashboardController dashboard;
        private FinishUIController finishUI;
        private CarArea carArea;
        private List<CarAgent> sortedCarAgents;

        //Pause timers
        private float lastResumeTime = 0f;
        private float previouslyElapsedTime = 0f;

        private float lastPlaceUpdate = 0f;
        private Dictionary<CarAgent, CarStatus> carStatuses;
        private class CarStatus
        {
            public int checkpointIndex = 0;
            public int lap = 0;
            public int place = 0;
            public float timeRemaining = 0f;
        }
        //Clock for race times
        public float RaceTime
        {
            get
            {
                if (GameManager.Instance.GameState == GameState.Playing)
                {
                    return previouslyElapsedTime + Time.time - lastResumeTime;
                }
                else
                {
                    return 0f;
                }
            }
        }

        //Get next checkpoint transform
        public Transform GetAgentNextCheckpoint(CarAgent agent)
        {
            return carArea.Checkpoints[carStatuses[agent].checkpointIndex].transform;
        }
        //get lap
        public int GetAgentLap(CarAgent agent)
        {
            return carStatuses[agent].lap;
        }

        //Get position
        public string GetAgentPlace(CarAgent agent)
        {
            int place = carStatuses[agent].place;
            if (place <= 0)
            {
                return string.Empty;
            }

            if (place >= 11 && place <= 13) return place.ToString() + "th";

            switch (place % 10)
            {
                case 1:
                    return place.ToString() + "st";
                case 2:
                    return place.ToString() + "nd";
                case 3:
                    return place.ToString() + "rd";
                default:
                    return place.ToString() + "th";
            }
        }
        public float GetAgentTime(CarAgent agent)
        {
            return carStatuses[agent].timeRemaining;
        }

        private void Awake()
        {
            dashboard = FindObjectOfType<DashboardController>();
            timerUI = FindObjectOfType<TimerUIController>();
            finishUI = FindObjectOfType<FinishUIController>();
            carArea = FindObjectOfType<CarArea>();
        }

        //Setup and race start
        private void Start()
        {
            GameManager.Instance.OnStateChange += OnStateChange;

            //Pick agent if no player
            ObservedAgent = GameObject.Find("CarAgent").GetComponent<CarAgent>();
            foreach (CarAgent agent in carArea.CarAgents)
            {
                agent.FreezeAgent();
                if (agent.gameObject.name != "CarAgent")
                {
                    agent.SetModel(GameManager.Instance.GameDifficulty.ToString(),
                        difficultyModels.Find(x => x.difficulty == GameManager.Instance.GameDifficulty).model);
                }
            }

            //Make dashboard follow the agent
            dashboard.ObservedAgent = ObservedAgent;

            // Hide UI
            dashboard.gameObject.SetActive(false);
            timerUI.gameObject.SetActive(false);
            finishUI.gameObject.SetActive(false);

            // Start the race
            StartCoroutine(StartRace());
        }

        //starts count down
        private IEnumerator StartRace()
        {
            timerUI.gameObject.SetActive(true);
            yield return timerUI.BeginCountdown();

            //Status tracking
            carStatuses = new Dictionary<CarAgent, CarStatus>();
            foreach (CarAgent agent in carArea.CarAgents)
            {
                CarStatus status = new CarStatus();
                status.lap = 1;
                status.timeRemaining = checkpointBonusTime;
                carStatuses.Add(agent, status);
            }
            //Unfreeze
            foreach (CarAgent agent in carArea.CarAgents) agent.UnFreezeAgent();

            //start play
            GameManager.Instance.GameState = GameState.Playing;
        }

        //React to state changes
        private void OnStateChange()
        {
            if (GameManager.Instance.GameState == GameState.Playing)
            {
                //Start game time, show dashboard, unfreeze agents
                dashboard.gameObject.SetActive(true);
                foreach (CarAgent agent in carArea.CarAgents) agent.UnFreezeAgent();
            }
            else if (GameManager.Instance.GameState == GameState.Gameover)
            {
                //Pause game time, hide dashboard, freeze agent
                previouslyElapsedTime += Time.time - lastResumeTime;
                dashboard.gameObject.SetActive(false);
                foreach (CarAgent agent in carArea.CarAgents) agent.FreezeAgent();

                //show game over 
                finishUI.gameObject.SetActive(true);
            }
            else
            {
                //reset time
                foreach (CarAgent agent in carArea.CarAgents) agent.FreezeAgent();
                lastResumeTime = 0f;
                previouslyElapsedTime = 0f;
            }
        }


        private void FixedUpdate()
        {
            if (GameManager.Instance.GameState == GameState.Playing)
            {
                // Update the place list every half second
                if (lastPlaceUpdate + .5f < Time.fixedTime)
                {
                    lastPlaceUpdate = Time.fixedTime;

                    if (sortedCarAgents == null)
                    {
                        // Get a copy of the list of agents for sorting
                        sortedCarAgents = new List<CarAgent>(carArea.CarAgents);
                    }

                    // Recalculate race places
                    sortedCarAgents.Sort((a, b) => PlaceComparer(a, b));
                    for (int i = 0; i < sortedCarAgents.Count; i++)
                    {
                        carStatuses[sortedCarAgents[i]].place = i + 1;
                    }
                }

                // Update agent statuses
                foreach (CarAgent agent in carArea.CarAgents)
                {
                    CarStatus status = carStatuses[agent];

                    // Update agent lap
                    if (status.checkpointIndex != agent.NewCheckpointIndex)
                    {
                        status.checkpointIndex = agent.NewCheckpointIndex;
                        status.timeRemaining = checkpointBonusTime;

                        if (status.checkpointIndex == 0)
                        {
                            status.lap++;
                            if (agent == ObservedAgent && status.lap > numLaps)
                            {
                                GameManager.Instance.GameState = GameState.Gameover;
                            }
                        }
                    }

                    // Update agent time remaining
                    status.timeRemaining = Mathf.Max(0f, status.timeRemaining - Time.fixedDeltaTime);
                    if (status.timeRemaining == 0f)
                    {
                        carArea.ResetAgentPosition(agent);
                        status.timeRemaining = checkpointBonusTime;
                    }
                }
            }
        }

        private int PlaceComparer(CarAgent a, CarAgent b)
        {
            CarStatus statusA = carStatuses[a];
            CarStatus statusB = carStatuses[b];
            int checkpointA = statusA.checkpointIndex + (statusA.lap - 1) * carArea.Checkpoints.Count;
            int checkpointB = statusB.checkpointIndex + (statusB.lap - 1) * carArea.Checkpoints.Count;
            if (checkpointA == checkpointB)
            {
                // Compare distances to the next checkpoint
                Vector3 nextCheckpointPosition = GetAgentNextCheckpoint(a).position;
                int compare = Vector3.Distance(a.transform.position, nextCheckpointPosition)
                    .CompareTo(Vector3.Distance(b.transform.position, nextCheckpointPosition));
                return compare;
            }
            else
            {
                // Compare number of checkpoints hit. The agent with more checkpoints is
                // ahead (lower place), so we flip the compare
                int compare = -1 * checkpointA.CompareTo(checkpointB);
                return compare;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnStateChange -= OnStateChange;
        }
    }
}
