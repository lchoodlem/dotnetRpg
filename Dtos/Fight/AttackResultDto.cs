using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetRpg.Dtos.Fight
{
    public class AttackResultDto
    {
        public string Attacker { get; set; } = string.Empty;
        public string Oppenent { get; set; }= string.Empty;
        public int AttackerHP { get; set; }
        public int OppenentHP { get; set; }
        public int Damage { get; set; }
    }
}