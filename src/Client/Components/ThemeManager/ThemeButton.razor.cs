using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace hotel_ize_frontend.Client.Components.ThemeManager;
public partial class ThemeButton
{
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }
}