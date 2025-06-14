using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers
{
    [Route("api/c/platforms/{platformId}/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly ICommandRepo _repo;
        private readonly IMapper _mapper;

        public CommandsController(ICommandRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(int platformId)
        {
            Console.WriteLine($"----> Hit GetCommandsForPlatform: {platformId}");
            if(_repo.PlatformExist(platformId))
            {
                Console.WriteLine($" Platform Exist fetching Commands for it");
                var commands = _repo.GetCommandsForPlatform(platformId);
                return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commands));
            }
            else
            {
                Console.WriteLine($" Platform does not exist, Sorry !! ");
                return NotFound();
            }
        }

        [HttpGet("{commandId}", Name = "GetCommandForPlatform")]
        public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId)
        {
             Console.WriteLine($"----> Hit GetCommandForPlatform: {platformId + " for Command Id : "+ commandId}");
             if(!_repo.PlatformExist(platformId))
             {
                Console.WriteLine($" Platform does not exist, Sorry !! ");
                return NotFound();
             }
             var command = _repo.GetCommand(platformId,commandId);
             if(command == null)
             {
                Console.WriteLine($" Command does not exist, Sorry !! ");
                return NotFound();
             }
             return Ok(_mapper.Map<CommandReadDto>(command));
        }

        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommandForPlatform(int platformId, CommandCreateDto commanddto)
        {
              Console.WriteLine($"----> Hit CreateCommandForPlatform: {platformId}");
             if(!_repo.PlatformExist(platformId))
             {
                Console.WriteLine($" Platform does not exist, Sorry !! "+ platformId);
                return NotFound();
             }
             var command = _mapper.Map<Command>(commanddto);
             _repo.CreateCommand(platformId,command);
             _repo.SaveChanges();

            
             var commandRead = _mapper.Map<CommandReadDto>(command);
            Console.WriteLine("command created successully , commandId is : " + commandRead.Id);
             return CreatedAtRoute(nameof(GetCommandForPlatform), new {  platformId, commandId = commandRead.Id },commandRead);
        }
    }
}