// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Game.cs" company="SharpChess.com">
//   SharpChess.com
// </copyright>
// <summary>
//   Represents the game of chess over its lfetime. Holds the board, players, turn number and everything related to the chess game in progress.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using SharpChess.Domain.Dto;
using System.Collections.Generic;

namespace SharpChess.Domain
{
    public interface IGameSaveFile
    {
        GameSaveDto Load(string strFileName);
        void Save(string strFileName, GameSaveDto gameDto, List<GameSaveMoveDto> moveRedoList);
    }
}