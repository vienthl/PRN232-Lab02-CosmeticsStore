using Microsoft.AspNetCore.Mvc;
using BusinessObjects;
using Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace WebAPI.Controllers;

[ApiController]
public class CosmeticInformationsController : ODataController
{
    private readonly ICosmeticInformationService _cosmeticInformationService;

    public CosmeticInformationsController(ICosmeticInformationService cosmeticInformationService)
    {
        _cosmeticInformationService = cosmeticInformationService;
    }

    [EnableQuery]
    [Authorize(Policy = "AdminOrStaffOrMember")]
    [HttpGet("/api/CosmeticInformations")]
    public async Task<ActionResult<IEnumerable<CosmeticInformation>>> GetCosmeticInformations()
    {
        try
        {
            var result = await _cosmeticInformationService.GetAllCosmetics();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(400, $"{ex.Message}");
        }
    }

    [Authorize(Policy = "AdminOrStaffOrMember")]
    [HttpGet("/api/CosmeticCategories")]
    public async Task<ActionResult<List<CosmeticCategory>>> GetCategories()
    {
        try
        {
            var result = await _cosmeticInformationService.GetAllCategories();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(400, $"{ex.Message}");
        }
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("/api/CosmeticInformations")]
    public async Task<ActionResult<CosmeticInformation>> AddCosmeticInformation([FromBody] CosmeticInformation cosmeticInformation)
    {
        try
        {
            var result = await _cosmeticInformationService.Add(cosmeticInformation);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(400, $"{ex.Message}");
        }
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("/api/CosmeticInformations/{id}")]
    public async Task<ActionResult<CosmeticInformation>> UpdateCosmeticInformation(string id, [FromBody] CosmeticInformation cosmeticInformation)
    {
        try
        {
            cosmeticInformation.CosmeticId = id;
            var result = await _cosmeticInformationService.Update(cosmeticInformation);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(400, $"{ex.Message}");
        }
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("/api/CosmeticInformations/{id}")]
    public async Task<ActionResult<CosmeticInformation>> DeleteCosmeticInformation(string id)
    {
        try
        {
            var result = await _cosmeticInformationService.Delete(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(400, $"{ex.Message}");
        }
    }

    [Authorize(Policy = "AdminOrStaffOrMember")]
    [HttpGet("/api/CosmeticInformations/{id}")]
    public async Task<ActionResult<CosmeticInformation>> GetCosmeticInformationById(string id)
    {
        try
        {
            var result = await _cosmeticInformationService.GetOne(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(400, $"{ex.Message}");
        }
    }
}
