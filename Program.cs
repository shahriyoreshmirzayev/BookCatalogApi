using BookApplication;
using BookInfrastructure;
using Microsoft.OpenApi.Models;


namespace BookCatalogApi
{
    public class Program
    {
        //static int a = 30;
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddApplicationServices();

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "Bearer Authentication with JWT Token",
                    Type = SecuritySchemeType.Http
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {{
                    new OpenApiSecurityScheme()
                    {
                       Reference=new OpenApiReference()
                       {
                           Id="Bearer",
                           Type=ReferenceType.SecurityScheme
                       }
                    },
                    new List<string>()
                } });
            });

            builder.Services.AddMemoryCache();  //In-Memory cache

            builder.Services.AddStackExchangeRedisCache(setupAction =>
            {
                setupAction.Configuration = builder.Configuration.GetConnectionString("RedisConnectionString");
            });


            builder.Services.AddCors(opt =>
            {
                opt.AddPolicy("PolicyForPDP",
                policy =>
                {
                    policy.WithOrigins("")
                    .WithHeaders("TestHeader")
                    .WithMethods("GET", "Put");
                });

                opt.AddPolicy("PolicyForMicrosoft",
                policy =>
                {
                    policy.WithOrigins("")
                    .WithHeaders("Net-Header")
                    .WithMethods("GET", "Post");
                });

                opt.AddDefaultPolicy(policyOpt =>
                {
                    policyOpt.WithOrigins("");

                });
            });

            builder.Services.AddResponseCaching();

            builder.Services.AddOutputCache();

            builder.Services.AddControllers();



            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            //app.UseRateLimiter();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.Use((context, next) =>
            {
                if (context.Request.Method.Equals("OPTIONS"))
                {
                    Console.WriteLine(context.Request.Method);
                    foreach (var item in context.Request.Headers)
                    {
                        Console.WriteLine(item.Key + ":" + item.Value);
                    }
                    next();

                    Console.WriteLine("\nThis is Response header \n");
                    foreach (var item in context.Response.Headers)
                    {
                        Console.WriteLine(item.Key + ":" + item.Value);
                    }

                }
                else
                {
                    next();
                }
                return Task.CompletedTask;
            });
            app.UseCors(opt =>
            {
                opt.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            });

            app.UseResponseCaching();

            app.UseOutputCache();
            //app.UseETagger();
            app.MapControllers();

            app.Run();
        }
    }
}
