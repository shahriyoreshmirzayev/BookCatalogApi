using AutoMapper;
using BookCatalogApi.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalogApi.Controllers;

[ApiController]
[ValidationActionFilters]
[CustomExceptionFilter]
public class ApiControllerBase : ControllerBase
{
    private readonly IMapper mapper;
    protected IMapper _mapper => mapper ?? HttpContext.RequestServices.GetRequiredService<IMapper>();

    
}
