using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Sandbox.ModAPI.Ingame;
using VRage;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace SpaceEngineersScripts.DockingApproachReporter
{
    internal class Program : MyGridProgram
    {
        string ReferenceBlockName = "Cockpit";
        DockingApproachesProvider dockingApproachesProvider;

        public Program()
        {
            dockingApproachesProvider = new DockingApproachesProvider(this)
            {
                ReferenceBlockName = ReferenceBlockName
            };
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string argument)
        {
            dockingApproachesProvider.Run();
        }

        class DockingApproach
        {
            public string ConnectorName { get; set; }
            public string ApproachName { get; set; }
            public Vector3D Up { get; set; }
            public Vector3D ConnectorOrientation { get; set; }
            public Vector3D ConnectorPosition { get; set; }
            public List<Waypoint> Waypoints { get; set; } = new List<Waypoint>();

            public class Waypoint
            {
                public Vector3D Position { get; set; }
                public float MaxSpeed { get; set; }
            }

            public MyTuple<string, string, Vector3D, Vector3D, Vector3D, ImmutableList<MyTuple<Vector3D, float>>> Serialize()
            {
                return new MyTuple<string, string, Vector3D, Vector3D, Vector3D, ImmutableList<MyTuple<Vector3D, float>>>
                    (ConnectorName, ApproachName, Up, ConnectorOrientation, ConnectorPosition, Waypoints.Select(w => new MyTuple<Vector3D, float>(w.Position, w.MaxSpeed)).ToImmutableList());
            }

            public static DockingApproach Deserialize(MyIGCMessage rawMessage)
            {
                var message = rawMessage.As<MyTuple<string, string, Vector3D, Vector3D, Vector3D, ImmutableList<MyTuple<Vector3D, float>>>>();
                return new DockingApproach
                {
                    ConnectorName = message.Item1,
                    ApproachName = message.Item2,
                    Up = message.Item3,
                    ConnectorOrientation = message.Item4,
                    ConnectorPosition = message.Item5,
                    Waypoints = message.Item6.Select(w => new Waypoint
                    {
                        Position = w.Item1,
                        MaxSpeed = w.Item2,
                    }).ToList()
                };
            }
        }

        class DockingApproachesProvider
        {
            private Program Program { get; set; }
            private IMyBroadcastListener dockingListener;
            public string ReferenceBlockName { get; set; } = "Cockpit";

            public DockingApproachesProvider(Program program)
            {
                Program = program;
                dockingListener = Program.IGC.RegisterBroadcastListener("DockingQuery");
            }

            public void Run()
            {
                while (dockingListener.HasPendingMessage)
                {
                    var message = dockingListener.AcceptMessage();
                    if (message.Data as string == "Query")
                    {
                        ReportOptions(message.Source);
                    }
                }
            }

            private void ReportOptions(long target)
            {
                List<IMyShipConnector> connectors = new List<IMyShipConnector>();
                Program.GridTerminalSystem.GetBlocksOfType(connectors, c => c.IsSameConstructAs(Program.Me) && MyIni.HasSection(c.CustomData, "AutoDock"));
                MyIni iniParser = new MyIni();

                var reference = Program.GridTerminalSystem.GetBlockWithName(ReferenceBlockName).WorldMatrix;

                foreach (var connector in connectors)
                {
                    iniParser.TryParse(connector.CustomData);

                    string connectorName = iniParser.Get("AutoDock", "Name").ToString();
                    var upDirection = ParseDirection(iniParser.Get("AutoDock", "Up").ToString());
                    var leftDirection = ParseDirection(iniParser.Get("AutoDock", "Left").ToString());
                    var connectorPosition = connector.WorldMatrix.Translation;
                    var connectorOrientation = connector.WorldMatrix.Forward;

                    var dockingOption = new DockingApproach()
                    {
                        ConnectorName = connectorName,
                        ConnectorPosition = connectorPosition,
                        ConnectorOrientation = connectorOrientation,
                        Up = reference.GetDirectionVector(upDirection)
                    };                        

                    List<string> sections = new List<string>();
                    iniParser.GetSections(sections);
                    foreach (var section in sections)
                    {
                        if (!section.StartsWith("DockingApproach")) continue;

                        string approachName = section.Substring("DockingApproach ".Length);

                        dockingOption.ApproachName = approachName;

                        string waypoints = iniParser.Get(section, "Waypoints").ToString();
                        var waypointsParsed = waypoints.Split('\n');

                        var waypointList = new List<DockingApproach.Waypoint>();
                        foreach (var waypoint in waypointsParsed)
                        {
                            List<float> numbers = waypoint.Split(',').Select(float.Parse).ToList();

                            var waypointPosition = connectorPosition
                                + numbers[0] * connectorOrientation
                                + numbers[1] * reference.GetDirectionVector(leftDirection)
                                + numbers[2] * reference.GetDirectionVector(upDirection);

                            waypointList.Add(new DockingApproach.Waypoint
                            {
                                Position = waypointPosition,
                                MaxSpeed = numbers[3]
                            });
                        }
                        dockingOption.Waypoints = waypointList;
                        Program.IGC.SendUnicastMessage(target, "DockingOption", dockingOption.Serialize());
                    }
                }
            }

            private Base6Directions.Direction ParseDirection(string direction)
            {
                switch (direction)
                {
                    case "Up": return Base6Directions.Direction.Up;
                    case "Down": return Base6Directions.Direction.Down;
                    case "Forward": return Base6Directions.Direction.Forward;
                    case "Backward": return Base6Directions.Direction.Backward;
                    case "Left": return Base6Directions.Direction.Left;
                    case "Right": return Base6Directions.Direction.Right;
                }

                throw new Exception($"Cannot parse direction: {direction}");
            }
        }
    }
}
