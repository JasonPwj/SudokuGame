﻿using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using SudokuGame.Services;

namespace SudokuGame;

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
      });

    //builder.Services.AddSingleton<GamePageView>();
    builder.Services.AddSingleton<IThemeService, ThemeService>();
    builder.AddAudio();

#if DEBUG
    builder.Logging.AddDebug();
#endif

    return builder.Build();
  }
}
