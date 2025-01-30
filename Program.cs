
using BookApplication;
using BookInfrastructure;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Timers;

namespace BookCatalogApi
{
    public class Program
    {
        static int a = 0;
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddApplicationServices();
            builder.Services.AddMemoryCache();

            builder.Services.AddStackExchangeRedisCache(setupAction =>
            {
                setupAction.Configuration = builder.Configuration.GetConnectionString("RedisConnectionString");

            });


            builder.Services.AddResponseCaching();
            builder.Services.AddOutputCache();
            builder.Services.AddControllers();

        
            builder.Services.AddRateLimiter(options =>
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
            });



            /*builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: "FixedWindowLimiter",
                    factory: x => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 4,
                        Window = TimeSpan.FromSeconds(10),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
            });*/



            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 1000;
            timer.Start();


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.Use((context, next) =>
            {
                Console.WriteLine("*********  Requst coming  *********");
                return next(context);
            });

            app.UseRateLimiter();

           


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

            app.Run();
        }

        private static void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (a % 20 == 0)
            {
                Console.WriteLine($"\nA: {a}\n");
            }
            a++;


            //throw new NotImplementedException();
            Console.WriteLine(e.SignalTime);
        }
    }
}
