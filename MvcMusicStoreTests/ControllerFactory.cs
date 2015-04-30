using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MvcMusicStore.Controllers;
using System.Web.Mvc;
using MvcMusicStoreTests.Fakes;
using MvcContrib.TestHelper;

namespace MvcMusicStoreTests {
	class ControllerFactory {

		public static TController GetWiredUpController<TController>(Func<FakeDataStore, TController> ctor, FormCollection formValues = null, FakeDataStore store = null, FakeUser user = null) where TController : MvcMusicStore.Controllers.ControllerBase
		{
			store = store ?? MusicStoreEntitiesFactory.GetEmpty();
			TController controller = ctor(store);

			TestControllerBuilder _builder = new TestControllerBuilder();
			_builder.HttpContext.User = user ?? new FakeUser();
			_builder.InitializeController(controller);

			if (formValues != null) {
				controller.ValueProvider = formValues.ToValueProvider();
			}
			return controller;
		}

	}
}
