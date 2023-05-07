using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets
{
    public enum GameState
    {
        Default,
        MainMenu,
        Preparing,
        Playing,
        Gameover
    }
    
    public enum GameDifficulty
    {
        Easy,
        Hard
    }

    public delegate void OnStateChangeHandler();
    public class GameManager : MonoBehaviour
    {
        //Event called when state changes
        public event OnStateChangeHandler OnStateChange;

        private GameState gameState;

        //accessor for current state
        public GameState GameState
        {
            get
            {
                return gameState;
            }

            set
            {
                gameState = value;
                //To stop errors
                if (OnStateChange != null) OnStateChange();
            }
        }

        public GameDifficulty GameDifficulty { get; set; }

        //singleton GameManager
        public static GameManager Instance
        {
            get; private set;
        }

        //Manage singleton and set resolution
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void OnApplicationQuit()
        {
            Instance = null;
        }

        //Load new level and sets state
        public void LoadLevel(string levelName, GameState newState)
        {
            StartCoroutine(LoadLevelAsync(levelName, newState));
        }
        private IEnumerator LoadLevelAsync(string levelName, GameState newState)
        {
            // Load level
            Debug.Log("Loading level: " + levelName); // Add this debug statement
            AsyncOperation operation = SceneManager.LoadSceneAsync(levelName);
            while (operation.isDone == false)
            {
                yield return null;
            }

            Debug.Log("Level loaded: " + levelName); // Add this debug statement

            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);

            // Update state
            GameState = newState;
        }

    }
}