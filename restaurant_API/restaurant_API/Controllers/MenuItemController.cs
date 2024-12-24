using Microsoft.AspNetCore.Mvc;
using restaurant_API.Data;
using restaurant_API.Models;
using restaurant_API.Models.DTO;
using restaurant_API.Services;
using restaurant_API.Utility;
using System.Net;

namespace restaurant_API.Controllers
{
    [Route("api/MenuItem")]
    [ApiController]
    public class MenuItemController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;

        private ApiResponse _response;

        public MenuItemController(ApplicationDbContext db, IFileService fileService)
        {
            _db = db;
            _fileService = fileService;
            _response = new ApiResponse();
        }

        [HttpGet]
        public async Task<IActionResult> GetMenuItems()
        {
            _response.Result = _db.MenuItems;
            _response.StatusCode = HttpStatusCode.OK;

            return Ok(_response);
        }

        [HttpGet("{id:int}", Name = "GetMenuItem")]
        public async Task<IActionResult> GetMenuItem(int id)
        {
            if (id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.isSuccess = false;
                return BadRequest(_response);
            }

            MenuItem menuItem = _db.MenuItems.FirstOrDefault(i => i.Id == id);

            if (menuItem == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.isSuccess = false;
                return NotFound(_response);
            }

            _response.Result = menuItem;
            _response.StatusCode = HttpStatusCode.OK;

            return Ok(_response);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateMenuItem([FromForm] MenuItemCreateDTO menuItemCreateDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menuItemCreateDTO.File == null || menuItemCreateDTO.File.Length == 0)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.isSuccess = false;
                        return BadRequest();
                    }

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuItemCreateDTO.File.FileName)}";

                    MenuItem menuItemToCreate = new()
                    {
                        Name = menuItemCreateDTO.Name,
                        Price = menuItemCreateDTO.Price,
                        Category = menuItemCreateDTO.Category,
                        SpecialTag = menuItemCreateDTO.SpecialTag,
                        Description = menuItemCreateDTO.Description,
                        Image = await _fileService.UploadFile(fileName, SD.SDStorageFolder, menuItemCreateDTO.File)
                    };

                    _db.MenuItems.Add(menuItemToCreate);
                    _db.SaveChanges();
                    _response.Result = menuItemToCreate;
                    _response.StatusCode = HttpStatusCode.Created;
                    return CreatedAtRoute("GetMenuItem", new { id = menuItemToCreate.Id }, _response);
                }
                else
                {
                    _response.isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }

            return _response;
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse>> UpdateMenuItem(int id, [FromForm] MenuItemUpdateDTO menuItemUpdateDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menuItemUpdateDTO == null || id != menuItemUpdateDTO.Id)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.isSuccess = false;
                        return BadRequest();
                    }

                    MenuItem menuItemFromDB = await _db.MenuItems.FindAsync(id);

                    if (menuItemFromDB == null)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.isSuccess = false;
                        return BadRequest();
                    }

                    menuItemFromDB.Name = menuItemUpdateDTO.Name;
                    menuItemFromDB.Price = menuItemUpdateDTO.Price;
                    menuItemFromDB.Category = menuItemUpdateDTO.Category;
                    menuItemFromDB.SpecialTag = menuItemUpdateDTO.SpecialTag;
                    menuItemFromDB.Description = menuItemUpdateDTO.Description;

                    if (menuItemUpdateDTO.File != null && menuItemUpdateDTO.File.Length > 0)
                    {
                        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuItemUpdateDTO.File.FileName)}";
                        await _fileService.DeleteFile(menuItemFromDB.Image.Split('\\').Last(), SD.SDStorageFolder);
                        menuItemFromDB.Image = await _fileService.UploadFile(fileName, SD.SDStorageFolder, menuItemUpdateDTO.File);
                    }

                    _db.MenuItems.Update(menuItemFromDB);
                    _db.SaveChanges();
                    _response.StatusCode = HttpStatusCode.NoContent;
                    return Ok(_response);
                }
                else
                {
                    _response.isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }

            return _response;
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse>> DeleteMenuItem(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.isSuccess = false;
                    return BadRequest();
                }

                MenuItem menuItemFromDB = await _db.MenuItems.FindAsync(id);

                if (menuItemFromDB == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.isSuccess = false;
                    return BadRequest();
                }

                await _fileService.DeleteFile(menuItemFromDB.Image.Split('\\').Last(), SD.SDStorageFolder);

                int milliseconds = 2000;
                Thread.Sleep(milliseconds);

                _db.MenuItems.Remove(menuItemFromDB);
                _db.SaveChanges();
                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.isSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
            }

            return _response;
        }
    }
}
