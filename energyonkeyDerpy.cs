using MelonLoader;
using BTD_Mod_Helper;
using energyonkeyDerpy;
using Il2CppAssets.Scripts.Models.Towers;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;
using Il2CppAssets.Scripts.Unity;
using BTD_Mod_Helper.Api.Display;
using Il2CppAssets.Scripts.Unity.Display;
using BTD_Mod_Helper.Api.Towers;
using Il2CppAssets.Scripts.Models.TowerSets;
using Il2Cpp;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppSystem.IO;
using Il2CppAssets.Scripts.Utils;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack;
using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Models.Towers.Weapons;

[assembly: MelonInfo(typeof(EnergyonkeyDerpy), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace energyonkeyDerpy;

public class EnergyonkeyDerpy : BloonsTD6Mod
{
    public override void OnApplicationStart()
    {
        ModHelper.Msg<EnergyonkeyDerpy>("energyonkeyDerpy loaded!");
    }
}
public class EnergyMonkey : ModTower
{
    public override string Name => "EnergyMonkey";
    public override TowerSet TowerSet => TowerSet.Magic;
    public override string BaseTower => "SuperMonkey-100";
    public override int Cost => 2000;
    public override string Description => "Base Stats: 2 damage, 2 pierce, Super Monkey range, half Super Monkey attack speed, no purple popping, lead popping, shoots lasers.";
    public override string DisplayName => "Energy Monkey";
    public override int TopPathUpgrades => 2;
    public override int MiddlePathUpgrades => 2;
    public override int BottomPathUpgrades => 5;
    //public override ParagonMode ParagonMode => ParagonMode.Base555;
    public override void ModifyBaseTowerModel(TowerModel towerModel)
    {

        //towerModel.GetBehavior<DisplayModel>().display = towerModel.display;
        towerModel.ApplyDisplay<Display000>();

        var attackModel = towerModel.GetAttackModel();
        var projectile = attackModel.weapons[0].projectile;

        projectile.GetDamageModel().damage = 2;
        projectile.pierce = 2;
        attackModel.weapons[0].rate *= 2f; //half attack speed
        attackModel.weapons[0].Rate *= 2f;

        towerModel.footprint = Game.instance.model.GetTowerFromId(TowerType.WizardMonkey).Duplicate().footprint;

        towerModel.radius = Game.instance.model.GetTowerFromId(TowerType.WizardMonkey).Duplicate().radius;
        towerModel.radiusSquared = Game.instance.model.GetTowerFromId(TowerType.WizardMonkey).Duplicate().radiusSquared;

        foreach (var damageModel in towerModel.GetDescendants<DamageModel>().ToArray())
        {
            damageModel.immuneBloonProperties &= ~BloonProperties.Lead; //add lead popping
        }

    }

    public override bool IsValidCrosspath(int[] tiers) => ModHelper.HasMod("Ultimate Crosspathing") || base.IsValidCrosspath(tiers);
}
public class Display000 : ModDisplay
{
    public override string BaseDisplay => Game.instance.model.GetTower("DartMonkey", 0, 0, 0).display.GUID;
    public override float Scale => 1.1f;
    public override void ModifyDisplayNode(UnityDisplayNode node)
    {
        node.SaveMeshTexture(0);
        node.SaveMeshTexture(1);
        node.SaveMeshTexture(2);
        SetMeshTexture(node, "display_000", 0);
        //SetMeshTexture(node, "display_000", 1); //not used
        SetMeshTexture(node, "display_yay!", 2);
        node.RemoveBone("DartMonkeyDart"); //Remove (hide) Dart

        //node.Scale *= 1.2f;
        //#if DEBUG
        node.PrintInfo();
        //#endif
    }
}
public class TimedExplosions : ModUpgrade<EnergyMonkey>
{
    public override int Path => TOP;
    public override int Tier => 1;
    public override int Cost => 9500; //Same as Spiked Mines
    public override string Icon => VanillaSprites.BiggerBombsUpgradeIcon;
    public override string Description => "Projectiles explode on expiration";
    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attackModel = towerModel.GetAttackModel();
        var projectile = attackModel.weapons[0].projectile;
        var alch = Game.instance.model.GetTowerFromId("Alchemist").Duplicate().GetWeapon().projectile;
        var bomb = Game.instance.model.GetTowerFromId("BombShooter").Duplicate().GetWeapon().projectile;
        projectile.AddBehavior(alch.GetBehavior<CreateProjectileOnExhaustFractionModel>());
        //projectile.AddBehavior(balls.GetBehavior<CreateSoundOnExhaustFractionModel>().Duplicate());
        projectile.AddBehavior(alch.GetBehavior<CreateEffectOnExhaustFractionModel>().Duplicate());
        projectile.GetBehavior<CreateEffectOnExhaustFractionModel>().effectModel = bomb.GetBehavior<CreateEffectOnContactModel>().effectModel;
        projectile.GetBehavior<CreateProjectileOnExhaustFractionModel>().projectile = bomb;
        projectile.GetBehavior<CreateProjectileOnExhaustFractionModel>().projectile.pierce = 50;
    }
}
public class InstantExplosions : ModUpgrade<EnergyMonkey>
{
    public override int Path => TOP;
    public override int Tier => 2;
    public override int Cost => 10200; //2x Hydra Rocket Pods
    public override string Icon => VanillaSprites.HydraRocketsUpgradeIcon;
    public override string Description => "Projectiles explode on expiration";
    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attackModel = towerModel.GetAttackModel();
        var projectile = attackModel.weapons[0].projectile;
        towerModel.RemoveBehavior<CreateProjectileOnContactModel>();
        towerModel.RemoveBehavior<CreateEffectOnExhaustFractionModel>();
        projectile.AddBehavior(Game.instance.model.GetTowerFromId("BombShooter").GetWeapon().projectile.GetBehavior<CreateProjectileOnContactModel>().Duplicate());
        projectile.AddBehavior(Game.instance.model.GetTowerFromId("BombShooter").GetWeapon().projectile.GetBehavior<CreateEffectOnContactModel>().Duplicate());
        projectile.GetBehavior<CreateProjectileOnContactModel>().projectile.pierce = 100;
    }
}

public class PlasmaBlasts : ModUpgrade<EnergyMonkey>
{
    public override int Path => MIDDLE;
    public override int Tier => 1;
    public override int Cost => 3000; //Same as Spiked Mines
    public override string Icon => VanillaSprites.PlasmaBlastUpgradeIcon;
    public override string Description => "Projectiles explode on expiration";
    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attackModel = towerModel.GetAttackModel();
        var projectile = attackModel.weapons[0].projectile;
        projectile.display = Game.instance.model.GetTowerFromId("SuperMonkey-200").GetWeapon().projectile.display;
        foreach (var projectileModel in towerModel.GetDescendants<ProjectileModel>().ToArray())
        {
            foreach (var damageModel in projectileModel.GetDescendants<DamageModel>().ToArray())
            {
                damageModel.damage++;
            }
            projectileModel.pierce++;
        }
    }
}
public class AcidEnergy : ModUpgrade<EnergyMonkey>
{
    public override int Path => MIDDLE;
    public override int Tier => 2;
    public override int Cost => 4400; //2x Embrittlement
    public override string Icon => VanillaSprites.PlasmaBlastUpgradeIcon;
    public override string Description => "Projectiles explode on expiration";
    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attackModel = towerModel.GetAttackModel();
        var projectile = attackModel.weapons[0].projectile;
        projectile.display = Game.instance.model.GetTowerFromId("SuperMonkey-040").GetWeapon().projectile.display;
        foreach (var projectileModel in towerModel.GetDescendants<ProjectileModel>().ToArray())
        {
            projectileModel.AddBehavior(Game.instance.model.GetTowerFromId("IceMonkey-400").GetDescendant<AddBonusDamagePerHitToBloonModel>());
            //projectileModel.GetBehavior<AddBonusDamagePerHitToBloonModel>().;
        }
    }
}
public class TwinBlasts : ModUpgrade<EnergyMonkey>
{
    public override int Path => BOTTOM;
    public override int Tier => 1;
    public override int Cost => 2550; //Same as Spiked Mines
    public override string Icon => VanillaSprites.DoubleShotUpgradeIcon;
    public override string Description => "Projectiles explode on expiration";
    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attackModel = towerModel.GetAttackModel();
        var projectile = attackModel.weapons[0].projectile;
        attackModel.AddWeapon(towerModel.GetWeapons()[0].Duplicate());
        attackModel.weapons[1].emission = new RandomEmissionModel("RandomEmissionModel_", 1, 45, 0, null, false, 1, 1, 0, true);
    }
}
public class SeekingBeams : ModUpgrade<EnergyMonkey>
{
    public override int Path => BOTTOM;
    public override int Tier => 2;
    public override int Cost => 4400; //2x Embrittlement
    public override string Icon => VanillaSprites.SeekingShurikenUpgradeIcon;
    public override string Description => "Projectiles seek out Bloons";
    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attackModel = towerModel.GetAttackModel();
        var projectile = attackModel.weapons[0].projectile;
        var sub = Game.instance.model.GetTowerFromId("MonkeySub").Duplicate().GetWeapon().projectile;
        towerModel.IncreaseRange(towerModel.range * 0.3f);

        foreach (var projectileModel in towerModel.GetDescendants<ProjectileModel>().ToArray())
        {
            projectileModel.AddBehavior(sub.GetBehavior<TrackTargetModel>());
        }
    }
}

public class ScatterShot : ModUpgrade<EnergyMonkey>
{
    public override int Path => BOTTOM;
    public override int Tier => 3;
    public override int Cost => 8800; //2x Embrittlement
    public override string Icon => VanillaSprites.SeekingShurikenUpgradeIcon;
    public override string Description => "Projectiles seek out Bloons";
    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attackModel = towerModel.GetAttackModel();
        var projectile = attackModel.weapons[0].projectile;
        attackModel.RemoveWeapon(attackModel.weapons[0]); // Remove the non-random projectile's weapon
        foreach (var weaponModel in towerModel.GetWeapons().ToArray())
        {
            weaponModel.Rate *= 1/1.5f; // x1.5
            weaponModel.rate *= 1/1.5f;
        }
        towerModel.IncreaseRange(towerModel.range / 1.3f * 0.7f);
        attackModel.weapons[0].emission = new RandomEmissionModel("RandomEmissionModel_", 5, 75, 0, null, false, 1, 1, 0, true);
    }
}
public class PainBeams : ModUpgrade<EnergyMonkey>
{
    public override int Path => BOTTOM;
    public override int Tier => 4;
    public override int Cost => 17600; //2x Embrittlement
    public override string Icon => VanillaSprites.EmoteAdoraVoidora;
    public override string Description => "Projectiles seek out Bloons";
    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attackModel = towerModel.GetAttackModel();
        var projectile = attackModel.weapons[0].projectile;
        //projectile.display.guidRef = "ec02a14e6de760f45b5ce1212d49f797"; //Uses Vengeful Adora's base projectile's display, cause it looks cool. //This dosen't work
        foreach (var weaponModel in towerModel.GetWeapons().ToArray()) //NOTE: how to use Voidora's projectile displays?
        {
            weaponModel.Rate *= 1.5f / 2f; // +0.33%.. attack speed
            weaponModel.rate *= 1.5f / 2f; // 
        }
        foreach (var projectileModel in towerModel.GetDescendants<ProjectileModel>().ToArray())
        {
            foreach (var damageModel in projectileModel.GetDescendants<DamageModel>().ToArray())
            {
                damageModel.damage += 5;
            }
            projectileModel.pierce += 3;
        }
        attackModel.weapons[0].emission = new RandomEmissionModel("RandomEmissionModel_", 9, 90, 0, null, false, 1, 1, 0, true);
    }
}
public class Reaper : ModUpgrade<EnergyMonkey>
{
    public override int Path => BOTTOM;
    public override int Tier => 5;
    public override int Cost => 60000; //2x Embrittlement
    public override string Icon => VanillaSprites.DartMonkey000;
    public override string Description => "Projectiles seek out Bloons";
    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attackModel = towerModel.GetAttackModel();
        var projectile = attackModel.weapons[0].projectile;
        foreach (var weaponModel in towerModel.GetWeapons().ToArray())
        {
            weaponModel.Rate *= 2f / 4f; 
            weaponModel.rate *= 2f / 4f; 
        }
        foreach (var projectileModel in towerModel.GetDescendants<ProjectileModel>().ToArray())
        {
            foreach (var damageModel in projectileModel.GetDescendants<DamageModel>().ToArray())
            {
                damageModel.damage += 8;
            }
        }
        attackModel.weapons[0].emission = new RandomEmissionModel("RandomEmissionModel_", 15, 70, 0, null, false, 1, 1, 0, true);
    }
}