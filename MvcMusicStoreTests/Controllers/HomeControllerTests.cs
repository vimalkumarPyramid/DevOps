using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Mvc;
using MvcMusicStoreTests.Fakes;
using MvcMusicStore.Controllers;
using MvcContrib.TestHelper;
using MvcMusicStore.Models;

namespace MvcMusicStoreTests.Controllers {
	[TestClass]
	public class HomeControllerTests {

        //[TestMethod]
        //public void Index_Get_ReturnsFiveAlbums() {
        //    HomeController controller = ControllerFactory.GetWiredUpController<HomeController>((s)=>new HomeController(s), store: GetDataStoreWithAlbums());

        //    ViewResult result = controller.IndexAsync().Result as ViewResult;

        //    Assert.IsInstanceOfType(result.Model, typeof(List<Album>),"Expected model to contain list of albums");
        //    //Assert.AreEqual(5,((List<Album>)result.Model).Count,"Expected 5 albums");
        //    Assert.AreEqual(5, ((List<Album>)result.Model).Count, "Expected 5 albums");
        //}

		private FakeDataStore GetDataStoreWithAlbums() 
		{
			FakeDataStore dataStore = MusicStoreEntitiesFactory.GetEmpty();
			for (int i = 1; i <= 10; i++) {
				dataStore.GenerateAndAddArtist(i);
				dataStore.GenerateAndAddGenre(i);
				dataStore.GenerateAndAddAlbum(i, i, i, i + 5);
			}

			return dataStore;
		}
	}
}
