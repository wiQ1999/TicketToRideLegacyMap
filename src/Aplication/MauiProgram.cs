using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace Aplication;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
        builder.Services.AddLogging(configure => configure.AddDebug());
#endif

        builder.Services.AddSingleton<ModalErrorHandler>();
        builder.Services.AddSingleton<IErrorHandler>(sp => sp.GetRequiredService<ModalErrorHandler>());
        builder.Services.AddSingleton<IMapDataProvider, MapDataProvider>();
        builder.Services.AddSingleton<IMapInteractionState, MapInteractionState>();
        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
}
