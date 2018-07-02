using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MvcMusicStore.Models;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace MvcMusicStore.Controllers {
	[SuppressMessage("Gendarme.Rules.Exceptions", "UseObjectDisposedExceptionRule", Justification = "If the controller is disposed mid-call we have bigger issues")]
	public class HomeController : ControllerBase {

		public HomeController() { }
		public HomeController(IMusicStoreEntities storeDb) : base(storeDb) { }

		//
		// GET: /Home/
		public async Task<ActionResult> IndexAsync() {
			// Get most popular albums
			//var albums = await GetTopSellingAlbumsAsync(5);
			
			//return View(albums);
            List<Album> albums = new List<Album>();
            //albums.Add(new Album { });
            return View(albums);
		}

		private async Task<List<Album>> GetTopSellingAlbumsAsync(int count) {
			// Group the order details by album and return
			// the albums with the highest count
			return await Task.Factory.StartNew<List<Album>>(() =>
			{
				return StoreDB.Albums
								.OrderByDescending(a => a.OrderDetails.Count())
								.Take(count)
								.ToList();
			});
		}
	}
}