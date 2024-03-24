using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers
{
	public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
	{
		private readonly IMapper _Mapper;
		public AuctionUpdatedConsumer(IMapper mapper)
        {
			_Mapper = mapper;
		}
        public async Task Consume(ConsumeContext<AuctionUpdated> context)
		{
			Console.WriteLine("--> Consuming auction updated:" + context.Message.Id);

			var item = _Mapper.Map<Item>(context.Message);

			var result = await DB.Update<Item>().Match(a => a.ID == context.Message.Id).ModifyOnly(
					x => new
					{
						x.Color,
						x.Make,
						x.Model,
						x.Year,
						x.Mileage
					},item).ExecuteAsync();

			if (!result.IsAcknowledged) 
				throw new MessageException(typeof(AuctionUpdated), "Problem updating mongodb");

			
		}
	}
}
