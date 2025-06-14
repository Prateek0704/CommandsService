using System.Text.Json;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;

namespace CommandsService.EventProcessing{ 
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;

        public EventProcessor(IServiceScopeFactory scopeFactory,IMapper mapper)
        {
            _scopeFactory = scopeFactory;
            _mapper = mapper;
        }
        private EventType DetermineEvent(string notificationMessage)
        {
            Console.WriteLine(" --> Determinig Event");
            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);
            switch(eventType.Event){
                case "Platform_Published":
                Console.WriteLine("--->  Platform published Event detected... ");
                return EventType.platformPublished;
                default:
                Console.WriteLine("----> Cannot determine the event type..");
                return EventType.Undetermind;
            }
        }
        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);
            switch(eventType){
                case EventType.platformPublished:
                addPlatform(message);
                break;
                default:
                //
                break;
            }
        }
        private void addPlatform(string platformPublishedMessage)
        {
            using(var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();
                var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);
                try{
                    var plat = _mapper.Map<Platform>(platformPublishedDto);
                    if(!repo.ExternalPlatformExist(plat.ExternalId))
                    {
                        repo.CreatePlatform(plat);
                        repo.SaveChanges();
                        Console.WriteLine("------> Platform added successfully");
                    }
                    else
                    {
                        Console.WriteLine("---> Platform already exist....");
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($" ---> Could not add Platform to DB {ex.Message}");
                }
            }
        }
    }
    enum EventType
    {
        platformPublished,
        Undetermind
    }
}