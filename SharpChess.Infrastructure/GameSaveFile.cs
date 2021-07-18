using SharpChess.Domain;
using SharpChess.Domain.Dto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Xml;

namespace SharpChess.Infrastructure
{
    public class GameSaveFile : IGameSaveFile
    {
        public GameSaveDto Load(string fileName)
        {         
            XmlDocument xmldoc = new XmlDocument();
            try
            {
                xmldoc.Load(fileName);
            }
            catch
            {
                return null;
            }

            XmlElement xmlnodeGame = (XmlElement)xmldoc.SelectSingleNode("/Game");

            if (xmlnodeGame == null)
            {
                return null;
            }

            string fen = xmlnodeGame.GetAttribute("FEN");

            Player.PlayerIntellegenceNames? whitePlayerIntellegence = null;
            if (xmlnodeGame.GetAttribute("WhitePlayer") != string.Empty)
            {
                whitePlayerIntellegence = xmlnodeGame.GetAttribute("WhitePlayer") == "Human"
                                               ? Player.PlayerIntellegenceNames.Human
                                               : Player.PlayerIntellegenceNames.Computer;
            }

            Player.PlayerIntellegenceNames? blackPlayerIntellegence = null;
            if (xmlnodeGame.GetAttribute("BlackPlayer") != string.Empty)
            {
                blackPlayerIntellegence = xmlnodeGame.GetAttribute("BlackPlayer") == "Human"
                                               ? Player.PlayerIntellegenceNames.Human
                                               : Player.PlayerIntellegenceNames.Computer;
            }

            Board.OrientationNames? boardOrientation = null;
            if (xmlnodeGame.GetAttribute("BoardOrientation") != string.Empty)
            {
                boardOrientation = xmlnodeGame.GetAttribute("BoardOrientation") == "White"
                                        ? Board.OrientationNames.White
                                        : Board.OrientationNames.Black;
            }

            int? difficultyLevel = null;
            if (xmlnodeGame.GetAttribute("DifficultyLevel") != string.Empty)
            {
                difficultyLevel = int.Parse(xmlnodeGame.GetAttribute("DifficultyLevel"));
            }

            int? clockMoves = null;
            if (xmlnodeGame.GetAttribute("ClockMoves") != string.Empty)
            {
                clockMoves = int.Parse(xmlnodeGame.GetAttribute("ClockMoves"));
            }

            int? clockMinutes = null; 
            if (xmlnodeGame.GetAttribute("ClockMinutes") != string.Empty)
            {
                clockMinutes = int.Parse(xmlnodeGame.GetAttribute("ClockMinutes"));
            }

            int? clockSeconds = null;
            if (xmlnodeGame.GetAttribute("ClockSeconds") != string.Empty)
            {
                clockSeconds = int.Parse(xmlnodeGame.GetAttribute("ClockSeconds"));
            }

            int? maximumSearchDepth = null;
            if (xmlnodeGame.GetAttribute("MaximumSearchDepth") != string.Empty)
            {
                maximumSearchDepth = int.Parse(xmlnodeGame.GetAttribute("MaximumSearchDepth"));
            }

            bool? pondering = null;
            if (xmlnodeGame.GetAttribute("Pondering") != string.Empty)
            {
                pondering = xmlnodeGame.GetAttribute("Pondering") == "1";
            }

            bool? useRandomOpeningMoves = null;
            if (xmlnodeGame.GetAttribute("UseRandomOpeningMoves") != string.Empty)
            {
                useRandomOpeningMoves = xmlnodeGame.GetAttribute("UseRandomOpeningMoves") == "1";
            }

            int? turnNo = null;
            if (xmlnodeGame.GetAttribute("TurnNo") != string.Empty)
            {
                turnNo = int.Parse(xmlnodeGame.GetAttribute("TurnNo"));
            }

            List<GameSaveMoveDto> moves = new List<GameSaveMoveDto>();
            XmlNodeList xmlnodelist = xmldoc.SelectNodes("/Game/Move");
            if (xmlnodelist != null)
            {
                foreach (XmlElement xmlnode in xmlnodelist)
                {
                    Square from;
                    Square to;
                    if (xmlnode.GetAttribute("FromFile") != string.Empty)
                    {
                        from = Board.GetSquare(
                            Convert.ToInt32(xmlnode.GetAttribute("FromFile")),
                            Convert.ToInt32(xmlnode.GetAttribute("FromRank")));
                        to = Board.GetSquare(
                            Convert.ToInt32(xmlnode.GetAttribute("ToFile")),
                            Convert.ToInt32(xmlnode.GetAttribute("ToRank")));
                    }
                    else
                    {
                        from = Board.GetSquare(xmlnode.GetAttribute("From"));
                        to = Board.GetSquare(xmlnode.GetAttribute("To"));
                    }

                    string name = xmlnode.GetAttribute("Name");

                    int? secondsElapsed = null;
                    if (xmlnode.GetAttribute("SecondsElapsed") != string.Empty)
                    {
                        secondsElapsed = int.Parse(xmlnode.GetAttribute("SecondsElapsed"));
                    }

                    moves.Add(new GameSaveMoveDto(from, to, name, secondsElapsed));
                }
            }

            return new GameSaveDto(fen,
                               whitePlayerIntellegence,
                               blackPlayerIntellegence,
                               boardOrientation,
                               difficultyLevel,
                               clockMoves,
                               clockMinutes,
                               clockSeconds,
                               maximumSearchDepth,
                               pondering,
                               useRandomOpeningMoves,
                               moves,
                               turnNo);
        }

        public void Save(string fileName, GameSaveDto gameDto, List<GameSaveMoveDto> moveRedoList)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement xmlnodeGame = xmldoc.CreateElement("Game");

            xmldoc.AppendChild(xmlnodeGame);

            xmlnodeGame.SetAttribute("FEN", gameDto.FEN);
            xmlnodeGame.SetAttribute("TurnNo", gameDto.TurnNo?.ToString(CultureInfo.InvariantCulture));
            xmlnodeGame.SetAttribute(
                "WhitePlayer", gameDto.WhitePlayerIntellegence == Player.PlayerIntellegenceNames.Human ? "Human" : "Computer");
            xmlnodeGame.SetAttribute(
                "BlackPlayer", gameDto.BlackPlayerIntellegence == Player.PlayerIntellegenceNames.Human ? "Human" : "Computer");
            xmlnodeGame.SetAttribute(
                "BoardOrientation", gameDto.BoardOrientation == Board.OrientationNames.White ? "White" : "Black");
            xmlnodeGame.SetAttribute("Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            xmlnodeGame.SetAttribute("DifficultyLevel", gameDto.DifficultyLevel?.ToString(CultureInfo.InvariantCulture));
            xmlnodeGame.SetAttribute("ClockMoves", gameDto.ClockMoves?.ToString(CultureInfo.InvariantCulture));
            xmlnodeGame.SetAttribute("ClockSeconds", gameDto.ClockSeconds?.ToString(CultureInfo.InvariantCulture));
            xmlnodeGame.SetAttribute("MaximumSearchDepth", gameDto.MaximumSearchDepth?.ToString(CultureInfo.InvariantCulture));
            xmlnodeGame.SetAttribute("Pondering", gameDto.Pondering.HasValue ? "1" : "0");
            xmlnodeGame.SetAttribute("UseRandomOpeningMoves", gameDto.UseRandomOpeningMoves.HasValue ? "1" : "0");

            foreach (GameSaveMoveDto move in gameDto.Moves)
            {
                AddSaveGameNode(xmldoc, xmlnodeGame, move);
            }

            // Redo moves
            for (int intIndex = moveRedoList.Count - 1; intIndex >= 0; intIndex--)
            {
                AddSaveGameNode(xmldoc, xmlnodeGame, moveRedoList[intIndex]);
            }

            xmldoc.Save(fileName);
        }

        /// <summary>
        ///   Add a move node to the save game XML document.
        /// </summary>
        /// <param name="xmldoc"> Xml document representing the save game file. </param>
        /// <param name="xmlnodeGame"> Parent game xmlnode. </param>
        /// <param name="move"> Move to append to the save game Xml document. </param>
        private void AddSaveGameNode(XmlDocument xmldoc, XmlElement xmlnodeGame, GameSaveMoveDto move)
        {
            XmlElement xmlnodeMove = xmldoc.CreateElement("Move");
            xmlnodeGame.AppendChild(xmlnodeMove);
            xmlnodeMove.SetAttribute("MoveNo", move.MoveNo?.ToString(CultureInfo.InvariantCulture));
            xmlnodeMove.SetAttribute("Name", move.Name.ToString());
            xmlnodeMove.SetAttribute("From", move.From.Name);
            xmlnodeMove.SetAttribute("To", move.To.Name);
            xmlnodeMove.SetAttribute("SecondsElapsed", move.SecondsElapsed?.ToString(CultureInfo.InvariantCulture));
        }
    }
}
