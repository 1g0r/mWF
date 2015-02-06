using System;
using System.Diagnostics;
using System.Threading;

namespace mWF.Executors
{
	[DebuggerStepThrough]
	internal class TimeoutPerWorkflowExecutor : TimeoutExecutorBase
	{
		internal TimeoutPerWorkflowExecutor(TimeSpan singleActionTimeout)
			: base(singleActionTimeout)
		{ }

		protected override TResult ExecuteSafe<TResult>(Func<TResult> action, CancellationToken token)
		{
			try
			{
				if (!token.IsCancellationRequested)
				{
					var w = new Stopwatch();
					w.Start();
					var result = action();
					w.Stop();
					var newTimeout = SingleActionTimeout.Add(w.Elapsed.Negate());
					SingleActionTimeout = newTimeout.TotalMilliseconds > 0 ? newTimeout : TimeSpan.Zero;
					return result;
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
