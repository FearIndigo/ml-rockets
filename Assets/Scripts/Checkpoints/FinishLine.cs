using FearIndigo.Ship;
using UnityEngine;

namespace FearIndigo.Checkpoints
{
    public class FinishLine : Checkpoint
    {
        protected override void OnCheckpointAcquired(ShipController ship)
        {
            base.OnCheckpointAcquired(ship);
            
            GameManager.shipManager.StopShip(ship);
            
            Debug.Log("Crossed the finish line!");
            
            ship.EndEpisode();
        }
    }
}