
using System.Text;
using System.Threading.Tasks;
using CommandsService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandsService.AsyncDataServices
{
    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IEventProcessor _eventProcessor;
        private IConnection _connection;
        private IChannel _channel;
        private string _queueName;

        public  MessageBusSubscriber(IConfiguration configuration, IEventProcessor eventProcessor)
        {
            _configuration = configuration;
            _eventProcessor = eventProcessor;
        }

        private async Task Init()
        {
            var factory = new ConnectionFactory(){HostName = _configuration["RabbitMQHost"], 
            Port = int.Parse(_configuration["RabbitMQPort"])};
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.ExchangeDeclareAsync(exchange: "trigger",type: ExchangeType.Fanout);
            var queue = await _channel.QueueDeclareAsync();
            _queueName = queue.QueueName;
            _channel.QueueBindAsync(_queueName,"trigger","");
            Console.WriteLine(" ---> Listening on message bus");
            _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutDown;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Init();

            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync +=  async (ModuleHandle, ea) => {
                Console.WriteLine("Event Receieved");
                var body = ea.Body;
                var notification = Encoding.UTF8.GetString(body.ToArray());
                _eventProcessor.ProcessEvent(notification);
                await Task.CompletedTask;
            };
            await _channel.BasicConsumeAsync(queue: _queueName,autoAck: true, consumer: consumer);
        }
        private async Task RabbitMQ_ConnectionShutDown(object sender,ShutdownEventArgs e){
             Console.WriteLine("----> Connection Shutdown");
    }
        public override void  Dispose()
        {
            if(_connection.IsOpen)
            {
                _channel.CloseAsync();
               _connection.CloseAsync();
            }
             base.Dispose();
        }
    }
}