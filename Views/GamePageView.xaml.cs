using Plugin.Maui.Audio;
using SudokuGame.ViewModels;

namespace SudokuGame.Views;

public partial class GamePageView : ContentPage
{
  public GamePageView(IAudioManager audioManager)
  {
    InitializeComponent();
    BindingContext = new GameViewModel(audioManager);
  }
}
