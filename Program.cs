
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

            /*app.Use((context, next) =>
            {
                a--;
                Console.WriteLine($"\nA: {a}\n");
                Console.WriteLine("\n*********  Requst coming  *********\n");
                return next(context);
            });*/

            //app.UseRateLimiter();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseResponseCaching();
            app.UseOutputCache();
            app.MapControllers();
            //System.Timers.Timer timer = new System.Timers.Timer();
            //timer.Elapsed += Timer_Elapsed;
            //timer.Interval = 1000;
            //timer.Start();

            app.Run();
        }

        /*private static void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (a % 30 == 0)
            {
                a += 5;
                Console.WriteLine($"\nReplanish:=> A: {a}\n");
            }
            //a++; 


            //throw new NotImplementedException();
            Console.WriteLine(e.SignalTime);
        }*/
    }
}
