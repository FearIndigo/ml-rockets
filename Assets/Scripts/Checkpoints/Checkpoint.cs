using UnityEngine;

namespace FearIndigo.Checkpoints
{
    public class Checkpoint : CheckpointBase
    {
        public SpriteRenderer spriteRenderer;
        public Color activeColor;
        public Color nextActiveColor;
        public Color inactiveColor;

        protected override void OnStateChanged()
        {
            spriteRenderer.color = state switch
            {
                State.Inactive => inactiveColor,
                State.NextActive => nextActiveColor,
                State.Active => activeColor,
                _ => Color.magenta
            };
        }
    }
}