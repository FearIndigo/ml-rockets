using System.Collections.Generic;
using FearIndigo.Checkpoints;
using FearIndigo.Ship;
using UnityEngine;

namespace FearIndigo.Managers
{
    public class CheckpointManager : SubManager
    {
        public Checkpoint checkpointPrefab;
        public FinishLine finishLinePrefab;
        
        [HideInInspector] public Dictionary<ShipController, Dictionary<int, float>> shipCheckpointSplits = new Dictionary<ShipController, Dictionary<int, float>>();
        [HideInInspector] public Dictionary<ShipController, int> shipActiveCheckpointIds = new Dictionary<ShipController, int>();
        [HideInInspector] public List<CheckpointBase> checkpoints = new List<CheckpointBase>();

        public Vector2 ActiveCheckpointDirection(ShipController ship)
        {
            return (Vector2)GetActiveCheckpoint(ship).transform.position - ship.rb.position;
        }

        public Vector2 NextActiveCheckpointDirection(ShipController ship)
        {
            var activeCheckpointId = GetActiveCheckpointId(ship);
            return activeCheckpointId < checkpoints.Count - 1
                ? (Vector2)checkpoints[activeCheckpointId + 1].transform.position - ship.rb.position
                : Vector2.zero;
        }

        public Vector2 PreviousActiveCheckpointDirection(ShipController ship)
        {
            var activeCheckpointId = GetActiveCheckpointId(ship);
            return (Vector2)checkpoints[(checkpoints.Count + activeCheckpointId - 1) % checkpoints.Count].transform.position - ship.rb.position;
        }
        public CheckpointBase GetActiveCheckpoint(ShipController ship) => checkpoints[GetActiveCheckpointId(ship)];

        public int GetActiveCheckpointId(ShipController ship)
        {
            if (!shipActiveCheckpointIds.TryGetValue(ship, out var activeCheckpointId))
            {
                shipActiveCheckpointIds.Add(ship, activeCheckpointId);
            }
            return activeCheckpointId;
        }
        
        ///<summary>
        /// <para>
        /// Create checkpoints.
        /// </para>
        /// </summary>
        public void CreateCheckpoints()
        {
            foreach (var checkpoint in checkpoints)
            {
                Destroy(checkpoint.gameObject);
            }
            
            shipCheckpointSplits.Clear();
            shipActiveCheckpointIds.Clear();
            checkpoints.Clear();
            
            var positions = GameManager.trackManager.GetCentreSplinePoints();
            for (var i = 0; i < positions.Length - 1; i++)
            {
                var checkpoint = Instantiate(checkpointPrefab, transform);
                var posIndex = i + 1;
                var position = positions[posIndex];
                checkpoint.Init(i, position);
                checkpoint.UpdateLine(GameManager.trackManager.GetLeftSplinePoint(posIndex) - position, GameManager.trackManager.GetRightSplinePoint(posIndex) - position);
                checkpoints.Add(checkpoint);
            }
            var finishLine = Instantiate(finishLinePrefab, transform);
            finishLine.Init(positions.Length - 1, positions[0]);
            finishLine.UpdateLine(GameManager.trackManager.GetLeftSplinePoint(0) - positions[0], GameManager.trackManager.GetRightSplinePoint(0) - positions[0]);
            checkpoints.Add(finishLine);
        }

        /// <summary>
        /// <para>
        /// Sets the active checkpoint, the next active checkpoint and deactivates the previously active checkpoint; for
        /// the provided ship.
        /// </para>
        /// </summary>
        /// <param name="ship"></param>
        /// <param name="checkpointId"></param>
        public void SetActiveCheckpoint(ShipController ship, int checkpointId)
        {
            var mainShip = ship == GameManager.shipManager.ships[0];
            
            var activeCheckpointId = GetActiveCheckpointId(ship);
            if(mainShip) checkpoints[activeCheckpointId].SetState(CheckpointBase.State.Inactive);
            
            if (checkpointId >= checkpoints.Count) return;
            if(mainShip) checkpoints[checkpointId].SetState(CheckpointBase.State.Active);
            shipActiveCheckpointIds[ship] = checkpointId;
            
            if (checkpointId >= checkpoints.Count - 1) return;
            if(mainShip) checkpoints[checkpointId + 1].SetState(CheckpointBase.State.NextActive);
        }

        /// <summary>
        /// <para>
        /// Set the timer split for the checkpointId.
        /// </para>
        /// </summary>
        /// <param name="ship"></param>
        /// <param name="checkpointId"></param>
        public void UpdateCheckpointSplit(ShipController ship, int checkpointId)
        {
            if (!shipCheckpointSplits.TryGetValue(ship, out var checkpointSplits))
            {
                checkpointSplits = new Dictionary<int, float>();
                shipCheckpointSplits.Add(ship, checkpointSplits);
            }
            checkpointSplits.Add(checkpointId, GameManager.timerManager.timer);
        }

        private void OnDrawGizmos()
        {
            DrawCheckpointGizmos();
        }

        ///<summary>
        /// <para>
        /// Draw checkpoint gizmos.
        /// </para>
        /// </summary>
        private void DrawCheckpointGizmos()
        {
            var origin = (Vector2)transform.localPosition;
            var points = GameManager.trackManager.GetCentreSplinePoints();
            foreach (var point in points)
            {
                Gizmos.DrawSphere(((Vector2)point + origin), 1f);
            }
        }
    }
}

