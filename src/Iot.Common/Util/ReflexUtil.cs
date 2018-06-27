// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Iot.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Linq;
    using System.Text;

    public sealed class ReflexUtil
    {
        public static Type[] GetKnownTypesOnAssembly( Type objType, string assemblyName )
        {
            Assembly assembly = GetAssemblyFromAllAssemblies(assemblyName);

            if (assembly != null)
                return assembly.GetTypes().Where(x => x.IsSubclassOf(objType)).ToArray();
            else
                return new Type[0];
        }

        public static Assembly GetAssemblyFromAllAssemblies( string assemblyName )
        {
            Assembly assemblyRet = null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsDynamic)
                {
                    if (assembly.GetName().Equals(assemblyName))
                    {
                        assemblyRet = assembly;
                        break;
                    }
                }
            }

            if (assemblyRet == null)
                assemblyRet = Assembly.LoadFrom(assemblyName);

            return assemblyRet;
        }

        public static Type[] GetKnownTypesOnCurrentAssembly(Type objType)
        {
            return typeof(DeviceEvent).Assembly.GetTypes().Where(x => x.IsSubclassOf(objType)).ToArray();
        }

        public static IEnumerable<Type> GetKnownTypesFromAllCurrentDomainAssemblies(Type objType)
        {
            List<Object> notFoundAssemblies = new List<object>();
            StringBuilder messages = new StringBuilder();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsDynamic)
                {
                    Type[] exportedTypes = null;
                    try
                    {
                        exportedTypes = assembly.GetExportedTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        exportedTypes = ex.Types;
                    }
                    catch(FileNotFoundException ex )
                    {
                        String message = ex.Message;

                        messages.Append(message);

                        notFoundAssemblies.Add(assembly);
                    }

                    if (exportedTypes != null)
                    {
                        foreach (var type in exportedTypes)
                        {
                            if (type.IsSubclassOf(objType))
                                yield return type;
                        }
                    }
                }
            }
        }
    }
}
