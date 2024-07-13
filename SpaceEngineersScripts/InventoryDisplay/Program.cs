using Sandbox.ModAPI.Ingame;
using VRage;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace SpaceEngineersScripts.InventoryDisplay
{
    public class Program: MyGridProgram
    {

        /*
            This script will render a stable inventory list onto target screen(s). The definition below specifies which items, in which order, should be displayed on which screen.
            The individual items/rows will always display in the same position, regardless of the amount that is currently stored in storage.
            It is recommended to use monospace font to ensure that the amounts column will be aligned
        */
        // Configuration
        List<Target> definitions = new List<Target>()
        {
            new Target
            {
                BlockName = "LCD Panel",
                DisplayId = 0,
                Items = new[]{"Iron Ore", "Cobalt Ore"}
            }
        };


        // Script start
        class Target
        {
            public string BlockName { get; set; }
            public int DisplayId { get; set; }
            public string[] Items { get; set; }
        }


        Dictionary<string, string> displayStringToInternalMap = new Dictionary<string, string>()
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
            surface.WriteText($"Inventory display\n" +
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
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();            
            GridTerminalSystem.GetBlocksOfType(blocks, cargo => cargo.IsSameConstructAs(Me) && cargo.HasInventory);
            foreach(var block in blocks)
            {
                for(int i = 0; i < block.InventoryCount; i++)
                {
                    List<MyInventoryItem> items = new List<MyInventoryItem>();
                    block.GetInventory(i).GetItems(items);
                    foreach (var item in items)
                    {
                        if (!amounts.ContainsKey(item.Type.ToString()))
                        {
                            amounts.Add(item.Type.ToString(), MyFixedPoint.Zero);
                        }
                        amounts[item.Type.ToString()] += item.Amount;
                    }
                }
            }
            List<string> errors = new List<string>();

            foreach(var definition in definitions)
            {
                var block = GridTerminalSystem.GetBlockWithName(definition.BlockName) as IMyTextSurfaceProvider;
                if (block == null)
                {
                    errors.Add($"Block not found: \n{definition.BlockName}");
                    continue;
                }
                var surface = block.GetSurface(definition.DisplayId);
                if (surface == null)
                {
                    errors.Add($"Display not found: \n{definition.DisplayId}");
                    continue;
                }
                surface.Font = "Monospace";
                surface.FontSize = 0.9f;
                surface.ContentType = ContentType.TEXT_AND_IMAGE;

                var rows = definition.Items.Select(displayName => {
                    if (!displayStringToInternalMap.ContainsKey(displayName))
                    {
                        errors.Add($"Unknown item: \n{displayName}");
                        return "???";
                    }

                    var internalName = displayStringToInternalMap[displayName];                    
                    MyFixedPoint amount;
                    if (!amounts.TryGetValue(internalName, out amount))
                    {
                        amount = MyFixedPoint.Zero;
                    }
                    int length = 21;
                    length -= displayName.Length;
                    
                    return $"{displayName}{new string(' ', length)}{formatAmount(amount)}";
                });
                surface.WriteText(string.Join("\n", rows));
            }

            if (errors.Count > 0)
            {
                Status(false, string.Join("\n", errors));
            } else
            {
                Status(true, "OK");
            }
        }
    }
}
