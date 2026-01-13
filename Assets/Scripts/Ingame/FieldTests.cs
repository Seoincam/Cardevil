using Cardevil.Core.Bootstrap;
using Cardevil.DebugConsole;
using Cardevil.Ingame.Entities;
using Cardevil.Ingame.Field;
using Cardevil.Pools;
using Cardevil.Utils;
using UnityEngine;
using UnityEngine.Scripting;

namespace Cardevil.Ingame
{
    public static class FieldTests
    {
        
        [ConsoleCommand("field.summon.rockpile"),Preserve]
        public static void SummonRockPile()
        {
            var field = Object.FindAnyObjectByType<Field.Field>();
            if (field == null)
            {
                Console.MessageError("Field not found in the scene.");
                return;
            }

            var pm = CardevilCore.Instance.Pool;
            var pile = pm.Get<RockPile>("RockPile");
            field.SummonEntityComponent(pile, TileVector.One);
        }
    }
}