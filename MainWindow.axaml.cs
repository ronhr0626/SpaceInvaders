using Avalonia.Controls;
using Avalonia.Interactivity;
using SpaceInvaders.Game;

namespace SpaceInvaders;

// Handles the menus. The game itself runs in GameCanvas.
public partial class MainWindow : Window
{
    private int _lastScore;

    public MainWindow()
    {
        InitializeComponent();

        // The canvas tells us when the game is over
        GameView.GameOver += OnGameOver;

        ShowStartScreen();
    }

    private void OnStartClick(object? sender, RoutedEventArgs e) => StartGame();
    private void OnPlayAgainClick(object? sender, RoutedEventArgs e) => StartGame();
    private void OnMenuClick(object? sender, RoutedEventArgs e) => ShowStartScreen();

    private void ShowStartScreen()
    {
        GameOverScreen.IsVisible = false;
        StartScreen.IsVisible = true;
        StartHighScoreList.ItemsSource = HighScoreStore.Format(HighScoreStore.Load());
        StartButton.Focus();
    }

    private void StartGame()
    {
        StartScreen.IsVisible = false;
        GameOverScreen.IsVisible = false;
        GameView.StartNewGame();
    }

    private void OnGameOver(int score)
    {
        _lastScore = score;
        bool madeTheList = HighScoreStore.Qualifies(score);

        FinalScoreText.Text = $"SCORE {score:D6}";
        NewHighScoreText.IsVisible = score > 0 && score > HighScoreStore.Best;
        GameOverHighScoreList.ItemsSource = HighScoreStore.Format(HighScoreStore.Load());

        NameEntryPanel.IsVisible = madeTheList;
        NoEntryText.IsVisible = !madeTheList;
        SavedText.IsVisible = false;

        // Enter = SAVE while typing a name, otherwise Enter = PLAY AGAIN
        SaveButton.IsDefault = madeTheList;
        PlayAgainButton.IsDefault = !madeTheList;

        GameOverScreen.IsVisible = true;

        if (madeTheList)
        {
            NameBox.Text = "";
            NameBox.Focus();
        }
        else
        {
            PlayAgainButton.Focus();
        }
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        var list = HighScoreStore.Add(NameBox.Text ?? "", _lastScore);
        GameOverHighScoreList.ItemsSource = HighScoreStore.Format(list);

        NameEntryPanel.IsVisible = false;
        SavedText.IsVisible = true;

        SaveButton.IsDefault = false;
        PlayAgainButton.IsDefault = true;
        PlayAgainButton.Focus();
    }
}
