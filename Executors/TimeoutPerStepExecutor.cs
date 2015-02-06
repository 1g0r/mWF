using System;
using System.Diagnostics;
using System.Threading;

namespace mWF.Executors
{
	[DebuggerStepThrough]
	internal class TimeoutPerStepExecutor : TimeoutExecutorBase
	{
		internal TimeoutPerStepExecutor(TimeSpan singleActionTimeout)
			: base(singleActionTimeout)
		{ }

		protected override TResult ExecuteSafe<TResult>(Func<TResult> action, CancellationToken token)
		{
			try
			{
				if (!token.IsCancellationRequested)
				{
					return action();
				}
				return default(TResult);
			}
			catch
			{
				// Поток уже отменен, поэтому побоку все его дальнейшие исключения
				if (!token.IsCancellationRequested)
					throw;
				return default(TResult);
			}
		}
	}
}
