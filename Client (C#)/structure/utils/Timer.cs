using System;
using System.Windows.Threading;

namespace app.structure.utils
{
    public class Timer
    {
        public delegate void TimerAction(int tickCount, Timer timer);

        int tickCount = 1;
        DispatcherTimer timer = new DispatcherTimer();
        TimerAction action;
        TimerConfig config;

        public static Timer SetTimeout(TimerConfig.onTickEnd onEnd, TimeSpan tickDelay)
        {
            Timer t = new Timer(null, tickDelay, new TimerConfig().setTickEndConfig(1, onEnd));
            t.start();

            return t;
        }

        public Timer(TimerAction action, TimeSpan tickDelay, TimerConfig config = null)
        {
            this.action = action;
            timer.Interval = tickDelay;
            timer.Tick += onTick;
            this.config = config;
        }

        private void onTick(object sender, EventArgs e)
        {

            action?.Invoke(tickCount++, this);
            if (config != null && config.stopAtTickCount == tickCount)
            {
                stop();
                config.end?.Invoke();
            }

            bool resetByCountRange = config != null && tickCount - config.tickCountRange == 1;
            bool resetByDefault = config == null && tickCount > 99;

            if (resetByCountRange || resetByDefault)
            {
                tickCount = 1;
            }
        }

        public void changeTickDelay(TimeSpan tickDelay)
        {
            timer.Interval = tickDelay;
        }

        public void start()
        {
            if (!timer.IsEnabled)
            {
                timer.Start();
            }
        }
        
        public void stop()
        {
            tickCount = 1;
            timer.Stop();
        }

        public bool isRunning()
        {
            return timer.IsEnabled;
        }
    }

    public class TimerConfig
    {
        public delegate void onTickEnd();

        public int stopAtTickCount;
        public int tickCountRange;

        public onTickEnd end;

        public TimerConfig()
        {
            stopAtTickCount = -1;
            tickCountRange = -1;
            end = null;
        }

        public TimerConfig setTickCountRange(int value)
        {
            tickCountRange = value;
            return this;
        }

        public TimerConfig setTickEndConfig(int stopAtTickCount, onTickEnd end = null)
        {
            this.stopAtTickCount = stopAtTickCount;
            this.end = end;
            return this;
        }
        

    }
}
