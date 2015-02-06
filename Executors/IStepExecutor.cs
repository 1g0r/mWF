using System;

namespace mWF.Executors
{
	/// <summary>
	/// Represets interface for pipeline actions executions.
	/// </summary>
	internal interface IStepExecutor
	{
		/// <summary>
		/// Executes single step of the pipeline.
		/// </summary>
		/// <param name="step">Action to be executed.</param>
		/// <param name="errorHandler">Compensation action that executes when main action failed.</param>
		TResult Execute<TResult>(Func<TResult> step, Action<Exception> errorHandler);
	}
}
