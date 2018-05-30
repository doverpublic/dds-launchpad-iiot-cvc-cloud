using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Common
{
    public class EventsContainer
    {
        public static string[] LeadFieldType = { "FieldName", "FieldType", "FieldUsage", "absoluteMinValue", "absoluteMaxValue", "normalMinValue", "normalMaxValue", "message", "replay" };

        public enum LeadFieldTypeIndex
        {
            FieldName,
            FieldType,
            FieldUsage,
            AbsoluteMinValue,
            AbsoluteMaxValue,
            NormalMinValue,
            NormalMaxValue,
            Message,
            Replay
        }

        private int fieldsCount;
        private bool replayFlag;
        private bool eventsFlag;
        private List<string> fieldNames = new List<string>();
        private List<string> fieldTypes = new List<string>();
        private List<string> fieldUsages = new List<string>();
        private List<string> fieldAbsoluteMinValues = new List<string>();
        private List<string> fieldAbsoluteMaxValues = new List<string>();
        private List<string> fieldNormalMinValues = new List<string>();
        private List<string> fieldNormalMaxValues = new List<string>();

        private List<FieldDescriptor> fields = new List<FieldDescriptor>();

        private MessageSchema messageSchema;

        private List<string[]> valuesList = new List<string[]>();

        public EventsContainer(string fileDataPath)
        {
            fieldsCount = 0;
            replayFlag = false;
            eventsFlag = ParseCSVFile(fileDataPath);

            InitializeFieldDefinitions();

            messageSchema = new MessageSchema(fields);
        }

        public EventsContainer(string fileDataPath, string fieldsDefinitionPath)
        {
            fieldsCount = 0;
            replayFlag = false;

            if(fieldsDefinitionPath != null && fieldsDefinitionPath.Length > 0)
            {
                eventsFlag = ParseCSVFile(fieldsDefinitionPath);

                InitializeFieldDefinitions();

                messageSchema = new MessageSchema(fields);

                if (!eventsFlag)
                    eventsFlag = ParseCSVFile(fileDataPath);
            }
            else
            {
                eventsFlag = ParseCSVFile(fileDataPath);

                InitializeFieldDefinitions();

                messageSchema = new MessageSchema(fields);
            }
        }

        public bool EventsFlag
        {
            get { return eventsFlag; }
            set { eventsFlag = value; }
        }

        public bool ReplayFlag
        {
            get { return replayFlag; }
            set { replayFlag = value; }
        }

        public dynamic GetEventMessageForValues( string[] values )
        {
            return messageSchema.GetMessage(values);
        }

        public NameValueCollection GetKeyFields( string[] values )
        {
            NameValueCollection colRet = new NameValueCollection();
            int[] keyFieldIndexes = messageSchema.GetKeyFieldIndexes();

            for( int index =0; index < keyFieldIndexes.Length; index++ )
            {
                FieldDescriptor field = messageSchema.GetFieldDescriptor(keyFieldIndexes[index]);

                colRet.Add(field.FieldName, field.GetObjectValue(values[keyFieldIndexes[index] + 1]).ToString() );
            }

            return colRet;
        }

        public List<string[]> GetValuesList()
        {
            return valuesList;
        }

        // PRIVATE METHODS
        private bool InitializeFieldDefinitions()
        {
            bool bRet = false;

            if( fieldsCount > 1 )
            {
                for( int index=0; index < (fieldsCount - 1); index++ )
                {
                    if( fieldAbsoluteMinValues.Count > 0 && fieldNormalMinValues.Count > 0 )
                    {
                        FieldDescriptor fieldDescriptor = new FieldDescriptor(  fieldNames[index],
                                                                                fieldTypes[index],
                                                                                fieldUsages[index],
                                                                                "",
                                                                                fieldAbsoluteMinValues[index],
                                                                                fieldAbsoluteMaxValues[index],
                                                                                fieldNormalMinValues[index],
                                                                                fieldNormalMaxValues[index]);
                        fields.Add(fieldDescriptor);
                    }
                    else if (fieldAbsoluteMinValues.Count > 0 )
                    {
                        FieldDescriptor fieldDescriptor = new FieldDescriptor(  fieldNames[index],
                                                                                fieldTypes[index],
                                                                                fieldUsages[index],
                                                                                "",
                                                                                fieldAbsoluteMinValues[index],
                                                                                fieldAbsoluteMaxValues[index]);
                        fields.Add(fieldDescriptor);
                    }
                    else
                    {
                        FieldDescriptor fieldDescriptor = new FieldDescriptor(  fieldNames[index],
                                                                                fieldTypes[index]);
                        fields.Add(fieldDescriptor);
                    }
                }
                bRet = true;
            }

            return bRet;
        }

        private string[] SplitCSVLine( char separator, string line )
        {
            List<string> listRet = new List<string>();
            StringBuilder element = new StringBuilder();
            int doubleQuoteCount = 0;
            char it = (char)0;
            bool moreThanOneValue = false;

            for (int index =0; index < line.Length; index++)
            {
                it = line.ElementAt(index);

                if (it == '\"')
                {
                    if (doubleQuoteCount == 0)
                        doubleQuoteCount++;
                    else
                        doubleQuoteCount--;

                    continue;
                }

                if (it == separator && doubleQuoteCount == 0)
                {
                    moreThanOneValue = true;
                    listRet.Add(element.ToString());
                    element.Clear();
                    continue;
                }

                element.Append(it);
            }

            if ( (it != (char)0) && moreThanOneValue  )
                listRet.Add(element.ToString());

            return listRet.ToArray();
        }

        private bool ParseCSVFile( string filePath )
        {
            bool bRet = false;
            StreamReader reader = new StreamReader(filePath);

            while( !reader.EndOfStream )
            {
                var line = reader.ReadLine();
                var values = SplitCSVLine(',', line);

                if (fieldsCount == 0)
                    fieldsCount = values.Length;

                if( fieldsCount > 0 )
                {
                    string leadField = values[0];
                    int leadFieldValue = 0;
                    bool leadFieldIsMessage = int.TryParse(values[0], out leadFieldValue);

                    if( leadField.Equals(LeadFieldType[(int)LeadFieldTypeIndex.FieldName], StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        for( int index = 1; index < fieldsCount; index++ )
                        {
                            fieldNames.Add(values[index]);
                        }
                    }
                    else if (leadField.Equals(LeadFieldType[(int)LeadFieldTypeIndex.FieldType], StringComparison.InvariantCultureIgnoreCase))
                    {
                        for (int index = 1; index < fieldsCount; index++)
                        {
                            fieldTypes.Add(values[index]);
                        }
                    }
                    else if (leadField.Equals(LeadFieldType[(int)LeadFieldTypeIndex.FieldUsage], StringComparison.InvariantCultureIgnoreCase))
                    {
                        for (int index = 1; index < fieldsCount; index++)
                        {
                            fieldUsages.Add(values[index]);
                        }
                    }
                    else if (leadField.Equals(LeadFieldType[(int)LeadFieldTypeIndex.AbsoluteMinValue], StringComparison.InvariantCultureIgnoreCase))
                    {
                        for (int index = 1; index < fieldsCount; index++)
                        {
                            fieldAbsoluteMinValues.Add(values[index]);
                        }
                    }
                    else if (leadField.Equals(LeadFieldType[(int)LeadFieldTypeIndex.AbsoluteMaxValue], StringComparison.InvariantCultureIgnoreCase))
                    {
                        for (int index = 1; index < fieldsCount; index++)
                        {
                            fieldAbsoluteMaxValues.Add(values[index]);
                        }
                    }
                    else if (leadField.Equals(LeadFieldType[(int)LeadFieldTypeIndex.NormalMinValue], StringComparison.InvariantCultureIgnoreCase))
                    {
                        for (int index = 1; index < fieldsCount; index++)
                        {
                            fieldNormalMinValues.Add(values[index]);
                        }
                    }
                    else if (leadField.Equals(LeadFieldType[(int)LeadFieldTypeIndex.NormalMaxValue], StringComparison.InvariantCultureIgnoreCase))
                    {
                        for (int index = 1; index < fieldsCount; index++)
                        {
                            fieldNormalMaxValues.Add(values[index]);
                        }
                    }
                    else if (leadFieldIsMessage || leadField.Equals(LeadFieldType[(int)LeadFieldTypeIndex.Message], StringComparison.InvariantCultureIgnoreCase))
                    {
                        bRet = true;

                        string[] valuesToUse = new string[fieldsCount];

                        for (int index = 0; index < fieldsCount; index++)
                        {
                            valuesToUse[index] = values[index];
                        }
                        valuesList.Add(valuesToUse);
                    }
                    else if (leadField.Equals(LeadFieldType[(int)LeadFieldTypeIndex.Replay], StringComparison.InvariantCultureIgnoreCase))
                    {
                        replayFlag = true;
                    }
                }
            }

            reader.Close();

            return bRet;
        }
    }
}
