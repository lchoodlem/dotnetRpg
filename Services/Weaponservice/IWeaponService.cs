using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnetRpg.Dtos.Weapon;

namespace dotnetRpg.Services.Weaponservice
{
    public interface IWeaponService
    {
        Task<ServiceResponse<GetCharacterDto>> AddWeapon(AddWeaponDto newWeapon);
    }
}