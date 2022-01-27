using Avalonia.ReactiveUI;
using Avalonia.Web.Blazor;

namespace TypefaceUtilAvalonia.Web;

public partial class App
{
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        WebAppBuilder.Configure<TypefaceUtil.Avalonia.App>()
            .UseReactiveUI()
            .SetupWithSingleViewLifetime();
    }
}
