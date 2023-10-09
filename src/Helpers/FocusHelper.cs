using Microsoft.AspNetCore.Mvc.Rendering;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Domain.Specifications;
using OregonNexus.Broker.SharedKernel;
using static OregonNexus.Broker.Web.Constants.Sessions.SessionKey;

namespace OregonNexus.Broker.Web.Helpers;

public class FocusHelper
{
    private readonly IRepository<EducationOrganization> _edOrgRepo;
    private readonly IRepository<UserRole> _userRoleRepo;
    private readonly IRepository<User> _userRepo;
    private readonly ISession _session;

    public FocusHelper(IRepository<EducationOrganization> edOrgRepo, IRepository<UserRole> userRoleRepo, IRepository<User> userRepo, IHttpContextAccessor httpContextAccessor)
    {
        _edOrgRepo = edOrgRepo;
        _userRepo = userRepo;
        _userRoleRepo = userRoleRepo;
        _session = httpContextAccessor.HttpContext?.Session;
    }

    public async Task<IEnumerable<SelectListItem>> GetFocusableEducationOrganizationsSelectList()
    {
        var selectListItems = new List<SelectListItem>();

        // Get currently logged in user
        var currentUser = _session.GetObjectFromJson<User>("User.Current");
        var allEdOrgs = currentUser?.AllEducationOrganizations;

        if (currentUser?.Id == null) return selectListItems;

        var organizations = await _edOrgRepo.ListAsync();
        organizations = organizations.OrderBy(x => x.ParentOrganization?.Name).ThenBy(x => x.Name).ToList();

        if (allEdOrgs == PermissionType.Read || allEdOrgs == PermissionType.Write)
        { 
            selectListItems.Add(new SelectListItem() {
                    Text = "All",
                    Value = "ALL",
                    Selected = (_session.GetString(FocusOrganizationCurrentKey) == "ALL")
                });
            
        

            foreach(var organization in organizations)
            {
                selectListItems.Add(new SelectListItem() {
                    Text = (organization.EducationOrganizationType == EducationOrganizationType.District) 
                        ? organization.Name 
                        : $"{organization.ParentOrganization?.Name} / {organization.Name}",
                    Value = organization.Id.ToString(),
                    Selected = (_session.GetString(FocusOrganizationCurrentKey) == organization.Id.ToString())
                });
            }
        }
        else
        {
            var userRoleSpec = new UserRolesByUserSpec(currentUser.Id);
            var userRoles = await _userRoleRepo.ListAsync(userRoleSpec);

            foreach(var userRole in userRoles.Where(role => role.EducationOrganization?.ParentOrganizationId is not null))
            {
                selectListItems.Add(new SelectListItem() {
                    Text = $"{userRole.EducationOrganization?.ParentOrganization?.Name} / {userRole.EducationOrganization?.Name}",
                    Value = userRole.EducationOrganizationId.ToString(),
                    Selected = (_session.GetString(FocusOrganizationCurrentKey) == userRole.EducationOrganizationId.ToString())
                });
            }
        }

        selectListItems = selectListItems.OrderBy(x => x.Text).ToList();

        if (_session.GetString(FocusOrganizationCurrentKey) == null)
        {
            if (selectListItems.FirstOrDefault() is not null)
            {
                _session.SetString(FocusOrganizationCurrentKey, selectListItems.FirstOrDefault()!.Value);
            }
        }

        var selectedValue = _session.GetString(FocusOrganizationCurrentKey);

        var selectedSelectList = new SelectList(
                selectListItems,
                "Value",
                "Text",
                selectedValue
            );

        return selectedSelectList;
    }

    public Guid? CurrentEdOrgFocus()
    {
        var currentEdOrgFocus = _session.GetString(FocusOrganizationCurrentKey);
        if (currentEdOrgFocus != "ALL")
        {
            Guid currentEdOrgFocusGuid;
            
            if (Guid.TryParse(currentEdOrgFocus, out currentEdOrgFocusGuid))
            {
                return currentEdOrgFocusGuid;
            }
        }
        return null;
    }

    public async Task<Guid?> CurrentDistrictEdOrgFocus()
    {
        var currentEdOrgFocus = CurrentEdOrgFocus();

        // Check if district
        if (currentEdOrgFocus.HasValue)
        {
            var edOrg = await _edOrgRepo.GetByIdAsync(currentEdOrgFocus.Value);
            if (edOrg is not null && edOrg.EducationOrganizationType == EducationOrganizationType.District)
            {
                return currentEdOrgFocus;
            }
        }
        return null;
    }

    public bool IsEdOrgAllFocus()
    {
        var currentEdOrgFocus = _session.GetString(FocusOrganizationCurrentKey);
        if (currentEdOrgFocus == "ALL")
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}