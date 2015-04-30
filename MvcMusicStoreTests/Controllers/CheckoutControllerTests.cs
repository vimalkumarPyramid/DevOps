using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcMusicStore.Controllers;
using System.Web.Mvc;
using System.Web.Routing;
using MvcMusicStore.Models;
using MvcContrib.TestHelper;
using MvcMusicStoreTests.Fakes;

namespace MvcMusicStoreTests.Controllers {

	[TestClass]
	public class CheckoutControllerTests {

		[TestMethod]
		public void AddressAndPayment_Get_ReturnsView() {
			CheckoutController controller = GetWiredUpController();

			ActionResult result = controller.AddressAndPayment();

			Assert.IsNotNull(result);
		}

		[TestMethod]
		public void AddressAndPayment_PostInvalidOrderNoPromotion_ReturnsOrderWithErrors() {
			FormCollection orderCollection = new FormCollection() {
				{"FirstName","fn"}
			};
			CheckoutController controller = GetWiredUpController(orderCollection);

			ViewResult result = controller.AddressAndPaymentAsync(orderCollection).Result as ViewResult;

			Assert.IsInstanceOfType(result.ViewData.Model, typeof(Order));
			Assert.AreNotEqual(0, result.ViewData.ModelState.Count);
		}

		[TestMethod]
		public void AddressAndPayment_PostInvalidOrderWithPromotion_ReturnsOrderWithErrors() {
			FormCollection orderCollection = new FormCollection() {
				{"FirstName","fn"},
				{"PromoCode", CheckoutController.PROMO_CODE}
			};
			CheckoutController controller = GetWiredUpController(orderCollection);

			ViewResult result = controller.AddressAndPaymentAsync(orderCollection).Result as ViewResult;

			Assert.IsInstanceOfType(result.ViewData.Model, typeof(Order));
			Assert.AreNotEqual(0, result.ViewData.ModelState.Count);
		}

		[TestMethod]
		public void AddressAndPayment_PostValidOrderWithoutPromotion_ReturnsOrderForRedisplay() {
			FormCollection orderCollection = new FormCollection() {
				{"FirstName","fn"},
				{"PromoCode", CheckoutController.PROMO_CODE}
			};
			CheckoutController controller = GetWiredUpController(orderCollection);

			ViewResult result = controller.AddressAndPaymentAsync(orderCollection).Result as ViewResult;

			Assert.IsInstanceOfType(result.ViewData.Model, typeof(Order));
		}

		[TestMethod]
		public void AddressAndPayment_PostValidOrderWithPromotion_ReturnsRedirectToComplete() {
			FormCollection orderCollection = new FormCollection() {
				{"FirstName","fn"},
				{"LastName","ln"},
				{"Address","add"},
				{"City","city"},
				{"State","state"},
				{"PostalCode","postal"},
				{"Country","country"},
				{"Phone","phone"},
				{"email","a@b.cd"},
				{"PromoCode", CheckoutController.PROMO_CODE}
			};
			CheckoutController controller = GetWiredUpController(orderCollection);

			ActionResult result = controller.AddressAndPaymentAsync(orderCollection).Result;

			Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
			Assert.AreEqual("Complete", (( RedirectToRouteResult)result).RouteValues["Action"]);
		}

		[TestMethod]
		public void Complete_ValidOrderIdAndUser_ReturnsProperView() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.Orders.Add(new Order() { OrderId=5, Username="Bob" });
			FakeUser user = new FakeUser(new FakeIdentity("Bob","",true));
			CheckoutController controller = GetWiredUpController(store: dataStore, user: user);
			
			ViewResult result = controller.Complete(5) as ViewResult;

			Assert.AreEqual(5, result.ViewData.Model);
		}

		[TestMethod]
		public void Complete_ValidOrderIdAndNoUser_ReturnsError() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.Orders.Add(new Order() { OrderId = 5, Username = "Bob" });
			CheckoutController controller = GetWiredUpController(store: dataStore);

			ViewResult result = controller.Complete(5) as ViewResult;

			Assert.AreEqual("Error", result.ViewName);
		}

		[TestMethod]
		public void Complete_InvalidOrderId_ReturnsError() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			CheckoutController controller = GetWiredUpController(store: dataStore);

			ViewResult result = controller.Complete(5) as ViewResult;

			Assert.AreEqual("Error", result.ViewName);
		}

		private CheckoutController GetWiredUpController(FormCollection formValues = null, FakeDataStore store = null, FakeUser user = null) {
			return ControllerFactory.GetWiredUpController<CheckoutController>(s => new CheckoutController(s), formValues, store, user);
		}
		
	}
}
