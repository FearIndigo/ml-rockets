using FearIndigo.Ship;
using FearIndigo.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace FearIndigo.Managers
{
    public class MainSceneManager : MonoBehaviour
    {
        public GameManager gameManager;
        public UIDocument uiDocument;
        public TimerUi timerUi;
        public CheckpointSplitsUi checkpointSplitsUi;
        public MainMenuUi mainMenuUi;
        public RandomSeedUi randomSeedUi;
        public MusicUi musicUi;

        public bool menuIsOpen;
        public bool isPaused;
        public bool roundOver;

        private void Start()
        {
            OpenPauseMenu(menuIsOpen);
            SetSinglePlayerMode();
        }

        public void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OpenPauseMenu(!menuIsOpen);
            }
            else if (isPaused && !menuIsOpen && Input.anyKeyDown)
            {
                if (roundOver)
                {
                    roundOver = false;
                    gameManager.Reset();
                }
                else
                {
                    SetPaused(false);
                }
            }
        }

        public void OpenPauseMenu(bool open)
        {
            menuIsOpen = open;
            SetPaused(true);

            timerUi.enabled = !open;
            checkpointSplitsUi.enabled = !open;
            mainMenuUi.enabled = open;
            randomSeedUi.enabled = open;
            musicUi.enabled = open;
        }

        public void RoundOver()
        {
            SetPaused(true);
            roundOver = true;
        }
        
        public void SetPaused(bool paused)
        {
            isPaused = paused;
            Time.timeScale = isPaused ? 0f : 1f;

            foreach (var ship in gameManager.shipManager.ships)
            {
                ship.PlayThrust(false);
            }
        }
        
        public void SetSinglePlayerMode()
        {
            gameManager.shipManager.numShips = 1;
            gameManager.shipManager.firstShipUseHeuristics = true;
            gameManager.Reset();
            roundOver = false;
        }
        
        public void SetAiOnlyMode()
        {
            gameManager.shipManager.numShips = 1;
            gameManager.shipManager.firstShipUseHeuristics = false;
            gameManager.Reset();
            roundOver = false;
        }
        
        public void SetHumanVsAiMode()
        {
            gameManager.shipManager.numShips = 2;
            gameManager.shipManager.firstShipUseHeuristics = true;
            gameManager.Reset();
            roundOver = false;
        }
    }
}
