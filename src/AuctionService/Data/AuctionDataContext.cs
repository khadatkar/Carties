using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data
{
	public class AuctionDataContext : DbContext
	{
		public AuctionDataContext(DbContextOptions options) : base(options)
		{
		}

        public DbSet<Auction> Auctions { get; set; }
    }
}
