using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OregonNexus.Broker.Web.TagHelpers;

[HtmlTargetElement("Search-Select", TagStructure = TagStructure.WithoutEndTag)]
[HtmlTargetElement(Attributes = "asp-name")]
[HtmlTargetElement(Attributes = nameof(ForAttributeName))]
public class SearchSelectTagHelper : TagHelper
{
    private readonly IHtmlHelper _html;

    private const string ForAttributeName = "asp-for";

    public SearchSelectTagHelper(IHtmlHelper htmlHelper)
    {
        _html = htmlHelper;
    }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext? ViewContext { get; set; }

    /// <summary>
    /// Gets or sets the policy name that determines access to the HTML block.
    /// </summary>
    [HtmlAttributeName("asp-name")]
    public string? Name { get; set; }

    public string Placeholder { get; set; }

    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression? For { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        string name = For!.Name;
        
        //Contextualize the html helper
        (_html as IViewContextAware)!.Contextualize(ViewContext);

        output.TagName = null; // required to get HTML to output
        output.TagMode = TagMode.StartTagAndEndTag;

        var content = await _html.PartialAsync("~/Views/Shared/TagHelpers/SearchSelect.cshtml", new { Placeholder = Placeholder });
        output.Content.SetHtmlContent(content);
    }

}