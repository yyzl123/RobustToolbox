using System;
using System.Collections.Generic;
using Robust.Client.Graphics;
using Robust.Client.Interfaces.GameObjects.Components;
using Robust.Shared.Interfaces.GameObjects;
using Robust.Shared.Utility;

namespace Robust.Client.Animations
{
    /// <summary>
    ///     An animation track that plays RSI state animations manually, so they can be precisely controlled etc.
    /// </summary>
    public sealed class AnimationTrackSpriteFlick : AnimationTrack
    {
        /// <summary>
        ///     A list of key frames for when to fire flicks.
        /// </summary>
        public readonly List<KeyFrame> KeyFrames = new List<KeyFrame>();

        // TODO: Should this layer key be per keyframe maybe?
        /// <summary>
        ///     The layer key of the layer to flick on.
        /// </summary>
        public object LayerKey { get; set; }

        public override (int KeyFrameIndex, float FramePlayingTime) InitPlayback()
        {
            return (-1, 0);
        }

        public override (int KeyFrameIndex, float FramePlayingTime)
            AdvancePlayback(object context, int prevKeyFrameIndex, float prevPlayingTime, float frameTime)
        {
            var entity = (IEntity) context;
            var sprite = entity.GetComponent<ISpriteComponent>();

            var playingTime = prevPlayingTime + frameTime;
            var keyFrameIndex = prevKeyFrameIndex;
            // Advance to the correct key frame.
            while (keyFrameIndex != KeyFrames.Count - 1 && KeyFrames[keyFrameIndex + 1].KeyTime < playingTime)
            {
                playingTime -= KeyFrames[keyFrameIndex + 1].KeyTime;
                keyFrameIndex += 1;
            }

            if (keyFrameIndex >= 0)
            {
                var keyFrame = KeyFrames[keyFrameIndex];
                // Advance animation on current key frame.
                var rsi = sprite.LayerGetActualRSI(LayerKey);
                if (rsi.TryGetState(keyFrame.State, out var state))
                {
                    var animationTime = Math.Min(state.AnimationLength - 0.01f, playingTime);
                    sprite.LayerSetAutoAnimated(LayerKey, false);
                    // TODO: Doesn't setting the state explicitly reset the animation
                    // so it's slightly more inefficient?
                    sprite.LayerSetState(LayerKey, keyFrame.State);
                    sprite.LayerSetAnimationTime(LayerKey, animationTime);
                }
            }

            return (keyFrameIndex, playingTime);
        }

        public struct KeyFrame
        {
            /// <summary>
            ///     The RSI state to play when this keyframe gets triggered.
            /// </summary>
            public readonly RSI.StateId State;

            /// <summary>
            ///     The time between this keyframe and the last.
            /// </summary>
            public readonly float KeyTime;

            public KeyFrame(RSI.StateId state, float keyTime)
            {
                State = state;
                KeyTime = keyTime;
            }
        }
    }
}
