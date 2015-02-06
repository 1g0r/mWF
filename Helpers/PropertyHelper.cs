using System;
using System.Reflection;

namespace mWF.Helpers
{
	internal static class PropertyHelper
	{
		public static void SetProperty<T>(this T obj, string name, object value)
		{
			var pi = typeof(T).GetProperty(name, BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (pi.GetSetMethod(true) != null)
				pi.Do(p => p.SetValue(obj, value, null));
			else
			{
				obj.GetType().GetBackingField(name)
					.Do(f => f.SetValue(obj, value));
			}
		}

		private static FieldInfo GetBackingField(this Type type, string name)
		{
			if (type == typeof(Object))
				return null;

			var field = type.GetField(
				string.Format("<{0}>k__BackingField", name),
				BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance
				);

			if (field != null)
				return field;
			return type.BaseType.GetBackingField(name);

		}

		public static TResult GetProperty<T, TResult>(this T obj, string name)
		{
			var pi = typeof(T).GetProperty(name, BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			return pi.Return(p => (TResult)p.GetValue(obj, null), default(TResult));
		}
	}
}
