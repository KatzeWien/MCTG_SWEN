using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTrading.Models
{
    internal class Trades
    {
        public string Id { get; set; }
        public string CardToTrade { get; set; }
        public string Type {  get; set; }
        public double MinimumDamage {  get; set; }
        public Trades(string id, string cardToTrade, string type, double minimumDamage)
        {
            this.Id = id;
            this.CardToTrade = cardToTrade;
            this.Type = type;
            this.MinimumDamage = minimumDamage;
        }
    }
}
