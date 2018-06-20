using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Common
{
    public class DatasetRegistry
    {
        private static DatasetConfiguration defaultDataseConfiguration = new DatasetConfiguration(Names.DATASET_TYPE_DEFAULT, "", false, "");
        private static ConcurrentDictionary<string, DatasetConfiguration> datasetTypesBag = new ConcurrentDictionary<string, DatasetConfiguration>();

        public static DatasetConfiguration GetDatasetConfiguration( string datasetType )
        {
            DatasetConfiguration datasetConfigurationRet = defaultDataseConfiguration;

            if (!IsDatasetTypeAlreadyRegistered(datasetType))
            {
                datasetTypesBag.TryGetValue(datasetType, out datasetConfigurationRet);
            }

            return datasetConfigurationRet;
        }

        public static bool IsDatasetTypeAlreadyRegistered(string datasetType)
        {
            return datasetTypesBag.ContainsKey(datasetType);
        }

        public static bool RegisterDatasetType( string datasetType, DatasetConfiguration datasetConfiguration)
        {
            bool bRet = false;

            if( !IsDatasetTypeAlreadyRegistered(datasetType))
            {
                datasetTypesBag.TryAdd(datasetType, datasetConfiguration);
                bRet = true;
            }

            return bRet;
        }

        public static bool UnRegisterDataseType(string datasetType)
        {
            bool bRet = false;

            if (!IsDatasetTypeAlreadyRegistered(datasetType))
            {
                datasetTypesBag.TryRemove(datasetType, out DatasetConfiguration datasetConfiguration);
                bRet = true;
            }

            return bRet;
        }

    }
}
