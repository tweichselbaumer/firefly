using MatFileHandler;
using System.Collections.Generic;
using System.Linq;

namespace FireFly.Utilities
{
    public static class MatlabUtilities
    {
        public static IStructureArray AddFieldToStructureArray(DataBuilder dataBuilder, IArray structureArray, string fieldName, params int[] dimensions)
        {
            IStructureArray result;
            if (structureArray == null || structureArray.IsEmpty || !(structureArray is IStructureArray))
            {
                result = dataBuilder.NewStructureArray(new List<string> { fieldName }, dimensions);
            }
            else
            {
                result = dataBuilder.NewStructureArray(new List<string> { fieldName }.Union((structureArray as IStructureArray).FieldNames), dimensions);
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

        public static IArray ResizeArray<T>(DataBuilder dataBuilder, IArray array, params int[] dimensions) where T : struct
        {
            IArrayOf<T> result;

            if (array == null || array.IsEmpty || !(array is IArrayOf<T>))
            {
                result = dataBuilder.NewArray<T>(dimensions);
            }
            else
            {
                result = dataBuilder.NewArray<T>(dimensions);

                for (int i = 0; i < array.Count && i < result.Count; i++)
                {
                    result[i] = (array as IArrayOf<T>)[i];
                }
            }

            return result;
        }
    }
}