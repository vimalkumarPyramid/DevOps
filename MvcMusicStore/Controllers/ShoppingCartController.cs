using System.Linq;
using System.Web.Mvc;
using MvcMusicStore.Models;
using MvcMusicStore.ViewModels;
using System.Diagnostics.CodeAnalysis;
using System;
using System.Threading.Tasks;

namespace MvcMusicStore.Controllers {
	[SuppressMessage("Gendarme.Rules.Exceptions", "UseObjectDisposedExceptionRule", Justification = "If the controller is disposed mid-call we have bigger issues")]
	public class ShoppingCartController : ControllerBase {

		public ShoppingCartController() { }
		public ShoppingCartController(IMusicStoreEntities storeDb) : base(storeDb) { }

		//
		// GET: /ShoppingCart/
		public async Task<ActionResult> IndexAsync() {
			using (var cart = ShoppingCart.GetCart(this.HttpContext, StoreDB)) {

				// Set up our ViewModel
				var viewModel = new ShoppingCartViewModel {
					CartItems = await cart.GetCartItemsAsync(),
					CartTotal = await cart.GetTotalAsync()
				};
				// Return the view
				return View(viewModel);
			}
		}
		//
		// GET: /Store/AddToCart/5
		public async Task<ActionResult> AddToCartAsync(int id) {
			// Retrieve the album from the database
			var addedAlbum = await Task.Factory.StartNew<Album>(() => StoreDB.Albums.SingleOrDefault(album => album.AlbumId == id));
			if (addedAlbum == null) {
				// not doing anything with this error, but better then waiting for an exception to popup somewhere as a method to handle the error
				return RedirectToAction("Index", new { Error="UnknownAlbum" });
			}

			// Add it to the shopping cart
			using (var cart = ShoppingCart.GetCart(this.HttpContext, StoreDB)) {
				await cart.AddToCartAsync(addedAlbum);
			}

			// Go back to the main store page for more shopping
			return RedirectToAction("Index");

		}
		//
		// AJAX: /ShoppingCart/RemoveFromCart/5
		[HttpPost]
		public async Task<ActionResult> RemoveFromCartAsync(int id) {
			// Remove the item from the cart
			using (var cart = ShoppingCart.GetCart(this.HttpContext, StoreDB)) {
				if (!StoreDB.Carts.Any(i => i.RecordId == id)) {
					return Json(null);
				}

				// Get the name of the album to display confirmation
				string albumName = StoreDB.Carts
					.Single(item => item.RecordId == id).Album.Title;

				// Remove from cart
				int itemCount = await cart.RemoveFromCartAsync(id);

				// Display the confirmation message
				var results = new ShoppingCartRemoveViewModel() {
					Message = Server.HtmlEncode(albumName) +
						" has been removed from your shopping cart.",
					CartTotal = await cart.GetTotalAsync(),
					CartCount = await cart.GetCountAsync(),
					ItemCount = itemCount,
					DeleteId = id
				};
				return Json(results);
			}
		}
		//
		// GET: /ShoppingCart/CartSummary
		[ChildActionOnly]
		public async Task<ActionResult> CartSummaryAsync() {
			using (var cart = ShoppingCart.GetCart(this.HttpContext, StoreDB)) {
				ViewData["CartCount"] = await cart.GetCountAsync();
			}
			return PartialView("CartSummary");
		}
	}
}