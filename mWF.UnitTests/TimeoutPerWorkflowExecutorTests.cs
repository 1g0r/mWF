using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using mWF.UnitTests.Mocks;
using mWF.Helpers;

namespace mWF.UnitTests
{
	[TestClass]
	public class TimeoutPerWorkflowExecutorTests
	{
		[TestMethod]
		public void InvokeChain()
		{
			//Arrange
			var request = new RequestMock("3", RequestStepMock.Step1);

			//Act
			RequestPipelineMock.ExecuteChain(request, new TimeSpan(0, 0, 2), TimeoutLifetime.Workflow);

			//Assert
			Assert.IsTrue(request.Done, "Цепочка вызовов не дошла до конца.");
			Assert.AreEqual(request.StrValue, "3", "Инициализация не выполнилась.");
			Assert.AreEqual(request.Value, 6, "Парсинг или умножение прошло не по плану");
		}

		[TestMethod]
		public void InvokeChain_DefaultErrorHandler()
		{
			//Arrange
			var initialValue = int.MaxValue.ToString(CultureInfo.InvariantCulture);
			var request = new RequestMock(initialValue, RequestStepMock.Step1);
			var handler = new ErrorHandlerMock();

			//Act
			RequestPipelineMock.ExecuteChain(request, new TimeSpan(0, 0, 2), TimeoutLifetime.Workflow, handler);

			//Assert
			Assert.IsFalse(request.Done, "Цепочка вызовов не должна была дойти до конца.");
			Assert.AreEqual(request.StrValue, initialValue, "Инициализация не выполнилась.");
			Assert.IsNull(request.Error, "Кастомный обработчик не должен был обработать исключение.");
			Assert.IsNotNull(handler.Error, "Исключение перехватить не удалось.");
			Assert.IsTrue(handler.Error is OverflowException, "Исполнитель должен возвращать исходную ошибку.");
			Assert.IsTrue(handler.Error.Message == "Arithmetic operation resulted in an overflow.");
		}

		[TestMethod]
		public void InvokeChain_CustomErrorHandler()
		{
			//Arrange
			var initialValue = int.MaxValue.ToString(CultureInfo.InvariantCulture);
			var request = new RequestMock(initialValue, RequestStepMock.Step1);
			var defaultHandler = new ErrorHandlerMock();
			var customHandler = new ErrorHandlerMock();


			//Act
			RequestPipelineMock.ExecuteChain(
				request,
				new TimeSpan(0, 0, 2),
				TimeoutLifetime.Workflow,
				defaultHandler,
				customErrorHandler: customHandler);

			//Assert
			Assert.IsFalse(request.Done, "Цепочка вызовов не должна была дойти до конца.");
			Assert.AreEqual(request.StrValue, initialValue, "Инициализация не выполнилась.");
			Assert.IsNull(defaultHandler.Error, "Обработчик по умолчанию не должен был отработать.");
			Assert.IsNotNull(customHandler.Error, "Кастомный обработчик отработал неправильно.");
			Assert.IsTrue(customHandler.Error is OverflowException, "IStepExecutor должен возвращать исходную ошибку.");
			Assert.IsTrue(customHandler.Error.Message == "Arithmetic operation resulted in an overflow.");
		}

		[TestMethod]
		public void InvokeChain_Timeout()
		{
			//Arrange
			const string initialValue = "33";
			var request = new RequestMock(initialValue, RequestStepMock.Step1, null, new TimeSpan(0, 0, 2));
			var customHandler = new ErrorHandlerMock();

			//Act
			RequestPipelineMock.ExecuteChain(
				request, 
				new TimeSpan(0, 0, 1), 
				TimeoutLifetime.Workflow, 
				customErrorHandler: customHandler);

			//Assert
			Assert.IsFalse(request.Done, "Цепочка вызовов не должна была дойти до конца.");
			Assert.IsNotNull(customHandler.Error, "Кастомный обработчик отработал неправильно.");
			Assert.IsTrue(customHandler.Error is OperationCanceledException, "IStepExecutor должен возвращать исходную ошибку.");
			Assert.IsTrue(customHandler.Error.Message == "The operation was canceled.");
		}

		[TestMethod]
		public void InvokeChain_DefaultInterceptor()
		{
			//Arrange
			const string initialValue = "11";
			var request = new RequestMock(initialValue, RequestStepMock.Step1, null, new TimeSpan(0, 0, 2));
			var customHandler = new ErrorHandlerMock();
			var interceptor = new InterceptorMock();
			const RequestStepMock passedSteps =
				RequestStepMock.Step1 |
				RequestStepMock.Step2 |
				RequestStepMock.Step3 |
				RequestStepMock.Step4;

			//Act
			RequestPipelineMock.ExecuteChain(request, new TimeSpan(0, 0, 1), TimeoutLifetime.Workflow,
				null, interceptor, customHandler);

			//Assert
			Assert.IsNotNull(customHandler.Error, "Кастомный обработчик отработал неправильно.");
			Assert.IsTrue(customHandler.Error is OperationCanceledException, "IStepExecutor должен возвращать исходную ошибку.");
			Assert.IsFalse(interceptor.DefaultMethod, "Должен был отработать кастомный интерсептор.");
			Assert.AreEqual(interceptor.LastStep, passedSteps, "Должны перехватываться все предыдущие шаги в цепочке.");
		}

		[TestMethod]
		public void InvokeChain_CustomInterceptor()
		{
			//Arrange
			const string initialValue = "11";
			var request = new RequestMock(initialValue, RequestStepMock.Step1, null, new TimeSpan(0, 0, 2));
			var customHandler = new ErrorHandlerMock();
			var interceptor = new InterceptorMock();
			const RequestStepMock passedSteps =
				RequestStepMock.Step1 |
				RequestStepMock.Step2 |
				RequestStepMock.Step3 |
				RequestStepMock.Step4;

			//Act
			RequestPipelineMock.ExecuteChain(request, new TimeSpan(0, 0, 1), TimeoutLifetime.Workflow,
				null, null, customHandler, interceptor);

			//Assert
			Assert.IsNotNull(customHandler.Error, "Кастомный обработчик отработал неправильно.");
			Assert.IsTrue(customHandler.Error is OperationCanceledException, "IStepExecutor должен возвращать исходную ошибку.");
			Assert.IsFalse(interceptor.DefaultMethod, "Должен был отработать кастомный интерсептор.");
			Assert.AreEqual(interceptor.LastStep, passedSteps, "Должны перехватываться все предыдущие шаги в цепочке.");
		}

		[TestMethod]
		public void InvokeChain_Continue()
		{
			//Arrange
			const string initialValue = "11";
			var request = new RequestMock(initialValue, RequestStepMock.Step3)
			{
				Value = 6
			};
			var customHandler = new ErrorHandlerMock();
			var interceptor = new InterceptorMock();
			const RequestStepMock passedSteps =
				RequestStepMock.Step3 |
				RequestStepMock.Step4 |
				RequestStepMock.Done;

			//Act
			RequestPipelineMock.ExecuteChain(request, new TimeSpan(0, 60, 1), TimeoutLifetime.Workflow,
				null, null, customHandler, interceptor);

			//Assert
			Assert.IsNull(customHandler.Error, "Ошибок быть не должно.");
			Assert.IsFalse(interceptor.DefaultMethod, "Должен был отработать кастомный интерсептор.");
			Assert.IsTrue(request.Default == initialValue, "Заявка должна быть правильно прионициализированна.");
			Assert.IsTrue(request.StrValue.IsNullOrEmpty(), "Первый шаг не должен был пройти");
			Assert.IsTrue(request.Value == 12, "Неправильный результат заявки.");
			Assert.AreEqual(interceptor.LastStep, passedSteps, "Должны перехватываться все предыдущие шаги в цепочке.");
		}
	}
}
