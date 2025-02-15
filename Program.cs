
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
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "Bearer Authentication with JWT Token",
                    Type = SecuritySchemeType.Http
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme()
                        {
                            Reference = new OpenApiReference()
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new List<string>()
                    }
                });
            });

            builder.Services.AddMemoryCache();
            builder.Services.AddStackExchangeRedisCache(setupAction =>
            {
                setupAction.Configuration = builder.Configuration.GetConnectionString("RedisConnectionString");

            });

            builder.Services.AddResponseCaching();
            builder.Services.AddOutputCache();
            builder.Services.AddControllers();

            /*builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: "FixedWindowLimiter",
                    factory: x => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 8,
                        Window = TimeSpan.FromSeconds(20),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                        AutoReplenishment = true,
                        SegmentsPerWindow = 3
                    }));
            });*/

            /*builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: "TokenBucketLimiter",
                    factory: x => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 10,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(30),
                        TokensPerPeriod = 5,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
            });*/

            /*builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetConcurrencyLimiter(
                    partitionKey: "TokenBucketLimiter",
                    factory: x => new ConcurrencyLimiterOptions
                    {
                        PermitLimit = 2,
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    }));
            });*/

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            //app.UseAuthorization();
            app.UseAuthorization(); // Ruxsatlar tekshiriladi

            app.UseAuthentication(); // Foydalanuvchi autentifikatsiyasi tekshiriladi

            app.UseResponseCaching();

            app.UseOutputCache();
            //app.UseETagger();
            app.MapControllers();

            app.Run();
        }
    }
}
