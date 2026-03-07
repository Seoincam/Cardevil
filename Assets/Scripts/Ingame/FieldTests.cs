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

            var pm = CardevilCore.Pool;
            var pile = pm.Get<RockPile>("RockPile");
            pile.Init(3);
            field.SummonEntityComponent(pile, TileVector.One);
        }
    }
}