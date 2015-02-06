using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace mWF.UnitTests.Mocks
{
	internal class RequestWorkflowMock
	{
		public void ReadString(RequestMock request)
		{
			request.StrValue = request.Default;
		}

		public void Parse(RequestMock request)
		{
			request.Value = int.Parse(request.StrValue);
		}

		public void Mul(RequestMock request)
		{
			request.Value = checked(2 * request.Value);
		}

		public IEnumerable<SubRequestMock> GetStrings(RequestMock request)
		{
			return request.Strings.Select(s => new SubRequestMock(s, request, null, request.SleepTimeout));
		}

		public void Sleep(RequestMock request)
		{
			if (request.SleepTimeout.HasValue)
			{
				Thread.Sleep(request.SleepTimeout.Value);
			}
		}

		public void Done(RequestMock request)
		{
			request.Done = true;
		}
	}
}
