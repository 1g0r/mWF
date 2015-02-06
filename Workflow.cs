using System;
using System.Collections.Generic;
using System.Diagnostics;
using mWF.Executors;

namespace mWF
{
	[DebuggerStepThrough]
	public class Workflow<TRequest, TStepOrdinal>
		where TStepOrdinal : struct
		where TRequest : RequestBase<TStepOrdinal>
	{
		private readonly IStepExecutor _executor;
		private readonly IErrorHandler<TRequest, TStepOrdinal> _defaultErrorHandler;
		private readonly IInterceptor<TRequest, TStepOrdinal> _defaultInterceptor;
		private readonly TRequest _request;

		internal Workflow(
			TRequest request,
			IStepExecutor executor,
			IErrorHandler<TRequest, TStepOrdinal> defaultErrorHandler,
			IInterceptor<TRequest, TStepOrdinal> defaultInterceptor)
		{
			if (executor == null)
				throw new ArgumentNullException("executor");
			_executor = executor;
			_defaultErrorHandler = defaultErrorHandler;
			_defaultInterceptor = defaultInterceptor;
			_request = request;
		}

		public Workflow<TRequest, TStepOrdinal> InvokeStep(
			Action<TRequest> stepFunc,
			TStepOrdinal stepOrdinal,
			IErrorHandler<TRequest, TStepOrdinal> customErrorHandler = null,
			IInterceptor<TRequest, TStepOrdinal> customInterceptor = null)
		{
			if (!_request.Continue(stepOrdinal))
				return this;

			_executor.Execute(
				Wrap<bool>(stepFunc, stepOrdinal, customInterceptor ?? _defaultInterceptor),
				WrapHandler(stepOrdinal, customErrorHandler ?? _defaultErrorHandler));

			return this;
		}

		public Workflow<TRequest, TStepOrdinal> InvokeLoop<TSubRequest, TSubStepOrdinal>(
			Func<TRequest, IEnumerable<TSubRequest>> itemsGetter,
			TStepOrdinal stepOrdinal,
			Action<TSubRequest> forEachAction,
			IErrorHandler<TRequest, TStepOrdinal> customErrorHandler = null,
			IInterceptor<TRequest, TStepOrdinal> customInterceptor = null)
			where TSubStepOrdinal : struct
			where TSubRequest : RequestBase<TSubStepOrdinal>
		{
			if (!_request.Continue(stepOrdinal))
				return this;

			var items = _executor.Execute(
				Wrap(itemsGetter, stepOrdinal, customInterceptor ?? _defaultInterceptor),
				WrapHandler(stepOrdinal, customErrorHandler ?? _defaultErrorHandler));

			//The loop
			if (items != null)
			{
				foreach (var item in items)
				{
					forEachAction(item);
				}
			}
			return this;
		}

		private Func<TResult> Wrap<TResult>(Action<TRequest> func, TStepOrdinal stepOrdinal, IInterceptor<TRequest, TStepOrdinal> interceptor)
		{
			if (interceptor == null)
				return () =>
				{
					func(_request);
					return default(TResult);
				};
			return () =>
			{
				interceptor.WrapCall(_request, stepOrdinal, func);
				return default(TResult);
			};
		}

		private Func<IEnumerable<TResult>> Wrap<TResult>(Func<TRequest, IEnumerable<TResult>> func, TStepOrdinal stepOrdinal, IInterceptor<TRequest, TStepOrdinal> interceptor)
		{
			if (interceptor == null)
				return () => func(_request);
			return () => interceptor.WrapCall(_request, stepOrdinal, func);
		}

		private Action<Exception> WrapHandler(TStepOrdinal stepOrdinal, IErrorHandler<TRequest, TStepOrdinal> errorHandler)
		{
			if (errorHandler == null)
				return null;
			return (ex) =>
			{
				_request.WasError = true;
				errorHandler.HandleError(_request, stepOrdinal, ex);
			};
		}
	}
}
