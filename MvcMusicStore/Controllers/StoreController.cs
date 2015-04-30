using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcMusicStore.Models;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace MvcMusicStore.Controllers
{
	[SuppressMessage("Gendarme.Rules.Exceptions", "UseObjectDisposedExceptionRule", Justification = "If the controller is disposed mid-call we have bigger issues")]
	public class StoreController : ControllerBase
	{

		public StoreController() { }
		public StoreController(IMusicStoreEntities storeDb) : base(storeDb) { }

		//
		// GET: /Home/
		public async Task<ActionResult> IndexAsync()
		{
			var genres = await Task.Factory.StartNew<List<Genre>>(() => StoreDB.Genres.ToList());
			return View(genres);
		}

		//
		// GET: /Store/Browse?genre=Disco
		public async Task<ActionResult> BrowseAsync(string genre)
		{
			var genreModel = await Task.Factory.StartNew<Genre>(() => StoreDB.Genres.Include("Albums").SingleOrDefault(g => g.Name == genre));
			if (genreModel != null)
				return View(genreModel);
			else
				return RedirectToAction("Index", new { Error = "Unrecognized Genre" });
		}

		//
		// GET: /Store/Details/5
		public async Task<ActionResult> DetailsAsync(int? id)
		{
			var album = await Task.Factory.StartNew<Album>(() => StoreDB.Albums.Find(id.GetValueOrDefault(-1)));
			if (album != null)
				return View(album);
			else
				return RedirectToAction("Index", new { Error = "Unrecognized Album" });
		}

		//
		// GET: /Store/GenreMenu
		[ChildActionOnly]
		public async Task<ActionResult> GenreMenuAsync()
		{
			var genres = await Task.Factory.StartNew<List<Genre>>(() => StoreDB.Genres.ToList());
			return PartialView(genres);
		}
	}
}
