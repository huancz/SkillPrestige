using System.Collections.Generic;
using SkillPrestige.Bonuses;
using SkillPrestige.Logging;
using SkillPrestige.SkillTypes;

namespace SkillPrestige.Framework.Bonuses.TypeRegistration
{
    // ReSharper disable once UnusedMember.Global - created through reflection.
    internal sealed class FarmingBonusTypeRegistration : BonusTypeRegistration
    {
        public override void RegisterBonusTypes()
        {
            Logger.LogInformation("Registering Farming bonus types...");
            FarmingToolProficiency = new BonusType
            {
                Code = "Farming1",
                Name = "Tool Proficiency",
                MaxLevel = 5,
                EffectDescriptions = new List<string>
                {
                    "+4 Hoe Proficiency.",
                    "+4 Watering Can Proficiency."
                },
                SkillType = SkillType.Farming,
                ApplyEffect = x =>
                {
                    ToolProficiencyHandler.AddedToolProficencies[ToolType.Hoe] = x * 4;
                    ToolProficiencyHandler.AddedToolProficencies[ToolType.WateringCan] = x * 4;
                }
            };
            BetterCrops = new BonusType
            {
                Code = "Farming2",
                Name = "Better Crops",
                MaxLevel = 10,
                EffectDescriptions = new List<string>
                {
                    "+10% chance of better quality crop."
                },
                SkillType = SkillType.Farming,
                ApplyEffect = x => CropQualityFactor.QualityImprovementChance = x / 10m
            };
            EfficientAnimals = new BonusType
            {
                Code = "Farming3",
                Name = "Efficient Animals",
                MaxLevel = 5,
                EffectDescriptions = new List<string>
                {
                    "+20% chance of receiving double animal products."
                },
                SkillType = SkillType.Farming,
                ApplyEffect = x => AnimalProduceHandler.QuantityIncreaseChance = x / 5m
            };
            RegrowthOpportunity = new BonusType
            {
                Code = "Farming4",
                Name = "Regrowth Opportunity",
                MaxLevel = 5,
                EffectDescriptions = new List<string>
                {
                    "+5% chance of receiving seeds with crops.",
                    "At max level, gives a 1/3 chance of receiving seeds from dead plants."
                },
                SkillType = SkillType.Farming,
                ApplyEffect = x =>
                {
                    CropRegrowthFactor.RegrowthChance = x / 20m;
                    if (x == MaxLevel)
                    {
                        CropRegrowthFactor.DeadRegrowthChance = 1 / 3m;
                    }
                }
            };
            Logger.LogInformation("Farming bonus types registered.");
        }
    }
}
