// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.Web.Models;
using Microsoft.AspNetCore.Authorization;
using OregonNexus.Broker.Web.Constants.DesignSystems;
using static OregonNexus.Broker.Web.Constants.Sessions.SessionKey;
namespace OregonNexus.Broker.Web.Controllers;

[Authorize]
public class FocusController : AuthenticatedController
{
    public FocusController(
        IHttpContextAccessor httpContextAccessor
    ) : base(httpContextAccessor)
    {
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetFocus(FocusViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.FocusEducationOrganizationId))
        {
            _httpContextAccessor.HttpContext?.Session.SetString(FocusOrganizationCurrentKey, model.FocusEducationOrganizationId);
        }
        else
        {
            TempData[VoiceTone.Critical] = "Unable to set focus.";
        }

        return Redirect(model.ReturnUrl);
    }
}
