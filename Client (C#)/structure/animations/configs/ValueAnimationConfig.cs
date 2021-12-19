using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app.structure.animations.configs
{
    public class ValueAnimationConfig<ValueType> : AnimationConfig
    {
        public bool useFromValue { get; } = false;
        public ValueType toValue { get; }
        public ValueType fromValue { get; }


        public ValueAnimationConfig(TimeSpan duration, ValueType toValue, AnimationEvent onEnd = null, AnimationEvent onStart = null) : base(duration, onEnd, onStart)
        {
            this.toValue = toValue;
        }

        public ValueAnimationConfig(TimeSpan duration, ValueType toValue, ValueType fromValue, AnimationEvent onEnd = null, AnimationEvent onStart = null) : base(duration, onEnd, onStart)
        {
            this.toValue = toValue;
            this.fromValue = fromValue;
            useFromValue = true;
        }

        public new ValueAnimationConfig<ValueType> forceKeepValue()
        {
            base.forceKeepValue();
            return this;
        }
    }
}
