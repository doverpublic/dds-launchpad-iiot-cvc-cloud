using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Common
{
    class MessageSchema
    {
        private List<FieldDescriptor> fields;

        public MessageSchema()
        {
            fields = new List<FieldDescriptor>();
        }

        public MessageSchema( List<FieldDescriptor> items )
        {
            fields = items;
        }

        public void AddField( FieldDescriptor field )
        {
            fields.Add(field);
        }

        public int FieldCount()
        {
            return fields.Count;
        }

        public FieldDescriptor GetFieldDescriptor( int index )
        {
            FieldDescriptor fieldRet = null;

            if (index > -1 && index < fields.Count)
                fieldRet = fields.ElementAt(index);

            return fieldRet;
        }

        public int[] GetKeyFieldIndexes()
        {
            List<int> listRet = new List<int>();

            for( int index = 0; index < fields.Count; index++ )
            {
                FieldDescriptor field = fields[index];

                if (field.IsKeyField())
                    listRet.Add(index);
            }
            return listRet.ToArray();
        }

        public dynamic GetMessage( string[] values )
        {
            dynamic objRet = new ExpandoObject();

            for( int index = 0; index < fields.Count; index++ )
            {
                FieldDescriptor field = fields.ElementAt(index);

                field.AddDynamicValue(objRet, values[index + 1]);
            }
            return objRet;
        }
    }

    public enum FieldTypes
    {
        Undefined,
        String,
        StringArray,
        Integer,
        IntegerArray,
        Timestamp
    }

    public enum FieldUsages
    {
        Undefined   = 0,
        Auto        = 1,
        Key         = 2
    }
    

    public class FieldDescriptor
    {
        public static string[] FieldTypesStrings = { "undefined", "string", "stringarray", "integer", "integerarray", "timestamp" };
        public static string[] FieldUsageStrings = { "undefined", "auto", "key" };
        public static int UndefinedIntValue = int.MinValue;

        private string fieldName;
        private FieldTypes fieldType;
        private FieldUsages fieldUsage;
        private string fieldLabel;
        private string fieldAbsoluteMinValue;
        private string fieldAbsoluteMaxValue;
        private string fieldNormalMinValue;
        private string fieldNormalMaxValue;

        public string FieldName
        {
            get { return fieldName; }
            set { fieldName = value; }
        }

        public FieldTypes FieldType
        {
            get { return fieldType; }
            set { fieldType = value; }
        }

        public FieldUsages FieldUsage
        {
            get { return fieldUsage; }
            set { fieldUsage = value; }
        }

        public string FieldLabel
        {
            get { return fieldLabel; }
            set { fieldLabel = value; }
        }

        public string FieldAbsoluteMinValue
        {
            get { return fieldAbsoluteMinValue; }
            set { fieldAbsoluteMinValue = value; }
        }

        public string FieldAbsoluteMaxValue
        {
            get { return fieldAbsoluteMaxValue; }
            set { fieldAbsoluteMaxValue = value; }
        }

        public string FieldNormalMinValue
        {
            get { return fieldNormalMinValue; }
            set { fieldNormalMinValue = value; }
        }

        public string FieldNormalMaxValue
        {
            get { return fieldNormalMaxValue; }
            set { fieldNormalMaxValue = value; }
        }

        public FieldDescriptor(string fieldName, string fieldType)
        {
            FieldName = fieldName;
            FieldType = GetFieldType(fieldType);
            FieldUsage = FieldUsages.Undefined;
            FieldLabel = "";
            FieldAbsoluteMinValue = "";
            FieldAbsoluteMaxValue = "";
            FieldNormalMinValue = "";
            FieldNormalMaxValue = "";
        }

        public FieldDescriptor(string fieldName, string fieldType, string fieldUsage)
        {
            FieldName = fieldName;
            FieldType = GetFieldType(fieldType);
            FieldUsage = GetFieldUsage(fieldUsage);
            FieldLabel = "";
            FieldAbsoluteMinValue = "";
            FieldAbsoluteMaxValue = "";
            FieldNormalMinValue = "";
            FieldNormalMaxValue = "";
        }

        public FieldDescriptor(string fieldName, string fieldType, string fieldUsage, string fieldLabel)
        {
            FieldName = fieldName;
            FieldType = GetFieldType(fieldType);
            FieldUsage = GetFieldUsage(fieldUsage);
            FieldLabel = fieldLabel;
            FieldAbsoluteMinValue = "";
            FieldAbsoluteMaxValue = "";
            FieldNormalMinValue = "";
            FieldNormalMaxValue = "";
        }

        public FieldDescriptor(string fieldName, string fieldType, string fieldUsage, string fieldLabel, string fieldAbsoluteMinValue, string fieldAbsoluteMaxValue)
        {
            FieldName = fieldName;
            FieldType = GetFieldType(fieldType);
            FieldUsage = GetFieldUsage(fieldUsage);
            FieldLabel = fieldLabel;
            FieldAbsoluteMinValue = fieldAbsoluteMinValue;
            FieldAbsoluteMaxValue = fieldAbsoluteMaxValue;
            FieldNormalMinValue = fieldAbsoluteMinValue;
            FieldNormalMaxValue = fieldAbsoluteMaxValue;
        }

        public FieldDescriptor(string fieldName, string fieldType, string fieldUsage, string fieldLabel, string fieldAbsoluteMinValue, string fieldAbsoluteMaxValue, string fieldNormalMinValue, string fieldNormalMaxValue)
        {
            FieldName = fieldName;
            FieldType = GetFieldType(fieldType);
            FieldUsage = GetFieldUsage(fieldUsage);
            FieldLabel = fieldLabel;
            FieldAbsoluteMinValue = fieldAbsoluteMinValue;
            FieldAbsoluteMaxValue = fieldAbsoluteMaxValue;
            FieldNormalMinValue = fieldNormalMinValue;
            FieldNormalMaxValue = fieldNormalMaxValue;
        }

        public dynamic AddDynamicValue(ExpandoObject expando, string value)
        {
            if (fieldType == FieldTypes.String)
            {
                AddProperty(expando, fieldName, value);
            }
            else if (fieldType == FieldTypes.StringArray)
            {
                AddProperty(expando, fieldName, GetStringArrayFrom(value));
            }
            else if (fieldType == FieldTypes.Undefined)
            {
                AddProperty(expando, fieldName, "");
            }
            else if (fieldType == FieldTypes.Integer)
            {
                int intValue = int.MinValue;
                if (int.TryParse(value, out intValue))
                {
                    AddProperty(expando, fieldName, intValue);
                }
            }
            else if (fieldType == FieldTypes.IntegerArray)
            {
                AddProperty(expando, fieldName, GetIntegerArrayFrom(value));
            }
            else if (fieldType == FieldTypes.Timestamp)
            {
                DateTimeOffset timestamp = DateTimeOffset.UtcNow;

                if( (fieldUsage & FieldUsages.Auto) == 0)
                    timestamp = DateTime.Parse(value);

                AddProperty(expando, fieldName, timestamp);
            }
            return expando;
        }

        public int GetAbsoluteMaxValueAsInt()
        {
            int iRet = int.MinValue;

            if (fieldType == FieldTypes.Integer)
                int.TryParse(fieldAbsoluteMaxValue, out iRet);

            return iRet;
        }

        public int GetAbsoluteMinValueAsInt()
        {
            int iRet = int.MinValue;

            if (fieldType == FieldTypes.Integer)
                int.TryParse(fieldAbsoluteMinValue, out iRet);

            return iRet;
        }

        public static FieldTypes GetFieldType(string fieldType)
        {
            FieldTypes typeRet = FieldTypes.Undefined;

            for (int index = 1; index < FieldTypesStrings.Length; index++)
            {
                if (fieldType.Equals(FieldTypesStrings[index], StringComparison.InvariantCultureIgnoreCase))
                {
                    typeRet = (FieldTypes)index;
                    break;
                }
            }

            return typeRet;
        }

        public static FieldUsages GetFieldUsage(string fieldUsage)
        {
            FieldUsages usageRet = FieldUsages.Undefined;

            for (int index = 1; index < FieldUsageStrings.Length; index++)
            {
               if( fieldUsage.Contains(FieldUsageStrings[index]))
                {
                    if (index == 1)
                        usageRet = usageRet | FieldUsages.Auto;
                    else if (index == 2)
                        usageRet = usageRet | FieldUsages.Key;
                }
            }

            return usageRet;
        }

        public static string GetFieldTypeString(FieldTypes fieldType)
        {
            return FieldTypesStrings[(int)fieldType];
        }

        public int GetNormalMaxValueAsInt()
        {
            int iRet = int.MinValue;

            if (fieldType == FieldTypes.Integer)
                int.TryParse(fieldNormalMaxValue, out iRet);

            return iRet;
        }

        public int GetNormalMinValueAsInt()
        {
            int iRet = int.MinValue;

            if (fieldType == FieldTypes.Integer)
                int.TryParse(fieldNormalMinValue, out iRet);

            return iRet;
        }

        public dynamic GetDynamicValue(string value)
        {
            dynamic objRet = new ExpandoObject();

            if (fieldType == FieldTypes.String)
            {
                AddProperty(objRet, fieldName, value);
            }
            else if( fieldType == FieldTypes.StringArray )
            {
                AddProperty(objRet, fieldName, GetStringArrayFrom(value));
            }
            else if (fieldType == FieldTypes.Undefined)
            {
                AddProperty(objRet, fieldName, "");
            }
            else if (fieldType == FieldTypes.Integer)
            {
                int intValue = int.MinValue;
                if (int.TryParse(value, out intValue))
                {
                    AddProperty(objRet, fieldName, intValue);
                }
            }
            else if (fieldType == FieldTypes.IntegerArray)
            {
                  AddProperty(objRet, fieldName, GetIntegerArrayFrom( value ) );
            }
            else if (fieldType == FieldTypes.Timestamp)
            {
                DateTimeOffset timestamp = DateTimeOffset.UtcNow;

                if( (fieldUsage & FieldUsages.Auto) == 0 )
                    timestamp = DateTime.Parse(value);

                AddProperty(objRet, fieldName, timestamp);
            }
            return objRet;
        }

        public object GetObjectValue(string value)
        {
            object objRet = null;

            if (fieldType == FieldTypes.String)
                objRet = value;
            else if (fieldType == FieldTypes.StringArray)
                objRet = GetStringArrayFrom(value);
            else if (fieldType == FieldTypes.Undefined)
                objRet = "";
            else if (fieldType == FieldTypes.Integer)
            {
                int intValue = int.MinValue;
                if (int.TryParse(value, out intValue))
                {
                    objRet = intValue;
                }
            }
            else if (fieldType == FieldTypes.IntegerArray)
                objRet = GetIntegerArrayFrom(value);
            else if (fieldType == FieldTypes.Timestamp)
            {
                DateTimeOffset timestamp = DateTimeOffset.UtcNow;

                if ((fieldUsage & FieldUsages.Auto) == 0)
                    timestamp = DateTime.Parse(value);

                objRet = timestamp;
            }

            return objRet;

        }

        public bool IsKeyField()
        {
            return (fieldUsage & FieldUsages.Key) > 0;
        }

        // PRIVATE METHODS
        private static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }

        private int[] GetIntegerArrayFrom( string stringArray )
        {
            int[] intArrayRet = new int[0];
            string list = stringArray.Substring(1, stringArray.Length - 2);
            string[] values = list.Split(',');

            intArrayRet = new int[values.Length];
            for( int index=0; index < values.Length; index++ )
            {
                int intValue = int.MinValue;
                if (int.TryParse(values[index], out intValue))
                {
                    intArrayRet[index] = intValue;
                }
            }

            return intArrayRet;
        }

        private string[] GetStringArrayFrom(string stringArray)
        {
            string list = stringArray.Substring(1, stringArray.Length - 2);

            return list.Split(',');
        }

    }
}
