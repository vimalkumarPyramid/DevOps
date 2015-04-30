using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcMusicStore.Models;
using System.Diagnostics.CodeAnalysis;

namespace MvcMusicStore.Controllers
{
	[Authorize(Roles = "Administrator")]
	[SuppressMessage("Gendarme.Rules.Exceptions", "UseObjectDisposedExceptionRule", Justification = "If the controller is disposed mid-call we have bigger issues")]
	public class StoreManagerController : ControllerBase
	{		
		public StoreManagerController() { }
		public StoreManagerController(IMusicStoreEntities storeDb) : base(storeDb) { }

		//
		// GET: /StoreManager/

		public ViewResult Index()
		{
			var albums = StoreDB.Albums.Include(a => a.Genre).Include(a => a.Artist);
			return View(albums.ToList());
		}

		//
		// GET: /StoreManager/Details/5

		public ActionResult Details(int? id)
		{
			Album album = StoreDB.Albums.Find(id.GetValueOrDefault(-1));
			if (album != null)
				return View(album);
			else
				return RedirectToAction("Index", new { Error = "Unrecognized Album" });
		}

		//
		// GET: /StoreManager/Create

		public ActionResult Create()
		{
			ViewBag.GenreId = new SelectList(StoreDB.Genres, "GenreId", "Name");
			ViewBag.ArtistId = new SelectList(StoreDB.Artists, "ArtistId", "Name");
			return View();
		} 

		//
		// POST: /StoreManager/Create

		[HttpPost]
		public ActionResult Create(Album album)
		{
			if (ModelState.IsValid)
			{
				StoreDB.Albums.Add(album);
				StoreDB.SaveChanges();
				return RedirectToAction("Index");  
			}

			ViewBag.GenreId = new SelectList(StoreDB.Genres, "GenreId", "Name", album.GenreId);
			ViewBag.ArtistId = new SelectList(StoreDB.Artists, "ArtistId", "Name", album.ArtistId);
			return View(album);
		}
		
		//
		// GET: /StoreManager/Edit/5
 
		public ActionResult Edit(int id)
		{
			Album album = StoreDB.Albums.Find(id);

			if (album == null)
				return RedirectToAction("Index", new { Error = "Unrecognized Album" });

			ViewBag.GenreId = new SelectList(StoreDB.Genres, "GenreId", "Name", album.GenreId);
			ViewBag.ArtistId = new SelectList(StoreDB.Artists, "ArtistId", "Name", album.ArtistId);
			return View(album);
		}

		//
		// POST: /StoreManager/Edit/5

		[HttpPost]
		public ActionResult Edit(Album album)
		{
			if (ModelState.IsValid)
			{
				StoreDB.SetModified(album);
				StoreDB.SaveChanges();
				return RedirectToAction("Index");
			}
			ViewBag.GenreId = new SelectList(StoreDB.Genres, "GenreId", "Name", album.GenreId);
			ViewBag.ArtistId = new SelectList(StoreDB.Artists, "ArtistId", "Name", album.ArtistId);
			return View(album);
		}

		//
		// GET: /StoreManager/Delete/5
 
		public ActionResult Delete(int? id)
		{
			Album album = StoreDB.Albums.Find(id.GetValueOrDefault(-1));
			if (album != null)
				return View(album);
			else
				return RedirectToAction("Index", new { Error = "Unrecognized Album" });
		}

		//
		// POST: /StoreManager/Delete/5

		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(int? id)
		{
			Album album = StoreDB.Albums.Find(id.GetValueOrDefault(-1));
			
			if(album == null)
				return RedirectToAction("Index", new { Error = "Unrecognized Album" });

			StoreDB.Albums.Remove(album);
			StoreDB.SaveChanges();
			return RedirectToAction("Index");
		}

		protected override void Dispose(bool disposing)
		{
			StoreDB.Dispose();
			base.Dispose(disposing);
		}
	}
}