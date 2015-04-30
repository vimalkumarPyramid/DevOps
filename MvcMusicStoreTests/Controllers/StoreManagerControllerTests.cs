using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcMusicStoreTests.Fakes;
using MvcMusicStore.Controllers;
using System.Web.Mvc;
using MvcMusicStore.Models;

namespace MvcMusicStoreTests.Controllers {
	[TestClass]
	public class StoreManagerControllerTests {

		private void AssertResultContainsGenreList(ViewResult result, string genreId) {
			SelectList genreList = (SelectList)result.ViewBag.GenreId;
			Assert.AreEqual(genreId, genreList.First().Value);	// genre id above is value of first item in genre list
		}

		private void AssertResultContainsArtistList(ViewResult result, string artistId) {
			SelectList artistList = (SelectList)result.ViewBag.ArtistId;
			Assert.AreEqual(artistId, artistList.First().Value);	// genre id above is value of first item in genre list
		}

		private void AssertIsIndexAndError(RedirectToRouteResult result) {
			Assert.IsTrue(result.RouteValues.ContainsValue("Index"));
			Assert.IsTrue(result.RouteValues.ContainsKey("Error"));
		}

		[TestMethod]
		public void Index_Get_ReturnsAlbumList() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			dataStore.GenerateAndAddAlbum(100,10,1,19.99m);
			dataStore.GenerateAndAddAlbum(101,10,1,19.99m);
			dataStore.GenerateAndAddAlbum(102,10,1,19.99m);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);

			ViewResult result = controller.Index() as ViewResult;
			List<Album> model = (List<Album>)result.Model;

			Assert.AreEqual(3, model.Count);
		}
		
		[TestMethod]
		public void Details_ValidAlbumId_ReturnsSpecifiedAlbum() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			dataStore.GenerateAndAddAlbum(100, 10, 1, 19.99m);
			Album secondAlbum = dataStore.GenerateAndAddAlbum(101, 10, 1, 19.99m);
			dataStore.GenerateAndAddAlbum(102, 10, 1, 19.99m);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);

			ViewResult result = controller.Details(101) as ViewResult;
			Album model = (Album)result.Model;

			Assert.AreSame(secondAlbum, model);
		}

		[TestMethod]
		public void Details_InvalidAlbumId_RedirectsToIndex() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			dataStore.GenerateAndAddAlbum(100, 10, 1, 19.99m);
			dataStore.GenerateAndAddAlbum(101, 10, 1, 19.99m);
			dataStore.GenerateAndAddAlbum(102, 10, 1, 19.99m);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);

			RedirectToRouteResult result = controller.Details(123) as RedirectToRouteResult;

			AssertIsIndexAndError(result);
		}

		[TestMethod]
		public void Details_NoAlbumId_RedirectsToIndex() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			dataStore.GenerateAndAddAlbum(100, 10, 1, 19.99m);
			dataStore.GenerateAndAddAlbum(101, 10, 1, 19.99m);
			dataStore.GenerateAndAddAlbum(102, 10, 1, 19.99m);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);

			RedirectToRouteResult result = controller.Details(null) as RedirectToRouteResult;

			AssertIsIndexAndError(result);
		}

		[TestMethod]
		public void Create_Get_ProvidesGenreSelectList() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);

			ViewResult result = controller.Create() as ViewResult;

			AssertResultContainsGenreList(result,"1");
		}


		[TestMethod]
		public void Create_Get_ProvidesArtistSelectList() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);

			ViewResult result = controller.Create() as ViewResult;

			AssertResultContainsArtistList(result, "10");
		}

		[TestMethod]
		public void Create_ValidAlbum_RedirectsToIndex() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);
			Album album = FakeDataStore.GenerateAlbum(100, 10, 1, 19.99M);

			RedirectToRouteResult result = controller.Create(album) as RedirectToRouteResult;

			Assert.IsTrue(result.RouteValues.ContainsValue("Index"));
		}

		[TestMethod]
		public void Create_ValidAlbum_AddsAlbumToDataStore() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);
			Album album = FakeDataStore.GenerateAlbum(100, 10, 1, 19.99M);

			RedirectToRouteResult result = controller.Create(album) as RedirectToRouteResult;

			Assert.AreEqual(album, dataStore.Albums.Where(a => a.AlbumId == album.AlbumId).FirstOrDefault());
		}

		[TestMethod]
		public void Create_InvalidAlbum_ReturnsAlbumForCorrection() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);
			controller.ModelState.AddModelError("Something", "Assume scaffolding magic found an error");
			Album album = new Album() { ArtistId = 10, GenreId = 1 };

			ViewResult result = controller.Create(album) as ViewResult;
			Album model = (Album) result.Model;

			Assert.AreEqual(album, model);
		}

		[TestMethod]
		public void Create_InvalidAlbum_ReturnsArtistListForAlbumCorrection() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);
			controller.ModelState.AddModelError("Something", "Assume scaffolding magic found an error");
			Album album = new Album() { ArtistId = 10, GenreId = 1 };

			ViewResult result = controller.Create(album) as ViewResult;

			AssertResultContainsArtistList(result, "10");
		}

		[TestMethod]
		public void Create_InvalidAlbum_ReturnsGenreListForAlbumCorrection() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);
			controller.ModelState.AddModelError("Something", "Assume scaffolding magic found an error");
			Album album = new Album() { ArtistId = 10, GenreId = 1 };

			ViewResult result = controller.Create(album) as ViewResult;

			AssertResultContainsGenreList(result, "1");
		}


		[TestMethod]
		public void Edit_ValidAlbumId_ReturnsAlbumForCorrection() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			Album testAlbum = dataStore.GenerateAndAddAlbum(100, 10, 1, 19.99m);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);

			ViewResult result = controller.Edit(testAlbum.AlbumId) as ViewResult;
			Album model = (Album)result.Model;

			Assert.AreEqual(testAlbum, model);
		}
		
		[TestMethod]
		public void Edit_ValidAlbumId_ReturnsGenreList() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			Album testAlbum = dataStore.GenerateAndAddAlbum(100, 10, 1, 19.99m);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);

			ViewResult result = controller.Edit(testAlbum.AlbumId) as ViewResult;

			AssertResultContainsGenreList(result, "1");
		}

		[TestMethod]
		public void Edit_ValidAlbumId_ReturnsArtistList() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			Album testAlbum = dataStore.GenerateAndAddAlbum(100, 10, 1, 19.99m);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);

			ViewResult result = controller.Edit(testAlbum.AlbumId) as ViewResult;

			AssertResultContainsArtistList(result, "10");
		}

		[TestMethod]
		public void Edit_InvalidAlbumId_RedirectsToIndex() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			Album testAlbum = dataStore.GenerateAndAddAlbum(100, 10, 1, 19.99m);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);

			RedirectToRouteResult result = controller.Edit(123) as RedirectToRouteResult;

			AssertIsIndexAndError(result);
		}

		[TestMethod]
		public void Edit_ValidAlbum_RedirectsToIndex() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			dataStore.GenerateAndAddAlbum(100, 10, 1, 29.99M); 
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);
			Album testAlbum = dataStore.GenerateAndAddAlbum(100, 10, 1, 19.99m);	// new and improved price

			RedirectToRouteResult result = controller.Edit(testAlbum) as RedirectToRouteResult;

			Assert.IsTrue(result.RouteValues.ContainsValue("Index"));
		}

		[TestMethod]
		public void Edit_ValidAlbum_SavesAlbum() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			dataStore.GenerateAndAddAlbum(100, 10, 1, 29.99M);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);
			Album testAlbum = dataStore.GenerateAndAddAlbum(100, 10, 1, 19.99m);	// new and improved price

			RedirectToRouteResult result = controller.Edit(testAlbum) as RedirectToRouteResult;

			Assert.IsTrue(dataStore.SavedThis(testAlbum));
		}

		[TestMethod]
		public void Edit_InvalidAlbum_ReturnsAlbumForCorrection() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			dataStore.GenerateAndAddAlbum(100, 10, 1, 29.99M);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);
			Album testAlbum = dataStore.GenerateAndAddAlbum(100, 10, 1, 19.99m);	// new and improved price
			controller.ModelState.AddModelError("Something", "Assume scaffolding magic found an error");

			ViewResult result = controller.Edit(testAlbum) as ViewResult;
			Album model = (Album)result.Model;

			Assert.AreEqual(testAlbum, model);
		}

		[TestMethod]
		public void Edit_InvalidAlbum_ReturnsGenreListForAlbumCorrection() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			dataStore.GenerateAndAddAlbum(100, 10, 1, 29.99M);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);
			Album testAlbum = dataStore.GenerateAndAddAlbum(100, 10, 1, 19.99m);	// new and improved price
			controller.ModelState.AddModelError("Something", "Assume scaffolding magic found an error");

			ViewResult result = controller.Edit(testAlbum) as ViewResult;

			AssertResultContainsGenreList(result, "1");
		}

		[TestMethod]
		public void Edit_InvalidAlbum_ReturnsArtistListForAlbumCorrection() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			dataStore.GenerateAndAddAlbum(100, 10, 1, 29.99M);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);
			Album testAlbum = dataStore.GenerateAndAddAlbum(100, 10, 1, 19.99m);	// new and improved price
			controller.ModelState.AddModelError("Something", "Assume scaffolding magic found an error");

			ViewResult result = controller.Edit(testAlbum) as ViewResult;

			AssertResultContainsArtistList(result, "10");
		}

		[TestMethod]
		public void Delete_ValidAlbumId_ReturnsSpecifiedAlbum() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			Album secondAlbum = dataStore.GenerateAndAddAlbum(101, 10, 1, 19.99m);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);

			ViewResult result = controller.Delete(101) as ViewResult;
			Album model = (Album)result.Model;

			Assert.AreSame(secondAlbum, model);
		}

		[TestMethod]
		public void Delete_InvalidAlbumId_RedirectsToIndex() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			dataStore.GenerateAndAddAlbum(101, 10, 1, 19.99m);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);

			RedirectToRouteResult result = controller.Delete(123) as RedirectToRouteResult;

			AssertIsIndexAndError(result);
		}

		[TestMethod]
		public void Delete_NoAlbumId_RedirectsToIndex() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			dataStore.GenerateAndAddAlbum(101, 10, 1, 19.99m);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);

			RedirectToRouteResult result = controller.Delete(null) as RedirectToRouteResult;

			AssertIsIndexAndError(result);
		}

		[TestMethod]
		public void DeleteConfirmed_ValidAlbumId_RedirectsToIndex() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			Album secondAlbum = dataStore.GenerateAndAddAlbum(101, 10, 1, 19.99m);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);

			RedirectToRouteResult result = controller.DeleteConfirmed(101) as RedirectToRouteResult;

			Assert.IsTrue(result.RouteValues.ContainsValue("Index"));
		}

		[TestMethod]
		public void DeleteConfirmed_ValidAlbumId_DeletesAlbum() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			Album secondAlbum = dataStore.GenerateAndAddAlbum(101, 10, 1, 19.99m);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);

			RedirectToRouteResult result = controller.DeleteConfirmed(101) as RedirectToRouteResult;

			Assert.IsFalse(dataStore.Albums.Any(a => a.AlbumId == 101));
		}

		[TestMethod]
		public void DeleteConfirmed_InvalidAlbumId_RedirectsToIndex() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			dataStore.GenerateAndAddAlbum(101, 10, 1, 19.99m);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);

			RedirectToRouteResult result = controller.DeleteConfirmed(123) as RedirectToRouteResult;

			AssertIsIndexAndError(result);
		}

		[TestMethod]
		public void DeleteConfirmed_NoAlbumId_RedirectsToIndex() {
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			dataStore.GenerateAndAddGenre(1);
			dataStore.GenerateAndAddArtist(10);
			dataStore.GenerateAndAddAlbum(101, 10, 1, 19.99m);
			StoreManagerController controller = ControllerFactory.GetWiredUpController<StoreManagerController>((s) => new StoreManagerController(s), store: dataStore);

			RedirectToRouteResult result = controller.DeleteConfirmed(null) as RedirectToRouteResult;

			AssertIsIndexAndError(result);
		}

	}
}
