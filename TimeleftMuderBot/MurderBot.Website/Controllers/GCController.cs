using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using MurderBot.Data.Context;

namespace MurderBot.Website.Controllers;

public class GCController : Controller
{
    private readonly ILogger<GCController> _logger;
    private readonly MurderContext _dbContext;

    public GCController(ILogger<GCController> logger, MurderContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IActionResult> ReAdd(Guid id)
    {
        var token = await _dbContext.AutoReAddToken.FirstOrDefaultAsync(
            g => g.TokenGuid == id);

        if (token == null)
            return NotFound();
        
        return Content($"<strong>Found token for {token.ParticipantId} for Group {token.GroupCheckIn.Group.Name}</strong>",
            "text/html");
    }
    
    

    public async Task<IActionResult> Status(Guid id)
    {
        /*
        if (id == null)
            return NotFound();

        Guid groupGuid;

        id = Uri.UnescapeDataString(id);
        try
        {
            var guidBytes = Convert.FromBase64String(id);
            groupGuid = new Guid(guidBytes);
        }
        catch (Exception e)
        {
            return NotFound();
        }
        */
        
        var checkin = await _dbContext.GroupCheckIn.FirstOrDefaultAsync(
            g => g.UrlGuid == id);
        
        if (checkin == null)
            return NotFound();
        
        return Content($"<strong>Found group check in {checkin.GroupCheckinId} for Group {checkin.Group.Name}</strong>",
            "text/html");
        
        
       // return View();
    }
}