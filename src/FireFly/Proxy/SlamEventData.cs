using FireFly.VI.SLAM;
using System;

namespace FireFly.Proxy
{
    public enum SlamPublishType : byte
    {
        Frame = 1,
        KeyframeWithPoints = 2
    }

    public class SlamEventData : AbstractProxyEventData
    {
        private Frame _Frame;
        private KeyFrame _KeyFrame;

        public Frame Frame
        {
            get
            {
                return _Frame;
            }

            set
            {
                _Frame = value;
            }
        }

        public KeyFrame KeyFrame
        {
            get
            {
                return _KeyFrame;
            }

            set
            {
                _KeyFrame = value;
            }
        }

        internal static SlamEventData Parse(byte[] data)
        {
            SlamEventData obj = new SlamEventData();

            SlamPublishType type = (SlamPublishType)data[0];
            int sizeFrame = ParseFrame(data, 1, obj);
            if (type == SlamPublishType.KeyframeWithPoints)
            {
                int sizeKeyFrame = ParseKeyFrame(data, 1 + sizeFrame, obj);
                obj.KeyFrame.Frame = obj.Frame;
                int sizePoint = 0;
                for (int i = 0; i < obj.KeyFrame.Points.Count; i++)
                {
                    Point p = new Point();
                    sizePoint = ParsePoint(data, 1 + sizeFrame + sizeKeyFrame + sizePoint * i, p);
                    obj.KeyFrame.Points[i] = p;
                }
            }

            return obj;
        }

        private static int ParseFrame(byte[] data, int offset, SlamEventData obj)
        {
            int index = offset;
            uint id = BitConverter.ToUInt32(data, index);
            index += 4;
            double tx = BitConverter.ToDouble(data, index);
            index += 8;
            double ty = BitConverter.ToDouble(data, index);
            index += 8;
            double tz = BitConverter.ToDouble(data, index);
            index += 8;
            double q1 = BitConverter.ToDouble(data, index);
            index += 8;
            double q2 = BitConverter.ToDouble(data, index);
            index += 8;
            double q3 = BitConverter.ToDouble(data, index);
            index += 8;
            double q4 = BitConverter.ToDouble(data, index);
            index += 8;
            double s = BitConverter.ToDouble(data, index);
            index += 8;

            obj._Frame = new Frame(id, tx, ty, tz, q1, q2, q3, q4, s);
            return index - offset;
        }

        private static int ParseKeyFrame(byte[] data, int offset, SlamEventData obj)
        {
            int index = offset;
            uint id = BitConverter.ToUInt32(data, index);
            index += 4;
            int points = BitConverter.ToInt32(data, index);
            index += 4;
            double fx = BitConverter.ToDouble(data, index);
            index += 8;
            double fy = BitConverter.ToDouble(data, index);
            index += 8;
            double cx = BitConverter.ToDouble(data, index);
            index += 8;
            double cy = BitConverter.ToDouble(data, index);
            index += 8;

            obj._KeyFrame = new KeyFrame(id, fx, fy, cx, cy, points, obj.Frame);
            return index - offset;
        }

        private static int ParsePoint(byte[] data, int offset, Point obj)
        {
            int index = offset;
            float u = BitConverter.ToSingle(data, index);
            index += 4;
            float v = BitConverter.ToSingle(data, index);
            index += 4;
            float inverseDepth = BitConverter.ToSingle(data, index);
            index += 4;
            Array.Copy(data, index, obj.Colors, 0, 8);
            index += 8;

            obj.U = u;
            obj.V = v;
            obj.InverseDepth = inverseDepth;

            return index - offset;
        }
    }
}