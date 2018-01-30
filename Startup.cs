using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using Transifex.Backend.Models;
using Transifex.Backend.Services;

namespace Transifex.Backend
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            
            services.AddSingleton (Configuration);
            services.AddSingleton (typeof (IRedisService), typeof (RedisService));
            services.AddSingleton (typeof (ITransifexService), typeof (TransifexService));
            services.AddSingleton (typeof (IMongoDbService), typeof (MongoDbService));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseMvc (routes => {
                routes.MapRoute (
                    name: "updateData",
                    template: "app/admin/updateData",
                    defaults: new {
                        controller = "Admin",
                        action = "UpdateData"
                    });
                routes.MapRoute (
                    name: "query",
                    template: "app/query",
                    defaults: new {
                        controller = "Home",
                        action = "Query"
                    });
            });
        }
    }
}
