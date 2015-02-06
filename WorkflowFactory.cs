using System;
using System.Diagnostics;
using mWF.Executors;

namespace mWF
{
	[DebuggerStepThrough]
	public static class WorkflowFactory
	{
		public static Workflow<TRequest, TStepOrdinal> Create<TRequest, TStepOrdinal>(
			TRequest request,
			TimeSpan timeout,
			TimeoutLifetime timeoutLifetime,
			IErrorHandler<TRequest, TStepOrdinal> defaultErrorHandler = null,
			IInterceptor<TRequest, TStepOrdinal> defaultInterceptor = null)
			where TStepOrdinal : struct
			where TRequest : RequestBase<TStepOrdinal>
		{
			return new Workflow<TRequest, TStepOrdinal>(
				request,
				timeoutLifetime == TimeoutLifetime.Workflow ?
					(IStepExecutor)new TimeoutPerWorkflowExecutor(timeout) :
					new TimeoutPerStepExecutor(timeout),
				defaultErrorHandler,
				defaultInterceptor);
		}

		public static Workflow<TRequest, TStepOrdinal> Create<TRequest, TStepOrdinal>(
			TRequest request,
			TimeSpan timeout,
			IErrorHandler<TRequest, TStepOrdinal> defaultErrorHandler = null,
			IInterceptor<TRequest, TStepOrdinal> defaultInterceptor = null)
			where TStepOrdinal : struct
			where TRequest : RequestBase<TStepOrdinal>
		{
			return new Workflow<TRequest, TStepOrdinal>(
				request,
				new StraightExecutor(),
				defaultErrorHandler,
				defaultInterceptor);
		}
	}
}
