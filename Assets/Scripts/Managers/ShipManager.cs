using System.Collections.Generic;
using FearIndigo.Ship;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine;

namespace FearIndigo.Managers
{
    public class ShipManager : SubManager
    {
        public bool resetOnAllShipsStopped;
        public ShipController shipPrefab;
        public bool firstShipUseHeuristics;
        public int numShips = 1;
        
        [HideInInspector] public List<ShipController> ships = new List<ShipController>();

        public ShipController MainShip => _mainShipIndex < ships?.Count ? ships[_mainShipIndex] : null;

        private int _mainShipIndex = 0;
        private int _shipsStopped;

        /// <summary>
        /// <para>
        /// Spawn new ships at track start position.
        /// </para>
        /// </summary>
        public void SpawnShips()
        {
            if (ships.Count > numShips)
            {
                for (var i = numShips; i < ships.Count; i++)
                {
                    Destroy(ships[i].gameObject);
                }
                ships.RemoveRange(numShips, ships.Count - numShips);
            }
            
            var position = GameManager.trackManager.trackSpline.centreSpline.points[0];
            for (var i = 0; i < numShips; i++)
            {
                ShipController ship;
                if (i < ships.Count)
                {
                    ship = ships[i];
                }
                else
                {
                    ship = Instantiate(shipPrefab, transform);
                    ships.Add(ship);
                }

                ship.Init(i, firstShipUseHeuristics && i == 0);
                ship.Teleport(position);
                GameManager.checkpointManager.SetActiveCheckpoint(ship, 0);
            }

            SetMainShipIndex(0);
            _shipsStopped = 0;
        }

        /// <summary>
        /// <para>
        /// Notify that a ship has been stopped. Update main ship or reset when all ships have been stopped.
        /// </para>
        /// </summary>
        /// <param name="ship"></param>
        public void ShipStopped(ShipController ship)
        {
            _shipsStopped++;
            
            if (_shipsStopped == ships.Count)
            {
                if (resetOnAllShipsStopped)
                {
                    GameManager.Reset();
                }
                else
                {
                    FindObjectOfType<MainSceneManager>()?.RoundOver();
                }
            }
            else
            {
                ship.enabled = false;
                if (ship.index == _mainShipIndex)
                {
                    for (var i = 0; i < ships.Count; i++)
                    {
                        var otherShip = ships[i];
                        if (!otherShip.enabled) continue;

                        SetMainShipIndex(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Set the current main ship index.
        /// Sets camera target and updates the checkpoint visuals.
        /// </summary>
        /// <param name="i"></param>
        public void SetMainShipIndex(int i)
        {
            if(i >= ships.Count || i < 0) return;
            
            _mainShipIndex = i;
            GameManager.cameraManager.SetCameraTarget(MainShip.transform);
            GameManager.checkpointManager.SetActiveCheckpoint(MainShip, GameManager.checkpointManager.GetActiveCheckpointId(MainShip));
        }
    }
}

