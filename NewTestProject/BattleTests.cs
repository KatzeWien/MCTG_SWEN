using MonsterTrading.BuisnessLogic;
using MonsterTrading.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewTestProject
{
    internal class BattleTests
    {
        private Battles battle;
        [SetUp]
        public void Setup()
        {
            battle = new Battles();
        }
        [Test]
        public void CompareMonsterCards()
        {
            string user1 = "player1";
            string user2 = "player2";
            Cards user1card = new Cards("123456", "WaterGoblin", 20, Cards.Elements.water, Cards.CardTypes.monster);
            Cards user2card = new Cards("234567", "Dragon", 30, Cards.Elements.normal, Cards.CardTypes.monster);
            string winner = this.battle.MonsterFight(user1card, user2card, user1, user2);
            Assert.AreEqual(winner, user2);
        }

        [Test]
        public void CompareSpellCards()
        {
            string user1 = "player1";
            string user2 = "player2";
            Cards user1card = new Cards("123456", "Waterspell", 20, Cards.Elements.water, Cards.CardTypes.spell);
            Cards user2card = new Cards("234567", "Dragon", 15, Cards.Elements.normal, Cards.CardTypes.monster);
            string winner = this.battle.RegularFight(user1card, user2card, user1, user2);
            Assert.AreEqual(winner, user2);
        }
    }
}
