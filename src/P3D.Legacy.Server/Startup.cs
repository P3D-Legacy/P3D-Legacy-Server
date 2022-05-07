using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using P3D.Legacy.Server.Abstractions.Utils;

using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace P3D.Legacy.Server
{
    public class Startup
    {
        private static JsonSerializerOptions ConfigureSite(JsonSerializerOptions opt)
        {
            opt.Converters.Add(new DateTimeOffsetSerializer());
            opt.Converters.Add(new DateTimeOffsetNullableSerializer());
            return Configure(opt);
        }
        private static JsonSerializerOptions Configure(JsonSerializerOptions opt)
        {
            opt.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            opt.PropertyNameCaseInsensitive = true;
            opt.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            opt.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            opt.Converters.Add(new DateTimeOffsetSerializer());
            opt.Converters.Add(new DateTimeOffsetNullableSerializer());
            return opt;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JsonSerializerOptions>(static opt => Configure(opt));
            services.Configure<JsonSerializerOptions>("P3D", static opt => ConfigureSite(opt));

            services.AddControllers()
                .AddControllersAsServices()
                .AddJsonOptions(static opt => Configure(opt.JsonSerializerOptions));

            services.AddRouting(static opt =>
            {
                opt.LowercaseUrls = true;
            });

            services.AddSwaggerGen(static opt =>
            {
                var appName = typeof(Startup).Assembly.GetName().Name;

                opt.SwaggerDoc("v1", new OpenApiInfo { Title = appName, Version = "v1" });
                opt.SupportNonNullableReferenceTypes();

                var xmlFile = $"{appName}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                opt.IncludeXmlComments(xmlPath);
            });

            services.AddCors(static options =>
            {
                options.AddPolicy("Development", static builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                );
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors("Development");
            }

            app.UseSwagger();
            app.UseSwaggerUI(static options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", typeof(Startup).Assembly.GetName().Name);
            });

            app.UseWebSockets();

            app.UseRouting();

            app.UseEndpoints(static endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}