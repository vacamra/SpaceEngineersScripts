using System.Linq;
using Sandbox.ModAPI.Ingame;
using VRage;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;
using static SpaceEngineers.Game.VoiceChat.OpusDevice;

namespace SpaceEngineersScripts.CraftingQueue
{
    public class Program: MyGridProgram
    {

        /*
            This script will render the full crafting queue onto target screen. 
        */
        // Configuration
        string displayName = "LCD Panel";
        int displayId = 0;

        // Script
        Dictionary<string, string> internalToDisplay = new Dictionary<string, string>()
        {
            { "MyObjectBuilder_BlueprintDefinition/Position0120_LargeCalibreAmmo", "Artillery Shell" },
            { "MyObjectBuilder_BlueprintDefinition/Position0110_MediumCalibreAmmo", "Assault Cannon Shell" },
            { "MyObjectBuilder_BlueprintDefinition/Position0090_AutocannonClip", "Autocannon Magazine" },
            { "MyObjectBuilder_BlueprintDefinition/BulletproofGlass", "Bulletproof Glass" },
            { "MyObjectBuilder_BlueprintDefinition/Position0030_Canvas", "Canvas" },
            { "MyObjectBuilder_BlueprintDefinition/CobaltOreToIngot", "Cobalt Ingot" },
            { "MyObjectBuilder_BlueprintDefinition/ComputerComponent", "Computer" },
            { "MyObjectBuilder_BlueprintDefinition/ConstructionComponent", "Construction Comp." },
            { "MyObjectBuilder_BlueprintDefinition/Position0040_Datapad", "Datapad" },
            { "MyObjectBuilder_BlueprintDefinition/DetectorComponent", "Detector Comp." },
            { "MyObjectBuilder_BlueprintDefinition/Display", "Display" },
            { "MyObjectBuilder_BlueprintDefinition/EngineerPlushie", "DisplayName_Item_EngineerPlushie" },
            { "MyObjectBuilder_BlueprintDefinition/SabiroidPlushie", "DisplayName_Item_SabiroidPlushie" },
            { "MyObjectBuilder_BlueprintDefinition/Position0040_AngleGrinder4", "Elite Grinder" },
            { "MyObjectBuilder_BlueprintDefinition/Position0080_HandDrill4", "Elite Hand Drill" },
            { "MyObjectBuilder_BlueprintDefinition/Position0120_Welder4", "Elite Welder" },
            { "MyObjectBuilder_BlueprintDefinition/Position0020_AngleGrinder2", "Enhanced Grinder" },
            { "MyObjectBuilder_BlueprintDefinition/Position0060_HandDrill2", "Enhanced Hand Drill" },
            { "MyObjectBuilder_BlueprintDefinition/Position0100_Welder2", "Enhanced Welder" },
            { "MyObjectBuilder_BlueprintDefinition/ExplosivesComponent", "Explosives" },
            { "MyObjectBuilder_BlueprintDefinition/Position0007_FireworksBoxBlue", "Fireworks Blue" },
            { "MyObjectBuilder_BlueprintDefinition/Position00071_FireworksBoxGreen", "Fireworks Green" },
            { "MyObjectBuilder_BlueprintDefinition/Position00074_FireworksBoxPink", "Fireworks Pink" },
            { "MyObjectBuilder_BlueprintDefinition/Position00075_FireworksBoxRainbow", "Fireworks Rainbow" },
            { "MyObjectBuilder_BlueprintDefinition/Position00072_FireworksBoxRed", "Fireworks Red" },
            { "MyObjectBuilder_BlueprintDefinition/Position00073_FireworksBoxYellow", "Fireworks Yellow" },
            { "MyObjectBuilder_BlueprintDefinition/Position0005_FlareGun", "Flare Gun" },
            { "MyObjectBuilder_BlueprintDefinition/Position0005_FlareGunMagazine", "Flare Gun Clip" },
            { "MyObjectBuilder_BlueprintDefinition/Position0080_NATO_25x184mmMagazine", "Gatling Ammo Box" },
            { "MyObjectBuilder_BlueprintDefinition/GirderComponent", "Girder" },
            { "MyObjectBuilder_BlueprintDefinition/GoldOreToIngot", "Gold Ingot" },
            { "MyObjectBuilder_BlueprintDefinition/StoneOreToIngot_Deconstruction", "Gravel" },
            { "MyObjectBuilder_BlueprintDefinition/GravityGeneratorComponent", "Gravity Comp." },
            { "MyObjectBuilder_BlueprintDefinition/Position0010_AngleGrinder", "Grinder" },
            { "MyObjectBuilder_BlueprintDefinition/Position0050_HandDrill", "Hand Drill" },
            { "MyObjectBuilder_BlueprintDefinition/Position0020_HydrogenBottle", "Hydrogen Bottle" },
            { "MyObjectBuilder_BlueprintDefinition/HydrogenBottlesRefill", "Hydrogen Bottles" },
            { "MyObjectBuilder_BlueprintDefinition/StoneOreToIngot", "Ingots" },
            { "MyObjectBuilder_BlueprintDefinition/Position0010_StoneOreToIngotBasic", "Ingots" },
            { "MyObjectBuilder_BlueprintDefinition/InteriorPlate", "Interior Plate" },
            { "MyObjectBuilder_BlueprintDefinition/IronOreToIngot", "Iron Ingot" },
            { "MyObjectBuilder_BlueprintDefinition/ScrapIngotToIronIngot", "Iron Ingot" },
            { "MyObjectBuilder_BlueprintDefinition/ScrapToIronIngot", "Iron Ingot" },
            { "MyObjectBuilder_BlueprintDefinition/Position0140_LargeRailgunAmmo", "Large Railgun Sabot" },
            { "MyObjectBuilder_BlueprintDefinition/LargeTube", "Large Steel Tube" },
            { "MyObjectBuilder_BlueprintDefinition/MagnesiumOreToIngot", "Magnesium Powder" },
            { "MyObjectBuilder_BlueprintDefinition/MedicalComponent", "Medical Comp." },
            { "MyObjectBuilder_BlueprintDefinition/MetalGrid", "Metal Grid" },
            { "MyObjectBuilder_BlueprintDefinition/MotorComponent", "Motor" },
            { "MyObjectBuilder_BlueprintDefinition/Position0040_AutomaticRifle", "MR-20 Rifle" },
            { "MyObjectBuilder_BlueprintDefinition/Position0040_AutomaticRifleGun_Mag_20rd", "MR-20 Rifle Magazine" },
            { "MyObjectBuilder_BlueprintDefinition/Position0070_UltimateAutomaticRifle", "MR-30E Rifle" },
            { "MyObjectBuilder_BlueprintDefinition/Position0070_UltimateAutomaticRifleGun_Mag_30rd", "MR-30E Rifle Magazine" },
            { "MyObjectBuilder_BlueprintDefinition/Position0050_RapidFireAutomaticRifle", "MR-50A Rifle" },
            { "MyObjectBuilder_BlueprintDefinition/Position0050_RapidFireAutomaticRifleGun_Mag_50rd", "MR-50A Rifle Magazine" },
            { "MyObjectBuilder_BlueprintDefinition/Position0060_PreciseAutomaticRifle", "MR-8P Rifle" },
            { "MyObjectBuilder_BlueprintDefinition/Position0060_PreciseAutomaticRifleGun_Mag_5rd", "MR-8P Rifle Magazine" },
            { "MyObjectBuilder_BlueprintDefinition/NickelOreToIngot", "Nickel Ingot" },
            { "MyObjectBuilder_BlueprintDefinition/IceToOxygen", "Oxygen" },
            { "MyObjectBuilder_BlueprintDefinition/Position0010_OxygenBottle", "Oxygen Bottle" },
            { "MyObjectBuilder_BlueprintDefinition/OxygenBottlesRefill", "Oxygen Bottles" },
            { "MyObjectBuilder_BlueprintDefinition/PlatinumOreToIngot", "Platinum Ingot" },
            { "MyObjectBuilder_BlueprintDefinition/PowerCell", "Power Cell" },
            { "MyObjectBuilder_BlueprintDefinition/Position0090_AdvancedHandHeldLauncher", "PRO-1 Rocket Launcher" },
            { "MyObjectBuilder_BlueprintDefinition/Position0030_AngleGrinder3", "Proficient Grinder" },
            { "MyObjectBuilder_BlueprintDefinition/Position0070_HandDrill3", "Proficient Hand Drill" },
            { "MyObjectBuilder_BlueprintDefinition/Position0110_Welder3", "Proficient Welder" },
            { "MyObjectBuilder_BlueprintDefinition/RadioCommunicationComponent", "Radio-comm Comp." },
            { "MyObjectBuilder_BlueprintDefinition/ReactorComponent", "Reactor Comp." },
            { "MyObjectBuilder_BlueprintDefinition/Position0080_BasicHandHeldLauncher", "RO-1 Rocket Launcher" },
            { "MyObjectBuilder_BlueprintDefinition/Position0100_Missile200mm", "Rocket" },
            { "MyObjectBuilder_BlueprintDefinition/Position0010_SemiAutoPistol", "S-10 Pistol" },
            { "MyObjectBuilder_BlueprintDefinition/Position0010_SemiAutoPistolMagazine", "S-10 Pistol Magazine" },
            { "MyObjectBuilder_BlueprintDefinition/Position0030_EliteAutoPistol", "S-10E Pistol" },
            { "MyObjectBuilder_BlueprintDefinition/Position0030_ElitePistolMagazine", "S-10E Pistol Magazine" },
            { "MyObjectBuilder_BlueprintDefinition/Position0020_FullAutoPistol", "S-20A Pistol" },
            { "MyObjectBuilder_BlueprintDefinition/Position0020_FullAutoPistolMagazine", "S-20A Pistol Magazine" },
            { "MyObjectBuilder_BlueprintDefinition/SiliconOreToIngot", "Silicon Wafer" },
            { "MyObjectBuilder_BlueprintDefinition/SilverOreToIngot", "Silver Ingot" },
            { "MyObjectBuilder_BlueprintDefinition/Position0130_SmallRailgunAmmo", "Small Railgun Sabot" },
            { "MyObjectBuilder_BlueprintDefinition/SmallTube", "Small Steel Tube" },
            { "MyObjectBuilder_BlueprintDefinition/SolarCell", "Solar Cell" },
            { "MyObjectBuilder_BlueprintDefinition/SteelPlate", "Steel Plate" },
            { "MyObjectBuilder_BlueprintDefinition/Superconductor", "Superconductor" },
            { "MyObjectBuilder_BlueprintDefinition/ThrustComponent", "Thruster Comp." },
            { "MyObjectBuilder_BlueprintDefinition/UraniumOreToIngot", "Uranium Ingot" },
            { "MyObjectBuilder_BlueprintDefinition/Position0090_Welder", "Welder" },
            { "MyObjectBuilder_BlueprintDefinition/ZoneChip", "Zone Chip" },
        };

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;            
            
        }

        private void Status(bool success, string status)
        {
            var surface = Me.GetSurface(0);
            surface.ContentType = ContentType.TEXT_AND_IMAGE;
            surface.BackgroundColor = success ? Color.Blue : Color.Red;
            surface.FontColor = Color.White;
            surface.FontSize = 2;
            surface.WriteText($"Craft Q Display\n" +
                $"Status: \n" +
                $"{status}", false);
        }

        private string formatAmount(MyFixedPoint value)
        {
            long withTwoDecimals = value.RawValue / 10000;

            long whole;
            long decimals;
            string suffix;
            if (withTwoDecimals > 100000000)
            {
                whole = withTwoDecimals / 100000000;
                decimals = (withTwoDecimals % 100000000) / 100;
                suffix = "M";
            } 
            else if (withTwoDecimals > 100000)
            {
                whole = withTwoDecimals / 100000;
                decimals = (withTwoDecimals % 100000) / 100;
                suffix = "k";
            } 
            else
            {
                whole = withTwoDecimals / 100;
                decimals = withTwoDecimals % 100;
                suffix = "";
            }

            return $"{whole}.{decimals}{suffix}";
        }

        public void Main(string argument)
        {
            Dictionary<string, MyFixedPoint> amounts = new Dictionary<string, MyFixedPoint>();
            List<IMyProductionBlock> blocks = new List<IMyProductionBlock>();
            GridTerminalSystem.GetBlocksOfType(blocks, cargo => cargo.IsSameConstructAs(Me));
            foreach(var production in blocks)
            {                               
                List<MyProductionItem> items = new List<MyProductionItem>();
                production.GetQueue(items);
                foreach(var item in items)
                {
                    if (!amounts.ContainsKey(item.BlueprintId.ToString()))
                    {
                        amounts.Add(item.BlueprintId.ToString(), MyFixedPoint.Zero);
                    }
                    amounts[item.BlueprintId.ToString()] += item.Amount;
                }
            }

            var block = GridTerminalSystem.GetBlockWithName(displayName) as IMyTextSurfaceProvider;
            if (block == null)
            {
                Status(false, $"Block not found: \n{displayName}");
                return;
            }
            var surface = block.GetSurface(displayId);
            if (surface == null)
            {
                Status(false, $"Display not found: \n{displayId}");
                return;
            }
            surface.Font = "Monospace";
            surface.FontSize = 0.9f;
            surface.ContentType = ContentType.TEXT_AND_IMAGE;

            var asList = amounts.ToList();
            asList.Sort((a1, a2) => a1.Value > a2.Value ? -1 : a1.Value == a2.Value ? 0 : 1);
            var rows = asList.Select(craftedItem => {
                var displayName = internalToDisplay[craftedItem.Key];
                int length = 21;
                length -= displayName.Length;

                var space = length > 0 ? new string(' ', length) : "";
                return $"{displayName}{space}{formatAmount(craftedItem.Value)}";
            });
            surface.WriteText(string.Join("\n", rows));

            Status(true, "OK");
        }
    }
}
