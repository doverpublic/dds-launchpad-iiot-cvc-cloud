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
    public class EntityRegistry
    {
        private static ConcurrentDictionary<string, EntityConfig>    entitiesBag = new ConcurrentDictionary<string, EntityConfig>();
        private static ConcurrentDictionary<Type, string> entitiesCrossReference = new ConcurrentDictionary<Type, string>();


        public static bool GetEntityConfigFor(string entityName, out Type entityType, out long partition, out string dictionaryName )
        {
            bool bRet = false;

            if (IsEntityAlreadyRegistered(entityName))
            {
                entitiesBag.TryGetValue(entityName, out EntityConfig config);

                entityType = config.EntityType;
                partition = config.Partition;
                dictionaryName = config.DictionaryName;
                bRet = true;
            }
            else
            {
                dictionaryName = null;
                entityType = null;
                partition = 0;
            }

            return bRet;
        }

        public static bool GetEntityConfigFor(Type entityType, out string entityName, out long partition, string dictionaryName)
        {
            bool bRet = false;

            if (IsEntityAlreadyRegistered(entityType))
            {
                entitiesCrossReference.TryGetValue(entityType, out entityName);
                entitiesBag.TryGetValue(entityName, out EntityConfig config);

                partition = config.Partition;
                dictionaryName = config.DictionaryName;
                bRet = true;
            }
            else
            {
                entityName = null;
                partition = 0;
                dictionaryName = null;
            }

            return bRet;
        }

        public static bool GetEntityConfigFor(string entityName, out Type entityType, out long partition)
        {
            bool bRet = false;

            if (IsEntityAlreadyRegistered(entityName))
            {
                entitiesBag.TryGetValue(entityName, out EntityConfig config);

                entityType = config.EntityType;
                partition = config.Partition;
                bRet = true;
            }
            else
            {
                entityType = null;
                partition = 0;
            }

            return bRet;
        }

        public static bool GetEntityConfigFor(Type entityType, out string entityName, out long partition)
        {
            bool bRet = false;

            if (IsEntityAlreadyRegistered(entityType))
            {
                entitiesCrossReference.TryGetValue(entityType, out entityName);
                entitiesBag.TryGetValue(entityName, out EntityConfig config);

                partition = config.Partition;
                bRet = true;
            }
            else
            {
                entityName = null;
                partition = 0;
            }

            return bRet;
        }

        public static bool GetEntityConfigFor(string entityName, out Type entityType)
        {
            bool bRet = false;

            if (IsEntityAlreadyRegistered(entityName))
            {
                entitiesBag.TryGetValue(entityName, out EntityConfig config);

                entityType = config.EntityType;
                bRet = true;
            }
            else
            {
                entityType = null;
            }

            return bRet;
        }

        public static bool GetEntityConfigFor(string entityName, out string dictionaryName)
        {
            bool bRet = false;

            if (IsEntityAlreadyRegistered(entityName))
            {
                entitiesBag.TryGetValue(entityName, out EntityConfig config);

                dictionaryName = config.DictionaryName; ;
                bRet = true;
            }
            else
            {
                dictionaryName = null;
            }

            return bRet;
        }

        public static bool GetEntityConfigFor(Type entityType, out string entityName)
        {
            bool bRet = false;

            if (IsEntityAlreadyRegistered(entityType))
            {
                entitiesCrossReference.TryGetValue(entityType, out entityName);
                bRet = true;
            }
            else
            {
                entityName = null;
            }

            return bRet;
        }

        public static bool GetEntityConfigFor(Type entityType, out string entityName, out string dictionaryName)
        {
            bool bRet = false;

            if (IsEntityAlreadyRegistered(entityType))
            {
                entitiesCrossReference.TryGetValue(entityType, out entityName);
                entitiesBag.TryGetValue(entityName, out EntityConfig config);

                dictionaryName = config.DictionaryName;

                bRet = true;
            }
            else
            {
                entityName = null;
                dictionaryName = null;
            }

            return bRet;
        }

        public static bool GetEntityConfig( string entityName, out long partition )
        {
            bool bRet = false;

            if(IsEntityAlreadyRegistered(entityName))
            {
                entitiesBag.TryGetValue(entityName, out EntityConfig config);

                partition = config.Partition;
                bRet = true;
            }
            else
            {
                partition = 0;
            }

            return bRet;
        }

        public static bool GetEntityConfigForType(Type entityType, out long partition)
        {
            bool bRet = false;

            if (IsEntityAlreadyRegistered(entityType))
            {
                entitiesCrossReference.TryGetValue(entityType, out string entityName);
                entitiesBag.TryGetValue(entityName, out EntityConfig config);

                partition = config.Partition;
                bRet = true;
            }
            else
            {
                partition = 0;
            }

            return bRet;
        }


        public static bool IsEntityAlreadyRegistered(string entityName)
        {
            return entitiesBag.ContainsKey(entityName);
        }

        public static bool IsEntityAlreadyRegistered(Type entityType)
        {
            return entitiesCrossReference.ContainsKey(entityType);
        }

        public static bool RegisterEntity( string dictionaryName, string entityName, Type entityType )
        {
            bool bRet = false;

            if( !IsEntityAlreadyRegistered(entityName))
            {
                entitiesBag.TryAdd(entityName, new EntityConfig(dictionaryName, entityName, entityType));
                entitiesCrossReference.TryAdd(entityType, entityName);
                bRet = true;
            }

            return bRet;
        }

        public static bool UnRegisterEntity(string entityName )
        {
            bool bRet = false;

            if (IsEntityAlreadyRegistered(entityName))
            {
                GetEntityConfigFor(entityName, out Type entityType);

                entitiesCrossReference.TryRemove(entityType, out string name);
                entitiesBag.TryRemove(entityName, out EntityConfig config );
                bRet = true;
            }

            return bRet;
        }


        // PRIVATE CLASSES
        private class EntityConfig
        {
            public EntityConfig( string dictionaryName, string entityName, Type entityType )
            {
                this.DictionaryName = dictionaryName;
                this.EntityName = entityName.ToLower();
                this.Partition = FnvHash.Hash(this.EntityName);
                this.EntityType = entityType;
            }

            public string DictionaryName { get; private set; }

            public string EntityName { get; private set; }
            
            public long Partition { get; private set; }

            public Type EntityType { get; private set; }
        }
    }
}
