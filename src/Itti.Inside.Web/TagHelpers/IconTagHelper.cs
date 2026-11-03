using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Itti.Inside.Web.TagHelpers;

/// <summary>Renders <c>&lt;icon name="clock" /&gt;</c> from the sprite at <c>wwwroot/images/icons.svg</c>.</summary>
[HtmlTargetElement("icon", TagStructure = TagStructure.WithoutEndTag)]
public sealed class IconTagHelper : TagHelper
{
    public string Name { get; set; } = string.Empty;

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = default!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        var pathBase = ViewContext.HttpContext.Request.PathBase;
        var use = new TagBuilder("use");
        use.Attributes["href"] = $"{pathBase}/images/icons.svg#{Name}";

        output.TagName = "svg";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("aria-hidden", "true");
        output.Attributes.SetAttribute("focusable", "false");
        output.AddClass("icon", HtmlEncoder.Default);
        output.Content.SetHtmlContent(use);
    }
}
