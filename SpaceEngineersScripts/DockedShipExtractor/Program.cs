using System.Diagnostics;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.Voxels;
using VRage.UserInterface.Media;
using VRageMath;

namespace SpaceEngineersScripts.DockedShipExtractor
{
    public class Program: MyGridProgram
    {
        /*
            
        */
        // Configuration
        const string targetStorageGroup = "Main Storage";


        // Script start

        private void Status(bool success, string status)
        {            
            var surface = Me.GetSurface(0);
            surface.ContentType = ContentType.TEXT_AND_IMAGE;
            surface.BackgroundColor = success ? Color.Blue : Color.Red;
            surface.FontColor = Color.White;
            surface.FontSize = 2;
            surface.WriteText($"Extract ship\n" +
                $"Status: \n" +
                $"{status}", false);
        }

        public void Main(string argument)
        {
            var connector = GridTerminalSystem.GetBlockWithName(argument) as IMyShipConnector;

            if (connector == null)
            {
                Status(false, $"block with {argument} not found or not a ship connector");
                return;
            }

            if (!connector.IsConnected)
            {
                Status(false, "no ship attached");
                return;
            }

            List<IMyCargoContainer> targetStorage = new List<IMyCargoContainer>();
            GridTerminalSystem.GetBlockGroupWithName(targetStorageGroup).GetBlocksOfType(targetStorage);

            if (targetStorage.Count == 0)
            {
                Status(false, $"group {targetStorageGroup} is either empty or contains no storage blocks");
                return;
            }

            var otherShipConnector = connector.OtherConnector;
            List<IMyTerminalBlock> sourceStorage = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType(sourceStorage, block => otherShipConnector.IsSameConstructAs(block) && block.HasInventory);

            foreach(var source in sourceStorage)
            {
                for(int i = 0; i < source.InventoryCount; i++)
                {
                    var sourceInventory = source.GetInventory(i);
                    
                    List<MyInventoryItem> items = new List<MyInventoryItem>();
                    sourceInventory.GetItems(items);

                    for(int itemIndex = 0; itemIndex < items.Count; itemIndex++)
                    {
                        var item = items[itemIndex]; 
                        float volume = item.Type.GetItemInfo().Volume;
                        MyFixedPoint leftToTransfer = item.Amount;
                        MyFixedPoint leftToTransferVolume = leftToTransfer * volume;
                        foreach(var target in targetStorage)
                        {
                            if (leftToTransfer == 0) break;

                            var targetInventory = target.GetInventory(0);
                            if (targetInventory.IsFull) continue;
                            if (!sourceInventory.CanTransferItemTo(targetInventory, item.Type))
                            {
                                Echo($"Could not move item from {source.Name} to {target.Name}");
                                continue;
                            }                                                                                   
                                                        
                            var availableSpace = targetInventory.MaxVolume - targetInventory.CurrentVolume;

                            var maxTransfer = (float)availableSpace / volume;

                            var transferCount = MyFixedPoint.Max((MyFixedPoint)maxTransfer, leftToTransfer);

                            var success = sourceInventory.TransferItemTo(targetInventory, itemIndex, null, true, transferCount);

                            leftToTransfer -= transferCount;
                        }
                    }
                }
            }

            Status(true, "OK");
        }
    }
}
