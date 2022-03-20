using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using P3D.Legacy.Server.Abstractions.Utils;

using System;
using System.IO;
using System.Reflection;
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
            services.Configure<JsonSerializerOptions>(options => Configure(options));
            services.Configure<JsonSerializerOptions>("P3D", options => ConfigureSite(options));

            var appName = typeof(Startup).Assembly.GetName().Name ?? "ERROR";

            services.AddControllers().AddControllersAsServices().AddJsonOptions(options => Configure(options.JsonSerializerOptions));

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = appName, Version = "v1" });
                options.SupportNonNullableReferenceTypes();

                var xmlFile = $"{appName}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            services.AddCors(options =>
            {
                options.AddPolicy("Development", builder => builder
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

            var appName = Assembly.GetEntryAssembly()!.GetName().Name;

            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", appName));

            app.UseWebSockets();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}