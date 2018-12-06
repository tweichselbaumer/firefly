using MatFileHandler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

            IArray timePre = (_DataStruct["PreOptimizationTrajectory", 0] as IStructureArray)["time", 0];
            IArray timePost = (_DataStruct["PreOptimizationTrajectory", 0] as IStructureArray)["time", 0];

            IArray idPre = (_DataStruct["PreOptimizationTrajectory", 0] as IStructureArray)["id", 0];
            IArray idPost = (_DataStruct["PostOptimizationTrajectory", 0] as IStructureArray)["id", 0];

            IArray T_cam_worldPre = (_DataStruct["PreOptimizationTrajectory", 0] as IStructureArray)["T_cam_world", 0];
            IArray T_cam_worldPost = (_DataStruct["PostOptimizationTrajectory", 0] as IStructureArray)["T_cam_world", 0];

            IArray kfidPost = (_DataStruct["PostOptimizationTrajectory", 0] as IStructureArray)["T_cam_world", 0];

            timePre = ResizeArray<double>(timePre, 1, frames.Count);
            timePost = ResizeArray<double>(timePost, 1, keyFrames.Count);

            idPre = ResizeArray<uint>(idPre, 1, frames.Count);
            idPost = ResizeArray<uint>(idPost, 1, keyFrames.Count);

            T_cam_worldPre = ResizeArray<double>(T_cam_worldPre, 4, 4, frames.Count);
            T_cam_worldPost = ResizeArray<double>(T_cam_worldPost, 4, 4, keyFrames.Count);

            kfidPost = ResizeArray<uint>(kfidPost, 1, keyFrames.Count);

            for (int i = 0; i < frames.Count; i++)
            {
                if (frames[i] != null)
                {
                    ((IArrayOf<double>)timePre)[0, i] = frames[i].Time;
                    ((IArrayOf<uint>)idPre)[0, i] = frames[i].Id;
                    for (int j = 0; j < 4; j++)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            ((IArrayOf<double>)T_cam_worldPre)[j, k, i] = frames[i].T_cam_world.Matrix[j, k];
                        }
                    }
                }
            }

            for (int i = 0; i < keyFrames.Count; i++)
            {
                if (keyFrames[i] != null && keyFrames[i].Frame != null)
                {
                    ((IArrayOf<double>)timePre)[0, i] = keyFrames[i].Frame.Time;
                    ((IArrayOf<uint>)idPre)[0, i] = keyFrames[i].Frame.Id;
                    ((IArrayOf<uint>)kfidPost)[0, i] = keyFrames[i].Id;

                    for (int j = 0; j < 4; j++)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            ((IArrayOf<double>)T_cam_worldPre)[j, k, i] = keyFrames[i].Frame.T_cam_world.Matrix[j, k];
                        }
                    }
                }
            }

            (_DataStruct["PreOptimizationTrajectory", 0] as IStructureArray)["time", 0] = timePre;
            (_DataStruct["PreOptimizationTrajectory", 0] as IStructureArray)["id", 0] = idPre;
            (_DataStruct["PreOptimizationTrajectory", 0] as IStructureArray)["T_cam_world", 0] = T_cam_worldPre;

            (_DataStruct["PostOptimizationTrajectory", 0] as IStructureArray)["time", 0] = timePost;
            (_DataStruct["PostOptimizationTrajectory", 0] as IStructureArray)["id", 0] = idPost;
            (_DataStruct["PostOptimizationTrajectory", 0] as IStructureArray)["T_cam_world", 0] = T_cam_worldPost;
            (_DataStruct["PostOptimizationTrajectory", 0] as IStructureArray)["kfid", 0] = kfidPost;
        }

        public void Open()
        {
            _DataBuilder = new DataBuilder();
            _DataStruct = _DataBuilder.NewStructureArray(new[] { "PreOptimizationTrajectory", "PostOptimizationTrajectory" }, 1);

            _DataStruct["PreOptimizationTrajectory", 0] = AddFieldToStructureArray(_DataStruct["PreOptimizationTrajectory", 0], "time", 1);
            _DataStruct["PreOptimizationTrajectory", 0] = AddFieldToStructureArray(_DataStruct["PreOptimizationTrajectory", 0], "id", 1);
            _DataStruct["PreOptimizationTrajectory", 0] = AddFieldToStructureArray(_DataStruct["PreOptimizationTrajectory", 0], "T_cam_world", 1);

            _DataStruct["PostOptimizationTrajectory", 0] = AddFieldToStructureArray(_DataStruct["PostOptimizationTrajectory", 0], "time", 1);
            _DataStruct["PostOptimizationTrajectory", 0] = AddFieldToStructureArray(_DataStruct["PostOptimizationTrajectory", 0], "id", 1);
            _DataStruct["PostOptimizationTrajectory", 0] = AddFieldToStructureArray(_DataStruct["PostOptimizationTrajectory", 0], "T_cam_world", 1);
            _DataStruct["PostOptimizationTrajectory", 0] = AddFieldToStructureArray(_DataStruct["PostOptimizationTrajectory", 0], "kfid", 1);

            (_DataStruct["PreOptimizationTrajectory", 0] as IStructureArray)["time", 0] = _DataBuilder.NewArray<double>(1, 0);
            (_DataStruct["PreOptimizationTrajectory", 0] as IStructureArray)["id", 0] = _DataBuilder.NewArray<uint>(1, 0);
            (_DataStruct["PreOptimizationTrajectory", 0] as IStructureArray)["T_cam_world", 0] = _DataBuilder.NewArray<double>(4, 4, 0);

            (_DataStruct["PostOptimizationTrajectory", 0] as IStructureArray)["time", 0] = _DataBuilder.NewArray<double>(1, 0);
            (_DataStruct["PostOptimizationTrajectory", 0] as IStructureArray)["id", 0] = _DataBuilder.NewArray<uint>(1, 0);
            (_DataStruct["PostOptimizationTrajectory", 0] as IStructureArray)["kfid", 0] = _DataBuilder.NewArray<uint>(1, 0);
            (_DataStruct["PostOptimizationTrajectory", 0] as IStructureArray)["T_cam_world", 0] = _DataBuilder.NewArray<double>(4, 4, 0);
        }

        private IStructureArray AddFieldToStructureArray(IArray structureArray, string fieldName, params int[] dimensions)
        {
            IStructureArray result;
            if (structureArray == null || structureArray.IsEmpty || !(structureArray is IStructureArray))
            {
                result = _DataBuilder.NewStructureArray(new List<string> { fieldName }, dimensions);
            }
            else
            {
                result = _DataBuilder.NewStructureArray(new List<string> { fieldName }.Union((structureArray as IStructureArray).FieldNames), dimensions);
                foreach (string field in (structureArray as IStructureArray).FieldNames)
                {
                    for (int i = 0; i < (structureArray as IStructureArray).Count && i < result.Count; i++)
                    {
                        result[field, i] = (structureArray as IStructureArray)[field, i];
                    }
                }
            }

            return result;
        }

        private IArray ResizeArray<T>(IArray array, params int[] dimensions) where T : struct
        {
            IArrayOf<T> result;

            if (array == null || array.IsEmpty || !(array is IArrayOf<T>))
            {
                result = _DataBuilder.NewArray<T>(dimensions);
            }
            else
            {
                result = _DataBuilder.NewArray<T>(dimensions);

                for (int i = 0; i < array.Count && i < result.Count; i++)
                {
                    result[i] = (array as IArrayOf<T>)[i];
                }
            }

            return result;
        }
    }
}