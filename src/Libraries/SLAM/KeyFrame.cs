using System.Collections.Generic;

namespace FireFly.VI.SLAM
{
    public class KeyFrame : Frame
    {
        private int _KfId;
        private List<Point> _Points = new List<Point>();

        public int KfId
        {
            get
            {
                return _KfId;
            }

            set
            {
                _KfId = value;
            }
        }

        public List<Point> Points
        {
            get
            {
                return _Points;
            }

            set
            {
                _Points = value;
            }
        }
    }
}