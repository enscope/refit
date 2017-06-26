﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Refit
{
    class FormValueDictionary : Dictionary<string, string>
    {
        static readonly Dictionary<Type, PropertyInfo[]> propertyCache
            = new Dictionary<Type, PropertyInfo[]>();

        public FormValueDictionary(object source) 
        {
            if (source == null) return;

            if (source is IDictionary dictionary)
            {
                foreach (var key in dictionary.Keys)
                {
                    Add(key.ToString(), string.Format("{0}", dictionary[key]));
                }

                return;
            }

            var type = source.GetType();

            lock (propertyCache) {
                if (!propertyCache.ContainsKey(type)) {
                    propertyCache[type] = GetProperties(type);
                }

                foreach (var property in propertyCache[type]) {
                    Add(GetFieldNameForProperty(property), string.Format("{0}", property.GetValue(source, null)));
                }
            }
        }

        PropertyInfo[] GetProperties(Type type) 
        {
            return type.GetProperties()
                .Where(p => p.CanRead)
                .ToArray();
        }

        string GetFieldNameForProperty(PropertyInfo propertyInfo)
        {
            var aliasAttr = propertyInfo.GetCustomAttributes(true)
                .OfType<AliasAsAttribute>()
                .FirstOrDefault();
            return aliasAttr != null ? aliasAttr.Name : propertyInfo.Name;
        }
    }
}
