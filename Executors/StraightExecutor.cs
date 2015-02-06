using System;
using System.Diagnostics;

namespace mWF.Executors
{
	[DebuggerStepThrough]
	internal class StraightExecutor : IStepExecutor
	{
		public TResult Execute<TResult>(Func<TResult> step, Action<Exception> errorHandler)
		{
			try
			{
				return step();
			}
			catch (Exception ex)
			{
				if (errorHandler != null)
				{
					errorHandler(ex);
					return default (TResult);
				}
				throw;
			}
		}
	}
}
