using System;

namespace mWF
{
	/// <summary>
	/// Represents interface for error handlers.
	/// </summary>
	/// <typeparam name="TRequest">The request type.</typeparam>
	/// <typeparam name="TStepOrdinal">Step ordinal type.</typeparam>
	public interface IErrorHandler<in TRequest, in TStepOrdinal>
		where TStepOrdinal : struct
		where TRequest : RequestBase<TStepOrdinal>
	{
		void HandleError(TRequest request, TStepOrdinal stepOrdinal, Exception exception);
	}
}
