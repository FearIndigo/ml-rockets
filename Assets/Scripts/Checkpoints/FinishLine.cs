using FearIndigo.Ship;

namespace FearIndigo.Checkpoints
{
    public class FinishLine : Checkpoint
    {
        protected override void OnCheckpointAcquired(ShipController ship)
        {
            base.OnCheckpointAcquired(ship);
            ship.CheckpointAcquired(true); // Can be called twice as SetReward() is used not AddReward()
            
            ship.StopShip();
        }
    }
}