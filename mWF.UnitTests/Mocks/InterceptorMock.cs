using System;
using System.Collections.Generic;

namespace mWF.UnitTests.Mocks
{
	internal class InterceptorMock : IInterceptor<RequestMock, RequestStepMock>
	{
		internal bool DefaultMethod { get; set; }

		internal RequestStepMock LastStep { get; set; }

		public void WrapCall(RequestMock request, RequestStepMock stepOrdinal, Action<RequestMock> stepAction)
		{
			DefaultMethod = false;
			LastStep |= stepOrdinal;
			stepAction(request);
		}

		public IEnumerable<TResult> WrapCall<TResult>(RequestMock request, RequestStepMock stepOrdinal, Func<RequestMock, IEnumerable<TResult>> stepFunc)
		{
			DefaultMethod = false;
			LastStep |= stepOrdinal;
			return stepFunc(request);
		}
	}

	internal class SubInterceptorMock : IInterceptor<SubRequestMock, RequestStepMock>
	{
		internal bool DefaultMethod { get; set; }

		internal RequestStepMock LastStep { get; set; }

		public void WrapCall(SubRequestMock request, RequestStepMock stepOrdinal, Action<SubRequestMock> stepAction)
		{
			DefaultMethod = false;
			LastStep |= stepOrdinal;
			stepAction(request);
		}

		public IEnumerable<TResult> WrapCall<TResult>(SubRequestMock request, RequestStepMock stepOrdinal, Func<SubRequestMock, IEnumerable<TResult>> stepFunc)
		{
			DefaultMethod = false;
			LastStep |= stepOrdinal;
			return stepFunc(request);
		}
	}
}
