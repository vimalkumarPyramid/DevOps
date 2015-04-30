using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcMusicStore.Controllers;
using System.Web.Mvc;
using MvcMusicStore.Models;
using System.Web.Security;
using MvcMusicStore.Services;
using Rhino.Mocks;
using MvcMusicStoreTests.Fakes;
using System.Web;

namespace MvcMusicStoreTests.Controllers {
	[TestClass]
	public class AccountControllerTests {

		[TestMethod]
		public void LogOn_Get_ReturnsView() 
		{
			AccountController controller = GetWiredUpAccountController();

			ViewResult result = controller.LogOn() as ViewResult;

            Assert.IsNotNull(result);
		}

		[TestMethod]
		public void LogOn_WithValidModelAndURL_RedirectsToSpecifiedURL() {
			AccountController controller = GetWiredUpAccountController();
			LogOnModel inputModel = new LogOnModel() { UserName = "Bob", Password = "BobsPassword", RememberMe = false };

			RedirectResult result = controller.LogOnAsync(inputModel, "/Checkout/").Result as RedirectResult;

			Assert.AreEqual("/Checkout/", result.Url);
		}

		[TestMethod]
		public void LogOn_WithValidModelAndNoURL_RedirectsToHome() {
			AccountController controller = GetWiredUpAccountController();
			LogOnModel inputModel = new LogOnModel() { UserName = "Bob", Password = "BobsPassword", RememberMe = false };

			RedirectToRouteResult result = controller.LogOnAsync(inputModel, "").Result as RedirectToRouteResult;

			AssertRouteIsHome(result);
		}
		
		[TestMethod]
		public void LogOn_WithValidModelAndBadURL_RedirectsToHome() {
			AccountController controller = GetWiredUpAccountController();
			LogOnModel inputModel = new LogOnModel() { UserName = "Bob", Password = "BobsPassword", RememberMe = false };

			RedirectToRouteResult result = controller.LogOnAsync(inputModel, "http://someothersite.com").Result as RedirectToRouteResult;

			AssertRouteIsHome(result);
		}

		[TestMethod]
		public void LogOn_WithValidModelAndBadCredentials_ReturnsCurrentView() {
			AccountController controller = GetWiredUpAccountController();
			LogOnModel inputModel = new LogOnModel() { UserName = "NotBob", Password = "BobsPassword", RememberMe = false };

			ViewResult result = controller.LogOnAsync(inputModel, "").Result as ViewResult;

			Assert.AreSame(inputModel,result.Model);
		}

		[TestMethod]
		public void LogOn_WithValidModelAndBadCredentials_AddsErrorToState() {
			AccountController controller = GetWiredUpAccountController();
			LogOnModel inputModel = new LogOnModel() { UserName = "NotBob", Password = "BobsPassword", RememberMe = false };

			ActionResult result = controller.LogOnAsync(inputModel, "").Result;

			Assert.IsTrue(controller.ModelState.ContainsKey("Error"));
		}

		[TestMethod]
		public void LogOn_WithRememberMe_AddsCookie() {
			IAuthenticationService authSvc = MockRepository.GenerateStub<IAuthenticationService>();
			AccountController controller = GetWiredUpAccountController(authSvc: authSvc);
			LogOnModel inputModel = new LogOnModel() { UserName = "Bob", Password = "BobsPassword", RememberMe = true };

			ActionResult result = controller.LogOnAsync(inputModel, "").Result;

			authSvc.AssertWasCalled(a => a.SetAuthCookie(Arg<string>.Matches((s) => s == "Bob"), Arg<bool>.Matches((b) => b == true), Arg<HttpResponseBase>.Is.Anything));
		}


		[TestMethod]
		public void LogOn_WithoutRememberMe_AddsCookie() {
			IAuthenticationService authSvc = MockRepository.GenerateStub<IAuthenticationService>();
			AccountController controller = GetWiredUpAccountController(authSvc: authSvc);
			LogOnModel inputModel = new LogOnModel() { UserName = "Bob", Password = "BobsPassword", RememberMe = false };

			ActionResult result = controller.LogOnAsync(inputModel, "").Result;

			authSvc.AssertWasCalled(a => a.SetAuthCookie(Arg<string>.Matches((s) => s == "Bob"), Arg<bool>.Matches((b) => b == false), Arg<HttpResponseBase>.Is.Anything));
		}

		[TestMethod]
		public void LogOff_Get_CallsSignout() {
			IAuthenticationService authSvc = MockRepository.GenerateStub<IAuthenticationService>();
			AccountController controller = GetWiredUpAccountController(authSvc: authSvc);

			ActionResult result = controller.LogOff();

			authSvc.AssertWasCalled(a => a.SignOut());
		}

		[TestMethod]
		public void LogOff_Get_RedirectsToHomePage() {
			IAuthenticationService authSvc = MockRepository.GenerateStub<IAuthenticationService>();
			AccountController controller = GetWiredUpAccountController(authSvc: authSvc);

			RedirectToRouteResult result = controller.LogOff() as RedirectToRouteResult;

			AssertRouteIsHome(result);
		}

		[TestMethod]
		public void Register_Get_ReturnsView() {
			IAuthenticationService authSvc = MockRepository.GenerateStub<IAuthenticationService>();
			AccountController controller = GetWiredUpAccountController(authSvc: authSvc);

			ViewResult result = controller.Register() as ViewResult;

			Assert.IsNotNull(result);
		}

		[TestMethod]
		public void Register_ValidModel_CreatesNewMember() {
			IMembershipService memSvc = MockRepository.GenerateMock<IMembershipService>();
			memSvc.Stub(m => m.CreateUser(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<bool>.Is.Anything, Arg<object>.Is.Anything)).Return(MembershipCreateStatus.Success);
			AccountController controller = GetWiredUpAccountController(memSvc: memSvc);
			RegisterModel model = new RegisterModel() {
				 UserName="Bob",
				 Password="BobsPassword",
				 ConfirmPassword="BobsPassword",
				Email="bobsemail@aspnetmvcmusicstore.com"
			};

			ViewResult result = controller.RegisterAsync(model).Result as ViewResult;

			memSvc.AssertWasCalled(m => m.CreateUser(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<bool>.Is.Anything, Arg<object>.Is.Anything));
		}

		[TestMethod]
		public void Register_InvalidModel_ReturnsSameView() {
			IMembershipService memSvc = MockRepository.GenerateMock<IMembershipService>();
			memSvc.Stub(m => m.CreateUser(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<bool>.Is.Anything, Arg<object>.Is.Anything)).Return(MembershipCreateStatus.Success);
			AccountController controller = GetWiredUpAccountController(memSvc: memSvc);
			RegisterModel model = new RegisterModel() {
				UserName = "",
				Password = "BobsPassword",
				ConfirmPassword = "BobsPassword",
				Email = "bobsemail@aspnetmvcmusicstore.com"
			};
			controller.ModelState.AddModelError("Username", "Username would have an error if it was modelbound");

			ViewResult result = controller.RegisterAsync(model).Result as ViewResult;

			Assert.AreSame(model, result.Model);
		}

		[TestMethod]
		public void Register_DuplicateName_ReturnsError() {
			IMembershipService memSvc = MockRepository.GenerateMock<IMembershipService>();
			memSvc.Stub(m => m.CreateUser(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<bool>.Is.Anything, Arg<object>.Is.Anything)).Return(MembershipCreateStatus.DuplicateUserName);
			AccountController controller = GetWiredUpAccountController(memSvc: memSvc);
			RegisterModel model = new RegisterModel() {
				UserName = "Bob",
				Password = "BobsPassword",
				ConfirmPassword = "BobsPassword",
				Email = "bobsemail@aspnetmvcmusicstore.com"
			};

			ViewResult result = controller.RegisterAsync(model).Result as ViewResult;

			Assert.IsTrue(controller.ModelState.ContainsKey("Error"));
		}

		[TestMethod]
		public void Register_ValidModel_LogsUserIn() {
			IMembershipService memSvc = MockRepository.GenerateMock<IMembershipService>();
			memSvc.Stub(m => m.CreateUser(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<bool>.Is.Anything, Arg<object>.Is.Anything)).Return(MembershipCreateStatus.Success);
			IAuthenticationService authSvc = MockRepository.GenerateMock<IAuthenticationService>();
			AccountController controller = GetWiredUpAccountController(memSvc: memSvc, authSvc: authSvc);
			RegisterModel model = new RegisterModel() {
				UserName = "Bob",
				Password = "BobsPassword",
				ConfirmPassword = "BobsPassword",
				Email = "bobsemail@aspnetmvcmusicstore.com"
			};

			ViewResult result = controller.RegisterAsync(model).Result as ViewResult;

			authSvc.AssertWasCalled(a => a.SetAuthCookie(Arg<string>.Matches((s) => s == "Bob"), Arg<bool>.Matches((b) => b == false), Arg<HttpResponseBase>.Is.Anything));
		}

		[TestMethod]
		public void Register_ValidModel_RedirectsUserToHome() {
			IMembershipService memSvc = MockRepository.GenerateMock<IMembershipService>();
			memSvc.Stub(m => m.CreateUser(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<bool>.Is.Anything, Arg<object>.Is.Anything)).Return(MembershipCreateStatus.Success);
			IAuthenticationService authSvc = MockRepository.GenerateMock<IAuthenticationService>();
			AccountController controller = GetWiredUpAccountController(memSvc: memSvc, authSvc: authSvc);
			RegisterModel model = new RegisterModel() {
				UserName = "Bob",
				Password = "BobsPassword",
				ConfirmPassword = "BobsPassword",
				Email = "bobsemail@aspnetmvcmusicstore.com"
			};

			RedirectToRouteResult result = controller.RegisterAsync(model).Result as RedirectToRouteResult;

			AssertRouteIsHome(result);
		}

		//TODO add coverage for cart migration


		[TestMethod]
		public void ChangePassword_Get_ReturnsView() {
			AccountController controller = GetWiredUpAccountController();

			ViewResult result = controller.ChangePassword() as ViewResult;

			Assert.IsNotNull(result);
		}

		[TestMethod]
		public void ChangePassword_WithModel_ChangesPassword(){
			IMembershipService memSvc = MockRepository.GenerateMock<IMembershipService>();
			memSvc.Stub(m => m.ChangePassword(Arg<string>.Is.Anything, Arg<bool>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(true);
			AccountController controller = GetWiredUpAccountController(memSvc: memSvc);
			ChangePasswordModel model = new ChangePasswordModel() { 				 OldPassword="1", NewPassword="2", ConfirmPassword="2"			};

			ActionResult result = controller.ChangePasswordAsync(model).Result;

			memSvc.AssertWasCalled(m => m.ChangePassword(Arg<string>.Is.Anything, Arg<bool>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything));
		}

		[TestMethod]
		public void ChangePassword_WithModel_Redirects() {
			IMembershipService memSvc = MockRepository.GenerateMock<IMembershipService>();
			memSvc.Stub(m => m.ChangePassword(Arg<string>.Is.Anything, Arg<bool>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(true);
			AccountController controller = GetWiredUpAccountController(memSvc: memSvc);
			ChangePasswordModel model = new ChangePasswordModel() { OldPassword = "1", NewPassword = "2", ConfirmPassword = "2" };

			RedirectToRouteResult result = controller.ChangePasswordAsync(model).Result as RedirectToRouteResult;

			Assert.IsTrue(result.RouteValues.ContainsValue("ChangePasswordSuccess"));
		}


		[TestMethod]
		public void ChangePassword_FailedChange_ReturnsError() {
			IMembershipService memSvc = MockRepository.GenerateMock<IMembershipService>();
			memSvc.Stub(m => m.ChangePassword(Arg<string>.Is.Anything, Arg<bool>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(false);
			AccountController controller = GetWiredUpAccountController(memSvc: memSvc);
			ChangePasswordModel model = new ChangePasswordModel() { OldPassword = "1", NewPassword = "2", ConfirmPassword = "" };

			ActionResult result = controller.ChangePasswordAsync(model).Result;

			Assert.IsTrue(controller.ModelState.ContainsKey("Error"));
		}

		[TestMethod]
		public void ChangePasswordSuccess_Get_ReturnsView() {
			AccountController controller = GetWiredUpAccountController();

			ActionResult result = controller.ChangePasswordSuccess();

			Assert.IsNotNull(result);
		}

		private AccountController GetWiredUpAccountController(IMembershipService memSvc = null, IAuthenticationService authSvc = null, FakeDataStore store = null) {
			memSvc = memSvc ?? MockRepository.GenerateMock<IMembershipService>();
			memSvc.Stub(a => a.ValidateUser(Arg.Is("Bob"), Arg.Is("BobsPassword"))).Return(true);

			authSvc = authSvc ?? MockRepository.GenerateStub<IAuthenticationService>();

			return ControllerFactory.GetWiredUpController<AccountController>(s => new AccountController(s, authSvc, memSvc), store: store);
		}

		private void AssertRouteIsHome(RedirectToRouteResult result) {
			if (!result.RouteValues.ContainsValue("Index") || !result.RouteValues.ContainsValue("Home")) {
				throw new AssertFailedException("Route is not Index/Home");
			}
		}
	}
}

