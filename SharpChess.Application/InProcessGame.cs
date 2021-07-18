using SharpChess.Application.Dto;
using SharpChess.Application.Interface;
using SharpChess.Domain;
using System;

namespace SharpChess.Application
{
    public class InProcessGame : Game
    {
        public InProcessGame(IWindowsRegistry windowsRegistry, IGameSaveFile gameSaveFile)
        {
            WindowsRegistry = windowsRegistry;
            GameSaveFile = gameSaveFile;

            if (WindowsRegistry.GetStringValue("FileName") == null)
            {
                SaveGameFileName = string.Empty;
            }
            else
            {
                SaveGameFileName = WindowsRegistry.GetStringValue("FileName");
            }

            if (WindowsRegistry.GetStringValue("ShowThinking") == null)
            {
                ShowThinking = true;
            }
            else
            {
                ShowThinking = WindowsRegistry.GetStringValue("ShowThinking") == "1";
            }

            // Delete deprecated values
            if (WindowsRegistry.GetStringValue("EnablePondering") != null)
            {
                WindowsRegistry.DeleteValue("EnablePondering");
            }

            if (WindowsRegistry.GetStringValue("DisplayMoveAnalysisTree") != null)
            {
                WindowsRegistry.DeleteValue("DisplayMoveAnalysisTree");
            }

            if (WindowsRegistry.GetStringValue("ClockMoves") != null)
            {
                WindowsRegistry.DeleteValue("ClockMoves");
            }

            if (WindowsRegistry.GetStringValue("ClockMinutes") != null)
            {
                WindowsRegistry.DeleteValue("ClockMinutes");

            }
        }

        public override void TerminateGame()
        {
            base.TerminateGame();

            WindowsRegistry.SetStringValue("FileName", SaveGameFileName);
            WindowsRegistry.SetStringValue("ShowThinking", ShowThinking ? "1" : "0");
        }

        public override bool LoadGame(string strFileName)
        {
            MoveRedoList.Clear();

            GameSaveDto loadedData = GameSaveFile.Load(strFileName);

            if (loadedData == null)
            {
                return false;
            }

            if (loadedData.FEN != string.Empty)
            {
                NewInternal(loadedData.FEN);
            }

            if (loadedData.WhitePlayerIntellegence.HasValue)
            {
                PlayerWhite.Intellegence = loadedData.WhitePlayerIntellegence.Value;
            }

            if (loadedData.BlackPlayerIntellegence.HasValue)
            {
                PlayerBlack.Intellegence = loadedData.BlackPlayerIntellegence.Value;
            }

            if (loadedData.BoardOrientation.HasValue)
            {
                Board.Orientation = loadedData.BoardOrientation.Value;
            }

            if (loadedData.DifficultyLevel.HasValue)
            {
                DifficultyLevel = loadedData.DifficultyLevel.Value;
            }

            if (loadedData.ClockMoves.HasValue)
            {
                ClockMaxMoves = loadedData.ClockMoves.Value;
            }

            if (loadedData.ClockMinutes.HasValue)
            {
                ClockTime = new TimeSpan(0, loadedData.ClockMinutes.Value, 0);
            }

            if (loadedData.ClockSeconds.HasValue)
            {
                ClockTime = new TimeSpan(0, 0, loadedData.ClockSeconds.Value);
            }

            if (loadedData.MaximumSearchDepth.HasValue)
            {
                MaximumSearchDepth = loadedData.MaximumSearchDepth.Value;
            }

            if (loadedData.Pondering.HasValue)
            {
                EnablePondering = loadedData.Pondering.Value;
            }

            if (loadedData.UseRandomOpeningMoves.HasValue)
            {
                UseRandomOpeningMoves = loadedData.UseRandomOpeningMoves.Value;
            }

            foreach (var move in loadedData.Moves)
            {
                MakeAMoveInternal(Move.MoveNameFromString(move.Name), move.From.Piece, move.To);

                TimeSpan tsnTimeStamp;
                if (!move.SecondsElapsed.HasValue)
                {
                    if (MoveHistory.Count <= 2)
                    {
                        tsnTimeStamp = new TimeSpan(0);
                    }
                    else
                    {
                        tsnTimeStamp = MoveHistory.PenultimateForSameSide.TimeStamp + (new TimeSpan(0, 0, 30));
                    }
                }
                else
                {
                    tsnTimeStamp = new TimeSpan(0, 0, move.SecondsElapsed.Value);
                }

                MoveHistory.Last.TimeStamp = tsnTimeStamp;
                MoveHistory.Last.Piece.Player.Clock.TimeElapsed = tsnTimeStamp;
            }

            int intTurnNo = loadedData.TurnNo.HasValue ? loadedData.TurnNo.Value : loadedData.Moves.Count;
            for (int intIndex = loadedData.Moves.Count; intIndex > intTurnNo; intIndex--)
            {
                UndoMoveInternal();
            }

            return true;
        }

        public override void SaveGame(string fileName)
        {
            GameSaveDto data = GameSaveDto.Create(this);
            GameSaveFile.Save(fileName, data, GameSaveDto.CreateMoves(MoveRedoList));
        }

        private IWindowsRegistry WindowsRegistry { get; }
        private IGameSaveFile GameSaveFile { get; }
    }
}
