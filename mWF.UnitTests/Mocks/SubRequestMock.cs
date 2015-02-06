using System;

namespace mWF.UnitTests.Mocks
{
	internal class SubRequestMock : RequestBase<RequestStepMock>
	{

		public SubRequestMock(
			string initialValue, 
			RequestMock parentRequest,
			RequestStepMock? stateToStartFrom,
			TimeSpan? sleepTimeout = null)
			: base(stateToStartFrom)
		{
			InitialString = initialValue;
			ParentRequest = parentRequest;
			SleepTimeout = sleepTimeout;
		}

		public string InitialString { get; private set; }
		public RequestMock ParentRequest { get; private set; }

		public int Value { get; set; }

		public string StrValue { get; set; }

		public TimeSpan? SleepTimeout { get; private set; }

	}
}
