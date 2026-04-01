using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Database
{
    public static class ReflectionUtil
    {
        private static readonly Dictionary<string, Type> TypeLookupCache = new Dictionary<string, Type>();
        private static readonly Dictionary<string, List<Type>> DerivedTypeCache = new Dictionary<string, List<Type>>();

        public static Type FindTypeByFullName(string fullName, bool searchAllAssembliesIfFailed = true)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return null;

            if (TypeLookupCache.TryGetValue(fullName, out var cachedType))
                return cachedType;

            var type = Type.GetType(fullName);
            if (type != null)
            {
                TypeLookupCache[fullName] = type;
                return type;
            }

            if (!searchAllAssembliesIfFailed)
                return null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    type = assembly.GetType(fullName, throwOnError: false, ignoreCase: false);
                    if (type != null)
                    {
                        TypeLookupCache[fullName] = type;
                        return type;
                    }
                }
                catch
                {
                }
            }

            Debug.LogWarning($"[ReflectionUtil] Failed to resolve type '{fullName}'.");
            return null;
        }

        public static Type FindTypeByName(string typeName, bool searchAllAssembliesIfFailed = true)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            if (TypeLookupCache.TryGetValue(typeName, out var cachedType))
                return cachedType;

            var type = Type.GetType(typeName);
            if (type != null)
            {
                TypeLookupCache[typeName] = type;
                return type;
            }

            if (!searchAllAssembliesIfFailed)
                return null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    type = GetLoadableTypes(assembly).FirstOrDefault(candidate =>
                        candidate.Name == typeName || candidate.Name.Replace("+", ".") == typeName);

                    if (type != null)
                    {
                        TypeLookupCache[typeName] = type;
                        return type;
                    }
                }
                catch
                {
                }
            }

            Debug.LogWarning($"[ReflectionUtil] Failed to resolve type name '{typeName}'.");
            return null;
        }

        public static List<Type> FindDerivedTypes<T>() where T : class
        {
            return FindDerivedTypes(typeof(T));
        }

        public static List<Type> FindDerivedTypes(Type baseType)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetLoadableTypes)
                .Where(type => type != baseType && baseType.IsAssignableFrom(type) && !type.IsAbstract)
                .ToList();
        }

        public static List<Type> FindDerivedTypesWithCache<T>()
        {
            return FindDerivedTypesWithCache(typeof(T));
        }

        public static List<Type> FindDerivedTypesWithCache(Type baseType)
        {
            string cacheKey = baseType.FullName;
            if (cacheKey == null)
                throw new ArgumentException("Base type must have a full name.", nameof(baseType));

            if (DerivedTypeCache.TryGetValue(cacheKey, out var cachedTypes))
                return new List<Type>(cachedTypes);

            var derivedTypes = FindDerivedTypes(baseType);
            DerivedTypeCache[cacheKey] = derivedTypes;

            foreach (var type in derivedTypes.Where(type => !string.IsNullOrEmpty(type.FullName)))
                TypeLookupCache[type.FullName] = type;

            return new List<Type>(derivedTypes);
        }

        public static T GetAttribute<T>(Type type) where T : Attribute
        {
            var attrs = type.GetCustomAttributes(typeof(T), inherit: false);
            if (attrs.Length > 0)
                return attrs[0] as T;

            return null;
        }

        public static bool IsDerivedFrom<T>(Type type) where T : class
        {
            Type baseType = typeof(T);
            return type != baseType && baseType.IsAssignableFrom(type);
        }

        public static bool IsDerivedFrom(Type type, Type baseType)
        {
            return type != baseType && baseType.IsAssignableFrom(type);
        }

        public static bool IsValidFullEnumType(string enumName)
        {
            return FindTypeByFullName(enumName) is Type type && type.IsEnum;
        }

        public static bool IsValidEnumType(string enumName, out string @namespace)
        {
            @namespace = null;
            Type type = FindTypeByName(enumName);
            if (type != null && type.IsEnum)
            {
                @namespace = type.Namespace;
                return true;
            }

            return false;
        }

        internal static bool HasDerivedTypeCache(Type baseType)
        {
            return baseType?.FullName != null && DerivedTypeCache.ContainsKey(baseType.FullName);
        }

        internal static void ClearCachesForTests()
        {
            TypeLookupCache.Clear();
            DerivedTypeCache.Clear();
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(type => type != null);
            }
        }
    }
}
