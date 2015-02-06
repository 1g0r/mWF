using System;

namespace mWF.UnitTests.Mocks
{
	[Flags]
	internal enum RequestStepMock : byte
	{
		Initial = 0,
		Step1 = 1,
		Step2 = 2,
		Step3 = 4,
		Step4 = 8,
		Step5 = 16,
		Step6 = 32,
		Step7 = 64,
		Done = 128
	}
}
