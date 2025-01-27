using BookApplication.Repositories;
using BookCatalogApiDomain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalogApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookController : ControllerBase
{
    private readonly IBookRepository _bookRepository;
    private readonly IValidator<Book> _validator;
    public BookController(IBookRepository bookRepository, IValidator<Book> bookValidator)
    {
        _bookRepository = bookRepository;
        _validator = bookValidator;
    }
    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllBooks()
    {
        return Ok(await _bookRepository.GetAsync(x => true));
    }
    [HttpGet("[action]/{id}")]
    public async Task<IActionResult> GetBookById(int id)
    {
        return Ok(await _bookRepository.GetByIdAsync(id));
    }
    [HttpPost("action")]
    public async Task<IActionResult> 




}
