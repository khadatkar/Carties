using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
	[ApiController]
	[Route("api/auctions")]
	public class AuctionsController : ControllerBase
	{
		private readonly AuctionDataContext _Context;
		private readonly IMapper _Mapper;

		public AuctionsController(AuctionDataContext context, IMapper mapper)
		{
			_Context = context;
			_Mapper = mapper;
		}

		[HttpGet]
		public async Task<ActionResult<List<AuctionDTO>>> GetAllAuctions()
		{
			var auctions = await _Context.Auctions
				.Include(x => x.Item)
				.OrderBy(x => x.Item.Make)
				.ToListAsync();

			return _Mapper.Map<List<AuctionDTO>>(auctions);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<AuctionDTO>> GetAuctionById(Guid id)
		{
			var auction = await _Context.Auctions
				.Include(x => x.Item)
				.FirstOrDefaultAsync(x => x.Id == id);

			if (auction == null) return NotFound();

			return _Mapper.Map<AuctionDTO>(auction);
		}

		[HttpPost]
		public async Task<ActionResult<AuctionDTO>> CreateAuction(CreateAuctionDTO auctionDTO)
		{
			var auction = _Mapper.Map<Auction>(auctionDTO);
			//TODO: add current user as seller

			auction.Seller = "test";

			_Context.Auctions.Add(auction);

			var result = await _Context.SaveChangesAsync() > 0;

			if(!result) return BadRequest("Could not save changes to the DB");

			return CreatedAtAction(nameof(GetAuctionById), new {auction.Id},_Mapper.Map<AuctionDTO>(auction));
		}

		[HttpPut("{id}")]
		public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDTO updateAuctionDTO)
		{
			var auction = await _Context.Auctions.Include(x => x.Item)
				.FirstOrDefaultAsync(x => x.Id == id);

			if (auction == null) return NotFound();

			//TODO: check seller == username

			auction.Item.Make = updateAuctionDTO.Make ?? auction.Item.Make;
			auction.Item.Model = updateAuctionDTO.Model ?? auction.Item.Model;
			auction.Item.Color = updateAuctionDTO.Color ?? auction.Item.Color;
			auction.Item.Mileage = updateAuctionDTO.Mileage ?? auction.Item.Mileage;
			auction.Item.Year = updateAuctionDTO.Year ?? auction.Item.Year;

			var result = await _Context.SaveChangesAsync() > 0;

			if (result) return Ok();

			return BadRequest("Problem saving changes");
		}

		[HttpDelete("{id}")]
		public async Task<ActionResult> DeleteAuction(Guid id)
		{
			var auction = await _Context.Auctions.FindAsync(id);

			if (auction == null) return NotFound();

			//TODO: check seller == username

			_Context.Auctions.Remove(auction);

			var result = await _Context.SaveChangesAsync() > 0;

			if (!result) return BadRequest("Could not update DB");

			return Ok();
		}

	}
}
