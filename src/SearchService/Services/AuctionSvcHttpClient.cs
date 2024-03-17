using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services
{
	public class AuctionSvcHttpClient
	{
		private readonly HttpClient _HttpClient;
		private readonly IConfiguration _Config;

		public AuctionSvcHttpClient(HttpClient httpClient,IConfiguration config)
        {
			_HttpClient = httpClient;
			_Config = config;
		}

		public async Task<List<Item>> GetItemsForSearchDb()
		{
			var lastUpdated = await DB.Find<Item,string>()
				.Sort(x => x.Descending(a => a.UpdatedAt))
				.Project(x => x.UpdatedAt.ToString())
				.ExecuteFirstAsync();

			return await _HttpClient.GetFromJsonAsync<List<Item>>(_Config["AuctionServiceUrl"]
				+"/api/auctions?date="+lastUpdated);
		}
    }
}
