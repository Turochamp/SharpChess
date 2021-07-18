using System;
using System.Globalization;

namespace SharpChess.Domain.Dto
{
    // Move to application layer
    public record GameSaveMoveDto(
        Square From, Square To,
        string Name,
        int? SecondsElapsed,
        int? MoveNo = null)
    {
        internal static GameSaveMoveDto Create(Move move)
        {
            return new GameSaveMoveDto(
                From: move.From,
                To: move.To,
                Name: move.Name.ToString(),
                SecondsElapsed: Convert.ToInt32(move.TimeStamp.TotalSeconds),
                MoveNo: move.MoveNo);
        }
    }
}
