using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;


namespace Iot.Common
{
    public class DatasetConfiguration
    {
        public const int FIELD_TYPE_UNDEFINED = 0;
        public const int FIELD_TYPE_NUMBER = 1;
        public const int FIELD_TYPE_TEXT = 2;
        public const int FIELD_TYPE_DATETIME = 3;

        public const string FIELD_TYPE_NUMBER_MASK = "98.6";
        public const string FIELD_TYPE_TEXT_MASK = "AAAAA555555";
        public const string FIELD_TYPE_DATETIME_MASK = "2018-06-11T13:37:15.531Z";

        private Dictionary<string, int> fieldDefinitions = new Dictionary<string, int>();

        public DatasetConfiguration( string name, string postUrl, bool historicDataAnalysisOn, string streamFieldDefinitions )
        {
            this.Name = name;
            this.PostUrl = postUrl;
            this.HistoricDataAnalysisOn = historicDataAnalysisOn;



            if (DatasetRegistry.IsDatasetTypeAlreadyRegistered(this.Name))
                DatasetRegistry.UnRegisterDataseType(this.Name);
            DatasetRegistry.RegisterDatasetType(this.Name, this);
        }

        public string Name { get; private set; }
        public bool HistoricDataAnalysisOn { get; private set; }
        public string PostUrl { get; private set; }

        public bool AddOrReplaceField( string fieldName, int fieldType)
        {
            bool bRet = false;

            if( this.fieldDefinitions.ContainsKey(fieldName))
            {
                bRet = true;
                this.fieldDefinitions.Remove(fieldName);
            }

            this.fieldDefinitions.Add(fieldName, fieldType);

            return bRet;
        }

        public int GetFieldType( string fieldName )
        {
            int iRet = FIELD_TYPE_UNDEFINED;

            if (this.fieldDefinitions.ContainsKey(fieldName))
            {
                this.fieldDefinitions.TryGetValue(fieldName, out iRet);
            }

            return iRet;
        }

        public string GetMessageToPublish(DeviceMessage deviceMessage)
        {
            return deviceMessage.ToString();
        }


        public bool RemoveField( string fieldName )
        {
            bool bRet = false;

            if (this.fieldDefinitions.ContainsKey(fieldName))
            {
                bRet = true;
                this.fieldDefinitions.Remove(fieldName);
            }

            return bRet;
        }

        //PRIVATE METHODS
        private void ParseFieldDefinitions( string streamFieldDefinitions )
        {
            JArray fieldDefinitionArray = JArray.Parse(streamFieldDefinitions);

            foreach( JObject fieldDefinitionList in fieldDefinitionArray)
            {
                foreach( var pair in fieldDefinitionList)
                {
                    string value = (string)pair.Value;

                    if (value.Equals(FIELD_TYPE_NUMBER_MASK))
                        this.fieldDefinitions.Add(pair.Key, FIELD_TYPE_NUMBER);
                    else if (value.Equals(FIELD_TYPE_TEXT_MASK))
                        this.fieldDefinitions.Add(pair.Key, FIELD_TYPE_TEXT);
                    else
                        this.fieldDefinitions.Add(pair.Key, FIELD_TYPE_DATETIME);
                }
            }
        }

    }
}
