using System;
using System.Collections.Generic;

namespace mWF.UnitTests.Mocks
{
	internal class RequestMock : RequestBase<RequestStepMock>
	{
		private readonly List<int> _values = new List<int>();
		private readonly List<string> _strings;
		public RequestMock(
			string defaultValue,
			RequestStepMock stateToStartFrom,
			List<string> strings = null,
			TimeSpan? sleepTimeout = null)
			: base(stateToStartFrom)
		{
			Default = defaultValue;
			_strings = strings ?? new List<string>();
			SleepTimeout = sleepTimeout;
		}

		public RequestMock ParentRequest { get; set; }

		public string Default { get; private set; }

		public string StrValue { get; set; }

		public int Value { get; set; }

		public bool Done { get; set; }

		public List<int> Values { get { return _values; } }

		public List<string> Strings { get { return _strings; } }

		public Exception Error { get; set; }

		public TimeSpan? SleepTimeout { get; private set; }
	}
}
