using SudokuGame.Services;

namespace SudokuGame;

public partial class App : Application
{
  private readonly IThemeService _themeService;

  public App(IThemeService themeService)
  {
    _themeService = themeService;
    InitializeComponent();
  }

  protected override Window CreateWindow(IActivationState? activationState)
  {
    return new Window(new AppShell(_themeService));
  }
}
