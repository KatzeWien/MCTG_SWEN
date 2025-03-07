using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTrading.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Coins { get; set; }
        public int Elo { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public User(string username, string password, int coins, int elo, int wins, int losses, string bio, string image, string name)
        {
            Username = username;
            Password = password;
            Coins = coins;
            Elo = elo;
            Wins = wins;
            Losses = losses;
            Bio = bio;
            Image = image;
            Name = name;
        }
    }
}
