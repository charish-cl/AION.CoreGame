using System.Collections.Generic;

namespace AION.Config.DamageCalculator
{
    public interface IDamageCalculator
    {
        public void Damager(Unit attacker, Unit target);
        
        public void AoeDamage(Unit attacker, List<Unit> target);
    }
}