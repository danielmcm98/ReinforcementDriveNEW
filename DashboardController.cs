using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets
{
    public class DashboardController : MonoBehaviour
    {
        [Tooltip("The text showing the current position in the race")]
        public TextMeshProUGUI currentPositionText;

        [Tooltip("The text showing the time remaining to reach the next checkpoint")]
        public TextMeshProUGUI remainingTimeText;

        [Tooltip("The text showing the current lap")]
        public TextMeshProUGUI currentLapText;

        // The agent this dashboard shows information for
        public CarAgent ObservedAgent { get; set; }

        private RaceManager raceMgr;

        private void Awake()
        {
            raceMgr = FindObjectOfType<RaceManager>();
        }

        private void Update()
        {
            if (ObservedAgent != null)
            {
                RefreshPositionText();
                RefreshTimeText();
                RefreshLapText();
            }
        }

        private void RefreshPositionText()
        {
            string position = raceMgr.GetAgentPlace(ObservedAgent);
            currentPositionText.text = position;
        }

        private void RefreshTimeText()
        {
            float time = raceMgr.GetAgentTime(ObservedAgent);
            remainingTimeText.text = "Time " + time.ToString("0.0");
        }

        private void RefreshLapText()
        {
            int lap = raceMgr.GetAgentLap(ObservedAgent);
            currentLapText.text = "Lap " + lap + "/" + raceMgr.numLaps;
        }

    }
}
