using AutoMapper;
using BookApplication.DTOs.AuthorDTO;
using BookApplication.Repositories;
using BookCatalogApiDomain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Memory;

namespace BookCatalogApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthorController : ControllerBase
{
    private readonly IBookRepository _bookRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly IValidator<Author> _validator;
    private readonly IMapper _mapper;

    private readonly IMemoryCache _memoryCache; // keshlash uchun

    private readonly string _Cashe_Key = "MyKey";

    public AuthorController(IBookRepository bookRepository, IAuthorRepository authorRepository, IValidator<Author> validator, IMapper mapper, IMemoryCache memoryCache)
    {
        _bookRepository = bookRepository;
        _authorRepository = authorRepository;
        _validator = validator;
        _mapper = mapper;
        _memoryCache = memoryCache;
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> GetAuthorById([FromQuery] int id)
    {
        if (_memoryCache.TryGetValue(id.ToString(), out AuthorGetDTO CashedAuthor))
        {
            return Ok(CashedAuthor);
        }



        Author author = await _authorRepository.GetByIdAsync(id);
        if (author == null)
        {
            return NotFound($"Author Id {id} not found. .....!");
        }
        AuthorGetDTO authorGet = _mapper.Map<AuthorGetDTO>(author);
        _memoryCache.Set(id.ToString(), authorGet);
        return Ok(authorGet);
    }

    [HttpGet("[action]")]
    [OutputCache(Duration = 30)]
    public async Task<IActionResult> GetAllAuthors()
    {
        bool casheHit = _memoryCache.TryGetValue(_Cashe_Key, out IEnumerable<AuthorGetDTO>? CashedAuthors);
        if (!casheHit)
        {
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                .SetSlidingExpiration(TimeSpan.FromSeconds(4));


            var Authors = await _authorRepository.GetAsync(x => true);
            IEnumerable<AuthorGetDTO> resAuthors = _mapper.Map<IEnumerable<AuthorGetDTO>>(Authors);
            _memoryCache.Set(_Cashe_Key, resAuthors, options);
            return Ok(resAuthors);
        }
        return Ok(CashedAuthors);

    }

    [HttpPost("[action]")]
    public async Task<IActionResult> CreateAuthor([FromBody] AuthorCreateDTO createDTO)
    {
        if (ModelState.IsValid)
        {
            Author author = _mapper.Map<Author>(createDTO);
            var validResult = _validator.Validate(author);
            if (!validResult.IsValid) return BadRequest(validResult);

            //for (int i = 0; i < author.Books.Count; i++)
            //{
            //    Book book = author.Books.ToArray()[i];
            //    book = await _bookRepository.GetByIdAsync(book.Id);
            //    if (book == null)
            //    {
            //        return NotFound("Book Id not found. . ...");
            //    }
            //}
            author = await _authorRepository.AddAsync(author);
            if (author == null) return NotFound();
            AuthorGetDTO authorGet = _mapper.Map<AuthorGetDTO>(author);
            return Ok(authorGet);
        }
        return BadRequest(ModelState);
    }

    [HttpPut("[action]")]
    public async Task<IActionResult> UpdateAuthor([FromBody] AuthorUpdateDTO createDTO)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        Author author = _mapper.Map<Author>(createDTO);
        var validationRes = _validator.Validate(author);
        if (validationRes.IsValid)
        {
            return BadRequest(validationRes);
        }

        author = await _authorRepository.AddAsync(author);
        if (author == null) return NotFound();
        AuthorGetDTO authorGet = _mapper.Map<AuthorGetDTO>(author);
        _memoryCache.Remove(_Cashe_Key);                             //Keyni o'chirib yuborish
        return Ok(authorGet);

    }

    [HttpDelete("[action]")]
    public async Task<IActionResult> DeleteAuthor([FromQuery] int id)
    {
        bool isDelete = await _authorRepository.DeleteAsync(id);
        return isDelete ? Ok("Deleted succesfuly ....") : BadRequest("Delete operation failed");
    }

}
