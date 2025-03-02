using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonsterTrading
{
    public class Cards
    {
        public enum Elements
        {
            fire,
            water,
            normal
        }
        public enum CardTypes
        {
            monster,
            spell
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public double Damage { get; set; }
        public Elements Element { get; set; }
        public CardTypes CardType { get; set; }
        public Cards(string id, string name, double damage, Elements element, CardTypes cardType)
        {
            this.Id = id;
            this.Name = name;
            this.Damage = damage;
            this.Element = element;
            this.CardType = cardType;
        }
    }
}