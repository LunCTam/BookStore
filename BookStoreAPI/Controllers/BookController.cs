using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStoreAPI.Contracts;
using BookStoreAPI.Data;
using BookStoreAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookStoreAPI.Controllers
{
    /// <summary>
    /// Endpoint used to interact with Book API Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class BookController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public BookController(IBookRepository bookRepository,
            ILoggerService logger,
            IMapper mapper)
        {
            _bookRepository = bookRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get All Books
        /// </summary>
        /// <returns>Return a list of Books</returns>
        // GET: api/<BookController>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks()
        {
            var location = GetControllerActionName();
            try
            {
                _logger.LogInfo($"{location}: Get All Books");
                var books = await _bookRepository.FindAll();
                var response = _mapper.Map<IList<BookDTO>>(books);
                _logger.LogInfo($"{location}: Get All Books - Success");
                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError($"{location}: Get All Books - Failed");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Get Book by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Return One Book</returns>
        // GET api/<BookController>/5
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBook(int id)
        {
            var location = GetControllerActionName();
            try
            {
                _logger.LogInfo($"{location}: Get Book with Id: {id}");
                var book = await _bookRepository.FindById(id);
                if (book == null)
                {
                    _logger.LogWarn($"{location}: Get Book with Id: {id} - Not Found");
                    return NotFound();
                }
                var response = _mapper.Map<BookDTO>(book);
                _logger.LogInfo($"{location}: Get Book with Id: {id} - Success");
                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError($"{location}: Get Book with Id: {id} - Failed");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Create One Book
        /// </summary>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        // POST api/<BookController>
        [HttpPost]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateBook([FromBody] BookCreateDTO bookDTO)
        {
            var location = GetControllerActionName();
            try
            {
                if (bookDTO == null)
                {
                    _logger.LogWarn($"{location}: Create Book - Bad Request");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Create Book - Data InValid");
                    return BadRequest(ModelState);
                }
                _logger.LogInfo($"{location}: Create Book");
                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Create(book);
                if (!isSuccess)
                {
                    return InternalError($"{location}: Create Book - Failed");
                }
                _logger.LogInfo($"{location}: Create Book - Success");
                return Created("Create Book - Success", new { book });
            }
            catch (Exception e)
            {
                _logger.LogError($"{location}: Create Book - Failed");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Update Book by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        // Put api/<BookController>/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] BookUpdateDTO bookDTO)
        {
            var location = GetControllerActionName();
            try
            {
                if (bookDTO == null || id < 1 || bookDTO.Id != id)
                {
                    _logger.LogWarn($"{location}: Update Book with Id: {id} - Bad Request");
                    return BadRequest(ModelState);
                }
                var isExist = await _bookRepository.isExists(id);
                if (!isExist)
                {
                    _logger.LogWarn($"{location}: Update Book with Id: {id} - Not Found");
                    return NotFound();
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Update Book with Id: {id} - Data InValid");
                    return BadRequest(ModelState);
                }
                _logger.LogInfo($"{location}: Update Book with Id: {id}");
                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Update(book);
                if (!isSuccess)
                {
                    return InternalError($"{location}: Update Book with Id: {id} - Failed");
                }
                _logger.LogInfo($"{location}: Update Book with Id: {id} - Success");
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError($"{location}: Update Book with Id: {id} - Failed");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Delete Book by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE api/<BookController>/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var location = GetControllerActionName();
            try
            {
                if (id < 1)
                {
                    _logger.LogWarn($"{location}: Delete Book by Id: {id} - Bad Request");
                    return BadRequest(ModelState);
                }
                var isExist = await _bookRepository.isExists(id);
                if (!isExist)
                {
                    _logger.LogWarn($"{location}: Delete Book with Id: {id} - Not Found");
                    return NotFound();
                }

                _logger.LogInfo($"{location}: Delete Book with Id: {id}");
                var book = await _bookRepository.FindById(id);
                var isSuccess = await _bookRepository.Delete(book);
                if (!isSuccess)
                {
                    return InternalError($"{location}: Delete Book with Id: {id} - Failed");
                }
                _logger.LogInfo($"{location}: Delete Book by Id: {id} - Success");
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError($"{location}: Delete Book with Id: {id} - Failed");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        // custom error message
        private string GetControllerActionName()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;
            return $"{controller} - {action}";
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong. Contact your Administrator");
        }
    }
}
