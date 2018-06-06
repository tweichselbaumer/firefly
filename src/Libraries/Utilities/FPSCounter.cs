using System.Collections.Generic;
using System.Linq;

namespace FireFly.Utilities
{
    public class FPSCounter
    {
        private List<long> _List = new List<long>();
        private System.Timers.Timer _Timer;
        private long current;

        public FPSCounter()
        {
            _List.AddRange(new long[5].ToList());

            _Timer = new System.Timers.Timer(1000);
            _Timer.Elapsed += _Timer_Elapsed;
            _Timer.Start();
        }

        public double FramesPerSecond
        {
            get
            {
                double result;
                lock (_List)
                {
                    result = (_List.Sum() + current) / (_List.Count + 1);
                }
                return result;
            }
        }

        public void CountFrame()
        {
            current++;
        }

        private void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (_List)
            {
                _List.Add(current);
                current = 0;
                _List.RemoveAt(0);
            }
        }
    }
}