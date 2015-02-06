using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using mWF.UnitTests.Mocks;
using mWF.Helpers;

namespace mWF.UnitTests
{
	[TestClass]
	public class TimeoutPerStepExecutorTests
	{
		[TestMethod]
		public void InvokeChain()
		{
			//Arrange
			var request = new RequestMock("3", RequestStepMock.Step1);

			//Act
			RequestPipelineMock.ExecuteChain(request, new TimeSpan(0, 0, 2), TimeoutLifetime.Step);

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
			RequestPipelineMock.ExecuteChain(request, new TimeSpan(0, 0, 2), TimeoutLifetime.Step, handler);

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
				TimeoutLifetime.Step, 
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
			RequestPipelineMock.ExecuteChain(request, new TimeSpan(0, 0, 1), TimeoutLifetime.Step, customErrorHandler: customHandler);

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
			RequestPipelineMock.ExecuteChain(request, new TimeSpan(0, 0, 1), TimeoutLifetime.Step,
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
			RequestPipelineMock.ExecuteChain(request, new TimeSpan(0, 0, 1), TimeoutLifetime.Step,
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
			RequestPipelineMock.ExecuteChain(request, new TimeSpan(0, 60, 1), TimeoutLifetime.Step,
				null, null, customHandler, interceptor);

			//Assert
			Assert.IsNull(customHandler.Error, "Ошибок быть не должно.");
			Assert.IsFalse(interceptor.DefaultMethod, "Должен был отработать кастомный интерсептор.");
			Assert.IsTrue(request.Default == initialValue, "Заявка должна быть правильно прионициализированна.");
			Assert.IsTrue(request.StrValue.IsNullOrEmpty(), "Первый шаг не должен был пройти");
			Assert.IsTrue(request.Value == 12, "Неправильный результат заявки.");
			Assert.AreEqual(interceptor.LastStep, passedSteps, "Должны перехватываться все предыдущие шаги в цепочке.");
		}

		//LOOP
		[TestMethod]
		public void Loop_AllOk()
		{
			//Arrange
			const string initialValue = "33";
			var initialValues = new List<string> {"1", "2", "9", "4", "5", "6"};
			var request = new RequestMock(initialValue, RequestStepMock.Step1, initialValues, new TimeSpan(0, 0, 2));
			
			//Act
			RequestPipelineMock.ExecuteLoop(request, new TimeSpan(0, 0, 3));

			//Assert
			Assert.IsTrue(request.Done, "Заявка должна была дойти до конца.");
			Assert.IsFalse(request.Strings == null || request.Strings.Count < 1, "Инициализация строк прошла неудачно.");
			Assert.IsFalse(request.Values == null || request.Values.Count < 1, "Цикл не прошел совсем.");
			Assert.IsTrue(request.Strings.Count == request.Values.Count, "Количество строк и значений различаются.");
			
		}

		[TestMethod]
		public void Loop_DefaultErrorHandler()
		{
			//Arrange
			const string initialValue = "33";
			var maxInt = int.MaxValue.ToString(CultureInfo.InvariantCulture);
			var initialValues = new List<string> { "1", "2", "3", "4", maxInt, "6" };
			var request = new RequestMock(initialValue, RequestStepMock.Step1, initialValues, new TimeSpan(0, 0, 3));
			var defaultErrorHandler = new SubErrorHandlerMock();

			//Act
			RequestPipelineMock.ExecuteLoop(request, new TimeSpan(0, 0, 2), defaultErrorHandler);

			//Assert
			Assert.IsTrue(request.Done, "Заявка должна была дойти до конца.");
			Assert.IsFalse(request.Strings == null || request.Strings.Count < 1, "Инициализация строк прошла неудачно.");
			Assert.IsFalse(request.Values == null || request.Values.Count < 1, "Цикл не прошел совсем.");
			Assert.IsTrue(request.Values.Count == request.Strings.Count - 2, "При возникновении ошибки цикл должен пережодить на следующую итерацию.");
			Assert.IsTrue(defaultErrorHandler.Errors.Count == 2, "Цикл должен обрабатывать ошибки каждой итерации.");
			Assert.IsTrue(defaultErrorHandler.Errors[0] is OperationCanceledException, "Сначала должен быть таймаут");
			Assert.IsTrue(defaultErrorHandler.Errors[1] is OverflowException, "Потом должено быть переполнение");
		}

		[TestMethod]
		public void Loop_CustomErrorHandler()
		{
			//Arrange
			const string initialValue = "33";
			var maxInt = int.MaxValue.ToString(CultureInfo.InvariantCulture);
			var initialValues = new List<string> { "1", "2", "3", "4", maxInt, "6" };
			var request = new RequestMock(initialValue, RequestStepMock.Step1, initialValues, new TimeSpan(0, 0, 3));
			var customErrorHandler = new SubErrorHandlerMock();

			//Act
			RequestPipelineMock.ExecuteLoop(request, new TimeSpan(0, 0, 2), null, customErrorHandler: customErrorHandler);

			//Assert
			Assert.IsTrue(request.Done, "Заявка должна была дойти до конца.");
			Assert.IsFalse(request.Strings == null || request.Strings.Count < 1, "Инициализация строк прошла неудачно.");
			Assert.IsFalse(request.Values == null || request.Values.Count < 1, "Цикл не прошел совсем.");
			Assert.IsTrue(request.Values.Count == request.Strings.Count - 2, "При возникновении ошибки цикл должен пережодить на следующую итерацию.");
			Assert.IsTrue(customErrorHandler.Errors.Count == 2, "Цикл должен обрабатывать ошибки каждой итерации.");
			Assert.IsTrue(customErrorHandler.Errors[0] is OperationCanceledException, "Сначала должен быть таймаут");
			Assert.IsTrue(customErrorHandler.Errors[1] is OverflowException, "Потом должено быть переполнение");
		}

		[TestMethod]
		public void Loop_DefaultInterceptor()
		{
			//Arrange
			const string initialValue = "33";
			var maxInt = int.MaxValue.ToString(CultureInfo.InvariantCulture);
			var initialValues = new List<string> { "1", "2", "3", "4", maxInt, "6" };
			var request = new RequestMock(initialValue, RequestStepMock.Step1, initialValues, new TimeSpan(0, 0, 3));
			var customErrorHandler = new SubErrorHandlerMock();

			
			var interceptor = new SubInterceptorMock();
			const RequestStepMock passedSteps =
				RequestStepMock.Step1 |
				RequestStepMock.Step2 |
				RequestStepMock.Step3 |
				RequestStepMock.Step4 |
				RequestStepMock.Step5;

			//Act
			RequestPipelineMock.ExecuteLoop(request, new TimeSpan(0, 0, 2), null, interceptor, customErrorHandler);

			//Assert
			Assert.IsTrue(request.Done, "Заявка должна была дойти до конца.");
			Assert.IsFalse(interceptor.DefaultMethod, "Должен был отработать кастомный интерсептор.");
			Assert.AreEqual(interceptor.LastStep, passedSteps, "Должны перехватываться все предыдущие шаги в цепочке.");
		}

		[TestMethod]
		public void Loop_CustomInterceptor()
		{
			//Arrange
			const string initialValue = "33";
			var maxInt = int.MaxValue.ToString(CultureInfo.InvariantCulture);
			var initialValues = new List<string> { "1", "2", "3", "4", maxInt, "6" };
			var request = new RequestMock(initialValue, RequestStepMock.Step1, initialValues, new TimeSpan(0, 0, 3));
			var customErrorHandler = new SubErrorHandlerMock();


			var interceptor = new SubInterceptorMock();
			const RequestStepMock passedSteps =
				RequestStepMock.Step1 |
				RequestStepMock.Step2 |
				RequestStepMock.Step3 |
				RequestStepMock.Step4 |
				RequestStepMock.Step5;

			//Act
			RequestPipelineMock.ExecuteLoop(request, new TimeSpan(0, 0, 2), null, null, customErrorHandler, interceptor);

			//Assert
			Assert.IsTrue(request.Done, "Заявка должна была дойти до конца.");
			Assert.IsFalse(interceptor.DefaultMethod, "Должен был отработать кастомный интерсептор.");
			Assert.AreEqual(interceptor.LastStep, passedSteps, "Должны перехватываться все предыдущие шаги в цепочке.");
		}
	}
}
