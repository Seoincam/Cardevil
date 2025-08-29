using UnityEngine;

namespace Cardevil.Dungeon.DungeonFactories
{
    public class DungeonFactoryChapter2 : DungeonFactory 
    {
        public override Dungeon Create(int dungeonId, DungeonConfigurationSO dungeonConfiguration)
        {
            DungeonBuilder builder = new DungeonBuilder(dungeonId, dungeonConfiguration);
            Debug.Log($"Creating Dungeon with ID: {dungeonId}");

            builder
                .AddNode(DungeonNodeTypes.Mob, null)
                .AddNode(DungeonNodeTypes.Mob, null)
                .AddNode(DungeonNodeTypes.Mob, null)
                .AddNode(DungeonNodeTypes.Heal, null)
                .AddNode(DungeonNodeTypes.Reinforce, null)
                .AddNode(DungeonNodeTypes.Mob, null)
                .AddNode(DungeonNodeTypes.Mob, null);

            /*
             * 첫 분기
             */
            builder
                .BranchStart()
                .AddNode(DungeonNodeTypes.Heal, null)
                .AddNode(DungeonNodeTypes.Random, null)
                .BranchEnd();
            builder
                .BranchStart()
                .AddNode(DungeonNodeTypes.Mob, null)
                .AddNode(DungeonNodeTypes.Devil, null)
                .BranchEnd();
            builder
                .AddNodeAndMergeBranch(DungeonNodeTypes.MiniBoss, null);
            
            /*
             * 두번째 분기
             */
            builder
                .BranchStart()
                .AddNode(DungeonNodeTypes.Heal, null)
                .BranchEnd();
            builder
                .BranchStart()
                .AddNode(DungeonNodeTypes.Reinforce, null)
                .BranchEnd();
            builder.AddNodeAndMergeBranch(DungeonNodeTypes.Mob, null);
            /*
             * 마지막 보스까지
             */
            builder
                .AddNode(DungeonNodeTypes.Mob, null)
                .AddNode(DungeonNodeTypes.Heal, null)
                .AddNode(DungeonNodeTypes.FinalBoss, null);
            Dungeon result = builder.Build();
            Debug.Log($"Dungeon with ID: {dungeonId} created successfully.");
            Debug.Log(result.GetDebugString());
            return result;
        }
    }
}