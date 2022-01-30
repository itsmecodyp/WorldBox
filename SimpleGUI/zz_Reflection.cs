using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimpleGUI
{
    
    public static class Reflection
    {
        // found on https://stackoverflow.com/questions/135443/how-do-i-use-reflection-to-invoke-a-private-method
        public static object CallMethod(this object o, string methodName, params object[] args)
        {
            var mi = o.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (mi != null)
            {
                return mi.Invoke(o, args);
            }
            return null;
        }
        // found on: https://stackoverflow.com/questions/3303126/how-to-get-the-value-of-private-field-in-c/3303182
        public static object GetField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }
        public static void SetField<T>(object originalObject, string fieldName, T newValue)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = originalObject.GetType().GetField(fieldName, bindFlags);
            field.SetValue(originalObject, newValue);
        }
    }

    
    // should be helpful for random tasks like saving actor data between map sessions using a text file
    public static class GetObjectProperties
    {

        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        public static List<string> GetProps(GameObject obj)
        {
            List<string> PropertiesList = new List<string>();
            GameObject Object = obj;
            FieldInfo[] fields;
            Component[] Components = obj.GetComponents(typeof(Component));
            foreach (var comp in Components)
            {
                fields = comp.GetType().GetFields();
                foreach (var fi in fields)
                {
                    PropertiesList.Add(fi.ToString());
                }
            }
            return PropertiesList;
        }
    }

}
