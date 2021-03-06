﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MvcMusicStore.Models;
using MvcMusicStore.Services;
using System.Web.Security;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace MvcMusicStore.Controllers {
	[SuppressMessage("Gendarme.Rules.Exceptions", "UseObjectDisposedExceptionRule", Justification = "If the controller is disposed mid-call we have bigger issues")]
	public class AccountController : ControllerBase {
		private IMembershipService _membership;
		private IAuthenticationService _authentication;

		public AccountController() {
			_membership = new MembershipService();
			_authentication = new AuthenticationService();
		}
		
		public AccountController(IMusicStoreEntities storeDb, IAuthenticationService authService, IMembershipService membershipService)
			: base(storeDb) {
			_membership = membershipService;
			_authentication = authService;
		}

		//
		// GET: /Account/LogOn

		public ActionResult LogOn() {
			return View();
		}

		//
		// POST: /Account/LogOn
		[HttpPost]
		public async Task<ActionResult> LogOnAsync(LogOnModel model, string returnUrl) {
			if (model == null) throw new ArgumentNullException("model");

			if (ModelState.IsValid) {
				bool isValid = await Task.Factory.StartNew<bool>(() => _membership.ValidateUser(model.UserName, model.Password));
				if (isValid) {
					await MigrateShoppingCartAsync(model.UserName);

					_authentication.SetAuthCookie(model.UserName, model.RememberMe, Response);
					try {
						if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1
							&& returnUrl.StartsWith("/")
							&& !returnUrl.StartsWith("//") &&
							!returnUrl.StartsWith("/\\")) {
							return Redirect(returnUrl);
						}
					}
					catch { }	// swallowing IsLocalUrl nullref
					return RedirectToAction("Index", "Home");
				}
				else {
					ModelState.AddModelError("Error", "The user name or password provided is incorrect.");
				}
			}
			// If we got this far, something failed, redisplay form
			return View(model);
		}

		//
		// GET: /Account/LogOff

		public ActionResult LogOff() {
			_authentication.SignOut();

			return RedirectToAction("Index", "Home");
		}

		//
		// GET: /Account/Register

		public ActionResult Register() {
			return View();
		}

		//
		// POST: /Account/Register
		[HttpPost]
		public async Task<ActionResult> RegisterAsync(RegisterModel model) {
			if (model == null) throw new ArgumentNullException("model");

			if (ModelState.IsValid) {
				// Attempt to register the user
				MembershipCreateStatus createStatus;
				createStatus = await Task.Factory.StartNew<MembershipCreateStatus>(() => _membership.CreateUser(model.UserName, model.Password, model.Email, "question", "answer", true, null));

				if (createStatus == MembershipCreateStatus.Success) {
					await MigrateShoppingCartAsync(model.UserName);

					_authentication.SetAuthCookie(model.UserName, false /*  createPersistentCookie */, Response);
					return RedirectToAction("Index", "Home");
				}
				else {
					ModelState.AddModelError("Error", ErrorCodeToString(createStatus));
				}
			}
			// If we got this far, something failed, redisplay form
			return View(model);
		}

		//
		// GET: /Account/ChangePassword

		[Authorize]
		public ActionResult ChangePassword() {
			return View();
		}

		//
		// POST: /Account/ChangePassword

		[Authorize]
		[HttpPost]
		public async Task<ActionResult> ChangePasswordAsync(ChangePasswordModel model) {
			if (model == null) throw new ArgumentNullException("model");

			if (ModelState.IsValid) {

				// ChangePassword will throw an exception rather
				// than return false in certain failure scenarios.
				bool changePasswordSucceeded;
				try {
					changePasswordSucceeded = await Task.Factory.StartNew<bool>(() => _membership.ChangePassword(User.Identity.Name, true, model.OldPassword, model.NewPassword));
				}
				catch {
					changePasswordSucceeded = false;
				}

				if (changePasswordSucceeded) {
					return RedirectToAction("ChangePasswordSuccess");
				}
				else {
					ModelState.AddModelError("Error", "The current password is incorrect or the new password is invalid.");
				}
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		//
		// GET: /Account/ChangePasswordSuccess

		public ActionResult ChangePasswordSuccess() {
			return View();
		}

		#region Status Codes
		private static string ErrorCodeToString(MembershipCreateStatus createStatus) {
			// See http://go.microsoft.com/fwlink/?LinkID=177550 for
			// a full list of status codes.
			switch (createStatus) {
				case MembershipCreateStatus.DuplicateUserName:
					return "User name already exists. Please enter a different user name.";

				case MembershipCreateStatus.DuplicateEmail:
					return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

				case MembershipCreateStatus.InvalidPassword:
					return "The password provided is invalid. Please enter a valid password value.";

				case MembershipCreateStatus.InvalidEmail:
					return "The e-mail address provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidAnswer:
					return "The password retrieval answer provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidQuestion:
					return "The password retrieval question provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidUserName:
					return "The user name provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.ProviderError:
					return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

				case MembershipCreateStatus.UserRejected:
					return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

				default:
					return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
			}
		}
		#endregion

		private async Task MigrateShoppingCartAsync(string userName) {
			// Associate shopping cart items with logged-in user
			ShoppingCart cart = await Task.Factory.StartNew<ShoppingCart>(() => ShoppingCart.GetCart(this.HttpContext, StoreDB));
			using (cart) {
				await cart.MigrateCartAsync(userName);
				Session[ShoppingCart.CartSessionKey] = userName;
			}
		}

		public string FunctionToGenerateWarnings(string x)
		{
			try {
				var s = x + " stuff";
				return s;
			}
			catch (Exception exc) 
			{ 
				// do nothing
				int i = 5;
				return "fail";
			}
		}
	}
}
