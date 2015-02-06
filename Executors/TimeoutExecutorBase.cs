using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace mWF.Executors
{
	[DebuggerStepThrough]
	internal abstract class TimeoutExecutorBase : IStepExecutor
	{
		protected TimeSpan SingleActionTimeout;

		protected TimeoutExecutorBase(
			TimeSpan singleActionTimeout)
		{
			SingleActionTimeout = singleActionTimeout;
		}

		public TResult Execute<TResult>(Func<TResult> action, Action<Exception> errorHandler)
		{
			try
			{
				using (var tokenSource = new CancellationTokenSource())
				{
					var token = tokenSource.Token;
					var task = new Task<TResult>(() => ExecuteSafe(action, token), token);

					task.Start();
					if (!task.Wait(SingleActionTimeout))
					{
						tokenSource.Cancel();
						token.ThrowIfCancellationRequested();
					}
					return task.Result;
				}
			}
			catch (Exception ex)
			{
				if (errorHandler != null)
				{
					errorHandler(StripException(ex));
					return default(TResult);
				}
				throw;
			}
		}

		protected abstract TResult ExecuteSafe<TResult>(Func<TResult> action, CancellationToken token);

		private Exception StripException(Exception ex)
		{
			var ae = ex as AggregateException;
			if (ae != null)
			{
				ae.Handle(e => true);
				//исключение должно быть только одно, т.к. поток мы создаем только один.
				return ae.InnerExceptions.FirstOrDefault() ?? ae.InnerException;
			}
			return ex;
		}
	}
}
