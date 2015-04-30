using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;

namespace MvcMusicStore.Models {
	public partial class ShoppingCart : IDisposable {
		IMusicStoreEntities storeDB;

		string ShoppingCartId { get; set; }
		public static readonly string CartSessionKey = "CartId";

		public static ShoppingCart GetCart(HttpContextBase context, IMusicStoreEntities datastore) {
			var cart = new ShoppingCart();
			cart.storeDB = datastore;
			cart.ShoppingCartId = cart.GetCartId(context);
			return cart;
		}
		// Helper method to simplify shopping cart calls
		public static ShoppingCart GetCart(Controller controller, IMusicStoreEntities datastore) {
			if (controller == null) throw new ArgumentNullException("controller");
			
			return GetCart(controller.HttpContext, datastore);
		}
		public async Task AddToCartAsync(Album album) {
			if (_disposed) throw new ObjectDisposedException(GetType().Name);

			// Get the matching cart and album instances
			var cartItem = storeDB.Carts.SingleOrDefault(
				c => c.CartId == ShoppingCartId
				&& c.AlbumId == album.AlbumId);

			if (cartItem == null) {
				// Create a new cart item if no cart item exists
				cartItem = new Cart {
					AlbumId = album.AlbumId,
					CartId = ShoppingCartId,
					Count = 1,
					DateCreated = DateTime.Now
				};
				storeDB.Carts.Add(cartItem);
			}
			else {
				// If the item does exist in the cart, 
				// then add one to the quantity
				cartItem.Count++;
			}
			// Save changes
			await storeDB.SaveChangesAsync();
		}
		public async Task<int> RemoveFromCartAsync(int id) {
			if (_disposed) throw new ObjectDisposedException(GetType().Name);

			// Get the cart
			Cart cartItem = await Task.Factory.StartNew<Cart>(() => 
			{
				return storeDB.Carts.Single(cart => cart.CartId == ShoppingCartId && cart.RecordId == id);
			});

			int itemCount = 0;

			if (cartItem != null) {
				if (cartItem.Count > 1) {
					cartItem.Count--;
					itemCount = cartItem.Count;
				}
				else {
					await Task.Factory.StartNew(() => storeDB.Carts.Remove(cartItem));
				}
				// Save changes
				await storeDB.SaveChangesAsync();
			}
			return itemCount;
		}
		public async Task EmptyCartAsync() {
			if (_disposed) throw new ObjectDisposedException(GetType().Name);

			await Task.Factory.StartNew(() =>
			{
				var cartItems = storeDB.Carts.Where(cart => cart.CartId == ShoppingCartId);

				foreach (var cartItem in cartItems)
				{
					storeDB.Carts.Remove(cartItem);
				}
			});

			// Save changes
			await storeDB.SaveChangesAsync();
		}
		public async Task<List<Cart>> GetCartItemsAsync() {
			if (_disposed) throw new ObjectDisposedException(GetType().Name);

			return await Task.Factory.StartNew<List<Cart>>(() => storeDB.Carts.Where(cart => cart.CartId == ShoppingCartId).Include(c => c.Album).ToList());
		}
		public async Task<int> GetCountAsync() {
			if (_disposed) throw new ObjectDisposedException(GetType().Name);

			// Get the count of each item in the cart and sum them up
			int? count = await Task.Factory.StartNew<int?>(() => 
			{
				return (from cartItems in storeDB.Carts
						where cartItems.CartId == ShoppingCartId
						select (int?)cartItems.Count).Sum();
			});

			// Return 0 if all entries are null
			return count ?? 0;
		}
		public async Task<decimal> GetTotalAsync() {
			if (_disposed) throw new ObjectDisposedException(GetType().Name);

			// Multiply album price by count of that album to get 
			// the current price for each of those albums in the cart
			// sum all album price totals to get the cart total
			decimal? total = await Task.Factory.StartNew<decimal?>(() =>
			{
				return (from cartItems in storeDB.Carts
						where cartItems.CartId == ShoppingCartId
						select (int?)cartItems.Count *
						cartItems.Album.Price).Sum();
			});

			return total ?? decimal.Zero;
		}
		public async Task<int> CreateOrderAsync(Order order) {
			if (order == null) throw new ArgumentNullException("order");
			if (_disposed) throw new ObjectDisposedException(GetType().Name);

			decimal orderTotal = 0;

			var cartItems = await GetCartItemsAsync();
			// Iterate over the items in the cart, 
			// adding the order details for each
			foreach (var item in cartItems) {
				var orderDetail = new OrderDetail {
					AlbumId = item.AlbumId,
					OrderId = order.OrderId,
					UnitPrice = item.Album.Price,
					Quantity = item.Count
				};
				// Set the order total of the shopping cart
				orderTotal += (item.Count * item.Album.Price);

				storeDB.OrderDetails.Add(orderDetail);

			}
			// Set the order's total to the orderTotal count
			order.Total = orderTotal;

			// Save the order
			await storeDB.SaveChangesAsync();
			// Empty the shopping cart
			await EmptyCartAsync();
			// Return the OrderId as the confirmation number
			return order.OrderId;
		}
		// We're using HttpContextBase to allow access to cookies.
		public string GetCartId(HttpContextBase context) {
			if (context == null) throw new ArgumentNullException("context");
			if (_disposed) throw new ObjectDisposedException(GetType().Name);

			string cartId = (string) context.Session[CartSessionKey];
			if (string.IsNullOrWhiteSpace(cartId)) {
				if (!string.IsNullOrWhiteSpace(context.User.Identity.Name)) {
					cartId = context.User.Identity.Name;
				}
				else {
					// Generate a new random GUID using System.Guid class
					Guid tempCartId = Guid.NewGuid();
					// Send tempCartId back to client as a cookie
					cartId = tempCartId.ToString();
				}
				context.Session[CartSessionKey] = cartId;
			}
			return cartId;
		}
		// When a user has logged in, migrate their shopping cart to
		// be associated with their username
		public async Task MigrateCartAsync(string userName) {
			if (_disposed) throw new ObjectDisposedException(GetType().Name);

			var shoppingCart = storeDB.Carts.Where(
				c => c.CartId == ShoppingCartId);

			foreach (Cart item in shoppingCart) {
				item.CartId = userName;
			}
			await storeDB.SaveChangesAsync();
		}

		private bool _disposed = false;
		public void Dispose() {
			if (!_disposed) {
				storeDB.Dispose();
				_disposed = true;
			}
		}
	}
}