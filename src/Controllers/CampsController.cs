using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers{

    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;
        public CampsController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }
        
        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsAsync(includeTalks);
                CampModel[] models = _mapper.Map<CampModel[]>(results);

                return Ok(models);
            }catch(Exception)
            {
                return this.StatusCode( StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var result = await _repository.GetCampAsync(moniker);
                if(result == null)
                { return NotFound(); }

                CampModel model = _mapper.Map<CampModel>(result);
                return Ok(model);
            }catch(Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Error");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false){
            try
            {
                var results = await _repository.GetAllCampsByEventDate(theDate, includeTalks);
                if(!results.Any()){
                    return NotFound();
                }
                return _mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,"Database error");
            }

        }

        [HttpPost()]
        public async Task<ActionResult<CampModel>> Post([FromBody]CampModel model)
        {
            try
            {
                var location = _linkGenerator.GetPathByAction("Get","Camps", new {moniker = model.Moniker});
                if(string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker");
                }

                //Create a new Camp
                var camp = _mapper.Map<Camp>(model);
                _repository.Add(camp);
                if(await _repository.SaveChangesAsync())
                {
                    return Created(location,_mapper.Map<CampModel>(camp));
                }
            }catch(Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
            return BadRequest();
        }

    }
}