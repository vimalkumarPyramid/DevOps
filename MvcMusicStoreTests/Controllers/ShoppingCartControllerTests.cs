using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcMusicStoreTests.Fakes;
using MvcMusicStore.Controllers;
using System.Web.Mvc;
using MvcMusicStore.ViewModels;
using MvcMusicStore.Models;

//TODO: refactor ShoppingCartController to use an abstraction instead of ShioppingCart so these tests can be turned into unit tests (currently integration)

namespace MvcMusicStoreTests.Controllers {
	[TestClass]
	public class ShoppingCartControllerTests {

		private const decimal ALBUM_PRICE = 123.45M;
		private const int SAMPLE_ALBUM_ID = 1;
		private const int SAMPLE_ALBUM_ID2 = 2;
		private const int SAMPLE_ALBUM_ID_BAD = 3;
		private const int SAMPLE_RECORD_ID = 100;
		private const int SAMPLE_RECORD_ID_BAD = 101;

		private FakeDataStore QuickDataStore() {
			FakeDataStore fakeStore = MusicStoreEntitiesFactory.GetEmpty();
			fakeStore.GenerateAndAddArtist(1);
			fakeStore.GenerateAndAddGenre(1);
			fakeStore.GenerateAndAddAlbum(SAMPLE_ALBUM_ID, 1, 1, ALBUM_PRICE);
			fakeStore.GenerateAndAddAlbum(SAMPLE_ALBUM_ID2, 1, 1, ALBUM_PRICE);
			return fakeStore;
		}

		private Cart QuickCart(FakeDataStore fakeStore, string cartId, int albumId, int recordId) {
			return new Cart() { 
				CartId = cartId, 
				Count = 1, 
				AlbumId = albumId, 
				RecordId = recordId, 
				Album = fakeStore.Albums.Where(a => a.AlbumId == albumId).First(), 
				DateCreated = DateTime.Now 
			};
		}

		private ShoppingCartController GetQuickControllerWithTemporaryCart(int albumId, int recordId) {
			string cartId = Guid.NewGuid().ToString("N");
			FakeDataStore fakeStore = QuickDataStore();
			fakeStore.Carts.Add(QuickCart(fakeStore, cartId, albumId, recordId));
			ShoppingCartController controller = ControllerFactory.GetWiredUpController<ShoppingCartController>((s) => new ShoppingCartController(s), store: fakeStore);
			int initialCount = fakeStore.Carts.Count();
			controller.Session["CartId"] = cartId;
			return controller;
		}

        [TestMethod]
		public void Index_GetWithNoCart_ReturnsNewCart() {
			FakeDataStore fakeStore = MusicStoreEntitiesFactory.GetEmpty();
			ShoppingCartController controller = ControllerFactory.GetWiredUpController<ShoppingCartController>((s) => new ShoppingCartController(s), store: fakeStore);

			ViewResult result = controller.IndexAsync().Result as ViewResult;

			Assert.IsInstanceOfType(result.Model, typeof(ShoppingCartViewModel));
		}

		[TestMethod]
		public void Index_GetWithTemporaryCart_ReturnsExpectedCart() {
			string cartId = Guid.NewGuid().ToString("N");
			FakeDataStore fakeStore = QuickDataStore();
			Cart expCart = QuickCart(fakeStore, cartId, SAMPLE_ALBUM_ID, SAMPLE_RECORD_ID);
			fakeStore.Carts.Add(expCart);
			ShoppingCartController controller = ControllerFactory.GetWiredUpController<ShoppingCartController>((s) => new ShoppingCartController(s), store: fakeStore);
			controller.Session["CartId"] = cartId;

			ViewResult result = controller.IndexAsync().Result as ViewResult;
			ShoppingCartViewModel viewModel = (ShoppingCartViewModel) result.Model;

			Assert.AreSame(expCart, viewModel.CartItems.First());
		}

		[TestMethod]
		public void Index_GetWithNamedCart_ReturnsExpectedCart() {
			FakeDataStore fakeStore = QuickDataStore();
			FakeUser user = new FakeUser(new FakeIdentity("bob", "something", true));
			ShoppingCartController controller = ControllerFactory.GetWiredUpController<ShoppingCartController>((s) => new ShoppingCartController(s), store: fakeStore, user: user);
			Cart expCart = QuickCart(fakeStore, "bob", SAMPLE_ALBUM_ID, SAMPLE_RECORD_ID);
			fakeStore.Carts.Add(expCart);

			ViewResult result = controller.IndexAsync().Result as ViewResult;
			ShoppingCartViewModel viewModel = (ShoppingCartViewModel)result.Model;

			Assert.AreSame(expCart, viewModel.CartItems.First());
		}

		[TestMethod]
		public void Index_GetWithTemporaryCart_ReturnsCorrectTotal() {
			ShoppingCartController controller = GetQuickControllerWithTemporaryCart(SAMPLE_ALBUM_ID, SAMPLE_RECORD_ID);

			ViewResult result = controller.IndexAsync().Result as ViewResult;
			ShoppingCartViewModel viewModel = (ShoppingCartViewModel)result.Model;

			Assert.AreEqual(ALBUM_PRICE, viewModel.CartTotal);
		}

		[TestMethod]
		public void Index_GetWithNamedCart_ReturnsCorrectTotal() {
			FakeDataStore fakeStore = QuickDataStore();
			FakeUser user = new FakeUser(new FakeIdentity("bob", "something", true));
			ShoppingCartController controller = ControllerFactory.GetWiredUpController<ShoppingCartController>((s) => new ShoppingCartController(s), store: fakeStore, user: user);
			fakeStore.Carts.Add(QuickCart(fakeStore, "bob", SAMPLE_ALBUM_ID, SAMPLE_RECORD_ID));

			ViewResult result = controller.IndexAsync().Result as ViewResult;
			ShoppingCartViewModel viewModel = (ShoppingCartViewModel)result.Model;

			Assert.AreEqual(ALBUM_PRICE, viewModel.CartTotal);
		}

		[TestMethod]
		public void AddToCart_WithNoCart_AddsItemToNewCart() 
		{
			FakeDataStore fakeStore = QuickDataStore();
			int initialCount = fakeStore.Carts.Count();
			ShoppingCartController controller = ControllerFactory.GetWiredUpController<ShoppingCartController>((s) => new ShoppingCartController(s), store: fakeStore);

			var x = controller.AddToCartAsync(SAMPLE_ALBUM_ID).Result;

			Assert.AreEqual(initialCount + 1, fakeStore.Carts.Count());
		}

		[TestMethod]
		public void AddToCart_WithTemporaryCart_AddsItemToCart() {
			string cartId = Guid.NewGuid().ToString("N");
			FakeDataStore fakeStore = QuickDataStore();
			fakeStore.Carts.Add(QuickCart(fakeStore, cartId, SAMPLE_ALBUM_ID, SAMPLE_RECORD_ID));
			int initialCount = fakeStore.Carts.Count();
			ShoppingCartController controller = ControllerFactory.GetWiredUpController<ShoppingCartController>((s) => new ShoppingCartController(s), store: fakeStore);

			var x = controller.AddToCartAsync(SAMPLE_ALBUM_ID2).Result;

			Assert.AreEqual(initialCount + 1, fakeStore.Carts.Count());
		}

		[TestMethod]
		public void AddToCart_WithNamedCart_AddsItemToCart() {
			string cartId = "bob";
			FakeUser user = new FakeUser(new FakeIdentity(cartId, "something", true));
			FakeDataStore fakeStore = QuickDataStore();
			fakeStore.Carts.Add(QuickCart(fakeStore, cartId, SAMPLE_ALBUM_ID, SAMPLE_RECORD_ID));
			int initialCount = fakeStore.Carts.Count();
			ShoppingCartController controller = ControllerFactory.GetWiredUpController<ShoppingCartController>((s) => new ShoppingCartController(s), store: fakeStore, user: user);

			var x = controller.AddToCartAsync(SAMPLE_ALBUM_ID2).Result;

			Assert.AreEqual(initialCount + 1, fakeStore.Carts.Count());
		}

		[TestMethod]
		public void AddToCart_UnknownAlbum_ReturnsError() {
			FakeDataStore fakeStore = QuickDataStore();
			int initialCount = fakeStore.Carts.Count();
			ShoppingCartController controller = ControllerFactory.GetWiredUpController<ShoppingCartController>((s) => new ShoppingCartController(s), store: fakeStore);

			RedirectToRouteResult result = controller.AddToCartAsync(SAMPLE_ALBUM_ID_BAD).Result as RedirectToRouteResult;

			Assert.IsTrue(result.RouteValues.ContainsValue("Index"));
			Assert.IsTrue(result.RouteValues.ContainsKey("Error"));
		}

		[TestMethod]
		public void RemoveFromCart_KnownAlbumInCart_RemovesItemFromCart() {
			string cartId = Guid.NewGuid().ToString("N");
			FakeDataStore fakeStore = QuickDataStore();
			fakeStore.Carts.Add(QuickCart(fakeStore, cartId, SAMPLE_ALBUM_ID, SAMPLE_RECORD_ID));
			ShoppingCartController controller = ControllerFactory.GetWiredUpController<ShoppingCartController>((s) => new ShoppingCartController(s), store: fakeStore);
			int initialCount = fakeStore.Carts.Count();
			controller.Session["CartId"] = cartId;

			JsonResult result = controller.RemoveFromCartAsync(SAMPLE_RECORD_ID).Result as JsonResult;

			Assert.AreEqual(initialCount - 1,fakeStore.Carts.Count());
		}

		[TestMethod]
		public void RemoveFromCart_KnownAlbumInCart_ReturnsIdInJson() {
			ShoppingCartController controller = GetQuickControllerWithTemporaryCart(SAMPLE_ALBUM_ID, SAMPLE_RECORD_ID);

			JsonResult result = controller.RemoveFromCartAsync(SAMPLE_RECORD_ID).Result as JsonResult;
			ShoppingCartRemoveViewModel model = (ShoppingCartRemoveViewModel) result.Data;

			Assert.AreEqual(SAMPLE_RECORD_ID,model.DeleteId);
		}

		[TestMethod]
		public void RemoveFromCart_KnownAlbumInCart_ReturnsUpdatedCartCountInJson() {
			ShoppingCartController controller = GetQuickControllerWithTemporaryCart(SAMPLE_ALBUM_ID, SAMPLE_RECORD_ID);

			JsonResult result = controller.RemoveFromCartAsync(SAMPLE_RECORD_ID).Result as JsonResult;
			ShoppingCartRemoveViewModel model = (ShoppingCartRemoveViewModel)result.Data;

			Assert.AreEqual(0, model.ItemCount);
		}

		[TestMethod]
		public void RemoveFromCart_KnownAlbumInCart_ReturnsUpdatedTotalInJson() {
			ShoppingCartController controller = GetQuickControllerWithTemporaryCart(SAMPLE_ALBUM_ID, SAMPLE_RECORD_ID);

			JsonResult result = controller.RemoveFromCartAsync(SAMPLE_RECORD_ID).Result as JsonResult;
			ShoppingCartRemoveViewModel model = (ShoppingCartRemoveViewModel)result.Data;

			Assert.AreEqual(0, model.CartTotal);
		}

		[TestMethod]
		public void RemoveFromCart_KnownAlbumNotInCart_ReturnsEmptyJsonOrSomething() {
			ShoppingCartController controller = GetQuickControllerWithTemporaryCart(SAMPLE_ALBUM_ID, SAMPLE_RECORD_ID);

			JsonResult result = controller.RemoveFromCartAsync(SAMPLE_RECORD_ID_BAD).Result as JsonResult;
			ShoppingCartRemoveViewModel model = (ShoppingCartRemoveViewModel)result.Data;

			Assert.IsNull(model);
		}

		[TestMethod]
		public void CartSummary_WithoutCart_ReportsZero() {
			ShoppingCartController controller = ControllerFactory.GetWiredUpController<ShoppingCartController>((s) => new ShoppingCartController(s), store: QuickDataStore());

			PartialViewResult result = controller.CartSummaryAsync().Result as PartialViewResult;

			Assert.AreEqual(0, result.ViewData["CartCount"]);
		}

		[TestMethod]
		public void CartSummary_WithTemporaryCart_ReportsCorrectQty() {
			ShoppingCartController controller = GetQuickControllerWithTemporaryCart(SAMPLE_ALBUM_ID, SAMPLE_RECORD_ID);

			PartialViewResult result = controller.CartSummaryAsync().Result as PartialViewResult;

			Assert.AreEqual(1, result.ViewData["CartCount"]);
		}

		[TestMethod]
		public void CartSummary_WithNamedCart_ReportsCorrectQty() {
			string cartId = "bob";
			FakeUser user = new FakeUser(new FakeIdentity(cartId, "something", true));
			FakeDataStore fakeStore = QuickDataStore();
			fakeStore.Carts.Add(QuickCart(fakeStore, cartId, SAMPLE_ALBUM_ID, SAMPLE_RECORD_ID));
			ShoppingCartController controller = ControllerFactory.GetWiredUpController<ShoppingCartController>((s) => new ShoppingCartController(s), store: fakeStore, user: user);

			PartialViewResult result = controller.CartSummaryAsync().Result as PartialViewResult;

			Assert.AreEqual(1, result.ViewData["CartCount"]);
		}
	}
}

