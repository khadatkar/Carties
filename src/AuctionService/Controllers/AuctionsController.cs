using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
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
		private readonly IPublishEndpoint _PublishEndpoint;

		public AuctionsController(AuctionDataContext context, IMapper mapper,IPublishEndpoint publishEndpoint)
		{
			_Context = context;
			_Mapper = mapper;
			_PublishEndpoint = publishEndpoint;
		}

		[HttpGet]
		public async Task<ActionResult<List<AuctionDTO>>> GetAllAuctions(string date)
		{
			var query = _Context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

			if (!string.IsNullOrEmpty(date))
			{
				query=query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);	
			}


			//var auctions = await _Context.Auctions
			//	.Include(x => x.Item)
			//	.OrderBy(x => x.Item.Make)
			//	.ToListAsync();

			//return _Mapper.Map<List<AuctionDTO>>(auctions);

			return await query.ProjectTo<AuctionDTO>(_Mapper.ConfigurationProvider).ToListAsync();
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

		[Authorize]
		[HttpPost]
		public async Task<ActionResult<AuctionDTO>> CreateAuction(CreateAuctionDTO auctionDTO)
		{
			var auction = _Mapper.Map<Auction>(auctionDTO);
			

			auction.Seller = User.Identity.Name;

			_Context.Auctions.Add(auction);

			var newAuction = _Mapper.Map<AuctionDTO>(auction);

			await _PublishEndpoint.Publish(_Mapper.Map<AuctionCreated>(newAuction));

			var result = await _Context.SaveChangesAsync() > 0;

			


			if (!result) return BadRequest("Could not save changes to the DB");

			return CreatedAtAction(nameof(GetAuctionById), new {auction.Id},newAuction);
		}

		[Authorize]
		[HttpPut("{id}")]
		public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDTO updateAuctionDTO)
		{
			var auction = await _Context.Auctions.Include(x => x.Item)
				.FirstOrDefaultAsync(x => x.Id == id);

			if (auction == null) return NotFound();

			if(auction.Seller != User.Identity.Name)
				return Forbid();

			auction.Item.Make = updateAuctionDTO.Make ?? auction.Item.Make;
			auction.Item.Model = updateAuctionDTO.Model ?? auction.Item.Model;
			auction.Item.Color = updateAuctionDTO.Color ?? auction.Item.Color;
			auction.Item.Mileage = updateAuctionDTO.Mileage ?? auction.Item.Mileage;
			auction.Item.Year = updateAuctionDTO.Year ?? auction.Item.Year;

			await _PublishEndpoint.Publish(_Mapper.Map<AuctionUpdated>(auction));

			var result = await _Context.SaveChangesAsync() > 0;

			if (result) return Ok();

			return BadRequest("Problem saving changes");
		}

		[Authorize]
		[HttpDelete("{id}")]
		public async Task<ActionResult> DeleteAuction(Guid id)
		{
			var auction = await _Context.Auctions.FindAsync(id);

			if (auction == null) return NotFound();

			if(auction.Seller != User.Identity.Name)
				return Forbid();

			_Context.Auctions.Remove(auction);

			await _PublishEndpoint.Publish<AuctionDeleted>(new { id = auction.Id.ToString() });

			var result = await _Context.SaveChangesAsync() > 0;

			if (!result) return BadRequest("Could not update DB");

			return Ok();
		}

	}
}
