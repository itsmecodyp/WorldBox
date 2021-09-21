using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GeneralPurposeOnlineServer
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
        // ex: AccessExtensions.call(typeof(StackEffects), "startSpawnEffect", new object[] { actor.currentTile, "spawn" });
        public static object CallMethodAlternative<T>(this T obj, string methodName, params object[] args)
        {
            var type = typeof(T);
            var method = type.GetTypeInfo().GetDeclaredMethod(methodName);
            return method.Invoke(obj, args);
        }
        // found on: https://stackoverflow.com/questions/3303126/how-to-get-the-value-of-private-field-in-c/3303182
        public static object GetField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }
        // ex: string firstName = AccessExtensions.GetField(data.GetType(), data, "firstName") as string;
        // .SetField(actor, "data", ) as ActorData;
        // List<SpriteRenderer> bodyParts = (List<SpriteRenderer>)Reflection.GetField(typeof(ActorBase), pActor, "bodyParts");
        public static void SetField<T>(object originalObject, string fieldName, T newValue)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = originalObject.GetType().GetField(fieldName, bindFlags);
            field.SetValue(originalObject, newValue);
        }
        // Vector3 targetAngle = (Vector3)Reflection.GetField(typeof(ActorBase), actor, "targetAngle");
        // Reflection.SetField(targetAngle, "targetAngle", default(Vector3));
    }

}
