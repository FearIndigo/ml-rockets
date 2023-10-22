using FearIndigo.Ship;

namespace FearIndigo.Checkpoints
{
    public class FinishLine : Checkpoint
    {
        protected override void OnCheckpointAcquired(ShipController ship)
        {
            base.OnCheckpointAcquired(ship);
            
            ship.StopShip();
        }
    }
}