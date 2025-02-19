using AutoMapper;
using BookApplication.DTOs.AuthorDTO;
using BookApplication.Repositories;
using BookCatalogApi.Filters;
using BookCatalogApiDomain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace BookCatalogApi.Controllers;

[Route("api/[controller]")]
public class AuthorController : ApiControllerBase
{
    private readonly IBookRepository _bookRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly IValidator<Author> _validator;
    private readonly IMemoryCache _memoryCache; // keshlash uchun
    private readonly IDistributedCache _cache;  // Redis uchun

    private readonly string _Cashe_Key = "MyKey";

    public AuthorController(IBookRepository bookRepository, IAuthorRepository authorRepository, IValidator<Author> validator,  IMemoryCache memoryCache, IDistributedCache cache)
    {
        _bookRepository = bookRepository;
        _authorRepository = authorRepository;
        _validator = validator;
        _memoryCache = memoryCache;
        _cache = cache;
    }

    [HttpGet("[action]")]
    [Authorize(Roles = "GetAuthor")]
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

    //[OutputCache(Duration = 30)]
    [HttpGet("[action]")]
    [CacheResourceFilter("AllAuthorsCache")]
    [CustomAuthorizationFilter("GetAllAuthors")]
    public async Task<IActionResult> GetAllAuthors()
    {
        /*string? CachedAuthors = await _cache.GetStringAsync(_Cashe_Key);

        if (string.IsNullOrEmpty(CachedAuthors))
        {
            // Ma'lumotlarni asinxron yuklash
            IQueryable<Author> query = await _authorRepository.GetAsync(x => true);
            List<Author> Authors = await query.ToListAsync();


            // DTO ga xaritalash
            IEnumerable<AuthorGetDTO> resAuthors = _mapper.Map<IEnumerable<AuthorGetDTO>>(Authors);

            // Cache-ga yozish
            await _cache.SetStringAsync(_Cashe_Key, JsonSerializer.Serialize(resAuthors), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            });

            return Ok(resAuthors);
        }

        Console.WriteLine("GetStringAsync return json .......");
        var res = JsonSerializer.Deserialize<IEnumerable<AuthorGetDTO>>(CachedAuthors);
        return Ok(res);*/
        // PDP Online kodi
        throw new NotImplementedException();
        string? CachedAuthors = await _cache.GetStringAsync(_Cashe_Key);

        if (string.IsNullOrEmpty(CachedAuthors))
        {
            Task<IQueryable<Author>> Authors = _authorRepository.GetAsync(x => true);
            IEnumerable<AuthorGetDTO> resAuthors = _mapper.Map<IEnumerable<AuthorGetDTO>>(Authors.Result.AsEnumerable());
            await _cache.SetStringAsync(_Cashe_Key, JsonSerializer.Serialize(resAuthors), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            });
            return Ok(resAuthors);
        }
        Console.WriteLine("GetStringAsync return json .......");
        var res = JsonSerializer.Deserialize<IEnumerable<AuthorGetDTO>>(CachedAuthors);
        return Ok(res);



        bool casheHit = _memoryCache.TryGetValue(_Cashe_Key, out IEnumerable<AuthorGetDTO>? CashedAuthors);
        if (!casheHit)
        {
            await Console.Out.WriteAsync("cashHit = false ......!");

            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                .SetSlidingExpiration(TimeSpan.FromSeconds(4));


            var Authors = await _authorRepository.GetAsync(x => true);
            IEnumerable<AuthorGetDTO> resAuthors = _mapper.Map<IEnumerable<AuthorGetDTO>>(Authors);
            _memoryCache.Set(_Cashe_Key, resAuthors, options);
            return Ok(resAuthors);
        }




        /*IEnumerable<AuthorGetDTO>? CashedAuthors = _memoryCache.GetOrCreate(_Cashe_Key,
            opt =>
            {
                opt.SetSlidingExpiration(TimeSpan.FromSeconds(10));
                opt.SetAbsoluteExpiration(TimeSpan.FromSeconds(30));

                Task<IQueryable<Author>> Authors = _authorRepository.GetAsync(x => true);
                IEnumerable<AuthorGetDTO> resAuthors = _mapper.Map<IEnumerable<AuthorGetDTO>>(Authors.Result.AsEnumerable());
                return resAuthors;
            });

        return Ok(CashedAuthors);*/

    }

    [HttpPost("[action]")]
    [CustomAuthorizationFilter("CreateAuthor")]
    //[ValidationActionFilters]
    public async Task<IActionResult> CreateAuthor([FromBody] AuthorCreateDTO createDTO)
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

    [HttpPut("[action]")]
    //[Authorize(Roles = "UpdateAuthor")]
    public async Task<IActionResult> UpdateAuthor([FromBody] AuthorUpdateDTO createDTO)
    {

        Author author = _mapper.Map<Author>(createDTO);
        var validationRes = _validator.Validate(author);
        if (validationRes.IsValid)
        {
            return BadRequest(validationRes);
        }

        author = await _authorRepository.AddAsync(author);
        if (author == null) return NotFound();
        AuthorGetDTO authorGet = _mapper.Map<AuthorGetDTO>(author);
        _memoryCache.Remove(authorGet.Id);                             //Keyni o'chirib yuborish
        _memoryCache.Remove(_Cashe_Key);
        return Ok(authorGet);

    }

    [HttpDelete("[action]")]
    //[Authorize(Roles = "DeletAuthor")]
    public async Task<IActionResult> DeleteAuthor([FromQuery] int id)
    {
        bool isDelete = await _authorRepository.DeleteAsync(id);
        _memoryCache.Remove(id);
        _memoryCache.Remove(_Cashe_Key);
        return isDelete ? Ok("Deleted succesfuly ....") : BadRequest("Delete operation failed");
    }

}
