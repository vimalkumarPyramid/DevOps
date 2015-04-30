using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcMusicStore.Models;

namespace MvcMusicStore.Controllers {
	public class ControllerBase : AsyncController {

		private IMusicStoreEntities _storeDB;

		protected IMusicStoreEntities StoreDB { get { return _storeDB; } }

		public ControllerBase() : this(new MusicStoreEntities()) { }

		public ControllerBase(IMusicStoreEntities storeDb) {
			_storeDB = storeDb;
		}

	}
}