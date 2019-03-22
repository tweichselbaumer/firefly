using FireFly.VI.SLAM.Sophus;
using MatFileHandler;
using System.Collections.Generic;
using System.IO;

namespace FireFly.VI.SLAM.Data
{
    public class VIMatlabImporter
    {
        private string _FileName;
        private IMatFile _MatFile;

        public VIMatlabImporter(string filename)
        {
            _FileName = filename;
        }

        public void Close()
        {
        }

        public List<Frame> GetFrames()
        {
            List<Frame> frames = new List<Frame>();

            IStructureArray dataStruct = _MatFile["data"].Value as IStructureArray;

            IArray timePre = (dataStruct["PreOptimization", 0] as IStructureArray)["time", 0];

            IArray idPre = (dataStruct["PreOptimization", 0] as IStructureArray)["id", 0];

            IArray T_cam_worldPre = (dataStruct["PreOptimization", 0] as IStructureArray)["T_cam_world", 0];

            IArray T_base_worldPre = (dataStruct["PreOptimization", 0] as IStructureArray)["T_base_world", 0];

            IArray velocityPre = (dataStruct["PreOptimization", 0] as IStructureArray)["velocity", 0];

            IArray bgPre = (dataStruct["PreOptimization", 0] as IStructureArray)["bias_gyroscope", 0];
            IArray baPre = (dataStruct["PreOptimization", 0] as IStructureArray)["bias_accelerometer", 0];

            IArray scalePre = (dataStruct["PreOptimization", 0] as IStructureArray)["scale", 0];

            for (int i = 0; i < timePre.Dimensions[1]; i++)
            {
                double[,] matrixTcw = new double[4, 4];
                double[,] matrixTbw = new double[4, 4];
                Vector3 v = new Vector3();
                Vector3 bg = new Vector3();
                Vector3 ba = new Vector3();

                for (int j = 0; j < 3; j++)
                {
                    v.Vector[j] = ((IArrayOf<double>)velocityPre)[j, i];
                    bg.Vector[j] = ((IArrayOf<double>)bgPre)[j, i];
                    ba.Vector[j] = ((IArrayOf<double>)baPre)[j, i];
                }

                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        matrixTcw[j, k] = ((IArrayOf<double>)T_cam_worldPre)[j, k, i];
                        matrixTbw[j, k] = ((IArrayOf<double>)T_base_worldPre)[j, k, i];
                    }
                }

                SE3 Tcw = new SE3(matrixTcw);
                SE3 Tbw = new SE3(matrixTbw);

                Frame frame = new Frame(((IArrayOf<uint>)idPre)[0, i], ((IArrayOf<double>)timePre)[0, i], Tcw, Tbw, v, bg, ba, ((IArrayOf<double>)scalePre)[0, i]);
                frames.Add(frame);
            }

            return frames;
        }

        public List<KeyFrame> GetKeyFrames()
        {
            List<KeyFrame> keyFrames = new List<KeyFrame>();

            IStructureArray dataStruct = _MatFile["data"].Value as IStructureArray;

            IArray timePost = (dataStruct["PostOptimization", 0] as IStructureArray)["time", 0];

            IArray idPost = (dataStruct["PostOptimization", 0] as IStructureArray)["id", 0];

            IArray T_cam_worldPost = (dataStruct["PostOptimization", 0] as IStructureArray)["T_cam_world", 0];

            IArray T_base_worldPost = (dataStruct["PostOptimization", 0] as IStructureArray)["T_base_world", 0];

            IArray velocityPost = (dataStruct["PostOptimization", 0] as IStructureArray)["velocity", 0];

            IArray bgPost = (dataStruct["PostOptimization", 0] as IStructureArray)["bias_gyroscope", 0];
            IArray baPost = (dataStruct["PostOptimization", 0] as IStructureArray)["bias_accelerometer", 0];

            IArray kfidPost = (dataStruct["PostOptimization", 0] as IStructureArray)["kfid", 0];

            IArray scalePost = (dataStruct["PostOptimization", 0] as IStructureArray)["scale", 0];

            IArray fxPost = (dataStruct["PostOptimization", 0] as IStructureArray)["fx", 0];
            IArray fyPost = (dataStruct["PostOptimization", 0] as IStructureArray)["fy", 0];
            IArray cxPost = (dataStruct["PostOptimization", 0] as IStructureArray)["cx", 0];
            IArray cyPost = (dataStruct["PostOptimization", 0] as IStructureArray)["cy", 0];

            for (int i = 0; i < timePost.Dimensions[1]; i++)
            {
                IStructureArray kfstruct = ((dataStruct["PostOptimization", 0] as IStructureArray)["point_cloud", 0] as IStructureArray)["keyframe", i] as IStructureArray;

                int points = (kfstruct["u", 0] as IArrayOf<double>).Dimensions[1];

                double[,] matrixTcw = new double[4, 4];
                double[,] matrixTbw = new double[4, 4];
                Vector3 v = new Vector3();
                Vector3 bg = new Vector3();
                Vector3 ba = new Vector3();

                for (int j = 0; j < 3; j++)
                {
                    v.Vector[j] = ((IArrayOf<double>)velocityPost)[j, i];
                    bg.Vector[j] = ((IArrayOf<double>)bgPost)[j, i];
                    ba.Vector[j] = ((IArrayOf<double>)baPost)[j, i];
                }

                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        matrixTcw[j, k] = ((IArrayOf<double>)T_cam_worldPost)[j, k, i];
                        matrixTbw[j, k] = ((IArrayOf<double>)T_base_worldPost)[j, k, i];
                    }
                }

                SE3 Tcw = new SE3(matrixTcw);
                SE3 Tbw = new SE3(matrixTbw);

                Frame frame = new Frame(((IArrayOf<uint>)idPost)[0, i], ((IArrayOf<double>)timePost)[0, i], Tcw, Tbw, v, bg, ba, ((IArrayOf<double>)scalePost)[0, i]);
                KeyFrame keyFrame = new KeyFrame(((IArrayOf<uint>)kfidPost)[0, i], ((IArrayOf<double>)fxPost)[0, i], ((IArrayOf<double>)fxPost)[0, i], ((IArrayOf<double>)fxPost)[0, i], ((IArrayOf<double>)fxPost)[0, i], points, frame);

                for (int j = 0; j < points; j++)
                {
                    keyFrame.Points[j] = new Point();
                    keyFrame.Points[j].U = (kfstruct["u", 0] as IArrayOf<double>)[0, j];
                    keyFrame.Points[j].V = (kfstruct["v", 0] as IArrayOf<double>)[0, j];
                    keyFrame.Points[j].InverseDepth = (kfstruct["inverse_depth", 0] as IArrayOf<double>)[0, j];
                    for (int k = 0; k < 8; k++)
                    {
                        keyFrame.Points[j].Colors[k] = (kfstruct["colors", 0] as IArrayOf<byte>)[k, j];
                    }
                }

                keyFrames.Add(keyFrame);
            }

            return keyFrames;
        }

        public void Open()
        {
            using (var fileStream = new FileStream(_FileName, FileMode.Open))
            {
                var reader = new MatFileReader(fileStream);
                _MatFile = reader.Read();
            }
        }
    }
}