﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEngineersScripts.Utilities
{
    internal class ItemsDisplayStringToInternal
    {
        private Dictionary<string, string> displayStringToInternalMap = new Dictionary<string, string>()
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
    }
}
