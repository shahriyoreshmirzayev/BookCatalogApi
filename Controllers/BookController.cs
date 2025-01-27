using AutoMapper;
using BookApplication.DTO;
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
    private readonly IAuthorRepository _authorRepository;
    private readonly IValidator<Book> _validator;
    private readonly IMapper _mapper;
    public BookController(IBookRepository bookRepository, IValidator<Book> bookValidator, IMapper mapper, IAuthorRepository authorRepository)
    {
        _bookRepository = bookRepository;
        _validator = bookValidator;
        _mapper = mapper;
        _authorRepository = authorRepository;
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
    public async Task<IActionResult> CreateBook([FromBody] BookCreateDTO bookCreate)
    {
        if (ModelState.IsValid)
        {
            Book book = _mapper.Map<Book>(bookCreate);
            var validationRes = _validator.Validate(book);
            if (!validationRes.IsValid) return BadRequest(validationRes);
            for (int i = 0; i < book.Authors.Count; i++)
            {
                Author author = book.Authors.ToArray()[i];
                author = await _authorRepository.GetByIdAsync(author.Id);
                if (author == null)
                {
                    return NotFound("Author Id: " + author.Id + " not found . . . . !");
                }
            }
            book = await _bookRepository.AddAsync(book);
            return Ok(book);
        }
        return BadRequest(ModelState);
    }

    [HttpPut("action")]
    public async Task<IActionResult> UpdateBook([FromBody] BookUpdateDTO bookCreate)
    {
        if (ModelState.IsValid)
        {
            Book book = _mapper.Map<Book>(bookCreate);
            var validationRes = _validator.Validate(book);
            if (!validationRes.IsValid)
            {
                return BadRequest(validationRes);
            }
            for (int i = 0; i < book.Authors.Count; i++)
            {
                Author author = book.Authors.ToArray()[i];
                author = await _authorRepository.GetByIdAsync(author.Id);
                if (author == null)
                {
                    return NotFound($"Author Id: {author.Id} not found ...... !");
                }
            }
            book = await _bookRepository.UpdateAsync(book);
            return Ok(book);
        }
        return BadRequest(ModelState);
    }
    [HttpDelete("action")]
    public async Task<IActionResult> DeleteBook([FromQuery] int bookId)
    {
        if (_bookRepository.DeleteAsync(bookId) != null)
        {
            return Ok("Deleted Succesfuly .....!");
        }
        return BadRequest("Delete operation failed .....");
    }
}
