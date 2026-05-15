using Cardevil.Core.Utils;
using Cardevil.Gameplay.Enemy.Gimmick;
using Cardevil.Test.DebugConsole;
using Cardevil.Test.DebugConsole.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace Cardevil.Gameplay.Enemy
{
    [ConsoleCommandClass]
    public class EnemyGimmikTestCommand : IConsoleCommand
    {
        public string Command => "enemy.gimmick";
        public string Description => "적의 기믹을 테스트합니다.";
        public string Signature => "enemy.gimmick <gimmickName>";

        public void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                LogEx.LogWarning("사용법: enemy.gimmick <gimmickName>");
                return;
            }

            string gimmickName = args[0];
            
            // 활성 Enemy 찾기 (예: Scene에서 활성화된 첫 번째 Enemy)
            Enemy targetEnemy = Object.FindAnyObjectByType<Enemy>();
            if (targetEnemy == null)
            {
                LogEx.LogError("활성화된 Enemy를 찾을 수 없습니다.");
                return;
            }

            // GimmickFactory를 통해 기믹 생성
            IGimmick gimmick = GimmickFactory.Instance.CreateGimmick(gimmickName);
            if (gimmick == null)
            {
                LogEx.LogError($"'{gimmickName}' 기믹을 찾을 수 없습니다.");
                return;
            }

            // Enemy에 기믹 적용
            gimmick.Apply(targetEnemy);
            LogEx.Log($"Enemy에 '{gimmickName}' 기믹이 적용되었습니다.");
        }

        void AutoComplete(Span<string> args, ref List<string> suggestions)
        {
            // 첫 번째 인자: 기믹 이름
            if (args.Length == 1)
            {
                // GimmickFactory의 등록된 모든 기믹 이름 가져오기
                var gimmickTypes = typeof(IGimmick).Assembly.GetTypes()
                    .Where(type =>
                        typeof(IGimmick).IsAssignableFrom(type) &&
                        !type.IsInterface &&
                        !type.IsAbstract
                    )
                    .Select(type => type.Name)
                    .OrderBy(name => name)
                    .ToList();

                // 입력값과 일치하는 기믹 이름만 제안
                string currentArg = args[0];
                foreach (var gimmickName in gimmickTypes)
                {
                    if (gimmickName.StartsWith(currentArg, System.StringComparison.OrdinalIgnoreCase))
                    {
                        suggestions.Add(gimmickName);
                    }
                }
            }
        }
    }
}