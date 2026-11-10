using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Itti.Inside.Web.TagHelpers;

/// <summary>
/// Opens a link in a new tab: <c>&lt;a href="..." new-tab="true"&gt;</c>. The portal opens every link
/// that leaves the page this way, so the landing page stays open behind it. <c>new-tab="false"</c>
/// keeps a link in the same tab, as for Home.
/// </summary>
[HtmlTargetElement("a", Attributes = AttributeName)]
public sealed class NewTabTagHelper : TagHelper
{
    /// <summary>The id of the hidden "Opens in a new tab" note in the layout, read out by screen readers.</summary>
    public const string NoteId = "new-tab-note";

    private const string AttributeName = "new-tab";

    [HtmlAttributeName(AttributeName)]
    public bool NewTab { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        if (!NewTab)
        {
            return;
        }

        output.Attributes.SetAttribute("target", "_blank");

        // Stops the new page from reaching back into this one through window.opener.
        output.Attributes.SetAttribute("rel", "noopener");
        output.Attributes.SetAttribute("aria-describedby", NoteId);
    }
}
