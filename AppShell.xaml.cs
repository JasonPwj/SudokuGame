using SudokuGame.Services;

namespace SudokuGame;

public partial class AppShell : Shell
{
  readonly IThemeService _themeService;

  public AppShell(IThemeService themeService)
  {
    _themeService = themeService;
    AppTheme currentTheme = Application.Current!.RequestedTheme;
    _themeService.SetTheme(currentTheme);
    Application.Current.RequestedThemeChanged += (s, e) =>
    {
      _themeService.SetTheme(Application.Current.RequestedTheme);
    };
    InitializeComponent();
  }
}
