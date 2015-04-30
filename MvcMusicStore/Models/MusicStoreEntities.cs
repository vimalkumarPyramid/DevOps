using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data;
using System.Threading.Tasks;

namespace MvcMusicStore.Models {
	public class MusicStoreEntities : DbContext, IMusicStoreEntities {
		public IDbSet<Album> Albums { get; set; }
		public IDbSet<Genre> Genres { get; set; }

		public IDbSet<Artist> Artists { get; set; }

		public IDbSet<Cart> Carts { get; set; }
		public IDbSet<Order> Orders { get; set; }
		public IDbSet<OrderDetail> OrderDetails { get; set; }

		public void SetModified(object target) {
			Entry(target).State = EntityState.Modified;
		}


		public Task<int> SaveChangesAsync()
		{
			return Task.Factory.StartNew<int>(() => SaveChanges());
		}
	}
}