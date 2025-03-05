using BookApplication.DTOs.BookDTO;
using BookApplication.Repositories;
using BookApplication.UseCases.Notifications;
using BookCatalogApi.Filters;
using BookCatalogApiDomain.Entities;
using FluentValidation;
using LazyCache;
using MediatR;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BookCatalogApi.Controllers;

[Route("api/[controller]")]
public class BookController : ApiControllerBase
{
    private readonly IBookRepository _bookRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly IValidator<Book> _validator;
    private readonly IAppCache _lazyCache;
    private const string _Key = "MyLazyCache";
    private readonly IMediator _mediator;
    public BookController(IBookRepository bookRepository, IValidator<Book> bookValidator, IAuthorRepository authorRepository, IAppCache lazyCache, IMediator mediator)
    {
        _bookRepository = bookRepository;
        _validator = bookValidator;
        _authorRepository = authorRepository;
        _lazyCache = lazyCache;
        _mediator = mediator;
    }

    //[HttpGet("[action]")]
    [EnableCors("PolicyForMicrosoft")]
    public async Task<IActionResult> GetAllBooks()
    {
        IEnumerable<BookGetDTO> res = await _lazyCache.GetOrAdd(_Key,
            async options =>
            {
                options.SetAbsoluteExpiration(TimeSpan.FromSeconds(30));
                options.SetSlidingExpiration(TimeSpan.FromSeconds(10));

                var books = await _bookRepository.GetAsync(x => true);

                IEnumerable<BookGetDTO> bookRes = _mapper.Map<IEnumerable<BookGetDTO>>(books);
                return bookRes;
            });
        return Ok(res);
    }

    [HttpGet("[action]/{id}")]
    [CustomAuthorizationFilter("GetBook")]
    public async Task<IActionResult> GetBookById(int id)
    {
        Book book = await _bookRepository.GetByIdAsync(id);
        if (book == null) NotFound($"Book Id: {id} not found ....!");
        return Ok(_mapper.Map<BookGetDTO>(book));
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> CreateBook([FromBody] BookCreateDTO bookCreate)
    {
        BookAddedNotification bookAddedNotification = new()
        {
            book = new Book()
            {
                Name = "Test"
            }
        };
        await _mediator.Publish(bookAddedNotification);
        Book book = _mapper.Map<Book>(bookCreate);
        var validationRes = _validator.Validate(book);
        if (!validationRes.IsValid)
            return BadRequest(validationRes);
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
        if (book == null) NotFound("Book not found . ....!");
        return Ok(_mapper.Map<BookGetDTO>(book));
    }

    [HttpPut("[action]")]
    public async Task<IActionResult> UpdateBook([FromBody] BookUpdateDTO bookCreate)
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
        if (book == null) NotFound("Book not found . ....!");
        return Ok(_mapper.Map<BookGetDTO>(book));
    }

    public async Task<IActionResult> DeleteBook([FromQuery] int bookId)
    {
        if (_bookRepository.DeleteAsync(bookId) != null)
        {
            return Ok("Deleted Succesfuly .....!");
        }
        return BadRequest("Delete operation failed .....");
    }

    [HttpGet]
    public async Task<IActionResult> SearchBook(string text)
    {
        return Ok(_bookRepository.SearchBokks(text));
    }
}
