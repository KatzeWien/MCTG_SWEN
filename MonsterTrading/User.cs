using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTrading
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Coins { get; set; }
        public int Elo {  get; set; }
        public int Wins {  get; set; }
        public int Losses {  get; set; }
        public string Bio {  get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public User(string username, string password, int coins, int elo, int wins, int losses, string bio, string image, string name) 
        {
            this.Username = username;
            this.Password = password;
            this.Coins = coins;
            this.Elo = elo;
            this.Wins = wins;
            this.Losses = losses;
            this.Bio = bio;
            this.Image = image;
            this.Name = name;
        }
    }
}
