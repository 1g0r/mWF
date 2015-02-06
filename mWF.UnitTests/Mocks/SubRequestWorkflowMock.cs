using System.Threading;

namespace mWF.UnitTests.Mocks
{
	internal class SubRequestWorkflowMock
	{
		public void ReadString(SubRequestMock request)
		{
			request.StrValue = request.InitialString;
		}

		public void Parse(SubRequestMock request)
		{
			request.Value = int.Parse(request.StrValue);
		}

		public void Mul(SubRequestMock request)
		{
			request.Value = checked(2 * request.Value);
		}

		public void AddValue(SubRequestMock request)
		{
			request.ParentRequest.Values.Add(request.Value);
		}

		public void Sleep(SubRequestMock request)
		{
			if (request.Value == 6 && request.SleepTimeout.HasValue)
			{
				Thread.Sleep(request.SleepTimeout.Value);
			}
		}
	}

}
