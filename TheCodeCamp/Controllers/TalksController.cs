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
    [RoutePrefix("api/camps/{moniker}/talks")]
    public class TalksController : ApiController
    {
        private readonly ICampRepository repository;
        private readonly IMapper mapper;

        public TalksController(ICampRepository repository, IMapper mapper) 
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        [Route()]
        public async Task<IHttpActionResult> Get(string moniker, bool includeSpeakers = false)
        {
            try
            {
                var results = await repository.GetTalksByMonikerAsync(moniker, includeSpeakers);

                return Ok(mapper.Map<IEnumerable<TalkModel>>(results));
            }
            catch(Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{id:int}", Name = "GetTalk")]
        public async Task<IHttpActionResult> Get(string moniker, int id, bool includeSpeakers = false)
        {
            try
            {
                var result = await repository.GetTalkByMonikerAsync(moniker, id, includeSpeakers);
                if (result == null)
                {
                    return NotFound();
                }

                return Ok(mapper.Map<TalkModel>(result));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route()]
        public async Task<IHttpActionResult> Post(string moniker, TalkModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var camp = await repository.GetCampAsync(moniker);
                    if (camp != null)
                    {
                        var talk = mapper.Map<Talk>(model);
                        talk.Camp = camp;

                        if (model.Speaker != null)
                        {
                            var speaker = await repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                            if (speaker != null)
                            {
                                talk.Speaker = speaker;
                            }
                        }

                        repository.AddTalk(talk);

                        if (await repository.SaveChangesAsync())
                        {
                            return CreatedAtRoute("GetTalk", new { Moniker = moniker, id = talk.TalkId }, mapper.Map<TalkModel>(talk));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            return BadRequest(ModelState);
        }

        [Route("{talkId:int}")]
        public async Task<IHttpActionResult> Put(string moniker, int talkId, TalkModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var talk = await repository.GetTalkByMonikerAsync(moniker, talkId, true);
                    if (talk == null)
                    {
                        return NotFound();
                    }

                    mapper.Map(model, talk);

                    if (talk.Speaker.SpeakerId != model.Speaker.SpeakerId)
                    {
                        var speaker = await repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                        if (speaker != null)
                        {
                            talk.Speaker = speaker;
                        }
                    }

                    if (await repository.SaveChangesAsync())
                    {
                        return Ok(mapper.Map<TalkModel>(talk));
                    }
                    else
                    {
                        return InternalServerError();
                    }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            return BadRequest(ModelState);
        }

        [Route("{talkId:int}")]
        public async Task<IHttpActionResult> Delete(string moniker, int talkId)
        {
            try
            {
                var talk = await repository.GetTalkByMonikerAsync(moniker, talkId);
                if (talk == null)
                {
                    return NotFound();
                }

                repository.DeleteTalk(talk);

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