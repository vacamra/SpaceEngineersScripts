using System.Net.WebSockets;
using System.Runtime.ExceptionServices;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI;
using VRage;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace SpaceEngineersScripts.MiningShipAllInOne
{
    public class Program: MyGridProgram
    {

        /*
          
        */
        // Configuration
        MyIni config = new MyIni();

        string batteryDisplayName;
        string cargoDisplayName;
        string inventoryDisplayName;


        // Script start
        private Dictionary<string, string> internalToDisplayName = new Dictionary<string, string>()
        {
            { "MyObjectBuilder_Component/BulletproofGlass", "Bulletproof Glass" },
            { "MyObjectBuilder_Component/Canvas", "Canvas" },
            { "MyObjectBuilder_Component/Computer", "Computer" },
            { "MyObjectBuilder_Component/Construction", "Construction Comp." },
            { "MyObjectBuilder_Component/Detector", "Detector Comp." },
            { "MyObjectBuilder_Component/Display", "Display" },
            { "MyObjectBuilder_Component/EngineerPlushie", "Engineer Plushie" },
            { "MyObjectBuilder_Component/Explosives", "Explosives" },
            { "MyObjectBuilder_Component/Girder", "Girder" },
            { "MyObjectBuilder_Component/GravityGenerator", "Gravity Comp." },
            { "MyObjectBuilder_Component/InteriorPlate", "Interior Plate" },
            { "MyObjectBuilder_Component/LargeTube", "Large Steel Tube" },
            { "MyObjectBuilder_Component/Medical", "Medical Comp." },
            { "MyObjectBuilder_Component/MetalGrid", "Metal Grid" },
            { "MyObjectBuilder_Component/Motor", "Motor" },
            { "MyObjectBuilder_Component/PowerCell", "Power Cell" },
            { "MyObjectBuilder_Component/RadioCommunication", "Radio-comm Comp." },
            { "MyObjectBuilder_Component/Reactor", "Reactor Comp." },
            { "MyObjectBuilder_Component/SabiroidPlushie", "Saberoid Plushie" },
            { "MyObjectBuilder_Component/SmallTube", "Small Steel Tube" },
            { "MyObjectBuilder_Component/SolarCell", "Solar Cell" },
            { "MyObjectBuilder_Component/SteelPlate", "Steel Plate" },
            { "MyObjectBuilder_Component/Superconductor", "Superconductor" },
            { "MyObjectBuilder_Component/Thrust", "Thruster Comp." },
            { "MyObjectBuilder_Component/ZoneChip", "Zone Chip" },
            { "MyObjectBuilder_GasProperties/Hydrogen", "Hydrogen" },
            { "MyObjectBuilder_GasProperties/Oxygen", "Oxygen" },
            { "MyObjectBuilder_Ingot/Cobalt", "Cobalt Ingot" },
            { "MyObjectBuilder_Ingot/Gold", "Gold Ingot" },
            { "MyObjectBuilder_Ingot/Stone", "Gravel" },
            { "MyObjectBuilder_Ingot/Iron", "Iron Ingot" },
            { "MyObjectBuilder_Ingot/Magnesium", "Magnesium Powder" },
            { "MyObjectBuilder_Ingot/Nickel", "Nickel Ingot" },
            { "MyObjectBuilder_Ingot/Scrap", "Old Scrap Metal" },
            { "MyObjectBuilder_Ingot/Platinum", "Platinum Ingot" },
            { "MyObjectBuilder_Ingot/Silicon", "Silicon Wafer" },
            { "MyObjectBuilder_Ingot/Silver", "Silver Ingot" },
            { "MyObjectBuilder_Ore/Gold", "Gold Ore" },
            { "MyObjectBuilder_Ore/Cobalt", "Cobalt Ore" },
            { "MyObjectBuilder_Ingot/Uranium", "Uranium Ingot" },
            { "MyObjectBuilder_Ore/Ice", "Ice" },
            { "MyObjectBuilder_Ore/Iron", "Iron Ore" },
            { "MyObjectBuilder_Ore/Magnesium", "Magnesium Ore" },
            { "MyObjectBuilder_Ore/Nickel", "Nickel Ore" },
            { "MyObjectBuilder_Ore/Organic", "Organic" },
            { "MyObjectBuilder_Ore/Platinum", "Platinum Ore" },
            { "MyObjectBuilder_Ore/Scrap", "Scrap Metal" },
            { "MyObjectBuilder_Ore/Silicon", "Silicon Ore" },
            { "MyObjectBuilder_Ore/Silver", "Silver Ore" },
            { "MyObjectBuilder_Ore/Stone", "Stone" },
            { "MyObjectBuilder_Ore/Uranium", "Uranium Ore" },
            { "MyObjectBuilder_ConsumableItem/ClangCola", "Clang Kola" },
            { "MyObjectBuilder_ConsumableItem/CosmicCoffee", "Cosmic Coffee" },
            { "MyObjectBuilder_Datapad/Datapad", "Datapad" },
            { "MyObjectBuilder_ConsumableItem/Medkit", "Medkit" },
            { "MyObjectBuilder_Package/Package", "Package" },
            { "MyObjectBuilder_ConsumableItem/Powerkit", "Powerkit" },
            { "MyObjectBuilder_PhysicalObject/SpaceCredit", "Space Credit" },
            { "MyObjectBuilder_PhysicalGunObject/AngleGrinder4Item", "Elite Grinder" },
            { "MyObjectBuilder_PhysicalGunObject/HandDrill4Item", "Elite Hand Drill" },
            { "MyObjectBuilder_PhysicalGunObject/Welder4Item", "Elite Welder" },
            { "MyObjectBuilder_PhysicalGunObject/AngleGrinder2Item", "Enhanced Grinder" },
            { "MyObjectBuilder_PhysicalGunObject/HandDrill2Item", "Enhanced Hand Drill" },
            { "MyObjectBuilder_PhysicalGunObject/Welder2Item", "Enhanced Welder" },
            { "MyObjectBuilder_PhysicalGunObject/AngleGrinderItem", "Grinder" },
            { "MyObjectBuilder_PhysicalGunObject/HandDrillItem", "Hand Drill" },
            { "MyObjectBuilder_PhysicalGunObject/FlareGunItem", "Flare Gun" },
            { "MyObjectBuilder_GasContainerObject/HydrogenBottle", "Hydrogen Bottle" },
            { "MyObjectBuilder_PhysicalGunObject/AutomaticRifleItem", "MR-20 Rifle" },
            { "MyObjectBuilder_PhysicalGunObject/UltimateAutomaticRifleItem", "MR-30E Rifle" },
            { "MyObjectBuilder_PhysicalGunObject/RapidFireAutomaticRifleItem", "MR-50A Rifle" },
            { "MyObjectBuilder_PhysicalGunObject/PreciseAutomaticRifleItem", "MR-8P Rifle" },
            { "MyObjectBuilder_OxygenContainerObject/OxygenBottle", "Oxygen Bottle" },
            { "MyObjectBuilder_PhysicalGunObject/AdvancedHandHeldLauncherItem", "PRO-1 Rocket Launcher" },
            { "MyObjectBuilder_PhysicalGunObject/AngleGrinder3Item", "Proficient Grinder" },
            { "MyObjectBuilder_PhysicalGunObject/HandDrill3Item", "Proficient Hand Drill" },
            { "MyObjectBuilder_PhysicalGunObject/Welder3Item", "Proficient Welder" },
            { "MyObjectBuilder_PhysicalGunObject/BasicHandHeldLauncherItem", "RO-1 Rocket Launcher" },
            { "MyObjectBuilder_PhysicalGunObject/SemiAutoPistolItem", "S-10 Pistol" },
            { "MyObjectBuilder_PhysicalGunObject/ElitePistolItem", "S-10E Pistol" },
            { "MyObjectBuilder_PhysicalGunObject/FullAutoPistolItem", "S-20A Pistol" },
            { "MyObjectBuilder_PhysicalGunObject/WelderItem", "Welder" },
        };

        float lastHydrogenLevel = -1f;
        float lastOxygenLevel = -1f;
        
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;                       

            config.TryParse(Storage);

            batteryDisplayName = config.Get("battery", "displayName").ToString() ?? "LCD Panel";
            cargoDisplayName = config.Get("cargo", "displayName").ToString() ?? "LCD Panel 2";
            inventoryDisplayName = config.Get("inventory", "displayName").ToString() ?? "LCD Panel 3";
        }

        public void Save()
        {
            config.Clear();

            config.Set("battery", "displayName", batteryDisplayName);
            config.Set("cargo", "displayName", cargoDisplayName);
            config.Set("inventory", "displayName", inventoryDisplayName);

            Storage = config.ToString();
        }

        public void Main(string argument)
        {
            DrawBatteryHydrogenOxygen(argument);
            PrintVolumeAndMass(argument);
            PrintItemStorage(argument);
            PrintVelocityInfo(argument);
            PrintComponentStatus(argument);
            Configuration(argument);
        }

        void DrawBatteryHydrogenOxygen(string argument)
        {
            var display = GridTerminalSystem.GetBlockWithName(batteryDisplayName) as IMyTextSurfaceProvider;
            var surface = display.GetSurface(0);
            surface.ContentType = ContentType.SCRIPT;
            surface.ScriptBackgroundColor = Color.ForestGreen;

            var frame = surface.DrawFrame();

            {
                List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
                GridTerminalSystem.GetBlocksOfType(batteries, b => b.IsSameConstructAs(Me));
                float sumIn = 0, sumOut = 0, sumCapacity = 0, sumCurrent = 0;
                for (int i = 0; i < batteries.Count; i++)
                {
                    sumIn += batteries[i].CurrentInput;
                    sumOut += batteries[i].CurrentOutput;
                    sumCapacity += batteries[i].MaxStoredPower;
                    sumCurrent += batteries[i].CurrentStoredPower;
                }
                int level = (int)(100 * sumCurrent / sumCapacity);
                float currentOutput = sumOut - sumIn;
                TimeSpan timeLeft = currentOutput >= 0 
                    ? TimeSpan.FromSeconds(sumCurrent / currentOutput)
                    : TimeSpan.FromSeconds((sumCapacity - sumCurrent) / currentOutput);
                DrawLevel(ref frame, 0, Color.Yellow, "IconEnergy", level, timeLeft);
            }

            {                
                List<IMyGasTank> hydrogenTanks = new List<IMyGasTank>();
                GridTerminalSystem.GetBlocksOfType(hydrogenTanks, t => t.IsSameConstructAs(Me) && t.BlockDefinition.SubtypeId.Contains("Hydrogen"));
                float sumCapacity = 0, sumCurrent = 0;
                for (int i = 0; i <= hydrogenTanks.Count; i++)
                {
                    sumCapacity += hydrogenTanks[i].Capacity;
                    sumCurrent += (float)hydrogenTanks[i].FilledRatio * hydrogenTanks[i].Capacity;
                }
                TimeSpan left;
                float level = sumCurrent / sumCapacity;
                if (lastHydrogenLevel == -1)
                {
                    left = TimeSpan.Zero;
                }
                else
                {
                    float consumedSinceLastRun = lastHydrogenLevel - level;
                    float consumptionPerSecond = consumedSinceLastRun / (float)Runtime.TimeSinceLastRun.TotalSeconds;
                    left = consumedSinceLastRun >= 0f 
                        ? TimeSpan.FromSeconds(sumCurrent / consumptionPerSecond)
                        : TimeSpan.FromSeconds((sumCapacity - sumCurrent) / consumptionPerSecond);
                }
                lastHydrogenLevel = level;
                DrawLevel(ref frame, 170, Color.DeepPink, "IconHydrogen", (int)level, left);
            }

            {
                List<IMyGasTank> oxygenTanks = new List<IMyGasTank>();
                GridTerminalSystem.GetBlocksOfType(oxygenTanks, t => t.IsSameConstructAs(Me) && t.BlockDefinition.SubtypeId.Contains("Oxygen"));
                float sumCapacity = 0, sumCurrent = 0;
                for (int i = 0; i <= oxygenTanks.Count; i++)
                {
                    sumCapacity += oxygenTanks[i].Capacity;
                    sumCurrent += (float)oxygenTanks[i].FilledRatio * oxygenTanks[i].Capacity;
                }
                TimeSpan left;
                float level = sumCurrent / sumCapacity;
                if (lastOxygenLevel == -1)
                {
                    left = TimeSpan.Zero;
                }
                else
                {
                    float consumedSinceLastRun = lastOxygenLevel - level;
                    float consumptionPerSecond = consumedSinceLastRun / (float)Runtime.TimeSinceLastRun.TotalSeconds;
                    left = consumedSinceLastRun >= 0f
                        ? TimeSpan.FromSeconds(sumCurrent / consumptionPerSecond)
                        : TimeSpan.FromSeconds((sumCapacity - sumCurrent) / consumptionPerSecond);
                }
                lastOxygenLevel = level;
                DrawLevel(ref frame, 340, Color.Blue, "IconOxygen", (int)level, left);
            }            

            frame.Dispose();
        }
        
        void DrawLevel(ref MySpriteDrawFrame frame, int offset, Color color, string resourceIcon, int level, TimeSpan timeLeftToDischarge)
        {
            frame.Add(new MySprite(SpriteType.TEXTURE, resourceIcon, new Vector2(offset + 35, 70f), new Vector2(100f, 100f), color, null, TextAlignment.LEFT)); // Battery_icon            
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareHollow", new Vector2(offset + 35, 270f), new Vector2(100f, 250f), Color.White, null, TextAlignment.LEFT)); // Battery_Outline            
            float levelHeight = 230f * level / 100f;
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(offset + 40, 430f - levelHeight), new Vector2(90f, levelHeight), color, null, TextAlignment.LEFT)); // Battery_level
            if (level <= 20)
            {
                frame.Add(new MySprite(SpriteType.TEXTURE, "Danger", new Vector2(offset + 35, 200f), new Vector2(100f, 100f), Color.White, null, TextAlignment.LEFT)); // battery_danger            
            }                                                                                                                                            
            frame.Add(new MySprite(SpriteType.TEXT, level + "%", new Vector2(offset + 85, 440f), null, Color.White, "Debug", TextAlignment.CENTER, 1.5f));; // battery_text
            string formattedTimeLeft;
            if (timeLeftToDischarge < TimeSpan.Zero)
            {
                timeLeftToDischarge = -timeLeftToDischarge;
                formattedTimeLeft = '↑' + (timeLeftToDischarge.TotalSeconds >= 100 ? $"{timeLeftToDischarge.TotalMinutes} min" : $"{timeLeftToDischarge.TotalSeconds} s");
            } 
            else if (timeLeftToDischarge == TimeSpan.Zero && level != 0)
            {
                formattedTimeLeft = "===";
            } else
            {
                formattedTimeLeft = '↓' + (timeLeftToDischarge.TotalSeconds >= 100 ? $"{timeLeftToDischarge.TotalMinutes} min" : $"{timeLeftToDischarge.TotalSeconds} s");
            }
            frame.Add(new MySprite(SpriteType.TEXT, formattedTimeLeft, new Vector2(offset + 85, 480f), null, Color.White, "Debug", TextAlignment.CENTER, 1f)); // battery_timeLeft
        }

        void PrintVolumeAndMass(string argument)
        {
            List<IMyTerminalBlock> inventoryBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType(inventoryBlocks, b => b.IsSameConstructAs(Me) && b.HasInventory);

            MyFixedPoint sumMass = MyFixedPoint.Zero;
            MyFixedPoint sumCurrentVolume = MyFixedPoint.Zero;
            MyFixedPoint sumMaxVolume = MyFixedPoint.Zero;

            for (int i = 0; i <= inventoryBlocks.Count; i++)
            {
                var inventory = inventoryBlocks[i].GetInventory();
                sumMass += inventory.CurrentMass;
                sumCurrentVolume += inventory.CurrentVolume;
                sumMaxVolume += inventory.MaxVolume;
            }

            var display = GridTerminalSystem.GetBlockWithName(cargoDisplayName) as IMyTextSurfaceProvider;
            var surface = display.GetSurface(0);
            surface.ContentType = ContentType.SCRIPT;
            surface.ScriptBackgroundColor = Color.ForestGreen;

            var frame = surface.DrawFrame();

            int level = (int)(100 * (float)sumCurrentVolume / (float)sumMaxVolume);
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareHollow", new Vector2(-80f, 0f), new Vector2(150f, 250f), Color.White, null, TextAlignment.CENTER));
            frame.Add(new MySprite(SpriteType.TEXTURE, "SquareSimple", new Vector2(-80f, -100f), new Vector2(140f, 100f), surface.ScriptBackgroundColor, null, TextAlignment.CENTER));
            frame.Add(new MySprite(SpriteType.TEXT, "CARGO", new Vector2(-100f, -240f), null, Color.White, "Debug", TextAlignment.CENTER, 2.5f));
            frame.Add(new MySprite(SpriteType.TEXT, $"Volume:\n {level}%", new Vector2(40f, -100f), null, Color.White, "Debug", TextAlignment.CENTER, 1f));

            frame.Add(new MySprite(SpriteType.TEXT, $"Mass:\n {sumMass.ToIntSafe()} kg", new Vector2(40f, -100f), null, Color.White, "Debug", TextAlignment.CENTER, 1f));

            int rowCount = MathHelper.RoundToInt(level / (100f / 24f));

            for (int i = 0; i < rowCount; i++)
            {
                float offsetX = (i % 2 == 0) ? 0 : 7f;
                float offsetY = i * 10;
                float dotCount = (i % 2 == 0) ? 10 : 9;
                for (int j = 0; j < dotCount; j++)
                {
                    Vector2 position = new Vector2(-142 + offsetX + j * 14, offsetY);
                    frame.Add(new MySprite(SpriteType.TEXTURE, "Circle", position, new Vector2(10f, 10f), Color.White, null, TextAlignment.CENTER));
                }
            }

            frame.Dispose();
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

        void PrintItemStorage(string argument)
        {
            Dictionary<string, MyFixedPoint> amounts = new Dictionary<string, MyFixedPoint>();
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
                        if (!amounts.ContainsKey(item.Type.ToString()))
                        {
                            amounts.Add(item.Type.ToString(), MyFixedPoint.Zero);
                        }
                        amounts[item.Type.ToString()] += item.Amount;
                    }
                }
            }

            var display = GridTerminalSystem.GetBlockWithName(inventoryDisplayName) as IMyTextSurfaceProvider;
            var surface = display.GetSurface(0);
            surface.ContentType = ContentType.TEXT_AND_IMAGE;
            surface.BackgroundColor = Color.ForestGreen;
            surface.Font = "Monospace";
            surface.FontSize = 0.9f;

            var rows = amounts.Select(item =>
            {
                var displayText = internalToDisplayName[item.Key] ?? item.Key;
                var amount = item.Value;

                int length = 21;
                length -= displayText.Length;

                return $"{displayText}{new string(' ', length)}{formatAmount(amount)}";
            });          
            surface.WriteText(string.Join("\n", rows));
        }

        void PrintVelocityInfo(string argument)
        {

        }

        struct ComponentStatus
        {
            public string Name;
            public string Status;
            public Color Color;
        }

        void PrintComponentStatus(string argument)
        {
            List<ComponentStatus> components = new List<ComponentStatus>();

            components.Add(GetMiningDrillStatus());
            components.Add(GetStoneDispenserStatus());
            components.Add(GetH2O2GeneratorStatus());
            components.Add(GetHydrogenEngineStatus());
            components.Add(GetOxygenStatus());
            components.Add(GetConnectorStatus());
            //TODO: maybe door status?


        }

        private ComponentStatus GetConnectorStatus()
        {
            List<IMyShipConnector> connectors = new List<IMyShipConnector>();
            GridTerminalSystem.GetBlocksOfType(connectors, c => c.DisplayName == "Docking" && c.IsSameConstructAs(Me));
            ComponentStatus status = new ComponentStatus
            {
                Name = "Connector"
            };
            if (connectors.Count == 0)
            {
                status.Color = Color.Red;
                status.Status = "NONE FOUND";
            } 
            else
            {
                if (connectors[0].Status == MyShipConnectorStatus.Connectable)
                {
                    status.Color = Color.Yellow;
                    status.Status = "READY";
                }
                else if (connectors[0].Status == MyShipConnectorStatus.Connected)
                {
                    status.Color = Color.Green;
                    status.Status = "OFF";
                }
                else
                {
                    status.Color = Color.Green;
                    status.Status = "CONNECTED";
                }
            }

            return status;
        }

        private ComponentStatus GetOxygenStatus()
        {
            List<IMyAirVent> vents = new List<IMyAirVent>();
            GridTerminalSystem.GetBlocksOfType(vents, v => v.IsSameConstructAs(Me));
            ComponentStatus status = new ComponentStatus
            {
                Name = "Oxygen"
            };
            if (vents.Count == 0)
            {
                status.Color = Color.Red;
                status.Status = "NO VENT";
            } 
            else
            {
                IMyAirVent vent = vents[0];
                if (!vent.CanPressurize)
                {
                    status.Color = Color.Red;
                    status.Status = "NOT AIRTIGHT";
                } 
                else if (vent.Status == SpaceEngineers.Game.ModAPI.Ingame.VentStatus.Depressurized)
                {
                    status.Color = Color.Yellow;
                    status.Status = "DEPRESSURIZED";
                }
                else if (vent.Status == SpaceEngineers.Game.ModAPI.Ingame.VentStatus.Depressurizing)
                {
                    status.Color = Color.Yellow;
                    status.Status = "DEPRESSURIZING";
                }
                else if (vent.Status == SpaceEngineers.Game.ModAPI.Ingame.VentStatus.Pressurizing)
                {
                    status.Color = Color.Yellow;
                    status.Status = "PRESSURIZING";
                }
                else
                {
                    status.Color = Color.Green;
                    status.Status = "PRESSURIZED";
                }
            }
            return status;
        }

        private ComponentStatus GetHydrogenEngineStatus()
        {
            List<IMyReactor> hydrogenEngines = new List<IMyReactor>();
            GridTerminalSystem.GetBlocksOfType(hydrogenEngines, d => d.IsSameConstructAs(Me));
            ComponentStatus status = new ComponentStatus
            {
                Name = "Hydrogen engines"
            };
            if (hydrogenEngines.Count == 0)
            {
                status.Color = Color.Red;
                status.Status = "NONE FOUND";
            } 
            else
            {
                float output = hydrogenEngines[0].CurrentOutputRatio;
                if (output == 0)
                {
                    status.Color = Color.Yellow;
                    status.Status = "Ready";
                } 
                else
                {
                    status.Color = Color.Green;
                    status.Status = string.Format("{0.00}%", output);
                }
            }
            return status;
        }

        private ComponentStatus GetH2O2GeneratorStatus()
        {
            List<IMyGasGenerator> generators = new List<IMyGasGenerator>();
            GridTerminalSystem.GetBlocksOfType(generators, g => g.IsSameConstructAs(Me));
            ComponentStatus status = new ComponentStatus
            {
                Name = "H2/O2 Generator"
            };
            if (generators.Count == 0)
            {
                status.Color = Color.Red;
                status.Status = "NONE FOUND";
            }
            else
            {
                if (generators[0].IsWorking)
                {
                    status.Color = Color.Green;
                    status.Status = "ON";
                } 
                else
                {
                    status.Color = Color.Yellow;
                    status.Status = "OFF";
                }
            }
            return status;
        }

        private ComponentStatus GetStoneDispenserStatus()
        {
            List<IMyShipConnector> connectors = new List<IMyShipConnector>();
            GridTerminalSystem.GetBlocksOfType(connectors, c => c.DisplayName == "Stone dispenser" && c.IsSameConstructAs(Me));
            ComponentStatus status = new ComponentStatus
            {
                Name = "Stone dispenser"
            };
            if (connectors.Count == 0)
            {
                status.Color = Color.Red;
                status.Status = "NONE FOUND";
            }
            else
            {
                if (connectors[0].ThrowOut)                
                {
                    status.Color = Color.Green;
                    status.Status = "ON";
                }
                else
                {
                    status.Color = Color.Yellow;
                    status.Status = "OFF";
                }
            }

            return status;
        }

        private ComponentStatus GetMiningDrillStatus()
        {
            List<IMyShipDrill> drills = new List<IMyShipDrill>();
            GridTerminalSystem.GetBlocksOfType(drills, d => d.IsSameConstructAs(Me));
            ComponentStatus status = new ComponentStatus
            {
                Name = "Mining drills"
            };
            if (drills.Count == 0)
            {
                status.Color = Color.Red;
                status.Status = "NONE FOUND";
            }
            else
            {
                var drill = drills[0];
                if (drill.IsActivated)
                {
                    status.Color = Color.Green;
                    status.Status = "ON";
                }
                else
                {
                    status.Color = Color.Yellow;
                    status.Status = "READY";
                }
            }
            return status;
        }

        void Configuration(string argument)
        {

        }
    }
}
