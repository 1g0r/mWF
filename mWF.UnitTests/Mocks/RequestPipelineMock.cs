using System;

namespace mWF.UnitTests.Mocks
{
	internal static class RequestPipelineMock
	{
		public static void ExecuteChain(
			RequestMock request, 
			TimeSpan timeout, 
			TimeoutLifetime timeoutLifetime,
			IErrorHandler<RequestMock, RequestStepMock> defaultErrorHandler = null,
			IInterceptor<RequestMock, RequestStepMock> defaultInterceptor = null,
			IErrorHandler<RequestMock, RequestStepMock> customErrorHandler = null,
			IInterceptor<RequestMock, RequestStepMock> customInterceptor = null)
		{
			var wf = new RequestWorkflowMock();
			var pipe = WorkflowFactory.Create(request, timeout, timeoutLifetime, defaultErrorHandler, defaultInterceptor);

			pipe.InvokeStep(wf.ReadString, RequestStepMock.Step1, customErrorHandler, customInterceptor)
				.InvokeStep(wf.Parse, RequestStepMock.Step2, customErrorHandler, customInterceptor)
				.InvokeStep(wf.Mul, RequestStepMock.Step3, customErrorHandler, customInterceptor)
				.InvokeStep(wf.Sleep, RequestStepMock.Step4, customErrorHandler, customInterceptor)
				.InvokeStep(wf.Done, RequestStepMock.Done, customErrorHandler, customInterceptor);
		}

		public static void ExecuteLoop(
			RequestMock request,
			TimeSpan timeout,
			IErrorHandler<SubRequestMock, RequestStepMock> defaultErrorHandler = null,
			IInterceptor<SubRequestMock, RequestStepMock> defaultInterceptor = null,
			IErrorHandler<SubRequestMock, RequestStepMock> customErrorHandler = null,
			IInterceptor<SubRequestMock, RequestStepMock> customInterceptor = null)
		{
			var wf = new RequestWorkflowMock();
			var requestPipe = WorkflowFactory.Create<RequestMock, RequestStepMock>(request, new TimeSpan(0, 0, 3), TimeoutLifetime.Step);
			var wf2 = new SubRequestWorkflowMock();

			requestPipe
				.InvokeStep(wf.ReadString, RequestStepMock.Step1)
				.InvokeStep(wf.Parse, RequestStepMock.Step2)
				.InvokeLoop<SubRequestMock, RequestStepMock>(wf.GetStrings, RequestStepMock.Step7,  r =>
				{
					var pipe = WorkflowFactory.Create(
						r, timeout, TimeoutLifetime.Workflow, defaultErrorHandler, defaultInterceptor);

					pipe.InvokeStep(wf2.ReadString, RequestStepMock.Step1, customErrorHandler, customInterceptor)
						.InvokeStep(wf2.Parse, RequestStepMock.Step2, customErrorHandler, customInterceptor)
						.InvokeStep(wf2.Mul, RequestStepMock.Step3, customErrorHandler, customInterceptor)
						.InvokeStep(wf2.Sleep, RequestStepMock.Step4, customErrorHandler, customInterceptor)
						.InvokeStep(wf2.AddValue, RequestStepMock.Step5, customErrorHandler, customInterceptor);
				})
				.InvokeStep(wf.Done, RequestStepMock.Done);
		}
	}
}
