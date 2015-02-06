using System;
using System.Collections.Generic;

namespace mWF.UnitTests.Mocks
{
	internal class ErrorHandlerMock : IErrorHandler<RequestMock, RequestStepMock>
	{
		public Exception Error { get; private set; }
		public void HandleError(RequestMock value, RequestStepMock stepOrdinal, Exception exception)
		{
			Error = exception;
		}
	}

	internal class SubErrorHandlerMock : IErrorHandler<SubRequestMock, RequestStepMock>
	{
		private readonly List<Exception> _exceptions = new List<Exception>();
		public List<Exception> Errors { get { return _exceptions; } }
		public void HandleError(SubRequestMock value, RequestStepMock stepOrdinal, Exception exception)
		{
			Errors.Add(exception);
		}
	}
}
