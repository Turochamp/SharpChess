using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpChess.Domain.Dto
{
    // Move to Application layer
    public record GameSaveDto(
        string FEN,
        Player.PlayerIntellegenceNames? WhitePlayerIntellegence, Player.PlayerIntellegenceNames? BlackPlayerIntellegence,
        Board.OrientationNames? BoardOrientation,
        int? DifficultyLevel,
        int? ClockMoves,
        int? ClockMinutes, int? ClockSeconds,
        int? MaximumSearchDepth,
        bool? Pondering,
        bool? UseRandomOpeningMoves,
        List<GameSaveMoveDto> Moves,
        int? TurnNo)
    {
        public static GameSaveDto Create(Game game)
        {
            return new(
                FEN: game.FenStartPosition == Fen.GameStartPosition ? string.Empty : game.FenStartPosition,
                WhitePlayerIntellegence: game.PlayerWhite.Intellegence,
                BlackPlayerIntellegence: game.PlayerBlack.Intellegence,
                BoardOrientation: Board.Orientation,
                DifficultyLevel: game.DifficultyLevel,
                ClockMoves: game.ClockMaxMoves,
                ClockMinutes: null,
                ClockSeconds: Convert.ToInt32(game.ClockTime.TotalSeconds),
                MaximumSearchDepth: game.MaximumSearchDepth,
                Pondering: game.EnablePondering,
                UseRandomOpeningMoves: game.UseRandomOpeningMoves,
                Moves: CreateMoves(game.MoveHistory),
                TurnNo: game.TurnNo);
        }

        public static List<GameSaveMoveDto> CreateMoves(Moves sourceMoves)
        {
            List<GameSaveMoveDto> result = new();
            foreach(Move source in sourceMoves)
            {
                result.Add(GameSaveMoveDto.Create(source));
            }
            return result;
        }
    }
}
