using System.Collections.Generic;
using FearIndigo.Ship;
using Unity.MLAgents.Policies;
using UnityEngine;

namespace FearIndigo.Managers
{
    public class ShipManager : SubManager
    {
        public ShipController shipPrefab;
        public bool firstShipUseHeuristics;
        public int numShips = 1;
        
        [HideInInspector] public List<ShipController> ships = new List<ShipController>();

        private int shipsStopped;

        /// <summary>
        /// <para>
        /// Spawn new ships at track start position.
        /// </para>
        /// </summary>
        public void SpawnShips()
        {
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
                
                ship.Teleport(position);
                ship.SetBehaviourType((firstShipUseHeuristics && i == 0) ?
                    BehaviorType.HeuristicOnly :
                    BehaviorType.Default);
                GameManager.checkpointManager.SetActiveCheckpoint(ship, 0);
            }

            shipsStopped = 0;
        }

        /// <summary>
        /// <para>
        /// Stop the ship. Reset when all ships have been stopped.
        /// </para>
        /// </summary>
        /// <param name="ship"></param>
        public void StopShip(ShipController ship)
        {
            shipsStopped++;
            
            if (shipsStopped == ships.Count)
            {
                foreach (var oldShip in ships)
                {
                    oldShip.gameObject.SetActive(true);
                    oldShip.EndEpisode();
                }
                
                GameManager.Reset();
            }
            else
            {
                ship.gameObject.SetActive(false);
            }
        }
    }
}
