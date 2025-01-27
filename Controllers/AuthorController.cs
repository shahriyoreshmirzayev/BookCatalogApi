using AutoMapper;
using BookApplication.DTOs.AuthorDTO;
using BookApplication.Repositories;
using BookCatalogApiDomain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalogApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IValidator<Author> _validator;
        private readonly IMapper _mapper;

        public AuthorController(IBookRepository bookRepository, IAuthorRepository authorRepository, IValidator<Author> validator, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _validator = validator;
            _mapper = mapper;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> GetAuthorById([FromQuery] int id)
        {
            Author author = await _authorRepository.GetByIdAsync(id);
            if (author == null)
            {
                return NotFound($"Author Id {id} not found. .....!");
            }
            return Ok(author);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllAuthors()
        {
            var Authors = await _authorRepository.GetAsync(x => true);
            var resAuthors = _mapper.Map<IEnumerable<Author>>(Authors);
            return Ok(resAuthors);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> CreateAuthor([FromQuery] AuthorCreateDTO createDTO)
        {
            if (ModelState.IsValid)
            {
                Author author = _mapper.Map<Author>(createDTO);
                var validResult = _validator.Validate(author);
                if (validResult.IsValid)
                {
                    for (int i = 0; i < author.Books.Count; i++)
                    {
                        Book book = author.Books.ToArray()[i];
                        book = await _bookRepository.GetByIdAsync(book.Id);
                        if (book == null)
                        {
                            return NotFound("Book Id not found. . ...");
                        }
                    }
                    author = await _authorRepository.AddAsync(author);
                    return author == null ? BadRequest() : Ok(author);
                }
                return BadRequest(validResult);
            }
            return BadRequest(ModelState);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> DeleteAuthor([FromQuery] int id)
        {
            bool isDelete = await _authorRepository.DeleteAsync(id);
            return isDelete ? Ok("Deleted succesfuly ....") : BadRequest("Delete operation failed");
        }

    }
}
