using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetRpg.Dtos.Fight
{
    public class FightRequestDto
    {
        public List<int> CharacterIds { get; set; } = new List<int>();
    }
}