using System.Reflection.PortableExecutable;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnetRpg.Dtos.Fight;

namespace dotnetRpg.Services.FightService
{
    public class FightService : IFightService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public FightService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<AttackResultDto>> WeaponAttack(WeaponAttackDto request)
        {
            var response = new ServiceResponse<AttackResultDto>();
            try
            {
                var attacker = await _context.Characters
                    .Include(c => c.Weapon)
                    .FirstOrDefaultAsync(c => c.Id == request.AttackerId);
                var opponent = await _context.Characters
                    .FirstOrDefaultAsync(c => c.Id == request.OpponentId);

                if (attacker == null || opponent == null || attacker.Weapon == null)
                    throw new Exception("Something fishy is going on!");

                int damage = DoWeaponAttack(attacker, opponent);

                if (opponent.HitPoints <= 0)
                    response.Message = $"{opponent.Name} has been defeated!";

                await _context.SaveChangesAsync();

                response.Data = new AttackResultDto
                {
                    Attacker = attacker.Name,
                    Oppenent = opponent.Name,
                    AttackerHP = attacker.HitPoints,
                    OppenentHP = opponent.HitPoints,
                    Damage = damage
                };



            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
              
            }

        return response;

        }

        private static int DoWeaponAttack(Character attacker, Character opponent)
        {
            if(attacker.Weapon is null)
               throw new Exception($"Attacker: '{attacker.Name}' has not weapon!");

            // totally made up algorithym for attacking and Damage
            int damage = attacker.Weapon.Damage + (new Random().Next(attacker.Strength));
            damage -= new Random().Next(opponent.Defeats);

            if (damage > 0)
                opponent.HitPoints -= damage;
            return damage;
        }

        public async Task<ServiceResponse<AttackResultDto>> SkillAttack(SkillAttackDto request)
        {
            var response = new ServiceResponse<AttackResultDto>();
            try
            {
                var attacker = await _context.Characters
                    .Include(c => c.Skills)
                    .FirstOrDefaultAsync(c => c.Id == request.AttackerId);
                var opponent = await _context.Characters
                    .FirstOrDefaultAsync(c => c.Id == request.OpponentId);

                if (attacker == null || opponent == null || attacker.Skills == null)
                    throw new Exception("Something fishy is going on!");


                var skill = attacker.Skills.FirstOrDefault(s => s.Id == request.SkillId);
                if (skill is null)
                {
                    response.Success = false;
                    response.Message = $"{attacker.Name} doesn't know that skill!";
                    return response;
                }
                // totally made up algorithym for attacking and Damage
                int damage = DoSkillAttack(attacker, opponent, skill);

                if (opponent.HitPoints <= 0)
                    response.Message = $"{opponent.Name} has been defeated!";

                await _context.SaveChangesAsync();

                response.Data = new AttackResultDto
                {
                    Attacker = attacker.Name,
                    Oppenent = opponent.Name,
                    AttackerHP = attacker.HitPoints,
                    OppenentHP = opponent.HitPoints,
                    Damage = damage
                };



            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
              
            }

        return response;

        }

        private static int DoSkillAttack(Character attacker, Character opponent, Skill skill)
        {
            int damage = skill.Damage + (new Random().Next(attacker.Intelligence));
            damage -= new Random().Next(opponent.Defeats);

            if (damage > 0)
                opponent.HitPoints -= damage;
            return damage;
        }

        public async Task<ServiceResponse<FightResultDto>> Fight(FightRequestDto request)
        {
            var response = new ServiceResponse<FightResultDto>
            {
                Data = new FightResultDto()
            };
            try 
            {
                var characters = await _context.Characters
                    .Include(c => c.Weapon)
                    .Include(c => c.Skills)
                    .Where(c => request.CharacterIds.Contains(c.Id))
                    .ToListAsync();

                    bool defeated = false;
                    // while loop and for each
                    while(!defeated)
                    {
                        foreach(var attacker in characters)
                        {
                            var opponents = characters.Where(c => c.Id != attacker.Id).ToList();
                            var opponent = opponents[new Random().Next(opponents.Count)];

                            int damage = 0;
                            string attackUsed = string.Empty;

                            bool useWeapon = new Random().Next(2) == 0; // Next(2) means the result is 0 or 1
                            if(useWeapon && attacker.Weapon is not null)
                            {
                                attackUsed = attacker.Weapon.Name;
                                damage = DoWeaponAttack(attacker, opponent);

                            }
                            else if(!useWeapon && attacker.Skills is not null)
                            {
                                var skill = attacker.Skills[new Random().Next(attacker.Skills.Count())];
                                attackUsed = skill.Name;
                                damage = DoSkillAttack(attacker, opponent, skill);

                            } 
                            else 
                            {
                                response.Data.Log
                                    .Add($"{attacker.Name} wasn't able to attack!");
                                continue;
                            }

                            response.Data.Log
                                .Add($"{attacker.Name} attacks {opponent.Name} using {attackUsed} with {(damage >= 0 ? damage : 0)} damage.");
                            if(opponent.HitPoints <= 0 )
                            {
                                defeated = true;
                                attacker.Victories++;
                                opponent.Defeats++;
                                response.Data.Log.Add($"{opponent.Name} has been defeated!");
                                 response.Data.Log.Add($"{attacker.Name} wins with {attacker.HitPoints} HP left!");      
                                 break; // break the for loop                         
                            }

                        }
                    }

                    characters.ForEach(c =>
                       {
                        c.Fights++;
                        c.HitPoints = 100;
                       });

                       await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;

        }

        public async Task<ServiceResponse<List<HighScoreDto>>> GetHighScore()
        {
            var characters = await _context.Characters
                .Where(c => c.Fights > 0)
                .OrderByDescending(c => c.Victories)
                .ThenBy(c => c.Defeats)
                .ToListAsync();

            var response = new ServiceResponse<List<HighScoreDto>> ()
            {   
                
                Data = characters.Select(c => _mapper.Map<HighScoreDto>(c)).ToList()

            };
            
            
            return response;
        }
    }
    
}