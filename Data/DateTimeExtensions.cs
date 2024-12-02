using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Data
{
	public static class DateTimeExtensions
	{
		public static DateTime ToUtc(this DateTime dateTime) 
		{
			if (dateTime.Kind == DateTimeKind.Utc) 
			{
				return dateTime;
			}

			return dateTime.ToUniversalTime();
		}
	}
}