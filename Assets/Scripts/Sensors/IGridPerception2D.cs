using UnityEngine;

namespace FearIndigo.Sensors
{
    /// <summary>
    /// An interface for GridSensor perception that defines the grid cells and collider detecting strategies.
    /// </summary>
    internal interface IGridPerception2D
    {
        LayerMask ColliderMask
        {
            get;
            set;
        }

        /// <summary>Converts the index of the cell to the 2D point (z is zero) relative to grid center</summary>
        /// <returns>Vector2 of the position of the center of the cell relative to grid center</returns>
        /// <param name="cellIndex">The index of the cell</param>
        Vector2 GetCellLocalPosition(int cellIndex);

        /// <summary>
        /// Converts the index of the cell to the 2D point (z is zero) in world space
        /// based on the result from GetCellLocalPosition()
        /// </summary>
        /// <returns>Vector2 of the position of the center of the cell in world space</returns>
        /// <param name="cellIndex">The index of the cell</param>
        Vector2 GetCellGlobalPosition(int cellIndex);

        /// <summary>
        /// Perceive the latest grid status. Detect colliders for each cell, parse the collider arrays,
        /// then trigger registered sensors to encode and update with the new grid status.
        /// </summary>
        void Perceive();

        /// <summary>
        /// Same as Perceive(), but only load data for debug gizmo.
        /// </summary>
        void UpdateGizmo();

        /// <summary>
        /// Register a sensor to this GridPerception to receive the grid perception results.
        /// When the GridPerception perceive a new observation, registered sensors will be triggered
        /// to encode the new observation and update its data.
        /// </summary>
        void RegisterSensor(GridSensor2DBase sensor);

        /// <summary>
        /// Register an internal debug sensor.
        /// Debug sensors will only be triggered when drawing debug gizmos.
        /// </summary>
        void RegisterDebugSensor(GridSensor2DBase debugSensor);
    }
}
