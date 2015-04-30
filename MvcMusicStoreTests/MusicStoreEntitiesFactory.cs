using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using MvcMusicStore.Models;
using Rhino.Mocks;
using MvcMusicStoreTests.Fakes;

namespace MvcMusicStoreTests {
	class MusicStoreEntitiesFactory {

		public static FakeDataStore GetEmpty() {
			FakeDataStore datastore = new FakeDataStore();
			datastore.Albums = new FakeDbSet<Album>();
			datastore.Artists = new FakeDbSet<Artist>();
			datastore.Carts = new FakeDbSet<Cart>();
			datastore.Genres = new FakeDbSet<Genre>();
			datastore.OrderDetails = new FakeDbSet<OrderDetail>();
			datastore.Orders = new FakeDbSet<Order>();
			return datastore;
		}


	}

}
