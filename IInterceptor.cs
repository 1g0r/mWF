using System;
using System.Collections.Generic;

namespace mWF
{
	/// <summary>
	/// Represents interception interface.
	/// </summary>
	public interface IInterceptor<TRequest, in TStepOrdinal>
		where TStepOrdinal : struct
		where TRequest : RequestBase<TStepOrdinal>
	{
		void WrapCall(TRequest request, TStepOrdinal stepOrdinal, Action<TRequest> stepAction);

		IEnumerable<TResult> WrapCall<TResult>(TRequest request, TStepOrdinal stepOrdinal, Func<TRequest, IEnumerable<TResult>> stepFunc);
	}
}
