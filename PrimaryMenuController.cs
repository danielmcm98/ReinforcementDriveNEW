using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets
{
    public class PrimaryMenuController : MonoBehaviour
    {
        [Tooltip("List containing playable levels")]
        public List<string> playableLevels;

        [Tooltip("Dropdown for level selection")]
        public TMP_Dropdown levelSelector;

        [Tooltip("Dropdown for difficulty selection")]
        public TMP_Dropdown difficultySelector;

        private string chosenLevel;
        private GameDifficulty chosenDifficulty;

        // Populate dropdown lists with information
        private void Start()
        {
            Debug.Assert(playableLevels.Count > 0, "No levels available");
            levelSelector.ClearOptions();
            levelSelector.AddOptions(playableLevels);
            chosenLevel = playableLevels[0];

            difficultySelector.ClearOptions();
            difficultySelector.AddOptions(Enum.GetNames(typeof(GameDifficulty)).ToList());
            chosenDifficulty = GameDifficulty.Easy;
        }

        public void AssignLevel(int levelIndex)
        {
            chosenLevel = playableLevels[levelIndex];
        }

        public void AssignDifficulty(int difficultyIndex)
        {
            chosenDifficulty = (GameDifficulty)difficultyIndex;
        }

        public void OnStartButtonPress()
        {
            GameManager.Instance.GameDifficulty = chosenDifficulty;

            // Load level in preparation mode
            GameManager.Instance.LoadLevel(chosenLevel, GameState.Preparing);
        }

        public void OnQuitButtonPress()
        {
            Application.Quit();
        }
    }
}
