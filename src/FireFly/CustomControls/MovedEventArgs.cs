namespace FireFly.CustomControls
{
    public class MovedEventArgs
    {
        #region Fields

        private double _DeltaX;
        private double _DeltaY;

        #endregion Fields

        #region Properties

        public double DeltaX
        {
            get
            {
                return _DeltaX;
            }

            set
            {
                _DeltaX = value;
            }
        }

        public double DeltaY
        {
            get
            {
                return _DeltaY;
            }

            set
            {
                _DeltaY = value;
            }
        }

        #endregion Properties
    }
}