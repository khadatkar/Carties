using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers
{
	public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
	{
		private readonly IMapper _Mapper;

		public AuctionCreatedConsumer(IMapper mapper)
        {
			_Mapper = mapper;
		}

        public async Task Consume(ConsumeContext<AuctionCreated> context)
		{
			Console.WriteLine("--> Consuming auction created:" + context.Message.Id);

			var item = _Mapper.Map<Item>(context.Message);

			if (item.Model == "Foo") throw new ArgumentException("Cannot sell car with name of Foo");

			await item.SaveAsync();
		}
	}
}
