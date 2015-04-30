using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Threading.Tasks;

namespace MvcMusicStore.Models {

	public interface IMusicStoreEntities : IDisposable {
		IDbSet<Album> Albums { get; set; }
		IDbSet<Genre> Genres { get; set; }

		IDbSet<Artist> Artists { get; set; }

		IDbSet<Cart> Carts { get; set; }
		IDbSet<Order> Orders { get; set; }
		IDbSet<OrderDetail> OrderDetails { get; set; }

		int SaveChanges();
		void SetModified(object target);

		Task<int> SaveChangesAsync();
	}

}