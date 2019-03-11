using FireFly.VI.SLAM;
using FireFly.VI.SLAM.Sophus;
using System;

namespace FireFly.Proxy
{
    public enum SlamPublishType : byte
    {
        Frame = 1,
        KeyframeWithPoints = 2,
        Reset = 3
    }

    public class SlamMapEventData : AbstractProxyEventData
    {
        private Frame _Frame;
        private KeyFrame _KeyFrame;
        private SlamPublishType _PublishType;

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

        public SlamPublishType PublishType
        {
            get
            {
                return _PublishType;
            }

            set
            {
                _PublishType = value;
            }
        }

        public static SlamMapEventData Parse(byte[] data)
        {
            SlamMapEventData obj = new SlamMapEventData();

            obj.PublishType = (SlamPublishType)data[0];

            if (obj.PublishType == SlamPublishType.Frame || obj.PublishType == SlamPublishType.KeyframeWithPoints)
            {
                int sizeFrame = ParseFrame(data, 1, obj);
                if (obj.PublishType == SlamPublishType.KeyframeWithPoints)
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
            }

            return obj;
        }

        private static int ParseVector3(byte[] data, int offset, Vector3 obj)
        {
            int index = offset;
            obj.X = BitConverter.ToDouble(data, index);
            index += 8;
            obj.Y = BitConverter.ToDouble(data, index);
            index += 8;
            obj.Z = BitConverter.ToDouble(data, index);
            index += 8;

            return index;
        }

        private static int ParseFrame(byte[] data, int offset, SlamMapEventData obj)
        {
            int index = offset;
            uint id = BitConverter.ToUInt32(data, index);
            index += 4;
            double time = BitConverter.ToDouble(data, index);
            index += 8;

            SE3 Tcw = new SE3();
            SE3 Tbw = new SE3();
            Vector3 v = new Vector3();
            Vector3 bg = new Vector3();
            Vector3 ba = new Vector3();

            index = ParseSE3(data, index, Tcw);
            index = ParseSE3(data, index, Tbw);

            index = ParseVector3(data, index, v);
            index = ParseVector3(data, index, bg);
            index = ParseVector3(data, index, ba);

            double scale = BitConverter.ToDouble(data, index);
            index += 8;

            obj._Frame = new Frame(id, time, new Sim3(1, Tcw), Tbw, v, bg, ba, scale);
            return index - offset;
        }

        private static int ParseKeyFrame(byte[] data, int offset, SlamMapEventData obj)
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

        private static int ParseSE3(byte[] data, int offset, SE3 obj)
        {
            int index = offset;
            obj.Translation.X = BitConverter.ToDouble(data, index);
            index += 8;
            obj.Translation.Y = BitConverter.ToDouble(data, index);
            index += 8;
            obj.Translation.Z = BitConverter.ToDouble(data, index);
            index += 8;
            double q1 = BitConverter.ToDouble(data, index);
            index += 8;
            double q2 = BitConverter.ToDouble(data, index);
            index += 8;
            double q3 = BitConverter.ToDouble(data, index);
            index += 8;
            double q4 = BitConverter.ToDouble(data, index);
            index += 8;

            obj.SO3.Quaternion = new Quaternion(q1, q2, q3, q4);
            return index;
        }
    }
}