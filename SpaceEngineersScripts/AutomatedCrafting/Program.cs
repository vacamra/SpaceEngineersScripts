using Sandbox.ModAPI.Ingame;
using VRage;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace SpaceEngineersScripts.AutomatedCrafting
{
    public class Program: MyGridProgram
    {
        /*
            This script will add items into the crafting queue of given Assembler with the goal of maintaining some given stockpile of items
        */
        // Configuration
        string masterAssembler = "Assembler";
        Dictionary<string, int> goals = new Dictionary<string, int>
        {
            { "Steel Plate", 1000 }
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
            surface.WriteText($"Automated crafting\n" +
                $"Status: \n" +
                $"{status}", false);
        }

        Dictionary<string, MyFixedPoint> GetCurrentCargo()
        {
            Dictionary<string, MyFixedPoint> currentCargo = new Dictionary<string, MyFixedPoint>();
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType(blocks, cargo => cargo.IsSameConstructAs(Me) && cargo.HasInventory);
            foreach (var block in blocks)
            {
                for (int i = 0; i < block.InventoryCount; i++)
                {
                    List<MyInventoryItem> items = new List<MyInventoryItem>();
                    block.GetInventory(i).GetItems(items);
                    foreach (var item in items)
                    {
                        if (!currentCargo.ContainsKey(item.Type.ToString()))
                        {
                            currentCargo.Add(item.Type.ToString(), MyFixedPoint.Zero);
                        }
                        currentCargo[item.Type.ToString()] += item.Amount;
                    }
                }
            }
            return currentCargo;
        }

        Dictionary<string, MyFixedPoint> GetCurrentCrafting()
        {
            Dictionary<string, MyFixedPoint> crafting = new Dictionary<string, MyFixedPoint>();
            List<IMyProductionBlock> blocks = new List<IMyProductionBlock>();
            GridTerminalSystem.GetBlocksOfType(blocks, production => production.IsSameConstructAs(Me));
            foreach (var production in blocks)
            {
                List<MyProductionItem> items = new List<MyProductionItem>();
                production.GetQueue(items);
                foreach (var item in items)
                {
                    if (!crafting.ContainsKey(item.BlueprintId.ToString()))
                    {
                        crafting.Add(item.BlueprintId.ToString(), MyFixedPoint.Zero);
                    }
                    crafting[item.BlueprintId.ToString()] += item.Amount;
                }
            }
            return crafting;
        }

        public void Main(string argument)
        {
            var currentCargo = GetCurrentCargo();
            var craftingQueue =  GetCurrentCrafting();

            var assembler = GridTerminalSystem.GetBlockWithName(masterAssembler) as IMyProductionBlock;
            if (assembler == null)
            {
                Status(false, "Assembler not found");
                return;
            }
            foreach (var goal in goals)
            {
                if (!displayStringToInternalItem.ContainsKey(goal.Key) || !displayToInternalCrafting.ContainsKey(goal.Key))
                {
                    Status(false, $"Unknown item: \n{goal.Key}");
                    continue;
                }

                MyFixedPoint current, crafting;
                if (!currentCargo.TryGetValue(displayStringToInternalItem[goal.Key], out current)) {
                    current = MyFixedPoint.Zero;
                }
                if (!craftingQueue.TryGetValue(displayToInternalCrafting[goal.Key], out crafting))
                {
                    crafting = MyFixedPoint.Zero;
                }                

                MyFixedPoint missing = MyFixedPoint.AddSafe(goal.Value ,-MyFixedPoint.AddSafe(current, crafting));
                if (missing > 0)
                {
                    Echo($"{missing} is greater than 0");
                    assembler.AddQueueItem(MyDefinitionId.Parse(displayToInternalCrafting[goal.Key]), missing);
                }
            }

            Status(true, "OK");
        }

        Dictionary<string, string> displayStringToInternalItem = new Dictionary<string, string>()
        {
            { "Bulletproof Glass", "MyObjectBuilder_Component/BulletproofGlass" },
            { "Canvas", "MyObjectBuilder_Component/Canvas" },
            { "Computer", "MyObjectBuilder_Component/Computer" },
            { "Construction Comp.", "MyObjectBuilder_Component/Construction" },
            { "Detector Comp.", "MyObjectBuilder_Component/Detector" },
            { "Display", "MyObjectBuilder_Component/Display" },
            { "Engineer Plushie", "MyObjectBuilder_Component/EngineerPlushie" },
            { "Explosives", "MyObjectBuilder_Component/Explosives" },
            { "Girder", "MyObjectBuilder_Component/Girder" },
            { "Gravity Comp.", "MyObjectBuilder_Component/GravityGenerator" },
            { "Interior Plate", "MyObjectBuilder_Component/InteriorPlate" },
            { "Large Steel Tube", "MyObjectBuilder_Component/LargeTube" },
            { "Medical Comp.", "MyObjectBuilder_Component/Medical" },
            { "Metal Grid", "MyObjectBuilder_Component/MetalGrid" },
            { "Motor", "MyObjectBuilder_Component/Motor" },
            { "Power Cell", "MyObjectBuilder_Component/PowerCell" },
            { "Radio-comm Comp.", "MyObjectBuilder_Component/RadioCommunication" },
            { "Reactor Comp.", "MyObjectBuilder_Component/Reactor" },
            { "Saberoid Plushie", "MyObjectBuilder_Component/SabiroidPlushie" },
            { "Small Steel Tube", "MyObjectBuilder_Component/SmallTube" },
            { "Solar Cell", "MyObjectBuilder_Component/SolarCell" },
            { "Steel Plate", "MyObjectBuilder_Component/SteelPlate" },
            { "Superconductor", "MyObjectBuilder_Component/Superconductor" },
            { "Thruster Comp.", "MyObjectBuilder_Component/Thrust" },
            { "Zone Chip", "MyObjectBuilder_Component/ZoneChip" },
            { "Hydrogen", "MyObjectBuilder_GasProperties/Hydrogen" },
            { "Oxygen", "MyObjectBuilder_GasProperties/Oxygen" },
            { "Cobalt Ingot", "MyObjectBuilder_Ingot/Cobalt" },
            { "Gold Ingot", "MyObjectBuilder_Ingot/Gold" },
            { "Gravel", "MyObjectBuilder_Ingot/Stone" },
            { "Iron Ingot", "MyObjectBuilder_Ingot/Iron" },
            { "Magnesium Powder", "MyObjectBuilder_Ingot/Magnesium" },
            { "Nickel Ingot", "MyObjectBuilder_Ingot/Nickel" },
            { "Old Scrap Metal", "MyObjectBuilder_Ingot/Scrap" },
            { "Platinum Ingot", "MyObjectBuilder_Ingot/Platinum" },
            { "Silicon Wafer", "MyObjectBuilder_Ingot/Silicon" },
            { "Silver Ingot", "MyObjectBuilder_Ingot/Silver" },
            { "Gold Ore", "MyObjectBuilder_Ore/Gold" },
            { "Cobalt Ore", "MyObjectBuilder_Ore/Cobalt" },
            { "Uranium Ingot", "MyObjectBuilder_Ingot/Uranium" },
            { "Ice", "MyObjectBuilder_Ore/Ice" },
            { "Iron Ore", "MyObjectBuilder_Ore/Iron" },
            { "Magnesium Ore", "MyObjectBuilder_Ore/Magnesium" },
            { "Nickel Ore", "MyObjectBuilder_Ore/Nickel" },
            { "Organic", "MyObjectBuilder_Ore/Organic" },
            { "Platinum Ore", "MyObjectBuilder_Ore/Platinum" },
            { "Scrap Metal", "MyObjectBuilder_Ore/Scrap" },
            { "Silicon Ore", "MyObjectBuilder_Ore/Silicon" },
            { "Silver Ore", "MyObjectBuilder_Ore/Silver" },
            { "Stone", "MyObjectBuilder_Ore/Stone" },
            { "Uranium Ore", "MyObjectBuilder_Ore/Uranium" },
            { "Clang Kola", "MyObjectBuilder_ConsumableItem/ClangCola" },
            { "Cosmic Coffee", "MyObjectBuilder_ConsumableItem/CosmicCoffee" },
            { "Datapad", "MyObjectBuilder_Datapad/Datapad" },
            { "Medkit", "MyObjectBuilder_ConsumableItem/Medkit" },
            { "Package", "MyObjectBuilder_Package/Package" },
            { "Powerkit", "MyObjectBuilder_ConsumableItem/Powerkit" },
            { "Space Credit", "MyObjectBuilder_PhysicalObject/SpaceCredit" },
            { "Elite Grinder", "MyObjectBuilder_PhysicalGunObject/AngleGrinder4Item" },
            { "Elite Hand Drill", "MyObjectBuilder_PhysicalGunObject/HandDrill4Item" },
            { "Elite Welder", "MyObjectBuilder_PhysicalGunObject/Welder4Item" },
            { "Enhanced Grinder", "MyObjectBuilder_PhysicalGunObject/AngleGrinder2Item" },
            { "Enhanced Hand Drill", "MyObjectBuilder_PhysicalGunObject/HandDrill2Item" },
            { "Enhanced Welder", "MyObjectBuilder_PhysicalGunObject/Welder2Item" },
            { "Grinder", "MyObjectBuilder_PhysicalGunObject/AngleGrinderItem" },
            { "Hand Drill", "MyObjectBuilder_PhysicalGunObject/HandDrillItem" },
            { "Flare Gun", "MyObjectBuilder_PhysicalGunObject/FlareGunItem" },
            { "Hydrogen Bottle", "MyObjectBuilder_GasContainerObject/HydrogenBottle" },
            { "MR-20 Rifle", "MyObjectBuilder_PhysicalGunObject/AutomaticRifleItem" },
            { "MR-30E Rifle", "MyObjectBuilder_PhysicalGunObject/UltimateAutomaticRifleItem" },
            { "MR-50A Rifle", "MyObjectBuilder_PhysicalGunObject/RapidFireAutomaticRifleItem" },
            { "MR-8P Rifle", "MyObjectBuilder_PhysicalGunObject/PreciseAutomaticRifleItem" },
            { "Oxygen Bottle", "MyObjectBuilder_OxygenContainerObject/OxygenBottle" },
            { "PRO-1 Rocket Launcher", "MyObjectBuilder_PhysicalGunObject/AdvancedHandHeldLauncherItem" },
            { "Proficient Grinder", "MyObjectBuilder_PhysicalGunObject/AngleGrinder3Item" },
            { "Proficient Hand Drill", "MyObjectBuilder_PhysicalGunObject/HandDrill3Item" },
            { "Proficient Welder", "MyObjectBuilder_PhysicalGunObject/Welder3Item" },
            { "RO-1 Rocket Launcher", "MyObjectBuilder_PhysicalGunObject/BasicHandHeldLauncherItem" },
            { "S-10 Pistol", "MyObjectBuilder_PhysicalGunObject/SemiAutoPistolItem" },
            { "S-10E Pistol", "MyObjectBuilder_PhysicalGunObject/ElitePistolItem" },
            { "S-20A Pistol", "MyObjectBuilder_PhysicalGunObject/FullAutoPistolItem" },
            { "Welder", "MyObjectBuilder_PhysicalGunObject/WelderItem" },

        };

        Dictionary<string, string> displayToInternalCrafting = new Dictionary<string, string>()
        {
            { "Artillery Shell", "MyObjectBuilder_BlueprintDefinition/Position0120_LargeCalibreAmmo" },
            { "Assault Cannon Shell", "MyObjectBuilder_BlueprintDefinition/Position0110_MediumCalibreAmmo" },
            { "Autocannon Magazine", "MyObjectBuilder_BlueprintDefinition/Position0090_AutocannonClip" },
            { "Bulletproof Glass", "MyObjectBuilder_BlueprintDefinition/BulletproofGlass" },
            { "Canvas", "MyObjectBuilder_BlueprintDefinition/Position0030_Canvas" },
            { "Cobalt Ingot", "MyObjectBuilder_BlueprintDefinition/CobaltOreToIngot" },
            { "Computer", "MyObjectBuilder_BlueprintDefinition/ComputerComponent" },
            { "Construction Comp.", "MyObjectBuilder_BlueprintDefinition/ConstructionComponent" },
            { "Datapad", "MyObjectBuilder_BlueprintDefinition/Position0040_Datapad" },
            { "Detector Comp.", "MyObjectBuilder_BlueprintDefinition/DetectorComponent" },
            { "Display", "MyObjectBuilder_BlueprintDefinition/Display" },
            { "DisplayName_Item_EngineerPlushie", "MyObjectBuilder_BlueprintDefinition/EngineerPlushie" },
            { "DisplayName_Item_SabiroidPlushie", "MyObjectBuilder_BlueprintDefinition/SabiroidPlushie" },
            { "Elite Grinder", "MyObjectBuilder_BlueprintDefinition/Position0040_AngleGrinder4" },
            { "Elite Hand Drill", "MyObjectBuilder_BlueprintDefinition/Position0080_HandDrill4" },
            { "Elite Welder", "MyObjectBuilder_BlueprintDefinition/Position0120_Welder4" },
            { "Enhanced Grinder", "MyObjectBuilder_BlueprintDefinition/Position0020_AngleGrinder2" },
            { "Enhanced Hand Drill", "MyObjectBuilder_BlueprintDefinition/Position0060_HandDrill2" },
            { "Enhanced Welder", "MyObjectBuilder_BlueprintDefinition/Position0100_Welder2" },
            { "Explosives", "MyObjectBuilder_BlueprintDefinition/ExplosivesComponent" },
            { "Fireworks Blue", "MyObjectBuilder_BlueprintDefinition/Position0007_FireworksBoxBlue" },
            { "Fireworks Green", "MyObjectBuilder_BlueprintDefinition/Position00071_FireworksBoxGreen" },
            { "Fireworks Pink", "MyObjectBuilder_BlueprintDefinition/Position00074_FireworksBoxPink" },
            { "Fireworks Rainbow", "MyObjectBuilder_BlueprintDefinition/Position00075_FireworksBoxRainbow" },
            { "Fireworks Red", "MyObjectBuilder_BlueprintDefinition/Position00072_FireworksBoxRed" },
            { "Fireworks Yellow", "MyObjectBuilder_BlueprintDefinition/Position00073_FireworksBoxYellow" },
            { "Flare Gun", "MyObjectBuilder_BlueprintDefinition/Position0005_FlareGun" },
            { "Flare Gun Clip", "MyObjectBuilder_BlueprintDefinition/Position0005_FlareGunMagazine" },
            { "Gatling Ammo Box", "MyObjectBuilder_BlueprintDefinition/Position0080_NATO_25x184mmMagazine" },
            { "Girder", "MyObjectBuilder_BlueprintDefinition/GirderComponent" },
            { "Gravel", "MyObjectBuilder_BlueprintDefinition/StoneOreToIngot_Deconstruction" },
            { "Gravity Comp.", "MyObjectBuilder_BlueprintDefinition/GravityGeneratorComponent" },
            { "Grinder", "MyObjectBuilder_BlueprintDefinition/Position0010_AngleGrinder" },
            { "Hand Drill", "MyObjectBuilder_BlueprintDefinition/Position0050_HandDrill" },
            { "Hydrogen Bottle", "MyObjectBuilder_BlueprintDefinition/Position0020_HydrogenBottle" },
            { "Hydrogen Bottles", "MyObjectBuilder_BlueprintDefinition/HydrogenBottlesRefill" },
            { "Interior Plate", "MyObjectBuilder_BlueprintDefinition/InteriorPlate" },
            { "Large Railgun Sabot", "MyObjectBuilder_BlueprintDefinition/Position0140_LargeRailgunAmmo" },
            { "Large Steel Tube", "MyObjectBuilder_BlueprintDefinition/LargeTube" },
            { "Magnesium Powder", "MyObjectBuilder_BlueprintDefinition/MagnesiumOreToIngot" },
            { "Medical Comp.", "MyObjectBuilder_BlueprintDefinition/MedicalComponent" },
            { "Metal Grid", "MyObjectBuilder_BlueprintDefinition/MetalGrid" },
            { "Motor", "MyObjectBuilder_BlueprintDefinition/MotorComponent" },
            { "MR-20 Rifle", "MyObjectBuilder_BlueprintDefinition/Position0040_AutomaticRifle" },
            { "MR-20 Rifle Magazine", "MyObjectBuilder_BlueprintDefinition/Position0040_AutomaticRifleGun_Mag_20rd" },
            { "MR-30E Rifle", "MyObjectBuilder_BlueprintDefinition/Position0070_UltimateAutomaticRifle" },
            { "MR-30E Rifle Magazine", "MyObjectBuilder_BlueprintDefinition/Position0070_UltimateAutomaticRifleGun_Mag_30rd" },
            { "MR-50A Rifle", "MyObjectBuilder_BlueprintDefinition/Position0050_RapidFireAutomaticRifle" },
            { "MR-50A Rifle Magazine", "MyObjectBuilder_BlueprintDefinition/Position0050_RapidFireAutomaticRifleGun_Mag_50rd" },
            { "MR-8P Rifle", "MyObjectBuilder_BlueprintDefinition/Position0060_PreciseAutomaticRifle" },
            { "MR-8P Rifle Magazine", "MyObjectBuilder_BlueprintDefinition/Position0060_PreciseAutomaticRifleGun_Mag_5rd" },
            { "Oxygen", "MyObjectBuilder_BlueprintDefinition/IceToOxygen" },
            { "Oxygen Bottle", "MyObjectBuilder_BlueprintDefinition/Position0010_OxygenBottle" },
            { "Oxygen Bottles", "MyObjectBuilder_BlueprintDefinition/OxygenBottlesRefill" },
            { "Power Cell", "MyObjectBuilder_BlueprintDefinition/PowerCell" },
            { "PRO-1 Rocket Launcher", "MyObjectBuilder_BlueprintDefinition/Position0090_AdvancedHandHeldLauncher" },
            { "Proficient Grinder", "MyObjectBuilder_BlueprintDefinition/Position0030_AngleGrinder3" },
            { "Proficient Hand Drill", "MyObjectBuilder_BlueprintDefinition/Position0070_HandDrill3" },
            { "Proficient Welder", "MyObjectBuilder_BlueprintDefinition/Position0110_Welder3" },
            { "Radio-comm Comp.", "MyObjectBuilder_BlueprintDefinition/RadioCommunicationComponent" },
            { "Reactor Comp.", "MyObjectBuilder_BlueprintDefinition/ReactorComponent" },
            { "RO-1 Rocket Launcher", "MyObjectBuilder_BlueprintDefinition/Position0080_BasicHandHeldLauncher" },
            { "Rocket", "MyObjectBuilder_BlueprintDefinition/Position0100_Missile200mm" },
            { "S-10 Pistol", "MyObjectBuilder_BlueprintDefinition/Position0010_SemiAutoPistol" },
            { "S-10 Pistol Magazine", "MyObjectBuilder_BlueprintDefinition/Position0010_SemiAutoPistolMagazine" },
            { "S-10E Pistol", "MyObjectBuilder_BlueprintDefinition/Position0030_EliteAutoPistol" },
            { "S-10E Pistol Magazine", "MyObjectBuilder_BlueprintDefinition/Position0030_ElitePistolMagazine" },
            { "S-20A Pistol", "MyObjectBuilder_BlueprintDefinition/Position0020_FullAutoPistol" },
            { "S-20A Pistol Magazine", "MyObjectBuilder_BlueprintDefinition/Position0020_FullAutoPistolMagazine" },
            { "Silicon Wafer", "MyObjectBuilder_BlueprintDefinition/SiliconOreToIngot" },
            { "Small Railgun Sabot", "MyObjectBuilder_BlueprintDefinition/Position0130_SmallRailgunAmmo" },
            { "Small Steel Tube", "MyObjectBuilder_BlueprintDefinition/SmallTube" },
            { "Solar Cell", "MyObjectBuilder_BlueprintDefinition/SolarCell" },
            { "Steel Plate", "MyObjectBuilder_BlueprintDefinition/SteelPlate" },
            { "Superconductor", "MyObjectBuilder_BlueprintDefinition/Superconductor" },
            { "Thruster Comp.", "MyObjectBuilder_BlueprintDefinition/ThrustComponent" },
            { "Welder", "MyObjectBuilder_BlueprintDefinition/Position0090_Welder" },
            { "Zone Chip", "MyObjectBuilder_BlueprintDefinition/ZoneChip" },
        };
    }
}
