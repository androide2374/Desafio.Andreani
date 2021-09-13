using Geocodificador.Configuration;
using Geocodificador.Context;
using Geocodificador.Controllers;
using Geocodificador.BackgroundServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geocodificador
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("MiDB")));

            services.Configure<RabbitMqConfiguration>(Configuration.GetSection("RabbitMqGeocodificar"));
            var RabbitConfig = Configuration
                .GetSection("RabbitMqGeocodificar")
                .Get<RabbitMqConfiguration>();
            services.AddSingleton(RabbitConfig);
            services.AddSingleton<RabbitManagement>();

            services.AddHttpClient();;

            services.AddHostedService<ConsumeRabbitMQHostedService>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
