using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace BookCatalogApi.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class CacheResourceFilterAttribute : Attribute, IAsyncResourceFilter
{
    private readonly IMemoryCache _cache;
    private readonly string _cacheKey;

    public CacheResourceFilterAttribute(string cacheKey)
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _cacheKey = cacheKey;
    }

    /* public void OnResourceExecuted(ResourceExecutedContext context)
     {
         if (_cache.TryGetValue(_cacheKey, out var cashedResult))
         {
             context.Result = cashedResult as IActionResult;
         }
     }

     public void OnResourceExecuting(ResourceExecutingContext context)
     {
         if(context.Result is IActionFilter result)
         {
             var option = new MemoryCacheEntryOptions()
                 .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                 .SetSlidingExpiration(TimeSpan.FromSeconds(10));

             _cache.Set(_cacheKey, result, option);
         }
     }*/

    public Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        if (_cache.TryGetValue(_cacheKey, out var cashedResult))
        {
            context.Result = cashedResult as IActionResult;
        }
        next();

        if (context.Result is IActionFilter result)
        {
            var option = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                .SetSlidingExpiration(TimeSpan.FromSeconds(10));

            _cache.Set(_cacheKey, result, option);
        }
        return Task.CompletedTask;
    }
}
