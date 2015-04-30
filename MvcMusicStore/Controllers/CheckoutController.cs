using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcMusicStore.Models;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace MvcMusicStore.Controllers {

	[Authorize]
	[SuppressMessage("Gendarme.Rules.Exceptions", "UseObjectDisposedExceptionRule", Justification = "If the controller is disposed mid-call we have bigger issues")]
	public class CheckoutController : ControllerBase {
		public static string PROMO_CODE = "FREE";

		public CheckoutController() { }
		public CheckoutController(IMusicStoreEntities storeDb) : base(storeDb) { }
		
		//
		// GET: /Checkout/AddressAndPayment
		public ActionResult AddressAndPayment() {
			return View();
		}

		//
		// POST: /Checkout/AddressAndPayment
		[HttpPost]
		public async Task<ActionResult> AddressAndPaymentAsync(FormCollection values) {
			if (values == null) throw new ArgumentNullException("values");

			var order = new Order();
			TryUpdateModel(order);

			if (!ModelState.IsValid) {
				return View(order);
			}
			
			try {
				if (string.Equals(values["PromoCode"], PROMO_CODE, StringComparison.OrdinalIgnoreCase) == false) {
					return View(order);
				}
				else {
					order.Username = User.Identity.Name;
					order.OrderDate = DateTime.Now;

					//Save Order
					StoreDB.Orders.Add(order);
					await StoreDB.SaveChangesAsync();
					//Process the order
					using (var cart = ShoppingCart.GetCart(this.HttpContext, StoreDB)) {
						await cart.CreateOrderAsync(order);
					}
					
					return RedirectToAction("Complete",
						new { id = order.OrderId });
				}
			}
			catch {
				//Invalid - redisplay with errors
				return View(order);
			}
		}

		//
		// GET: /Checkout/Complete
		public ActionResult Complete(int id) {
			// Validate customer owns this order
			bool isValid = StoreDB.Orders.Any(o => o.OrderId == id && o.Username == User.Identity.Name);

			if (isValid) {
				return View(id);
			}
			else {
				return View("Error");
			}
		}
	}
}
