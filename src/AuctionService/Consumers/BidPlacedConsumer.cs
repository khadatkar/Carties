using AuctionService.Data;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers
{
	public class BidPlacedConsumer : IConsumer<Contracts.BidPlaced>
	{
		private readonly AuctionDataContext _dbContext;

		public BidPlacedConsumer(AuctionDataContext dbContext)
		{
			_dbContext = dbContext;
		}
		public async Task Consume(ConsumeContext<BidPlaced> context)
		{
			Console.WriteLine("--> Consuming bid placed");
			var auction = await _dbContext.Auctions.FindAsync(context.Message.AuctionId);

			if(auction.CurrentHighBid == null ||
				context.Message.BidStatus.Contains("Accepte") &&
				context.Message.Amount>auction.CurrentHighBid
				)
			{
				auction.CurrentHighBid = context.Message.Amount;
				await _dbContext.SaveChangesAsync();
			}
		}
	}
}
