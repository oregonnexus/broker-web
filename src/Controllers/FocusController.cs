// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.Web.Models;
using Microsoft.AspNetCore.Authorization;
using OregonNexus.Broker.Web.Constants.DesignSystems;
using static OregonNexus.Broker.Web.Constants.Sessions.SessionKey;
namespace OregonNexus.Broker.Web.Controllers;

[Authorize]
public class FocusController : Controller
{
    private readonly ISession _session;

    public FocusController(
        ISession session
    )
    {
        _session = session;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetFocus(FocusViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.FocusEducationOrganizationId))
        {
            _session.SetString(FocusOrganizationCurrentKey, model.FocusEducationOrganizationId);
        }
        else
        {
            TempData[VoiceTone.Critical] = "Unable to set focus.";
        }

        return Redirect(model.ReturnUrl);
    }
}
