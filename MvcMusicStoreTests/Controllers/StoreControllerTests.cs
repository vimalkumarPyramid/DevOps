using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcMusicStoreTests.Fakes;
using System.Web.Mvc;
using MvcMusicStore.Controllers;
using MvcMusicStore.Models;

namespace MvcMusicStoreTests.Controllers {
	[TestClass]
	public class StoreControllerTests {

		[TestMethod]
		public void Index_Get_ReturnsAllGenres() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddGenre(2);
			dataStore.GenerateAndAddGenre(3);
			dataStore.GenerateAndAddGenre(4);
			StoreController controller = ControllerFactory.GetWiredUpController<StoreController>((s)=>new StoreController(s), store: dataStore);

			ViewResult result = controller.IndexAsync().Result as ViewResult;
			List<Genre> model = (List<Genre>) result.Model;

			Assert.AreEqual(4, model.Count);
		}

		[TestMethod]
		public void Browse_ValidGenre_ReturnsMatchingGenre() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1, "One");
			dataStore.GenerateAndAddGenre(2, "Two");
			dataStore.GenerateAndAddGenre(3, "Three");
			StoreController controller = ControllerFactory.GetWiredUpController<StoreController>((s) => new StoreController(s), store: dataStore);

			ViewResult result = controller.BrowseAsync("One").Result as ViewResult;
			Genre model = (Genre)result.Model;

			Assert.AreEqual(1, model.GenreId);
		}

		[TestMethod]
		public void Browse_InvalidGenre_ReturnsIndex() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1, "One");
			dataStore.GenerateAndAddGenre(2, "Two");
			dataStore.GenerateAndAddGenre(3, "Three");
			StoreController controller = ControllerFactory.GetWiredUpController<StoreController>((s) => new StoreController(s), store: dataStore);

			RedirectToRouteResult result = controller.BrowseAsync("Whatever").Result as RedirectToRouteResult;

			Assert.IsTrue(result.RouteValues.ContainsValue("Index"));
			Assert.IsTrue(result.RouteValues.ContainsKey("Error"));
		}

		[TestMethod]
		public void Browse_NoGenre_ReturnsIndex() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1, "One");
			dataStore.GenerateAndAddGenre(2, "Two");
			dataStore.GenerateAndAddGenre(3, "Three");
			StoreController controller = ControllerFactory.GetWiredUpController<StoreController>((s) => new StoreController(s), store: dataStore);

			RedirectToRouteResult result = controller.BrowseAsync("").Result as RedirectToRouteResult;

			Assert.IsTrue(result.RouteValues.ContainsValue("Index"));
			Assert.IsTrue(result.RouteValues.ContainsKey("Error"));
		}

		[TestMethod]
		public void Details_ValidAlbumId_ReturnsSpecifiedAlbum() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			Album album = dataStore.GenerateAndAddAlbum(100, 10, 1, 15.99M);
			StoreController controller = ControllerFactory.GetWiredUpController<StoreController>((s) => new StoreController(s), store: dataStore);

			ViewResult result = controller.DetailsAsync(100).Result as ViewResult;
			Album model = (Album)result.Model;

			Assert.AreEqual(album, model);
		}

		[TestMethod]
		public void Details_InvalidAlbumId_RedirectsToIndex() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			Album album = dataStore.GenerateAndAddAlbum(100, 10, 1, 15.99M);
			StoreController controller = ControllerFactory.GetWiredUpController<StoreController>((s) => new StoreController(s), store: dataStore);

			RedirectToRouteResult result = controller.DetailsAsync(123).Result as RedirectToRouteResult;

			Assert.IsTrue(result.RouteValues.ContainsValue("Index"));
			Assert.IsTrue(result.RouteValues.ContainsKey("Error"));
		}
		
		[TestMethod]
		public void Details_NoAlbumId_RedirectsToIndex() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			Album album = dataStore.GenerateAndAddAlbum(100, 10, 1, 15.99M);
			StoreController controller = ControllerFactory.GetWiredUpController<StoreController>((s) => new StoreController(s), store: dataStore);

			RedirectToRouteResult result = controller.DetailsAsync(null).Result as RedirectToRouteResult;

			Assert.IsTrue(result.RouteValues.ContainsValue("Index"));
			Assert.IsTrue(result.RouteValues.ContainsKey("Error"));
		}

		[TestMethod]
		public void GenreMenu_Get_ReturnsListOfGenres() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1, "One");
			dataStore.GenerateAndAddGenre(2, "Two");
			dataStore.GenerateAndAddGenre(3, "Three");
			StoreController controller = ControllerFactory.GetWiredUpController<StoreController>((s) => new StoreController(s), store: dataStore);

			PartialViewResult result = controller.GenreMenuAsync().Result as PartialViewResult;
			List<Genre> model = (List<Genre>)result.Model;

			Assert.AreEqual(3, model.Count);
		}
	}
}
