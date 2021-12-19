using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace app.structure.animations.configs
{
    public class AnimationConfig
    {
        public delegate void AnimationEvent(FrameworkElement component);
        public TimeSpan duration { get; }
        public AnimationEvent onEnd { get; }
        public AnimationEvent onStart { get; }

        public FillBehavior behavior = FillBehavior.Stop;

        public AnimationConfig(TimeSpan duration, AnimationEvent onEnd = null, AnimationEvent onStart = null)
        {
            this.duration = duration;
            this.onEnd = onEnd;
            this.onStart = onStart;
        }

        public AnimationConfig forceKeepValue()
        {
            behavior = FillBehavior.HoldEnd;
            return this;
        }

    }

}
