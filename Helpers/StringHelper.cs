using System.Globalization;

namespace mWF.Helpers
{
	public static class StringHelper
	{
		public static bool IsNullOrEmpty(this string value)
		{
			return string.IsNullOrEmpty(value);
		}

		public static string Fill(this string format, params object[] args )
		{
			return string.Format(format, args);
		}

		public static string Convert(this int val)
		{
			return val.ToString(CultureInfo.InvariantCulture);
		}

		public static string Convert(this int? value)
		{
			return value.HasValue ? value.Value.Convert() : string.Empty;
		}
	}
}
