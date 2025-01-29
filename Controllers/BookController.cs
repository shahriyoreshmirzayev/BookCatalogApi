﻿using AutoMapper;
using BookApplication.DTOs.BookDTO;
using BookApplication.Repositories;
using BookCatalogApiDomain.Entities;
using FluentValidation;
using LazyCache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BookCatalogApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookController : ControllerBase
{
    private readonly IBookRepository _bookRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly IValidator<Book> _validator;
    private readonly IMapper _mapper;
    private readonly IAppCache _lazyCache;
    private const string _Key = "MyLazyCache";
    public BookController(IBookRepository bookRepository, IValidator<Book> bookValidator, IMapper mapper, IAuthorRepository authorRepository, IAppCache lazyCache)
    {
        _bookRepository = bookRepository;
        _validator = bookValidator;
        _mapper = mapper;
        _authorRepository = authorRepository;
        _lazyCache = lazyCache;
    }

    [HttpGet("[action]")]
    [ResponseCache(Duration = 20)]
    public async Task<IActionResult> GetAllBooks()
    {
        bool IsActive = _lazyCache.TryGetValue(_Key, out IEnumerable<BookGetDTO> CacheBooks);
        if(!IsActive)
        {
            var books = await _bookRepository.GetAsync(x => true);
            if(books == null)
            {
                IEnumerable<BookGetDTO> booksRes = _mapper.Map<IEnumerable<BookGetDTO>>(books);

                var entryOptions = new MemoryCacheEntryOptions()
                                   .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                                   .SetSlidingExpiration(TimeSpan.FromSeconds(10));

                _lazyCache.Add(_Key, booksRes, entryOptions);
                Console.WriteLine("_lazycache hit.....");
                return Ok(booksRes);
            }
            return NoContent();
        }
        return Ok(CacheBooks);
        
        
        
        
        

        /*var books = (await _bookRepository.GetAsync(x => true));
        if (books == null)
        {
            return Ok();
        }
        IEnumerable<BookGetDTO> booksRes = _mapper.Map<IEnumerable<BookGetDTO>>(books);*/
        //return Ok(booksRes);
    }

    [HttpGet("[action]/{id}")]
    public async Task<IActionResult> GetBookById(int id)
    {
        Book book = await _bookRepository.GetByIdAsync(id);
        if (book == null) NotFound($"Book Id: {id} not found ....!");
        return Ok(_mapper.Map<BookGetDTO>(book));
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> CreateBook([FromBody] BookCreateDTO bookCreate)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

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
        if (ModelState.IsValid)
            return BadRequest(ModelState);


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
    [HttpDelete("[action]")]
    public async Task<IActionResult> DeleteBook([FromQuery] int bookId)
    {
        if (_bookRepository.DeleteAsync(bookId) != null)
        {
            return Ok("Deleted Succesfuly .....!");
        }
        return BadRequest("Delete operation failed .....");
    }
}
