using app.structure.animations.configs;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Animation;

namespace app.structure.animations
{
    public abstract class Animation<AnimationType, EffectedValue, ConfigType> where AnimationType : AnimationTimeline where ConfigType : AnimationConfig
    {
        protected AnimationType animation;
        protected Dictionary<FrameworkElement, AnimationComponentConfig<EffectedValue>> componentConfigs = new Dictionary<FrameworkElement, AnimationComponentConfig<EffectedValue>>();

        protected abstract DependencyProperty getProperty();
        protected abstract EffectedValue getPropertyValue(FrameworkElement component);

        protected abstract void initializeAnimation();

        protected abstract void applyAnimation(FrameworkElement component, ConfigType config);

        protected abstract void onAnimationEnded(FrameworkElement component, ConfigType config);

        public void start(FrameworkElement component, ConfigType config)
        {
            stop(component);

            if (!componentConfigs.ContainsKey(component))
            {
                componentConfigs.Add(component, new AnimationComponentConfig<EffectedValue>(getPropertyValue(component)));
            }

            AnimationStatus status = componentConfigs[component].status;

            if (status == AnimationStatus.NONE)
            {
                config.onStart?.Invoke(component);
                initializeAnimation();
                applyAnimation(component, config);
                animation.FillBehavior = config.behavior;
                animation.Duration = config.duration;
                componentConfigs[component].end = delegate (object sender, EventArgs e) {
                    componentConfigs[component].status = AnimationStatus.NONE;
                    onAnimationEnded(component, config);
                    config.onEnd?.Invoke(component);
                };
                animation.Completed += componentConfigs[component].end;
                componentConfigs[component].status = AnimationStatus.RUNNING;
                component.BeginAnimation(getProperty(), animation);
            }

        }

        public void stop(FrameworkElement component)
        {
            if (componentConfigs.ContainsKey(component))
            {
                if(animation != null)
                {
                    animation.Completed -= componentConfigs[component].end;
                }
                componentConfigs[component].status = AnimationStatus.NONE;
                component.BeginAnimation(getProperty(), null);
                componentConfigs.Remove(component);
            }
        }

    }

    public enum AnimationStatus
    {
        NONE,
        RUNNING
    }

    public class AnimationComponentConfig<T>
    {
        public AnimationStatus status = AnimationStatus.NONE;
        public T originValue { get; }

        public EventHandler end;

        public AnimationComponentConfig(T originValue)
        {
            this.originValue = originValue;
        }
    }
   
    public class AnimationElement<Type>
    {
        public Type To;
        public Type From;
    }
}
