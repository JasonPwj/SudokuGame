using Plugin.Maui.Audio;
using SudokuGame.Services;
using SudokuGame.ViewModels;

namespace SudokuGame.Views;

public partial class GamePageView : ContentPage
{
  public GamePageView(IAudioManager audioManager, IThemeService themeService)
  {
    InitializeComponent();
    BindingContext = new GameViewModel(audioManager, themeService);
  }
}
