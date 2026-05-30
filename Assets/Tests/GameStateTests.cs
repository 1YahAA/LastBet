// // Зачем: Юнит-тесты для GameState
// using NUnit.Framework;
// using UnityEngine;

// public class GameStateTests
// {
//     // Создаём чистый GameState для каждого теста
//     private GameState Make()
//     {
//         var s = ScriptableObject.CreateInstance<GameState>();
//         s.ResetAll();
//         return s;
//     }

//     // БАЗОВЫЕ ТОКЕНЫ

//     [Test] public void AddRevolt_Plus1()
//     {
//         var s = Make();
//         s.AddToken(TokenType.Revolt);
//         Assert.AreEqual(1, s.revolt);
//         Assert.AreEqual(0, s.obedience);
//         Assert.AreEqual(0, s.analysis);
//     }

//     [Test] public void AddObedience_Plus1()
//     {
//         var s = Make();
//         s.AddToken(TokenType.Obedience);
//         Assert.AreEqual(1, s.obedience);
//     }

//     [Test] public void AddAnalysis_Plus1()
//     {
//         var s = Make();
//         s.AddToken(TokenType.Analysis);
//         Assert.AreEqual(1, s.analysis);
//     }

//     [Test] public void AddToken_CustomAmount()
//     {
//         var s = Make();
//         s.AddToken(TokenType.Revolt, 5);
//         Assert.AreEqual(5, s.revolt);
//     }

//     // КОНЦОВКИ

//     [Test] public void Ending_RevoltDominates_Freedom()
//     {
//         var s = Make();
//         s.AddToken(TokenType.Revolt, 3);
//         s.AddToken(TokenType.Obedience, 1);
//         s.AddToken(TokenType.Analysis, 1);
//         Assert.AreEqual(EndingType.Freedom, s.GetEnding());
//     }

//     [Test] public void Ending_ObedienceDominates_Submission()
//     {
//         var s = Make();
//         s.AddToken(TokenType.Obedience, 3);
//         s.AddToken(TokenType.Revolt, 1);
//         s.AddToken(TokenType.Analysis, 1);
//         Assert.AreEqual(EndingType.Submission, s.GetEnding());
//     }

//     [Test] public void Ending_AnalysisDominates_Death()
//     {
//         var s = Make();
//         s.AddToken(TokenType.Analysis, 3);
//         s.AddToken(TokenType.Revolt, 1);
//         s.AddToken(TokenType.Obedience, 1);
//         Assert.AreEqual(EndingType.Death, s.GetEnding());
//     }

//     [Test] public void Ending_AllEqual_Death()
//     {
//         // При ничье — Death (никто не доминирует строго)
//         var s = Make();
//         s.AddToken(TokenType.Revolt, 2);
//         s.AddToken(TokenType.Obedience, 2);
//         s.AddToken(TokenType.Analysis, 2);
//         Assert.AreEqual(EndingType.Death, s.GetEnding());
//     }

//     [Test] public void Ending_RevoltEqualsObedience_Death()
//     {
//         // Revolt = Obedience — ни один не доминирует строго → Death
//         var s = Make();
//         s.AddToken(TokenType.Revolt, 3);
//         s.AddToken(TokenType.Obedience, 3);
//         s.AddToken(TokenType.Analysis, 1);
//         Assert.AreEqual(EndingType.Death, s.GetEnding());
//     }

//     // КОКТЕЙЛЬ

//     [Test] public void Cocktail_FirstDrink_ObedienceOnly()
//     {
//         var s = Make();
//         s.DrinkCocktail();
//         Assert.AreEqual(1, s.obedience);
//         Assert.AreEqual(0, s.analysis);  // первый раз — без Анализа
//         Assert.IsTrue(s.cocktailDrunk);
//         Assert.AreEqual(1, s.cocktailCount);
//     }

//     [Test] public void Cocktail_SecondDrink_ObedienceAndAnalysis()
//     {
//         var s = Make();
//         s.DrinkCocktail(); // первый
//         s.DrinkCocktail(); // второй — добавляет Анализ
//         Assert.AreEqual(2, s.obedience);
//         Assert.AreEqual(1, s.analysis);
//         Assert.AreEqual(2, s.cocktailCount);
//     }

//     [Test] public void Cocktail_Refuse_RevoltOnly()
//     {
//         var s = Make();
//         s.RefuseCocktail();
//         Assert.AreEqual(1, s.revolt);
//         Assert.IsFalse(s.cocktailDrunk);
//         Assert.AreEqual(0, s.cocktailCount);
//     }

//     // СБРОС

//     [Test] public void ResetAll_ClearsEverything()
//     {
//         var s = Make();
//         s.AddToken(TokenType.Revolt, 5);
//         s.DrinkCocktail();
//         s.currentSceneIndex = 4;
//         s.returnSceneName = "Scene3_Bar";

//         s.ResetAll();

//         Assert.AreEqual(0, s.revolt);
//         Assert.AreEqual(0, s.obedience);
//         Assert.AreEqual(0, s.analysis);
//         Assert.AreEqual(0, s.currentSceneIndex);
//         Assert.AreEqual("", s.returnSceneName);
//         Assert.IsFalse(s.cocktailDrunk);
//         Assert.AreEqual(0, s.cocktailCount);
//     }
// }