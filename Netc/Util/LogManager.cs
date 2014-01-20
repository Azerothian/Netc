using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace Netc.Util
{
	public class LogManager
	{

		public delegate void LogDelegate(LogType type, string message, params object[] objects);

		public static event LogDelegate OnLog;

		public static void Critical(string message, params object[] objects)
		{
			if(OnLog != null)
			{
				OnLog(LogType.Critical, message, objects);
			}
		}

		public static void Info(string message, params object[] objects)
		{
			if (OnLog != null)
			{
				OnLog(LogType.Information, message, objects);
			}
		}
		[Serializable]
		public enum LogType
		{
			Information,
			Warning,
			Critical

		}

		internal static void Log(LogType logType, string message, params object[] objects)
		{
			if (OnLog != null)
			{
				OnLog(logType, message, objects);
			}
		}
	}
}
