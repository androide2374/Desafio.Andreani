using Geocodificador.Configuration;
using Geocodificador.Context;
using Geocodificador.Models.Result;
using Geocodificador.Models.Tables;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Geocodificador.BackgroundServices
{
    public class ConsumeRabbitMQHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private RabbitMqConfiguration _rabbit;
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpclient;
        private readonly IConfiguration _config;

        public ConsumeRabbitMQHostedService(ILoggerFactory loggerFactory, RabbitMqConfiguration rabbit, IServiceScopeFactory factory,
            IHttpClientFactory httpclient, IConfiguration config)
        {
            _config = config;
            if (loggerFactory is null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            _httpclient = httpclient;
            _rabbit = rabbit;
            this._logger = loggerFactory.CreateLogger<ConsumeRabbitMQHostedService>();
            _context = factory.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
            InitRabbitMQ();
        }
        private void InitRabbitMQ()
        {
            var factory = new ConnectionFactory { HostName = _rabbit.Hostname, Port = Convert.ToInt32(_rabbit.Port), UserName = _rabbit.UserName, Password = _rabbit.Password };

            // create connection  
            _connection = factory.CreateConnection();

            // create channel  
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(_rabbit.QueueName, false, false, false, null);

            _channel.BasicQos(0, 1, false);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
            var noprocesado = _context.Geocodificar.Where(x => x.Estado == "Procesando").ToListAsync();
            noprocesado.Result.ForEach(noproc =>
            {
                var pedidosSinProcesar = _context.PedidoGeo.FirstOrDefault(x => x.Id == noproc.IdPedido);
                Geocoding(pedidosSinProcesar, noproc);
            });

        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var content = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());

                Geocoding(JsonConvert.DeserializeObject<PedidoGeo>(content));
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume(_rabbit.QueueName, false, consumer);
            return Task.CompletedTask;
        }

        private async Task Geocoding(PedidoGeo content, Geocodificar geocodificar = null)
        {
            Console.WriteLine($"--- Procesando {content.Id} ----");
            if (geocodificar == null)
            {
                geocodificar = new Geocodificar()
                {
                    IdPedido = content.Id,
                    Estado = "Procesando"
                };
                await _context.Geocodificar.AddAsync(geocodificar);
                await _context.SaveChangesAsync();
                Thread.Sleep(20000);
            }
            string urlrequest = $"https://nominatim.openstreetmap.org/search.php?street={content.Numero},{content.Calle}&city={content.Ciudad}&state={content.Provincia}&country={content.Pais}&postalcode={content.CP}&format=jsonv2";
            //configuro httpclient
            var request = new HttpRequestMessage(HttpMethod.Get, urlrequest);

            request.Headers.Add("User-Agent", "HttpClientFactory-Sample");

            var clienthttp = _httpclient.CreateClient();

            var response = await clienthttp.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = response.Content.ReadAsStringAsync();
                var resp = JsonConvert.DeserializeObject<RootObject>(((Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(responseStream.Result)).First.ToString());

                geocodificar.Latitud = resp.lat;
                geocodificar.Longitud = resp.lon;
                geocodificar.Estado = "Terminado";
                _context.Entry(geocodificar).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
            _context.Dispose();
        }
    }
}
