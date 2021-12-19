using app.structure.utils;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace app.structure.animations
{
    public class RotateAnimation
    {
        public delegate void OnRotationEnd();

        private FrameworkElement element;
        private bool rotateOnce = false;
        private double rotateTo = 360;
        private TimeSpan time = TimeSpan.FromMilliseconds(600);

        private RotateTransform rotateTransform;
        private DoubleAnimation doubleAnimation;

        public OnRotationEnd onEnd;

        public RotateAnimation(FrameworkElement element, bool rotateOnce = false)
        {
            this.element = element;
            this.rotateOnce = rotateOnce;
        }
        public RotateAnimation(FrameworkElement element, bool rotateOnce, double rotateTo)
        {
            this.element = element;
            this.rotateTo = rotateTo;
            this.rotateOnce = rotateOnce;
        }

        public RotateAnimation(FrameworkElement element, bool rotateOnce, double rotateTo, TimeSpan time)
        {
            this.element = element;
            this.time = time;
            this.rotateTo = rotateTo;
            this.rotateOnce = rotateOnce;
        }

        public bool isRotating
        {
            get
            {
                return rotateTransform != null;
            }
        }

        public void start()
        {
            if (rotateTransform == null)
            {
                rotateTransform = new RotateTransform();
                doubleAnimation = new DoubleAnimation(0, rotateTo, new Duration(time));
                doubleAnimation.FillBehavior = FillBehavior.HoldEnd;

                element.RenderTransform = rotateTransform;
                element.RenderTransformOrigin = new Point(0.5, 0.5);

                if (!rotateOnce)
                {
                    doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
                }

                doubleAnimation.Completed += onEnded;

                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, doubleAnimation);
            }
        }

        private void onEnded(object sender, EventArgs e)
        {
            onEnd();
        }

        public void stop()
        {
            if (rotateTransform != null)
            {
                doubleAnimation.Completed -= onEnded;
                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
                rotateTransform = null;
                doubleAnimation = null;
            }
        }

    }
}
