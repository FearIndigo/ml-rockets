namespace FearIndigo.Sensors
{
    /// <summary>
    /// A custom component for 2D Ray Perception.
    /// </summary>
    public class CustomRayPerceptionSensorComponent2D : CustomRayPerceptionSensorComponentBase
    {
        /// <inheritdoc/>
        public override CustomRayPerceptionCastType GetCastType()
        {
            return CustomRayPerceptionCastType.Cast2D;
        }
    }
}