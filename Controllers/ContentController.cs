
using MaxemusAPI.Data;
using MaxemusAPI.Models.Helper;
using MaxemusAPI.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using MaxemusAPI.Helpers;
using AutoMapper;
using MaxemusAPI.Models;
using MaxemusAPI.Models.Dtos;
using MaxemusAPI.Repository;
using MaxemusAPI.Common;

namespace MaxemusAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private IContentRepository _ContentRepository;

        private ApplicationDbContext _context;
        private readonly IMapper _mapper;
        protected APIResponse _response;



        public ContentController(
            ApplicationDbContext context,
            IMapper mapper,
            IContentRepository contentRepository
        )
        {
            _ContentRepository = contentRepository;
            _context = context;
            _response = new();
            _mapper = mapper;

        }

        #region GetCountries
        /// <summary>
        ///  Get country list.
        /// </summary>
        /// <param  name="searchQuery"> Search by country name</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCountries")]
        public async Task<ActionResult> GetCountries(string? searchQuery)
        {
            try
            {
                var countryList = await _ContentRepository.GetCountries();
                if (!String.IsNullOrEmpty(searchQuery))
                {
                    countryList = countryList.Where(s => s.countryName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = ResponseMessages.msgShownSuccess;
                _response.Data = countryList;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgSomethingWentWrong;
                return Ok(_response);
            }
        }
        #endregion

        #region GetStates
        /// <summary>
        ///  Get state list.
        /// </summary>
        /// <param  name="countryId"> The id of country</param>
        /// <param  name="searchQuery"> Search by state name</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetStates")]
        public async Task<ActionResult> GetStatesByCountryId(int countryId, string? searchQuery)
        {
            try
            {
                var stateList = await _ContentRepository.GetStatesByCountryId(countryId);
                if (!String.IsNullOrEmpty(searchQuery))
                {
                    stateList = stateList.Where(s => s.stateName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Messages = ResponseMessages.msgShownSuccess;
                _response.Data = stateList;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Messages = ResponseMessages.msgSomethingWentWrong;
                return Ok(_response);
            }
        }
        #endregion

        // #region GetCities
        // /// <summary>
        // ///  Get state list.
        // /// </summary>
        // /// <param  name="stateId"> The id of state</param>
        // /// <param  name="searchQuery"> Search by city name</param>
        // /// <returns></returns>
        // [HttpGet]
        // [Route("GetCities")]
        // public async Task<ActionResult> GetCitiesByStateId(int stateId, string? searchQuery)
        // {
        //     try
        //     {
        //         var cityList = await _ContentRepository.GetCitiesByStateId(stateId);
        //         if (!String.IsNullOrEmpty(searchQuery))
        //         {
        //             cityList = cityList.Where(s => s.cityName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
        //             ).ToList();
        //         }
        //         _response.StatusCode = HttpStatusCode.OK;
        //         _response.IsSuccess = true;
        //         _response.Messages = "Shown successfully.";
        //         _response.Data = cityList;
        //         return Ok(_response);
        //     }
        //     catch (Exception ex)
        //     {
        //         _response.StatusCode = HttpStatusCode.InternalServerError;
        //         _response.IsSuccess = false;
        //         _response.Messages = "Something went wrong.";
        //         return Ok(_response);
        //     }
        // }
        // #endregion

        // #region GetAllStates
        // /// <summary>
        // ///  Get state list.
        // /// </summary>
        // /// <param  name="searchQuery"> Search by state name</param>
        // /// <returns></returns>
        // [HttpGet]
        // [Route("GetAllStates")]
        // public async Task<ActionResult> GetAllStates(string? searchQuery)
        // {
        //     try
        //     {
        //         var stateList = await _ContentRepository.GetStates();
        //         if (!String.IsNullOrEmpty(searchQuery))
        //         {
        //             stateList = stateList.Where(s => s.stateName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
        //             ).ToList();
        //         }
        //         _response.StatusCode = HttpStatusCode.OK;
        //         _response.IsSuccess = true;
        //         _response.Messages = "Shown successfully.";
        //         _response.Data = stateList;
        //         return Ok(_response);
        //     }
        //     catch (Exception ex)
        //     {
        //         _response.StatusCode = HttpStatusCode.InternalServerError;
        //         _response.IsSuccess = false;
        //         _response.Messages = "Something went wrong.";
        //         return Ok(_response);
        //     }
        // }
        // #endregion

    }
}
