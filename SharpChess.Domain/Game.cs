// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Game.cs" company="SharpChess.com">
//   SharpChess.com
// </copyright>
// <summary>
//   Represents the game of chess over its lfetime. Holds the board, players, turn number and everything related to the chess game in progress.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#region License

// SharpChess
// Copyright (C) 2012 SharpChess.com
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

namespace SharpChess.Domain
{
    #region Using

    using System;
    using SharpChess.Domain.AI;

    #endregion

    // TODO: Move Use Cases to Application layer 
    /// <summary>
    ///   Represents the game of chess over its lfetime. Holds the board, players, turn number and everything related to the chess game in progress.
    /// </summary>
    public class Game
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes members of the <see cref="Game" /> class.
        /// </summary>
        public Game()
        {
            // Do nothing if already initialized
            if (_instance != null)
                return;

            _instance = this;

            EnableFeatures();
            ClockIncrementPerMove = new TimeSpan(0, 0, 0);
            ClockFixedTimePerMove = new TimeSpan(0, 0, 0);
            DifficultyLevel = 1;
            ClockTime = new TimeSpan(0, 5, 0);
            ClockMaxMoves = 40;
            UseRandomOpeningMoves = true;
            MoveRedoList = new Moves();
            MaximumSearchDepth = 1;
            MoveAnalysis = new Moves();
            MoveHistory = new Moves();
            FenStartPosition = string.Empty;
            HashTable.Initialise();
            HashTablePawn.Initialise();
            HashTableCheck.Initialise();

            PlayerWhite = new PlayerWhite();
            PlayerBlack = new PlayerBlack();
            PlayerToPlay = PlayerWhite;
            Board.EstablishHashKey();
            OpeningBookSimple.Initialise();

            PlayerWhite.Brain.ReadyToMakeMoveEvent += PlayerReadyToMakeMove;
            PlayerBlack.Brain.ReadyToMakeMoveEvent += PlayerReadyToMakeMove;

            // OpeningBook.BookConvert(Game.PlayerWhite);
        }

        #endregion

        #region Delegates

        /// <summary>
        ///   The game event type, raised to the UI when significant game events occur.
        /// </summary>
        public delegate void GameEvent();

        #endregion

        #region Public Events

        /// <summary>
        ///   Raised when the board position changes.
        /// </summary>
        public event GameEvent BoardPositionChanged;

        /// <summary>
        ///   Raised when the game is paused.
        /// </summary>
        public event GameEvent GamePaused;

        /// <summary>
        ///   Raised when the game is resumed.
        /// </summary>
        public event GameEvent GameResumed;

        /// <summary>
        ///   Riased when the game is saved.
        /// </summary>
        public event GameEvent GameSaved;

        /// <summary>
        ///   Raised when settings are updated.
        /// </summary>
        public event GameEvent SettingsUpdated;

        #endregion

        #region Enums

        /// <summary>
        ///   Game stages.
        /// </summary>
        public enum GameStageNames
        {
            /// <summary>
            ///   The opening.
            /// </summary>
            Opening,

            /// <summary>
            ///   The middle.
            /// </summary>
            Middle,

            /// <summary>
            ///   The end.
            /// </summary>
            End
        }

        #endregion

        #region Public Properties

        private static Game _instance;
        public static Game Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException("Instance must be initialized before usage.");
                }
                return _instance;
            }
        }

        /// <summary>
        ///   Gets the available MegaBytes of free computer memory.
        /// </summary>
        public static uint AvailableMegaBytes
        {
            get
            {
                try
                {
                    // PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes"); 
                    // return ((uint)Math.Max(Convert.ToInt32(ramCounter.NextValue()) - 25, 16));
                    return 16;
                }
                catch
                {
                    return 16;
                }
            }
        }

        /// <summary>
        ///   Gets or sets the Backup Game Path.
        /// </summary>
        public string BackupGamePath { private get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether CaptureMoveAnalysisData.
        /// </summary>
        public bool CaptureMoveAnalysisData { get; set; }

        /// <summary>
        ///   Gets or sets the Clock Fixed Time Per Move.
        /// </summary>
        public TimeSpan ClockFixedTimePerMove { get; set; }

        /// <summary>
        ///   Gets or sets the Clock Increment Per Move.
        /// </summary>
        public TimeSpan ClockIncrementPerMove { get; set; }

        /// <summary>
        ///   Gets or sets the max number of moves on the clock. e.g. 60 moves in 30 minutes
        /// </summary>
        public int ClockMaxMoves { get; set; }

        /// <summary>
        ///   Gets or sets the Clock Time.
        /// </summary>
        public TimeSpan ClockTime { get; set; }

        /// <summary>
        ///   Gets or sets game Difficulty Level.
        /// </summary>
        public int DifficultyLevel { get; set; }

        /// <summary>
        ///   Gets a value indicating whether Edit Mode is Active.
        /// </summary>
        public bool EditModeActive { get; private set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use Aspiration Search.
        /// </summary>
        public bool EnableAspiration { get; private set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use Search Extensions.
        /// </summary>
        public bool EnableExtensions { get; private set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use the history heuristic ( <see cref="HistoryHeuristic" /> class).
        /// </summary>
        public bool EnableHistoryHeuristic { get; private set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use the killer move heuristic ( <see cref="KillerMoves" /> class).
        /// </summary>
        public bool EnableKillerMoves { get; private set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use Null Move Forward Pruning.
        /// </summary>
        public bool EnableNullMovePruning { get; private set; }

        /// <summary>
        ///   Gets or sets a value indicating whether Pondering has been enabled.
        /// </summary>
        public bool EnablePondering { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use PVS Search.
        /// </summary>
        public bool EnablePvsSearch { get; private set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use Quiescense.
        /// </summary>
        public bool EnableQuiescense { get; private set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use Search Reductions.
        /// </summary>
        public bool EnableReductions { get; private set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use Late Move Reductions.
        /// </summary>
        public bool EnableReductionLateMove { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use Margin Futilty Reductions.
        /// </summary>
        public bool EnableReductionFutilityMargin { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use Fixed Depth Futilty Reductions.
        /// </summary>
        public bool EnableReductionFutilityFixedDepth { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use the transposition table ( <see cref="HashTable" /> class).
        /// </summary>
        public static bool EnableTranspositionTable { get; set; }

        /// <summary>
        ///   Gets or sets the FEN string for the chess Start Position.
        /// </summary>
        public string FenStartPosition { get; set; }

        /// <summary>
        ///   Gets or sets FiftyMoveDrawBase. Appears to be a value set when using a FEN string. Doesn't seem quite right! TODO Invesigate FiftyMoveDrawBase.
        /// </summary>
        public int FiftyMoveDrawBase { get; set; }

        /// <summary>
        ///   Gets the current game save file name.
        /// </summary>
        public string FileName
        {
            get
            {
                return SaveGameFileName == string.Empty ? "New Game" : SaveGameFileName;
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether Analyse Mode is active.
        /// </summary>
        public bool IsInAnalyseMode { get; set; }

        /// <summary>
        ///   Gets a value indicating whether the game is paused.
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return !PlayerToPlay.Clock.IsTicking;
            }
        }

        /// <summary>
        ///   Gets the lowest material count for black or white.
        /// </summary>
        public int LowestMaterialCount
        {
            get
            {
                int intWhiteMaterialCount = PlayerWhite.MaterialCount;
                int intBlackMaterialCount = PlayerBlack.MaterialCount;
                return intWhiteMaterialCount < intBlackMaterialCount ? intWhiteMaterialCount : intBlackMaterialCount;
            }
        }

        /// <summary>
        ///   Gets the largest valid Material Count.
        /// </summary>
        public int MaxMaterialCount
        {
            get
            {
                return 7;
            }
        }

        /// <summary>
        ///   Gets or sets the maximum search depth.
        /// </summary>
        public int MaximumSearchDepth { get; set; }

        /// <summary>
        ///   Gets or sets the list of move-analysis moves.
        /// </summary>
        public Moves MoveAnalysis { get; set; }

        /// <summary>
        ///   Gets the currebt move history.
        /// </summary>
        public Moves MoveHistory { get; private set; }

        /// <summary>
        ///   Gets the current move number.
        /// </summary>
        public int MoveNo
        {
            get
            {
                return TurnNo >> 1;
            }
        }

        /// <summary>
        ///   Gets the move redo list.
        /// </summary>
        public Moves MoveRedoList { get; private set; }

        /// <summary>
        ///   Gets black player.
        /// </summary>
        public Player PlayerBlack { get; private set; }

        /// <summary>
        ///   Gets or sets the player to play.
        /// </summary>
        public Player PlayerToPlay { get; set; }

        /// <summary>
        ///   Gets white player.
        /// </summary>
        public Player PlayerWhite { get; private set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to show thinking.
        /// </summary>
        public bool ShowThinking { get; set; }

        /// <summary>
        ///   Gets current game stage.
        /// </summary>
        public GameStageNames Stage
        {
            get
            {
                if (LowestMaterialCount >= MaxMaterialCount)
                {
                    return GameStageNames.Opening;
                }

                return LowestMaterialCount <= 3 ? GameStageNames.End : GameStageNames.Middle;
            }
        }

        /// <summary>
        ///   Gets ThreadCounter.
        /// </summary>
        public int ThreadCounter { get; internal set; }

        /// <summary>
        ///   Gets the current turn number.
        /// </summary>
        public int TurnNo { get; internal set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use random opening moves.
        /// </summary>
        public bool UseRandomOpeningMoves { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Captures all pieces.
        /// </summary>
        public void CaptureAllPieces()
        {
            PlayerWhite.CaptureAllPieces();
            PlayerBlack.CaptureAllPieces();
        }

        /// <summary>
        ///   Demotes all pieces.
        /// </summary>
        public void DemoteAllPieces()
        {
            PlayerWhite.DemoteAllPieces();
            PlayerBlack.DemoteAllPieces();
        }

        /// <summary>
        ///   Load a saved game.
        /// </summary>
        /// <param name="fileName"> File name. </param>
        /// <returns> Returns True is game loaded successfully. </returns>
        public bool Load(string fileName)
        {
            SuspendPondering();

            NewInternal();
            SaveGameFileName = fileName;
            bool blnSuccess = LoadGame(fileName);
            if (blnSuccess)
            {
                SaveBackup();
                SendBoardPositionChangeEvent();
            }

            PausePlay();

            return blnSuccess;
        }

        /// <summary>
        ///   Load backup game.
        /// </summary>
        /// <returns> Returns True is game loaded successfully. </returns>
        public bool LoadBackup()
        {
            return LoadGame(BackupGamePath);
        }

        /// <summary>
        ///   Make a move.
        /// </summary>
        /// <param name="moveName"> The move name. </param>
        /// <param name="piece"> The piece to move. </param>
        /// <param name="square"> The square to move to. </param>
        public void MakeAMove(Move.MoveNames moveName, Piece piece, Square square)
        {
            SuspendPondering();
            MakeAMoveInternal(moveName, piece, square);
            SaveBackup();
            SendBoardPositionChangeEvent();
            CheckIfAutoNextMove();
        }

        /// <summary>
        ///   Start a new game.
        /// </summary>
        public void New()
        {
            New(string.Empty);
        }

        /// <summary>
        ///   Start a new game using a FEN string.
        /// </summary>
        /// <param name="fenString"> The FEN string. </param>
        public void New(string fenString)
        {
            SuspendPondering();

            NewInternal(fenString);
            SaveBackup();
            SendBoardPositionChangeEvent();
            ResumePondering();
        }

        /// <summary>
        ///   Pause the game.
        /// </summary>
        public void PausePlay()
        {
            PlayerToPlay.Clock.Stop();
            PlayerToPlay.Brain.ForceImmediateMove();
            GamePaused();
        }

        /// <summary>
        ///   Redo all moves.
        /// </summary>
        public void RedoAllMoves()
        {
            SuspendPondering();
            while (MoveRedoList.Count > 0)
            {
                RedoMoveInternal();
            }

            SaveBackup();
            SendBoardPositionChangeEvent();
            ResumePondering();
        }

        /// <summary>
        ///   Redo a move.
        /// </summary>
        public void RedoMove()
        {
            SuspendPondering();
            RedoMoveInternal();
            SaveBackup();
            SendBoardPositionChangeEvent();
            ResumePondering();
        }

        /// <summary>
        ///   Resume then game.
        /// </summary>
        public void ResumePlay()
        {
            PlayerToPlay.Clock.Start();
            GameResumed();
            if (PlayerToPlay.Intellegence == Player.PlayerIntellegenceNames.Computer)
            {
                MakeNextComputerMove();
            }
            else
            {
                ResumePondering();
            }
        }

        /// <summary>
        ///   Resume pondering.
        /// </summary>
        public void ResumePondering()
        {
            if (IsPaused)
            {
                return;
            }

            if (!EnablePondering)
            {
                return;
            }

            if (!PlayerToPlay.CanMove)
            {
                return;
            }

            if (PlayerWhite.Intellegence == Player.PlayerIntellegenceNames.Computer
                && PlayerBlack.Intellegence == Player.PlayerIntellegenceNames.Computer)
            {
                return;
            }

            if (PlayerToPlay.OpposingPlayer.Intellegence == Player.PlayerIntellegenceNames.Computer)
            {
                if (!PlayerToPlay.Brain.IsPondering)
                {
                    PlayerToPlay.Brain.StartPondering();
                }
            }
        }

        /// <summary>
        ///   Save the game as a file name.
        /// </summary>
        /// <param name="fileName"> The file name. </param>
        public void Save(string fileName)
        {
            SuspendPondering();

            SaveBackup();
            SaveGame(fileName);
            SaveGameFileName = fileName;

            GameSaved();

            ResumePondering();
        }

        /// <summary>
        ///   Call when settings have been changed in the UI.
        /// </summary>
        public void SettingsUpdate()
        {
            SuspendPondering();
            if (!WinBoard.Active)
            {
                SaveBackup();
            }

            SettingsUpdated();
            ResumePondering();
        }

/*
        /// <summary>
        ///   Start normal game.
        /// </summary>
        public void StartNormalGame()
        {
            PlayerToPlay.Clock.Start();
            ResumePondering();
        }
*/

        /// <summary>
        ///   Suspend pondering.
        /// </summary>
        public void SuspendPondering()
        {
            if (PlayerToPlay.Brain.IsPondering)
            {
                PlayerToPlay.Brain.ForceImmediateMove();
            }
            else if (PlayerToPlay.Brain.IsThinking)
            {
                PlayerToPlay.Brain.ForceImmediateMove();
                UndoMove();
            }
        }

        /// <summary>
        ///   Terminate the game.
        /// </summary>
        public virtual void TerminateGame()
        {
            WinBoard.StopListener();

            SuspendPondering();
            PlayerWhite.Brain.AbortThinking();
            PlayerBlack.Brain.AbortThinking();
        }

        /// <summary>
        ///   Instruct the computer to begin thinking, and take its turn.
        /// </summary>
        public void Think()
        {
            SuspendPondering();
            MakeNextComputerMove();
        }

        /// <summary>
        ///   Toggle edit mode.
        /// </summary>
        public void ToggleEditMode()
        {
            EditModeActive = !EditModeActive;
        }

        /// <summary>
        ///   Undo all moves.
        /// </summary>
        public void UndoAllMoves()
        {
            SuspendPondering();
            UndoAllMovesInternal();
            SaveBackup();
            SendBoardPositionChangeEvent();
            ResumePondering();
        }

        /// <summary>
        ///   Undo the last move.
        /// </summary>
        public void UndoMove()
        {
            SuspendPondering();
            UndoMoveInternal();
            SaveBackup();
            SendBoardPositionChangeEvent();
            ResumePondering();
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Start then next move automatically, if its the computers turn.
        /// </summary>
        private void CheckIfAutoNextMove()
        {
            if (PlayerWhite.Intellegence == Player.PlayerIntellegenceNames.Computer
                && PlayerBlack.Intellegence == Player.PlayerIntellegenceNames.Computer)
            {
                // Dont want an infinate loop of Computer moves
                return;
            }

            if (PlayerToPlay.Intellegence == Player.PlayerIntellegenceNames.Computer)
            {
                if (PlayerToPlay.CanMove)
                {
                    MakeNextComputerMove();
                }
            }
        }

        /// <summary>
        /// Enable or disable SharpChess's features
        /// </summary>
        private void EnableFeatures()
        {
            EnableAspiration = false;
            EnableExtensions = true;
            EnableHistoryHeuristic = true;
            EnableKillerMoves = true;
            EnableNullMovePruning = true;
            EnablePvsSearch = true;
            EnableQuiescense = true;
            EnableReductions = true;
            EnableReductionFutilityMargin = false;
            EnableReductionFutilityFixedDepth = true;
            EnableReductionLateMove = true;
            EnableTranspositionTable = true;
        }

        /// <summary>
        ///   Load game from the specified file name.
        /// </summary>
        /// <param name="strFileName"> The file name. </param>
        /// <returns> True if load was successful. </returns>
        public virtual bool LoadGame(string strFileName)
        {
            return false;
        }

        /// <summary>
        ///   Make the specified move. For internal use only.
        /// </summary>
        /// <param name="moveName"> The move name. </param>
        /// <param name="piece"> The piece to move. </param>
        /// <param name="square"> The square to move to. </param>
        protected void MakeAMoveInternal(Move.MoveNames moveName, Piece piece, Square square)
        {
            MoveRedoList.Clear();
            Move move = piece.Move(moveName, square);
            move.EnemyStatus = move.Piece.Player.OpposingPlayer.Status;
            PlayerToPlay.Clock.Stop();
            MoveHistory.Last.TimeStamp = PlayerToPlay.Clock.TimeElapsed;
            if (PlayerToPlay.Intellegence == Player.PlayerIntellegenceNames.Computer)
            {
                WinBoard.SendMove(move);
                if (!PlayerToPlay.OpposingPlayer.CanMove)
                {
                    if (PlayerToPlay.OpposingPlayer.IsInCheckMate)
                    {
                        WinBoard.SendCheckMate();
                    }
                    else if (!PlayerToPlay.OpposingPlayer.IsInCheck)
                    {
                        WinBoard.SendCheckStaleMate();
                    }
                }
                else if (PlayerToPlay.OpposingPlayer.CanClaimThreeMoveRepetitionDraw)
                {
                    WinBoard.SendDrawByRepetition();
                }
                else if (PlayerToPlay.OpposingPlayer.CanClaimFiftyMoveDraw)
                {
                    WinBoard.SendDrawByFiftyMoveRule();
                }
                else if (PlayerToPlay.OpposingPlayer.CanClaimInsufficientMaterialDraw)
                {
                    WinBoard.SendDrawByInsufficientMaterial();
                }
            }

            PlayerToPlay = PlayerToPlay.OpposingPlayer;
            PlayerToPlay.Clock.Start();
        }

        /// <summary>
        ///   Instruct the computer to make its next move.
        /// </summary>
        private void MakeNextComputerMove()
        {
            if (PlayerToPlay.CanMove)
            {
                PlayerToPlay.Brain.StartThinking();
            }
        }

        /// <summary>
        ///   Start a new game. For internal use only.
        /// </summary>
        public void NewInternal()
        {
            NewInternal(string.Empty);
        }

        /// <summary>
        ///   Start a new game from the specified FEN string position. For internal use only.
        /// </summary>
        /// <param name="fenString"> The str fen. </param>
        public void NewInternal(string fenString)
        {
            if (fenString == string.Empty)
            {
                fenString = Fen.GameStartPosition;
            }

            Fen.Validate(fenString);

            HashTable.Clear();
            HashTablePawn.Clear();
            HashTableCheck.Clear();
            KillerMoves.Clear();
            HistoryHeuristic.Clear();

            UndoAllMovesInternal();
            MoveRedoList.Clear();
            SaveGameFileName = string.Empty;
            Fen.SetBoardPosition(fenString);
            PlayerWhite.Clock.Reset();
            PlayerBlack.Clock.Reset();
        }

        /// <summary>
        ///   Called when the computer has finished thinking, and is ready to make its move.
        /// </summary>
        /// <exception cref="ApplicationException">Raised when principal variation is empty.</exception>
        private void PlayerReadyToMakeMove()
        {
            Move move;
            if (PlayerToPlay.Brain.PrincipalVariation.Count > 0)
            {
                move = PlayerToPlay.Brain.PrincipalVariation[0];
            }
            else
            {
                throw new ApplicationException("Player_ReadToMakeMove: Principal Variation is empty.");
            }

            MakeAMoveInternal(move.Name, move.Piece, move.To);
            SaveBackup();
            SendBoardPositionChangeEvent();
            ResumePondering();
        }

        /// <summary>
        ///   Redo move. For internal use only.
        /// </summary>
        private void RedoMoveInternal()
        {
            if (MoveRedoList.Count > 0)
            {
                Move moveRedo = MoveRedoList[MoveRedoList.Count - 1];
                PlayerToPlay.Clock.Revert();
                moveRedo.Piece.Move(moveRedo.Name, moveRedo.To);
                PlayerToPlay.Clock.TimeElapsed = moveRedo.TimeStamp;
                MoveHistory.Last.TimeStamp = moveRedo.TimeStamp;
                MoveHistory.Last.EnemyStatus = moveRedo.Piece.Player.OpposingPlayer.Status; // 14Mar05 Nimzo
                PlayerToPlay = PlayerToPlay.OpposingPlayer;
                MoveRedoList.RemoveLast();
                if (!IsPaused)
                {
                    PlayerToPlay.Clock.Start();
                }
            }
        }

        /// <summary>
        ///   Save a backup of the current game.
        /// </summary>
        private void SaveBackup()
        {
            if (!WinBoard.Active)
            {
                // Only save backups if not using WinBoard.
                SaveGame(BackupGamePath);
            }
        }

        /// <summary>
        ///   Save game using the specified file name.
        /// </summary>
        /// <param name="fileName"> The file name. </param>
        public virtual void SaveGame(string fileName)
        {
        }

        /// <summary>
        ///   The send board position change event.
        /// </summary>
        private void SendBoardPositionChangeEvent()
        {
            BoardPositionChanged();
        }

        /// <summary>
        ///   Undo all moves. For internal use pnly.
        /// </summary>
        protected void UndoAllMovesInternal()
        {
            while (MoveHistory.Count > 0)
            {
                UndoMoveInternal();
            }
        }

        /// <summary>
        ///   Undo move. For internal use only.
        /// </summary>
        protected void UndoMoveInternal()
        {
            if (MoveHistory.Count > 0)
            {
                Move moveUndo = MoveHistory.Last;
                PlayerToPlay.Clock.Revert();
                MoveRedoList.Add(moveUndo);
                Move.Undo(moveUndo);
                PlayerToPlay = PlayerToPlay.OpposingPlayer;
                if (MoveHistory.Count > 1)
                {
                    Move movePenultimate = MoveHistory[MoveHistory.Count - 2];
                    PlayerToPlay.Clock.TimeElapsed = movePenultimate.TimeStamp;
                }
                else
                {
                    PlayerToPlay.Clock.TimeElapsed = new TimeSpan(0);
                }

                if (!IsPaused)
                {
                    PlayerToPlay.Clock.Start();
                }
            }
        }

        #endregion

        /// <summary>
        ///   The file name.
        /// </summary>
        protected string SaveGameFileName { get; set; } = string.Empty;
    }
}