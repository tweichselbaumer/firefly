using FireFly.Utilities;
using MatFileHandler;
using System.Collections.Generic;
using System.IO;

namespace FireFly.VI.SLAM.Data
{
    public class VIMatlabExporter
    {
        private DataBuilder _DataBuilder;
        private IStructureArray _DataStruct;
        private string _FileName;

        public VIMatlabExporter(string filename)
        {
            _FileName = filename;
        }

        public void Close()
        {
            using (Stream stream = new FileStream(_FileName, FileMode.Create))
            {
                var writer = new MatFileWriter(stream);
                writer.Write(_DataBuilder.NewFile(new[] { _DataBuilder.NewVariable("data", _DataStruct) }));
            }
        }

        public void ExportMap(Map map)
        {
            List<KeyFrame> keyFrames = map.KeyFrames;
            List<Frame> frames = map.Frames;

            IArray timePre = (_DataStruct["PreOptimization", 0] as IStructureArray)["time", 0];
            IArray timePost = (_DataStruct["PostOptimization", 0] as IStructureArray)["time", 0];

            IArray idPre = (_DataStruct["PreOptimization", 0] as IStructureArray)["id", 0];
            IArray idPost = (_DataStruct["PostOptimization", 0] as IStructureArray)["id", 0];

            IArray T_cam_worldPre = (_DataStruct["PreOptimization", 0] as IStructureArray)["T_cam_world", 0];
            IArray T_cam_worldPost = (_DataStruct["PostOptimization", 0] as IStructureArray)["T_cam_world", 0];

            IArray T_base_worldPre = (_DataStruct["PreOptimization", 0] as IStructureArray)["T_base_world", 0];
            IArray T_base_worldPost = (_DataStruct["PostOptimization", 0] as IStructureArray)["T_base_world", 0];

            IArray velocityPre = (_DataStruct["PreOptimization", 0] as IStructureArray)["velocity", 0];
            IArray velocityPost = (_DataStruct["PostOptimization", 0] as IStructureArray)["velocity", 0];

            IArray bgPre = (_DataStruct["PreOptimization", 0] as IStructureArray)["bias_gyroscope", 0];
            IArray baPre = (_DataStruct["PreOptimization", 0] as IStructureArray)["bias_accelerometer", 0];

            IArray bgPost = (_DataStruct["PostOptimization", 0] as IStructureArray)["bias_gyroscope", 0];
            IArray baPost = (_DataStruct["PostOptimization", 0] as IStructureArray)["bias_accelerometer", 0];

            IArray kfidPost = (_DataStruct["PostOptimization", 0] as IStructureArray)["kfid", 0];

            IArray fxPost = (_DataStruct["PostOptimization", 0] as IStructureArray)["fx", 0];
            IArray fyPost = (_DataStruct["PostOptimization", 0] as IStructureArray)["fy", 0];
            IArray cxPost = (_DataStruct["PostOptimization", 0] as IStructureArray)["cx", 0];
            IArray cyPost = (_DataStruct["PostOptimization", 0] as IStructureArray)["cy", 0];

            IArray scalePre = (_DataStruct["PreOptimization", 0] as IStructureArray)["scale", 0];
            IArray scalePost = (_DataStruct["PostOptimization", 0] as IStructureArray)["scale", 0];

            (_DataStruct["PostOptimization", 0] as IStructureArray)["point_cloud", 0] = AddFieldToStructureArray(((_DataStruct["PostOptimization", 0] as IStructureArray)["point_cloud", 0] as IStructureArray), "keyframe", keyFrames.Count);

            timePre = ResizeArray<double>(timePre, 1, frames.Count);
            timePost = ResizeArray<double>(timePost, 1, keyFrames.Count);

            idPre = ResizeArray<uint>(idPre, 1, frames.Count);
            idPost = ResizeArray<uint>(idPost, 1, keyFrames.Count);

            T_cam_worldPre = ResizeArray<double>(T_cam_worldPre, 4, 4, frames.Count);
            T_cam_worldPost = ResizeArray<double>(T_cam_worldPost, 4, 4, keyFrames.Count);

            T_base_worldPre = ResizeArray<double>(T_base_worldPre, 4, 4, frames.Count);
            T_base_worldPost = ResizeArray<double>(T_base_worldPost, 4, 4, keyFrames.Count);

            velocityPre = ResizeArray<double>(timePre, 3, frames.Count);
            velocityPost = ResizeArray<double>(timePost, 3, keyFrames.Count);

            bgPre = ResizeArray<double>(timePre, 3, frames.Count);
            baPre = ResizeArray<double>(timePost, 3, frames.Count);

            bgPost = ResizeArray<double>(timePre, 3, keyFrames.Count);
            baPost = ResizeArray<double>(timePost, 3, keyFrames.Count);

            kfidPost = ResizeArray<uint>(kfidPost, 1, keyFrames.Count);

            fxPost = ResizeArray<double>(fxPost, 1, keyFrames.Count);
            fyPost = ResizeArray<double>(fyPost, 1, keyFrames.Count);
            cxPost = ResizeArray<double>(cxPost, 1, keyFrames.Count);
            cyPost = ResizeArray<double>(cyPost, 1, keyFrames.Count);

            scalePre = ResizeArray<double>(scalePre, 1, frames.Count);
            scalePost = ResizeArray<double>(scalePost, 1, keyFrames.Count);

            for (int i = 0; i < frames.Count; i++)
            {
                if (frames[i] != null)
                {
                    ((IArrayOf<double>)timePre)[0, i] = frames[i].Time;
                    ((IArrayOf<uint>)idPre)[0, i] = frames[i].Id;
                    ((IArrayOf<double>)scalePre)[0, i] = frames[i].Scale;

                    for (int j = 0; j < 3; j++)
                    {
                        ((IArrayOf<double>)velocityPre)[j, i] = frames[i].Velocity.Vector[j];
                        ((IArrayOf<double>)bgPre)[j, i] = frames[i].BiasGyroscope.Vector[j];
                        ((IArrayOf<double>)baPre)[j, i] = frames[i].BiasAccelerometer.Vector[j];
                    }

                    for (int j = 0; j < 4; j++)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            ((IArrayOf<double>)T_cam_worldPre)[j, k, i] = frames[i].T_cam_world.Matrix[j, k];
                            ((IArrayOf<double>)T_base_worldPre)[j, k, i] = frames[i].T_base_world.Matrix[j, k];
                        }
                    }
                }
            }

            for (int i = 0; i < keyFrames.Count; i++)
            {
                if (keyFrames[i] != null && keyFrames[i].Frame != null)
                {
                    IStructureArray kfstruct = ((_DataStruct["PostOptimization", 0] as IStructureArray)["point_cloud", 0] as IStructureArray)["keyframe", i] as IStructureArray;

                    kfstruct = AddFieldToStructureArray(kfstruct, "u", 1);
                    kfstruct = AddFieldToStructureArray(kfstruct, "v", 1);
                    kfstruct = AddFieldToStructureArray(kfstruct, "inverse_depth", 1);
                    kfstruct = AddFieldToStructureArray(kfstruct, "colors", 1);

                    kfstruct["u", 0] = _DataBuilder.NewArray<double>(1, keyFrames[i].Points.Count);
                    kfstruct["v", 0] = _DataBuilder.NewArray<double>(1, keyFrames[i].Points.Count);
                    kfstruct["inverse_depth", 0] = _DataBuilder.NewArray<double>(1, keyFrames[i].Points.Count);
                    kfstruct["colors", 0] = _DataBuilder.NewArray<byte>(8, keyFrames[i].Points.Count);

                    for (int j = 0; j < keyFrames[i].Points.Count; j++)
                    {
                        (kfstruct["u", 0] as IArrayOf<double>)[0, j] = keyFrames[i].Points[j].U;
                        (kfstruct["v", 0] as IArrayOf<double>)[0, j] = keyFrames[i].Points[j].V;
                        (kfstruct["inverse_depth", 0] as IArrayOf<double>)[0, j] = keyFrames[i].Points[j].InverseDepth;
                        for (int k = 0; k < 8; k++)
                        {
                            (kfstruct["colors", 0] as IArrayOf<byte>)[k, j] = keyFrames[i].Points[j].Colors[k];
                        }
                    }

                    ((IArrayOf<double>)timePost)[0, i] = keyFrames[i].Frame.Time;
                    ((IArrayOf<uint>)idPost)[0, i] = keyFrames[i].Frame.Id;
                    ((IArrayOf<uint>)kfidPost)[0, i] = keyFrames[i].Id;
                    ((IArrayOf<double>)scalePost)[0, i] = keyFrames[i].Frame.Scale;

                    ((IArrayOf<double>)fxPost)[0, i] = keyFrames[i].Fx;
                    ((IArrayOf<double>)fyPost)[0, i] = keyFrames[i].Fy;
                    ((IArrayOf<double>)cxPost)[0, i] = keyFrames[i].Cx;
                    ((IArrayOf<double>)cyPost)[0, i] = keyFrames[i].Cy;

                    for (int j = 0; j < 3; j++)
                    {
                        ((IArrayOf<double>)velocityPost)[j, i] = keyFrames[i].Frame.Velocity.Vector[j];
                        ((IArrayOf<double>)bgPost)[j, i] = keyFrames[i].Frame.BiasGyroscope.Vector[j];
                        ((IArrayOf<double>)baPost)[j, i] = keyFrames[i].Frame.BiasAccelerometer.Vector[j];
                    }

                    for (int j = 0; j < 4; j++)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            ((IArrayOf<double>)T_cam_worldPost)[j, k, i] = keyFrames[i].Frame.T_cam_world.Matrix[j, k];
                            ((IArrayOf<double>)T_base_worldPost)[j, k, i] = keyFrames[i].Frame.T_base_world.Matrix[j, k];
                        }
                    }

                    ((_DataStruct["PostOptimization", 0] as IStructureArray)["point_cloud", 0] as IStructureArray)["keyframe", i] = kfstruct;
                }
            }

            (_DataStruct["PreOptimization", 0] as IStructureArray)["time", 0] = timePre;
            (_DataStruct["PreOptimization", 0] as IStructureArray)["id", 0] = idPre;
            (_DataStruct["PreOptimization", 0] as IStructureArray)["T_cam_world", 0] = T_cam_worldPre;
            (_DataStruct["PreOptimization", 0] as IStructureArray)["T_base_world", 0] = T_base_worldPre;
            (_DataStruct["PreOptimization", 0] as IStructureArray)["velocity", 0] = velocityPre;
            (_DataStruct["PreOptimization", 0] as IStructureArray)["bias_gyroscope", 0] = bgPre;
            (_DataStruct["PreOptimization", 0] as IStructureArray)["bias_accelerometer", 0] = baPre;
            (_DataStruct["PreOptimization", 0] as IStructureArray)["scale", 0] = scalePre;

            (_DataStruct["PostOptimization", 0] as IStructureArray)["time", 0] = timePost;
            (_DataStruct["PostOptimization", 0] as IStructureArray)["id", 0] = idPost;
            (_DataStruct["PostOptimization", 0] as IStructureArray)["T_cam_world", 0] = T_cam_worldPost;
            (_DataStruct["PostOptimization", 0] as IStructureArray)["T_base_world", 0] = T_base_worldPost;
            (_DataStruct["PostOptimization", 0] as IStructureArray)["kfid", 0] = kfidPost;
            (_DataStruct["PostOptimization", 0] as IStructureArray)["velocity", 0] = velocityPost;
            (_DataStruct["PostOptimization", 0] as IStructureArray)["bias_gyroscope", 0] = bgPost;
            (_DataStruct["PostOptimization", 0] as IStructureArray)["bias_accelerometer", 0] = baPost;
            (_DataStruct["PostOptimization", 0] as IStructureArray)["scale", 0] = scalePost;
            (_DataStruct["PostOptimization", 0] as IStructureArray)["fx", 0] = fxPost;
            (_DataStruct["PostOptimization", 0] as IStructureArray)["fy", 0] = fyPost;
            (_DataStruct["PostOptimization", 0] as IStructureArray)["cx", 0] = cxPost;
            (_DataStruct["PostOptimization", 0] as IStructureArray)["cy", 0] = cyPost;
        }

        public void Open()
        {
            _DataBuilder = new DataBuilder();
            _DataStruct = _DataBuilder.NewStructureArray(new[] { "PreOptimization", "PostOptimization" }, 1);

            _DataStruct["PreOptimization", 0] = AddFieldToStructureArray(_DataStruct["PreOptimization", 0], "time", 1);
            _DataStruct["PreOptimization", 0] = AddFieldToStructureArray(_DataStruct["PreOptimization", 0], "id", 1);
            _DataStruct["PreOptimization", 0] = AddFieldToStructureArray(_DataStruct["PreOptimization", 0], "T_cam_world", 1);
            _DataStruct["PreOptimization", 0] = AddFieldToStructureArray(_DataStruct["PreOptimization", 0], "T_base_world", 1);
            _DataStruct["PreOptimization", 0] = AddFieldToStructureArray(_DataStruct["PreOptimization", 0], "velocity", 1);
            _DataStruct["PreOptimization", 0] = AddFieldToStructureArray(_DataStruct["PreOptimization", 0], "bias_gyroscope", 1);
            _DataStruct["PreOptimization", 0] = AddFieldToStructureArray(_DataStruct["PreOptimization", 0], "bias_accelerometer", 1);
            _DataStruct["PreOptimization", 0] = AddFieldToStructureArray(_DataStruct["PreOptimization", 0], "scale", 1);

            _DataStruct["PostOptimization", 0] = AddFieldToStructureArray(_DataStruct["PostOptimization", 0], "time", 1);
            _DataStruct["PostOptimization", 0] = AddFieldToStructureArray(_DataStruct["PostOptimization", 0], "id", 1);
            _DataStruct["PostOptimization", 0] = AddFieldToStructureArray(_DataStruct["PostOptimization", 0], "T_cam_world", 1);
            _DataStruct["PostOptimization", 0] = AddFieldToStructureArray(_DataStruct["PostOptimization", 0], "T_base_world", 1);
            _DataStruct["PostOptimization", 0] = AddFieldToStructureArray(_DataStruct["PostOptimization", 0], "kfid", 1);
            _DataStruct["PostOptimization", 0] = AddFieldToStructureArray(_DataStruct["PostOptimization", 0], "velocity", 1);
            _DataStruct["PostOptimization", 0] = AddFieldToStructureArray(_DataStruct["PostOptimization", 0], "bias_gyroscope", 1);
            _DataStruct["PostOptimization", 0] = AddFieldToStructureArray(_DataStruct["PostOptimization", 0], "bias_accelerometer", 1);
            _DataStruct["PostOptimization", 0] = AddFieldToStructureArray(_DataStruct["PostOptimization", 0], "scale", 1);
            _DataStruct["PostOptimization", 0] = AddFieldToStructureArray(_DataStruct["PostOptimization", 0], "fx", 1);
            _DataStruct["PostOptimization", 0] = AddFieldToStructureArray(_DataStruct["PostOptimization", 0], "fy", 1);
            _DataStruct["PostOptimization", 0] = AddFieldToStructureArray(_DataStruct["PostOptimization", 0], "cx", 1);
            _DataStruct["PostOptimization", 0] = AddFieldToStructureArray(_DataStruct["PostOptimization", 0], "cy", 1);
            _DataStruct["PostOptimization", 0] = AddFieldToStructureArray(_DataStruct["PostOptimization", 0], "point_cloud", 1);

            (_DataStruct["PreOptimization", 0] as IStructureArray)["time", 0] = _DataBuilder.NewArray<double>(1, 0);
            (_DataStruct["PreOptimization", 0] as IStructureArray)["id", 0] = _DataBuilder.NewArray<uint>(1, 0);
            (_DataStruct["PreOptimization", 0] as IStructureArray)["T_cam_world", 0] = _DataBuilder.NewArray<double>(4, 4, 0);
            (_DataStruct["PreOptimization", 0] as IStructureArray)["T_base_world", 0] = _DataBuilder.NewArray<double>(4, 4, 0);
            (_DataStruct["PreOptimization", 0] as IStructureArray)["velocity", 0] = _DataBuilder.NewArray<double>(3, 0);
            (_DataStruct["PreOptimization", 0] as IStructureArray)["bias_gyroscope", 0] = _DataBuilder.NewArray<double>(3, 0);
            (_DataStruct["PreOptimization", 0] as IStructureArray)["bias_accelerometer", 0] = _DataBuilder.NewArray<double>(3, 0);
            (_DataStruct["PreOptimization", 0] as IStructureArray)["scale", 0] = _DataBuilder.NewArray<double>(1, 0);

            (_DataStruct["PostOptimization", 0] as IStructureArray)["time", 0] = _DataBuilder.NewArray<double>(1, 0);
            (_DataStruct["PostOptimization", 0] as IStructureArray)["id", 0] = _DataBuilder.NewArray<uint>(1, 0);
            (_DataStruct["PostOptimization", 0] as IStructureArray)["kfid", 0] = _DataBuilder.NewArray<uint>(1, 0);
            (_DataStruct["PostOptimization", 0] as IStructureArray)["T_cam_world", 0] = _DataBuilder.NewArray<double>(4, 4, 0);
            (_DataStruct["PostOptimization", 0] as IStructureArray)["T_base_world", 0] = _DataBuilder.NewArray<double>(4, 4, 0);
            (_DataStruct["PostOptimization", 0] as IStructureArray)["velocity", 0] = _DataBuilder.NewArray<double>(3, 0);
            (_DataStruct["PostOptimization", 0] as IStructureArray)["bias_gyroscope", 0] = _DataBuilder.NewArray<double>(3, 0);
            (_DataStruct["PostOptimization", 0] as IStructureArray)["bias_accelerometer", 0] = _DataBuilder.NewArray<double>(3, 0);
            (_DataStruct["PostOptimization", 0] as IStructureArray)["scale", 0] = _DataBuilder.NewArray<double>(1, 0);
            (_DataStruct["PostOptimization", 0] as IStructureArray)["fx", 0] = _DataBuilder.NewArray<double>(1, 0);
            (_DataStruct["PostOptimization", 0] as IStructureArray)["fy", 0] = _DataBuilder.NewArray<double>(1, 0);
            (_DataStruct["PostOptimization", 0] as IStructureArray)["cx", 0] = _DataBuilder.NewArray<double>(1, 0);
            (_DataStruct["PostOptimization", 0] as IStructureArray)["cy", 0] = _DataBuilder.NewArray<double>(1, 0);

        }

        private IStructureArray AddFieldToStructureArray(IArray structureArray, string fieldName, params int[] dimensions)
        {
            return MatlabUtilities.AddFieldToStructureArray(_DataBuilder, structureArray, fieldName, dimensions);
        }

        private IArray ResizeArray<T>(IArray array, params int[] dimensions) where T : struct
        {
            return MatlabUtilities.ResizeArray<T>(_DataBuilder, array, dimensions);
        }
    }
}