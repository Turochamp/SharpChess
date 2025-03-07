﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GameTest.cs" company="SharpChess.com">
//   SharpChess.com
// </copyright>
// <summary>
//   This is a test class for GameTest and is intended
//   to contain all GameTest Unit Tests
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#region License

// SharpChess
// Copyright (C) 2012 SharpChess.com
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

namespace SharpChess.Model.Tests
{
    #region Using

    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharpChess.Model;
    using SharpChess.Model.AI;

    #endregion

    /// <summary>
    /// This is a test class for GameTest and is intended
    ///  to contain all GameTest Unit Tests
    /// </summary>
    [TestClass]
    public class HashTableCheckTest
    {
        #region Public Properties

        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        #endregion

        // You can use the following additional attributes as you write your tests:
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext)
        // {
        // }
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup()
        // {
        // }
        // Use TestInitialize to run code before running each test
        // [TestInitialize()]
        // public void MyTestInitialize()
        // {
        // }
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup()
        // {
        // }
        #region Public Methods

        /// <summary>
        /// A test for Move Ordering - at the start of a game - no moves played.
        /// </summary>
        [TestMethod]
        public void HashTableCheck_Opening()
        {
            int positions = this.NodeCountTest("", 5);
            int h = HashTableCheck.Hits;
            int o = HashTableCheck.Overwrites;
            int p = HashTableCheck.Probes;
            int w = HashTableCheck.Writes;
        }

        /// <summary>
        /// A test for Move Ordering - at the start of a game - no moves played.
        /// </summary>
        [TestMethod]
        public void HashTableCheck_Ending()
        {
            int positions = this.NodeCountTest("8/2R2pk1/2P5/2r5/1p6/1P2Pq2/8/2K1B3 w - - 5 44", 5);
            int h = HashTableCheck.Hits;
            int o = HashTableCheck.Overwrites;
            int p = HashTableCheck.Probes;
            int w = HashTableCheck.Writes;
        }


        /// <summary>
        /// ECM Test
        /// </summary>
        [TestMethod]
        public void TimerTest()
        {
            TimeSpan t = this.NodeCountTime("r4rk1/1b3Npp/p7/1p3Q2/3P4/1B2q3/P5PP/3n1R1K b", 5);
            Assert.IsTrue(t.Ticks <= 53174165);
            // Nodes: 11,298
        }

        #endregion

        private int NodeCountTest(string fen, int depth)
        {
            Game.NewInternal(fen);
            Game.MaximumSearchDepth = depth;
            Game.ClockFixedTimePerMove = new TimeSpan(0, 10, 0); // 10 minute max
            Game.UseRandomOpeningMoves = false;
            Game.PlayerToPlay.Brain.Think();
            // TimeSpan elpased = Game.PlayerToPlay.Brain.ThinkingTimeElpased;
            return Game.PlayerToPlay.Brain.Search.PositionsSearchedThisTurn;
        }

        private TimeSpan NodeCountTime(string fen, int depth)
        {
            Game.NewInternal(fen);
            Game.MaximumSearchDepth = depth;
            Game.ClockFixedTimePerMove = new TimeSpan(0, 10, 0); // 10 minute max
            Game.UseRandomOpeningMoves = false;
            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            s.Start();
            Game.PlayerToPlay.Brain.Think();
           s.Stop();
           System.Diagnostics.Debug.WriteLine("elapsted = " + s.Elapsed);
           return s.Elapsed;
            // Console.Debug("asdafsd");            
//            // TimeSpan elpased = Game.PlayerToPlay.Brain.ThinkingTimeElpased;
 //           return Game.PlayerToPlay.Brain.Search.PositionsSearchedThisTurn;
        }

    }
}