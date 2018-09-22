﻿using BallsAndLines.Helpers;
using BallsAndLines.Model; 
using System.Windows; 
using System.Windows.Input;

namespace BallsAndLines.ViewModels
{
    internal class MainViewModel : BaseViewModel
    {
        private BoardViewModel _boardViewModel; 
        private ScoreViewModel _scoreViewModel;

        private ICommand _startNewGame;
        private ICommand _saveGame;
        private ICommand _continueGame; 
        private ICommand _openSettings;
        private ICommand _openAboutGame;
         
        private Visibility _settingsWindowVisibility = Visibility.Collapsed;
        private Visibility _aboutGameWindowVisibility = Visibility.Collapsed;

        public MainViewModel()
        {
            InitGameComponent();
        }

        private void InitGameComponent()
        {
            BoardViewModel = new BoardViewModel(GameController.CreateNewGameBoard(Settings.BoardSize));
             
            Score uploadScore = GetSavedScore();

            if (uploadScore != null)
            {
                uploadScore.CurrentScore = 0;

                ScoreViewModel = new ScoreViewModel(uploadScore);
            }
            else
                ScoreViewModel = new ScoreViewModel();
              
            InitGameEvent(); 
            InitCellEvent();
        }

        private Score GetSavedScore() => Score.Open(GameController.GameStorePath + "score.xml");
         
        public BoardViewModel BoardViewModel
        {
            get => _boardViewModel;
            set
            {
                _boardViewModel = value;
                RaisePropertyChanged();
            }
        }

        public ScoreViewModel ScoreViewModel
        {
            get => _scoreViewModel;
            set
            {
                _scoreViewModel = value;
                RaisePropertyChanged();
            }
        }

        public Visibility SettingsWindowVisibility
        {
            get => _settingsWindowVisibility;
            set
            {
                _settingsWindowVisibility = value; 
                RaisePropertyChanged();
            }
        }

        public Visibility AboutGameWindowVisibility
        {
            get => _aboutGameWindowVisibility;
            set
            {
                _aboutGameWindowVisibility = value;
                RaisePropertyChanged();
            }
        }

        public string RandomDroppingBalls
        {
            get => Settings.DropBallsPerStep.ToString();
            set
            {
                if (MessageBox.Show("Change number of random dropping balls and reset game?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (GameController.ChangeDroppingBallsPerStep(int.Parse(value)))
                    { 
                        MessageBox.Show("Count dropping balls changed.");

                        InitGameComponent();

                        RaisePropertyChanged();
                    }
                    else
                    {
                        MessageBox.Show("Count of balls too high or low.", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
               
            }
        }

        public string NumBallsInLine
        {
            get => Settings.NumBallsInLine.ToString();
            set
            {
                if (MessageBox.Show("Change number balls for line collected and reset game?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (GameController.ChangeCountBallsForLineCollected(int.Parse(value)))
                    {
                        MessageBox.Show("Count balls for line changed.");

                        InitGameComponent();

                        RaisePropertyChanged();
                    }
                    else
                    {
                        MessageBox.Show("Count of balls too high or low.", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                } 
            }
        }

        public string BoardSize
        {
            get => Settings.BoardSize.ToString();
            set
            {
                if (MessageBox.Show("Change size board and reset game?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (GameController.ChangeBoardSize(int.Parse(value)))
                    {
                        MessageBox.Show("Board size changed.");

                        InitGameComponent();

                        RaisePropertyChanged();
                    } 
                } 
            }
        }

        public ICommand StartNewGame => _startNewGame ?? (_startNewGame = new RelayCommand(ResetBoardAndDropBalls));

        public ICommand SaveGame => _saveGame ?? (_saveGame = new RelayCommand(SerializeCurrentGame));

        public ICommand ContinueGame => _continueGame ?? (_continueGame = new RelayCommand(DeserializeGame));

        public ICommand OpenSetting => _openSettings ?? (_openSettings = new RelayCommand((_) => { 
                SettingsWindowVisibility = GetChangedVisiblyMode(_settingsWindowVisibility); 
        }));

        public ICommand OpenAboutGame => _openAboutGame ?? (_openAboutGame = new RelayCommand((_) => { 
                AboutGameWindowVisibility = GetChangedVisiblyMode(_aboutGameWindowVisibility); 
        }));

        private void InitGameEvent()
        {
            GameController.GameOver += OnGameOver; 
            GameController.ScoreUpdated += ScoreViewModel.OnUpdateGameScore;
        }

        private void InitCellEvent()
        { 
            GameController.CellSelected += BoardViewModel.OnCellSelected;
            GameController.CellUnselected += BoardViewModel.OnCellUnselected;
        }

        private void ResetBoardAndDropBalls(object o)
        {
            InitGameComponent();

            BoardViewModel.DropRandomBalls();
        }

        private void SerializeCurrentGame(object o)
        {
            BoardViewModel.SerializeGameBoard("game.xml");
            ScoreViewModel.SerializeScore("score.xml");

            SaveGameSettings();

            MessageBox.Show("Game successfully saved.", "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeserializeGame(object o)
        {
            Board savedBoard = GetSerializedBoard();
             
            if (savedBoard != null)
            {
                InitSavedGameSettings();

                int boardSize = savedBoard.Cells.Count;

                BoardViewModel = new BoardViewModel(GameController.CreateNewGameBoard(boardSize, savedBoard));
                ScoreViewModel = new ScoreViewModel(Score.Open(GameController.GameStorePath + "score.xml"));

                InitCellEvent();

                InitGameEvent();
            }
        }

        private Board GetSerializedBoard() => Board.Open(GameController.GameStorePath + "game.xml");

        private void SaveGameSettings()
        {
            Properties.Settings.Default.SavedBoardSize = Settings.BoardSize.ToString();
            Properties.Settings.Default.SavedDropBallsPerStep = Settings.DropBallsPerStep.ToString();
            Properties.Settings.Default.SavedBallsInLine = Settings.NumBallsInLine.ToString();
        }

        private void InitSavedGameSettings()
        {
            Settings.BoardSize = int.Parse(Properties.Settings.Default.SavedBoardSize);
            Settings.DropBallsPerStep = int.Parse(Properties.Settings.Default.SavedDropBallsPerStep);
            Settings.NumBallsInLine = int.Parse(Properties.Settings.Default.SavedBallsInLine);

            RaisePropertyChanged("BoardSize");
            RaisePropertyChanged("RandomDroppingBalls");
            RaisePropertyChanged("NumBallsInLine");
        }
         
        private void OnGameOver()
        {
            if (MessageBox.Show("Start new game?", "GAME OVER", MessageBoxButton.YesNo, MessageBoxImage.None) == MessageBoxResult.Yes)
                ResetBoardAndDropBalls(null);
        }

        private Visibility GetChangedVisiblyMode(Visibility visibility) => 
            visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;  
    }
} 
