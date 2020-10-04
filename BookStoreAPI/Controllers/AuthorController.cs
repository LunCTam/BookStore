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
    /// Endpoint used to interact with Author API Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public AuthorController(IAuthorRepository authorRepository,
            ILoggerService logger,
            IMapper mapper)
        {
            _authorRepository = authorRepository;
            _logger = logger;
            _mapper = mapper; 
        }

        /// <summary>
        /// Get All Authors
        /// </summary>
        /// <returns>Return a list of Authors</returns>
        // GET: api/<AuthorController>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            var location = GetControllerActionName();
            try
            {
                _logger.LogInfo("Get All Authors");
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                _logger.LogInfo("Get All Authors - Success");
                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError("Get All Authors - Failed");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Get Author by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Return One Author</returns>
        // GET api/<AuthorController>/5
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            var location = GetControllerActionName();
            try
            {
                _logger.LogInfo($"Get Author with Id: {id}");
                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    _logger.LogWarn($"Get Author with Id: {id} - Not Found");
                    return NotFound();
                }
                var response = _mapper.Map<AuthorDTO>(author);
                _logger.LogInfo($"Get Author with Id: {id} - Success");
                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError($"Get Author with Id: {id} - Failed");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Create One Author
        /// </summary>
        /// <param name="authorDTO"></param>
        /// <returns>Create One Author</returns>
        // POST api/<AuthorController>
        [HttpPost]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAuthor([FromBody] AuthorCreateDTO authorDTO)
        {
            var location = GetControllerActionName();
            try
            {
                if (authorDTO == null)
                {
                    _logger.LogWarn($"Create Author - Bad Request");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"Create Author - Data InValid");
                    return BadRequest(ModelState);
                }

                _logger.LogInfo("Create Author");
                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Create(author);

                if (!isSuccess)
                {
                    _logger.LogError("Create Author - Failed");
                    return InternalError("Create Author - Failed");
                }
                _logger.LogInfo("Create Author - Success");
                return Created("Create Author - Success", new { author });
            }
            catch (Exception e)
            {
                _logger.LogError($"Create Author - Failed");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Update One Author by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        // PUT api/<AuthorController>/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAuthor(int id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            var location = GetControllerActionName();
            try
            {
                if (authorDTO == null || id < 1 || authorDTO.Id != id)
                {
                    _logger.LogWarn($"Update Author with Id: {id} - Bad Request");
                    return BadRequest(ModelState);
                }
                var isExist = await _authorRepository.isExists(id);
                if (!isExist)
                {
                    _logger.LogWarn($"Update Author with Id: {id} - Not Found");
                    return NotFound();
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"Update Author with Id: {id} - Data InValid");
                    return BadRequest(ModelState);
                }
                _logger.LogInfo($"Update Author with Id: {id}");
                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Update(author);
                if (!isSuccess)
                {
                    _logger.LogError($"Update Author with Id: {id} - Failed");
                    return InternalError($"Update Author with Id: {id} - Failed");
                }
                _logger.LogInfo($"Update Author with Id: {id} - Success");
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError($"Update Author with Id: {id} - Failed");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Delete One Author
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE api/<AuthorController>/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            var location = GetControllerActionName();
            try
            {
                if (id < 1)
                {
                    _logger.LogWarn($"Delete Author with Id: {id} - Bad Request");
                    return BadRequest(ModelState);
                }
                var isExist = await _authorRepository.isExists(id);
                if (!isExist)
                {
                    _logger.LogWarn($"Delete Author with Id: {id} - Not Found");
                    return NotFound();
                }

                _logger.LogInfo($"Delete Author with Id: {id}");
                var author = await _authorRepository.FindById(id);
                var isSuccess = await _authorRepository.Delete(author);
                if (!isSuccess)
                {
                    return InternalError($"Delete Author with Id: {id} - Failed");
                }
                _logger.LogInfo($"Delete Author with Id: {id} - Success");
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError($"Delete Author with Id: {id} - Failed");
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
