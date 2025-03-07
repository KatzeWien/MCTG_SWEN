using MonsterTrading.DB;
using MonsterTrading.Models;
using MonsterTrading.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonsterTrading.BuisnessLogic
{
    public class Battles
    {
        private PackagesAndCardsDB packagesAndCardsDB;
        private ServerResponse response;
        private UserDB userDB;
        public Battles()
        {
            packagesAndCardsDB = new PackagesAndCardsDB();
            response = new ServerResponse();
            userDB = new UserDB();
        }
        public async Task StartBattle(string user1, string user2, StreamWriter writer)
        {
            if (user1 != null && user2 != null)
            {
                List<Cards> player1 = await packagesAndCardsDB.PickRandomCard(user1);
                List<Cards> player2 = await packagesAndCardsDB.PickRandomCard(user2);
                Cards player1card = player1[0];
                Cards player2card = player2[0];
                string winner = CompareCards(player1card, player2card, user1, user2);
                if (winner == user1)
                {
                    await response.WriteResponse(writer, 201, $"Winner: {winner} with {player1card.Id} vs {player2card.Id}");
                    await userDB.WinnerOfBattle(user1, player2card);
                    await userDB.LosserOfBattle(user2, player2card);
                }
                else
                {
                    await response.WriteResponse(writer, 201, $"Winner: {winner} with {player2card.Id} vs {player1card.Id}");
                    await userDB.WinnerOfBattle(user2, player1card);
                    await userDB.LosserOfBattle(user1, player1card);
                }

            }
            else
            {
                await response.WriteResponse(writer, 409, "users not found");
            }
        }

        public string CompareCards(Cards player1, Cards player2, string user1, string user2)
        {
            if (player1.Name.Contains("Kraken") && player2.CardType == Cards.CardTypes.spell)
            {
                return user1;
            }
            else if (player2.Name.Contains("Kraken") && player1.CardType == Cards.CardTypes.spell)
            {
                return user2;
            }
            else if (player1.Name.Contains("Knight") && player2.Element == Cards.Elements.water)
            {
                return user2;
            }
            else if (player2.Name.Contains("Knight") && player1.Element == Cards.Elements.water)
            {
                return user1;
            }
            else if (player1.CardType == Cards.CardTypes.monster && player2.CardType == Cards.CardTypes.monster)
            {
                return MonsterFight(player1, player2, user1, user2);
            }
            else
            {
                return RegularFight(player1, player2, user1, user2);
            }
        }

        public string MonsterFight(Cards player1, Cards player2, string user1, string user2)
        {
            if (player1.Name.Contains("Goblin") && player2.Name.Contains("Dragon"))
            {
                return user2;
            }
            else if (player2.Name.Contains("Goblin") && player1.Name.Contains("Dragon"))
            {
                return user1;
            }
            else if (player1.Name.Contains("Wizard") && player2.Name.Contains("Ork"))
            {
                return user1;
            }
            else if (player2.Name.Contains("Wizard") && player1.Name.Contains("Ork"))
            {
                return user2;
            }
            else if (player1.Name.Contains("FireElf") && player2.Name.Contains("Dragon"))
            {
                return user1;
            }
            else if (player2.Name.Contains("FireElf") && player1.Name.Contains("Dragon"))
            {
                return user2;
            }
            else
            {
                if (player1.Damage > player2.Damage)
                {
                    return user1;
                }
                else
                {
                    return user2;
                }
            }
        }

        public string RegularFight(Cards player1, Cards player2, string user1, string user2)
        {
            if (player1.Element == Cards.Elements.fire && player2.Element == Cards.Elements.water)
            {
                player1.Damage = player1.Damage / 2;
                player2.Damage = player2.Damage * 2;
            }
            else if (player2.Element == Cards.Elements.fire && player1.Element == Cards.Elements.water)
            {
                player2.Damage = player2.Damage / 2;
                player1.Damage = player1.Damage * 2;
            }
            else if (player1.Element == Cards.Elements.normal && player2.Element == Cards.Elements.fire)
            {
                player1.Damage = player1.Damage / 2;
                player2.Damage = player2.Damage * 2;
            }
            else if (player2.Element == Cards.Elements.normal && player1.Element == Cards.Elements.fire)
            {
                player2.Damage = player2.Damage / 2;
                player1.Damage = player1.Damage * 2;
            }
            else if (player1.Element == Cards.Elements.water && player2.Element == Cards.Elements.normal)
            {
                player1.Damage = player1.Damage / 2;
                player2.Damage = player2.Damage * 2;
            }
            else if (player2.Element == Cards.Elements.water && player1.Element == Cards.Elements.normal)
            {
                player2.Damage = player2.Damage / 2;
                player1.Damage = player1.Damage * 2;
            }
            if (player1.Damage > player2.Damage)
            {
                return user1;
            }
            else
            {
                return user2;
            }
        }
    }
}