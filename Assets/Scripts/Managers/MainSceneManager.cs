using System.Collections.Generic;
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

        public List<MonoBehaviour> gameplayUis;
        public List<MonoBehaviour> menuUis;
        public MonoBehaviour continueButtonUi;

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
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                OpenPauseMenu(!menuIsOpen);
            }
            else if (isPaused && !roundOver && !menuIsOpen && Input.anyKeyDown)
            {
                SetPaused(false);
            }
        }

        public void OpenPauseMenu(bool open)
        {
            menuIsOpen = open;
            SetPaused(true);

            foreach (var ui in gameplayUis)
            {
                ui.enabled = !open;
            }

            foreach (var ui in menuUis)
            {
                ui.enabled = open;
            }
        }

        public void Continue()
        {
            SetRoundOver(false);
            SetPaused(true);
            gameManager.Reset();
        }
        
        public void ResetCurrentTrack()
        {
            SetRoundOver(false);
            SetPaused(true);
            gameManager.ResetCurrent();
        }

        public void SetRoundOver(bool over = true)
        {
            SetPaused(true);
            roundOver = over;
            
            if (!menuIsOpen)
            {
                continueButtonUi.enabled = over;
            }
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
