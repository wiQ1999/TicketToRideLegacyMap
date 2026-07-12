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

                // Legendy Zachodu — design system typefaces (patrz Resources/Styles/Typography.xaml).
                fonts.AddFont("Cinzel-Regular.ttf", "CinzelRegular");
                fonts.AddFont("Cinzel-Bold.ttf", "CinzelBold");
                fonts.AddFont("Cinzel-Black.ttf", "CinzelBlack");
                fonts.AddFont("CinzelDecorative-Regular.ttf", "CinzelDecorativeRegular");
                fonts.AddFont("CinzelDecorative-Bold.ttf", "CinzelDecorativeBold");
                fonts.AddFont("CinzelDecorative-Black.ttf", "CinzelDecorativeBlack");
                fonts.AddFont("Bitter-Regular.ttf", "BitterRegular");
                fonts.AddFont("Bitter-SemiBold.ttf", "BitterSemibold");
                fonts.AddFont("Bitter-Bold.ttf", "BitterBold");
            });

#if DEBUG
        builder.Logging.AddDebug();
        builder.Services.AddLogging(configure => configure.AddDebug());
#endif

        builder.Services.AddSingleton<ModalErrorHandler>();
        builder.Services.AddSingleton<IErrorHandler>(sp => sp.GetRequiredService<ModalErrorHandler>());
        builder.Services.AddSingleton<IMapDataProvider, MapDataProvider>();
        builder.Services.AddSingleton<IMapInteractionState, MapInteractionState>();
        builder.Services.AddSingleton<ICityNameCatalog, CityNameCatalog>();
        builder.Services.AddSingleton<IDeveloperMapEditor, DeveloperMapEditor>();
        builder.Services.AddSingleton<IMapDataExporter, MapDataExporter>();
        builder.Services.AddTransient<MainMenuPageModel>();
        builder.Services.AddTransient<MainMenuPage>();
        builder.Services.AddTransient<MapPage>();
        builder.Services.AddTransient<DeveloperPageModel>();
        builder.Services.AddTransient<DeveloperPage>();

        return builder.Build();
    }
}
