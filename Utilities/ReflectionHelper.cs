using System.Reflection;

namespace RestClient
{
    public static class ReflectionHelper
    {
        public static object GetProperty(this object obj, string propertyName)
        {
            if (obj == null)
                return null;

            return obj.GetType().GetProperty(propertyName).GetValue(obj, null);
        }

        public static void SetProperty(this object obj, string propertyName, object value)
        {
            if (obj == null)
                return;

            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
                prop.SetValue(obj, value, null);
        }
    }
}
