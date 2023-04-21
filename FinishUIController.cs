using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets
{
    public class FinishUIController : MonoBehaviour
    {
        [Tooltip("Text to display finishing position (e.g. 2nd place)")]
        public TextMeshProUGUI positionText;

        private RaceManager raceMgr;

        private void Awake()
        {
            raceMgr = FindObjectOfType<RaceManager>();
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.GameState == GameState.Gameover)
            {
                // Gets the finishing position and updates the text
                string position = raceMgr.GetAgentPlace(raceMgr.ObservedAgent);
                this.positionText.text = position + " Place";
            }
        }

        /// <summary>
        /// Loads the MainMenu scene
        /// </summary>
        public void ClickedMainMenuButton()
        {
            GameManager.Instance.LoadLevel("MainMenu", GameState.MainMenu);
        }
    }
}
