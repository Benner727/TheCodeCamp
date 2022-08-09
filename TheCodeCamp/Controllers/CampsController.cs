using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using TheCodeCamp.Data;
using TheCodeCamp.Models;

namespace TheCodeCamp.Controllers
{
    [RoutePrefix("api/camps")]
    public class CampsController : ApiController
    {
        private readonly ICampRepository repository;
        private readonly IMapper mapper;

        public CampsController(ICampRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        [Route()]
        public async Task<IHttpActionResult> Get(bool includeTalks = false)
        {
            try
            {
                var result = await repository.GetAllCampsAsync(includeTalks);

                if (result == null)
                    return NotFound();

                // Mapping
                var mappedResult = mapper.Map<IEnumerable<CampModel>>(result);

                return Ok(mappedResult);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{moniker}", Name  = "GetCamp")]
        public async Task<IHttpActionResult> Get(string moniker, bool includeTalks = false)
        {
            try
            {
                var result = await repository.GetCampAsync(moniker, includeTalks);

                if (result == null)
                    return NotFound();

                var mappedResult = mapper.Map<CampModel>(result);

                return Ok(mappedResult);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("searchByDate/{eventDate:datetime}")]
        [HttpGet]
        public async Task<IHttpActionResult> SearchByEventDate(DateTime eventDate, bool includeTalks = false)
        {
            try
            {
                var result = await repository.GetAllCampsByEventDate(eventDate, includeTalks);

                var mappedResult = mapper.Map<CampModel[]>(result);

                return Ok(mappedResult);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route()]
        public async Task<IHttpActionResult> Post(CampModel model)
        {
            try
            {
                if (await repository.GetCampAsync(model.Moniker) != null)
                {
                    ModelState.AddModelError("Moniker", "Moniker in use");
                }

                if (ModelState.IsValid)
                {

                    var camp = mapper.Map<Camp>(model);
                    
                    repository.AddCamp(camp);
                    
                    if (await repository.SaveChangesAsync())
                    {
                        var newModel = mapper.Map<CampModel>(camp);
                        var location = Url.Link("GetCamp", new { moniker = newModel.Moniker });

                        return Created(location, newModel);
                    }
                }
            }
            catch(Exception ex)
            {
                return InternalServerError(ex);
            }

            return BadRequest(ModelState);
        }

        [Route("{moniker}")]
        public async Task<IHttpActionResult> Put(string moniker, CampModel model)
        {
            try
            {
                var camp = await repository.GetCampAsync(moniker);
                if (camp == null)
                {
                    return NotFound();
                }

                mapper.Map(model, camp);

                if (await repository.SaveChangesAsync())
                {
                    return Ok(mapper.Map<CampModel>(camp));
                }
                else
                {
                    return InternalServerError();
                }    
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{moniker}")]
        public async Task<IHttpActionResult> Delete(string moniker)
        {
            try
            {
                var camp = await repository.GetCampAsync(moniker);
                if (camp == null)
                {
                    return NotFound();
                }

                repository.DeleteCamp(camp);

                if (await repository.SaveChangesAsync())
                {
                    return Ok();
                }
                else
                {
                    return InternalServerError();
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}