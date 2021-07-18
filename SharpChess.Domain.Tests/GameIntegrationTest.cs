using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpChess.Application;
using System.IO;

// TODO: Move to SharpChess.Application.Tests?
namespace SharpChess.Domain.Tests
{
    [TestClass()]
    public class GameIntegrationTest
    {
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            // Game.Instance is set by constructor
            GameFactory.CreateLocal();
        }

        public Game Game => Game.Instance;

        [TestMethod()]
        public void SaveAndLoad_MidGame()
        {
            // Assemble
            Game.NewInternal(GameTest.MidGameFen);
            // TODO?: Add some move to SaveAndLoad_MidGame

            string fileName = "SaveAndLoad_MidGame.SharpChess";

            // Act
            Game.SaveGame(fileName);
            Game.LoadGame(fileName);

            // Assert
            Assert.IsTrue(File.Exists(fileName));
            // TODO?: Analyse why set/get Fen versions are not the same
            Assert.AreNotEqual(GameTest.MidGameFen, Fen.GameStartPosition); 
        }
    }
}
