using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.Audio;
using SudokuGame.Models;
using SudokuGame.Services;

namespace SudokuGame.ViewModels;

public partial class GameViewModel : ObservableObject
{
  public const string Easy = "Easy";
  public const string Medium = "Medium";
  public const string Hard = "Hard";

  [ObservableProperty]
  private ObservableCollection<SudokuCell> _board;

  [ObservableProperty]
  private SudokuCell? _selectedSudoku = null;

  [ObservableProperty]
  private bool _canUndo = false;

  [ObservableProperty]
  private bool _canErase = false;

  [ObservableProperty]
  private bool _isSuccess = false;

  [ObservableProperty]
  private string _newGameText = "New easy game";

  [ObservableProperty]
  private DiffItem _selectedDiff;

  [ObservableProperty]
  private bool _isHintEnabled = false;

  [ObservableProperty]
  private bool _isSelectGameBottomSheetOpen = false;

  [ObservableProperty]
  private ObservableCollection<KeyNumEnable> _isKeyEnabled = new ObservableCollection<KeyNumEnable>
  {
    new KeyNumEnable { IsEnabled = false },
    new KeyNumEnable { IsEnabled = false },
    new KeyNumEnable { IsEnabled = false },
    new KeyNumEnable { IsEnabled = false },
    new KeyNumEnable { IsEnabled = false },
    new KeyNumEnable { IsEnabled = false },
    new KeyNumEnable { IsEnabled = false },
    new KeyNumEnable { IsEnabled = false },
    new KeyNumEnable { IsEnabled = false },
  };

  [ObservableProperty]
  private bool _isPlayingSound = false;

  private DifficultyLevel _newGameLevel = DifficultyLevel.Easy;

  private SudokuGenerator _sudokuGenerator;

  private List<OperationItem> _operations = new List<OperationItem>();

  public List<DiffItem> Diffs =>
    new List<DiffItem>
    {
      new DiffItem { Name = Easy, Diff = DifficultyLevel.Easy },
      new DiffItem { Name = Medium, Diff = DifficultyLevel.Medium },
      new DiffItem { Name = Hard, Diff = DifficultyLevel.Hard }
    };

  private int[] _numberCounts { get; set; } = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

  private DateTime _begin = DateTime.Now;

  private readonly IAudioManager _audioManager;
  private readonly IThemeService _themeService;

  public GameViewModel(IAudioManager audioManager, IThemeService themeService)
  {
    _audioManager = audioManager;
    _sudokuGenerator = new SudokuGenerator(themeService);
    _board = new ObservableCollection<SudokuCell>();
    SelectedDiff = Diffs.First();
    _themeService = themeService;

    Application.Current!.RequestedThemeChanged += (s, e) =>
    {
      switch (e.RequestedTheme)
      {
        default:
        case AppTheme.Light:
          SetBoardFixedForeColor(Colors.Black);
          break;
        case AppTheme.Dark:
          SetBoardFixedForeColor(Colors.White);
          break;
      }
    };
  }

  private void SetBoardFixedForeColor(Color color)
  {
    foreach (var board in Board)
    {
      if (board.IsFixed)
      {
        board.ForeColor = color;
      }
    }
  }

  partial void OnIsHintEnabledChanged(bool value)
  {
    SelectedSudoku = null;
  }

  partial void OnSelectedDiffChanged(DiffItem value)
  {
    _newGameLevel = value?.Diff ?? DifficultyLevel.Easy;
    NewGameCommand.Execute(null);
  }

  partial void OnSelectedSudokuChanged(SudokuCell? value)
  {
    if (value == null)
    {
      SetAllKeys();
      ResetBoardBackColor();
      if (CanErase)
      {
        CanErase = false;
      }
      return;
    }

    if (value.IsFixed)
    {
      SetAllKeys();
      return;
    }

    SetKeyByNumberCount();

    if (IsHintEnabled)
    {
      CheckKeyEnabled(value.Row, value.Col);
    }

    int rowBox = (value.Row / 3) * 3,
      colBox = (value.Col / 3) * 3;
    for (int i = 0; i < 9; i++)
    {
      for (int j = 0; j < 9; j++)
      {
        if (
          value.Row == i
          || value.Col == j
          || (i >= rowBox && i < rowBox + 3 && j >= colBox && j < colBox + 3)
        )
        {
          Board[i * 9 + j].BackColor = _themeService.CollisionBack;
        }
        else
        {
          Board[i * 9 + j].BackColor = Colors.Transparent;
        }
      }
    }
    value.BackColor = _themeService.SelectedBack;

    if (!value.IsFixed && value.Value != 0)
    {
      CanErase = true;
    }
    else
    {
      CanErase = false;
    }
  }

  public void UpdateCellValue(int index, int newValue)
  {
    if (!Board[index].IsFixed)
    {
      Board[index].Value = newValue;
    }
  }

  [RelayCommand]
  private void NumberInput(string value)
  {
    if (int.TryParse(value, out int number))
    {
      if (SelectedSudoku != null && !SelectedSudoku.IsFixed)
      {
        AddOperation(
          new OperationItem
          {
            Row = SelectedSudoku.Row,
            Col = SelectedSudoku.Col,
            Value = SelectedSudoku.Value
          }
        );
        if (IsValid(SelectedSudoku.Row, SelectedSudoku.Col, number))
        {
          SelectedSudoku.IsValid = true;
          SelectedSudoku.ForeColor = _themeService.CorrectColor;

          if (CheckSuccess())
          {
            if (Application.Current?.Windows.First().Page is Page page)
            {
              page.DisplayAlert(
                "Congratulation",
                $"You win, you're the best! You took a total of {GetOperationCount()} steps, {(DateTime.Now - _begin).TotalSeconds:n0} seconds",
                "OK"
              );
            }
            IsSuccess = true;
            if (IsPlayingSound)
            {
              if (PlaySuccessSoundCommand.CanExecute(null))
              {
                PlaySuccessSoundCommand.Execute(null);
              }
            }
            SetAllKeys();
          }
          else
          {
            if (IsPlayingSound)
            {
              if (PlayCorrectSoundCommand.CanExecute(null))
              {
                PlayCorrectSoundCommand.Execute(null);
              }
            }
          }
        }
        else
        {
          SelectedSudoku.IsValid = false;
          SelectedSudoku.ForeColor = _themeService.WrongColor;
          if (IsPlayingSound)
          {
            if (PlayWrongSoundCommand.CanExecute(null))
            {
              PlayWrongSoundCommand.Execute(null);
            }
          }
        }
        SelectedSudoku.Value = number;

        InitNumberCounts();

        if (!CanErase)
        {
          CanErase = true;
        }
      }
      else
      {
        if (Application.Current?.Windows[0].Page is Page page)
        {
          page.DisplayAlert("Info", "Please select the cell which is needed to fill first", "OK");
        }
      }
    }
  }

  [RelayCommand]
  private void Erase()
  {
    if (SelectedSudoku != null && !SelectedSudoku.IsFixed)
    {
      SelectedSudoku.Value = 0;
      InitNumberCounts();
    }
  }

  [RelayCommand]
  private void Undo()
  {
    RemoveOperation();
  }

  [RelayCommand]
  private async Task NewGame(string diff = "0")
  {
    IsSelectGameBottomSheetOpen = false;
    SelectedSudoku = null;
    ResetBoard();

    try
    {
      var toast = Toast.Make("Generating new game...", ToastDuration.Short, 14);
      if (toast != null)
        await toast.Show();
    }
    catch { }

    if (diff == "1")
    {
      _newGameLevel = DifficultyLevel.Easy;
      NewGameText = "New easy game";
    }
    else if (diff == "2")
    {
      _newGameLevel = DifficultyLevel.Medium;
      NewGameText = "New medium game";
    }
    else if (diff == "3")
    {
      _newGameLevel = DifficultyLevel.Hard;
      NewGameText = "New hard game";
    }

    var boards = await Task.Run(() => _sudokuGenerator.GenerateBoard(_newGameLevel));
    Board = new ObservableCollection<SudokuCell>(boards);

    IsSuccess = false;
    ClearOperations();

    InitNumberCounts();

    _begin = DateTime.Now;
  }

  [RelayCommand]
  private void OpenSelectGameBottomSheet()
  {
    Debug.WriteLine("Set BottomSheet IsOpen true");
    IsSelectGameBottomSheetOpen = true;
  }

  [RelayCommand]
  private void SelectGame(string hurdlestr)
  {
    IsSelectGameBottomSheetOpen = false;
    var hurdle = Convert.ToInt32(hurdlestr);
    try
    {
      var toast = Toast.Make($"Selecting game {hurdlestr}", ToastDuration.Short, 14);
      if (toast != null)
        toast.Show();
    }
    catch { }
    var boards = _sudokuGenerator.SelectGame(hurdle);
    Board = new ObservableCollection<SudokuCell>(boards);

    IsSuccess = false;
    ClearOperations();
    InitNumberCounts();

    _begin = DateTime.Now;
  }

  private void ResetBoard()
  {
    for (int i = 0; i < Board.Count; i++)
    {
      Board[i].Value = 0;
    }
  }

  private int GetOperationCount()
  {
    return _operations.Count;
  }

  private void AddOperation(OperationItem item)
  {
    _operations.Add(item);
    if (!CanUndo)
    {
      CanUndo = true;
    }
  }

  private void RemoveOperation()
  {
    if (_operations.Count > 0)
    {
      var item = _operations.Last();
      SelectedSudoku = Board[item.Row * 9 + item.Col];
      SelectedSudoku.Value = item.Value;
      InitNumberCounts();
      _operations.RemoveAt(_operations.Count - 1);
      if (_operations.Count == 0)
      {
        if (CanUndo)
        {
          CanUndo = false;
        }
      }
    }
  }

  private void ClearOperations()
  {
    _operations.Clear();
    CanUndo = false;
  }

  private bool IsValid(int row, int col, int num)
  {
    if (Board == null)
    {
      return false;
    }
    for (int i = 0; i < 9; i++)
    {
      if (
        (Board[row * 9 + i].Value == num && col != i)
        || (Board[i * 9 + col].Value == num && row != i)
      )
      {
        return false;
      }
    }
    int boxRow = (row / 3) * 3,
      boxCol = (col / 3) * 3;
    for (int i = 0; i < 3; i++)
    {
      for (int j = 0; j < 3; j++)
      {
        if (boxRow + i == row && boxCol + j == col)
        {
          continue;
        }
        if (Board[(boxRow + i) * 9 + boxCol + j].Value == num)
        {
          return false;
        }
      }
    }
    return true;
  }

  private bool CheckSuccess()
  {
    for (int i = 0; i < 9; i++)
    {
      for (int j = 0; j < 9; j++)
      {
        if (!Board[i * 9 + j].IsFixed && !Board[i * 9 + j].IsValid)
        {
          return false;
        }
      }
    }
    return true;
  }

  [RelayCommand]
  private async Task PlayCorrectSoundAsync()
  {
    //try
    {
      var stream = await FileSystem.OpenAppPackageFileAsync("correct.mp3");
      var player = _audioManager.CreatePlayer(stream);
      if (player != null)
      {
        player.Play();
      }
    }
    //catch { }
  }

  [RelayCommand]
  private async Task PlayWrongSoundAsync()
  {
    try
    {
      var player = _audioManager.CreatePlayer(
        await FileSystem.OpenAppPackageFileAsync("wrong.mp3")
      );
      if (player != null)
      {
        player.Play();
      }
    }
    catch { }
  }

  [RelayCommand]
  private async Task PlaySuccessSoundAsync()
  {
    try
    {
      var player = _audioManager.CreatePlayer(
        await FileSystem.OpenAppPackageFileAsync("success.mp3")
      );
      if (player != null)
      {
        player.Play();
      }
    }
    catch { }
  }

  private void CheckKeyEnabled(int row, int col)
  {
    if (!Board[row * 9 + col].IsFixed)
    {
      SetAllKeys(true);
      for (int i = 0; i < 9; i++)
      {
        var value = Board[row * 9 + i].Value;
        if (value > 0 && value <= 9)
        {
          IsKeyEnabled[value - 1].IsEnabled = false;
        }
        value = Board[i * 9 + col].Value;
        if (value > 0 && value <= 9)
        {
          IsKeyEnabled[value - 1].IsEnabled = false;
        }
      }
      int boxRow = (row / 3) * 3,
        boxCol = (col / 3) * 3;
      for (int i = 0; i < 3; i++)
      {
        for (int j = 0; j < 3; j++)
        {
          var value = Board[(boxRow + i) * 9 + boxCol + j].Value;
          if (value > 0 && value <= 9)
          {
            IsKeyEnabled[value - 1].IsEnabled = false;
          }
        }
      }
    }
  }

  private void SetAllKeys(bool b = false)
  {
    for (int i = 0; i < IsKeyEnabled.Count; i++)
      IsKeyEnabled[i].IsEnabled = b;
  }

  private void ResetNumberCounts()
  {
    for (int i = 0; i < _numberCounts.Length; i++)
    {
      _numberCounts[i] = 0;
    }
  }

  private void IncrementNumberCount(int num, int count = 1)
  {
    if (num > 0 && num <= _numberCounts.Length)
    {
      _numberCounts[num - 1] += count;
    }
  }

  private void SetKeyByNumberCount()
  {
    for (int i = 0; i < _numberCounts.Length; i++)
    {
      if (_numberCounts[i] >= 9)
      {
        IsKeyEnabled[i].IsEnabled = false;
      }
      else
      {
        IsKeyEnabled[i].IsEnabled = true;
      }
    }
  }

  private void InitNumberCounts()
  {
    ResetNumberCounts();
    for (int i = 0; i < Board.Count; i++)
    {
      IncrementNumberCount(Board[i].Value);
    }
  }

  private void ResetBoardBackColor()
  {
    for (int i = 0; i < Board.Count; i++)
    {
      Board[i].BackColor = Colors.Transparent;
    }
  }
}
