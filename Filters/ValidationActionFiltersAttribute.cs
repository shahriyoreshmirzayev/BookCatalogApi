using Microsoft.AspNetCore.Mvc.Filters;

namespace BookCatalogApi.Filters
{
    public class ValidationActionFiltersAttribute : IActionFilter 
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            throw new NotImplementedException();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
