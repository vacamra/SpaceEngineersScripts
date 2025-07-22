using System.Collections.Immutable;
using System.Text;
using Sandbox.Game.Gui;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineersScripts.Utilities;
using VRage;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;
using VRageRender;
using static Sandbox.Game.GUI.MyGuiProgressCompositeTexture;

namespace SpaceEngineersScripts.Autopilots.Autopilot
{
    public class Program : MyGridProgram
    {
        public enum Mode { Off, Cruise, Mining}

        ThrusterAutopilot autopilot;
        GyroControl gyroControl;
        OrientationAutopilot orientationAutopilot;
        DockingApproachesFetcher dockingApproachesFetcher;
        ThrusterAnalysis thrusterAnalysis;
        PositionAutopilot positionAutopilot;
        //TableScreenControl tableScreenControl;
        Mode mode = Mode.Off;
        

        public Program()
        {
            PID yawPid = new PID(50, 0, 25000, this);
            PID pitchPid = new PID(50, 0, 25000, this);
            thrusterAnalysis = new ThrusterAnalysis(this);
            autopilot = new ThrusterAutopilot(this, thrusterAnalysis);
            gyroControl = new GyroControl(this);
            orientationAutopilot = new OrientationAutopilot(this, gyroControl, yawPid, pitchPid);
            dockingApproachesFetcher = new DockingApproachesFetcher(this);
            positionAutopilot = new PositionAutopilot(autopilot, this, thrusterAnalysis);
            //tableScreenControl = new TableScreenControl(this)
            //{
            //    DisplayName = "LCD Panel",
            //    DisplayId = 0
            //};
            //tableScreenControl.AddHeader("Header 1", 0.75f);
            //tableScreenControl.AddHeader("Header 2", 0.25f);

            //tableScreenControl.AddCell("Cell 1");
            //tableScreenControl.AddCell("123");
            //tableScreenControl.AddCell("Cell 2");
            //tableScreenControl.AddCell("123kk");
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Main(string argument)
        {
            if (argument == "turnOn")
            {
                mode = Mode.Cruise;
                autopilot.Velocity = Vector3.Zero;
                SetDampeners(false);                
            } 
            else if (argument == "shutOff")
            {
                mode = Mode.Off;
                SetAllDrills(false);
                SetAllGyroOverrides(false);
                autopilot.DisableAllOverrides();
                SetDampeners(true);
            }
            else if (argument == "mining toggle")
            {
                if (mode == Mode.Cruise)
                {
                    mode = Mode.Mining;
                    SetAllDrills(true);
                    SetAllGyroOverrides(true);
                    autopilot.Velocity = new Vector3(0, 0, -0.5);
                }
                else
                {
                    mode = Mode.Cruise;
                    SetAllDrills(false);
                    SetAllGyroOverrides(false);
                    autopilot.Velocity = Vector3.Zero;
                }                
                
            }
            else if (argument == "speed up")
            {
                autopilot.Velocity -= new Vector3(0, 0, mode == Mode.Cruise ? 10 : 0.05);
            }
            else if (argument == "speed down")
            {
                autopilot.Velocity += new Vector3(0, 0, mode == Mode.Cruise ? 10 : 0.05);
            }
            else if (argument == "rotate")
            {
                var oppositeGyroscope = GridTerminalSystem.GetBlockWithName("Control Seat");
                orientationAutopilot.DesiredOrientation = oppositeGyroscope.WorldMatrix.Backward;                
            } 
            else if (argument == "fetch approaches")
            {
                dockingApproachesFetcher.RequestApproaches();
            }
            else if (argument == "choose approach")
            {
                var waypoint = dockingApproachesFetcher.Approaches[0].Waypoints[0];
                positionAutopilot.ReferenceBlockName = "Main Connector";
                positionAutopilot.TargetPosition = waypoint.Position;
                positionAutopilot.MaxSpeed = waypoint.MaxSpeed;
            }
            

            orientationAutopilot.ReferenceBlock = GridTerminalSystem.GetBlockWithName("Control Seat");
            gyroControl.ReferenceBlock = GridTerminalSystem.GetBlockWithName("Control Seat");

            //tableScreenControl.Render();
            dockingApproachesFetcher.Run();
            
            if (mode != Mode.Off)
            {
                positionAutopilot.Actuate();
                autopilot.Actuate();
                //orientationAutopilot.Actuate();
                //gyroControl.Actuate();                
            }

            //PrintApproaches();
            //PrintThrusterInfo();
        }

        private void SetAllDrills(bool enable)
        {
            List<IMyShipDrill> drills = new List<IMyShipDrill>();
            GridTerminalSystem.GetBlocksOfType(drills, d => d.IsSameConstructAs(Me));
            foreach(var drill in drills)
            {
                drill.Enabled = enable;
            }
        }

        private void SetAllGyroOverrides(bool enable)
        {
            List<IMyGyro> gyros = new List<IMyGyro>();
            GridTerminalSystem.GetBlocksOfType(gyros, g => g.IsSameConstructAs(Me));
            foreach(var gyro in gyros)
            {
                gyro.GyroOverride = enable;
            }
        }

        private void SetDampeners(bool enable)
        {
            List<IMyCockpit> cockpits = new List<IMyCockpit>();
            GridTerminalSystem.GetBlocksOfType(cockpits, c => c.IsSameConstructAs(Me));
            foreach(var cockpit in cockpits)
            {
                cockpit.DampenersOverride = enable;
            }
        }

        private void PrintApproaches()
        {
            var provider = GridTerminalSystem.GetBlockWithName("TestLCD") as IMyTextSurfaceProvider;
            var surface = provider.GetSurface(0);
            surface.ContentType = ContentType.TEXT_AND_IMAGE;
            surface.WriteText($"Approach count: {dockingApproachesFetcher.Approaches.Count}\n", append: false);

            foreach(var approach in dockingApproachesFetcher.Approaches)
            {
                surface.WriteText($"Connector: {approach.ConnectorName}; Approach: {approach.ApproachName}\n", append: true);
                surface.WriteText($"Position: {approach.ConnectorPosition}; Orientation: {approach.ConnectorOrientation}\n", append: true);
                surface.WriteText($"Up: {approach.Up}; Waypoints:\n", append: true);

                foreach(var waypoint in approach.Waypoints)
                {
                    surface.WriteText($"Position: {waypoint.Position}; MaxSpeed: {waypoint.MaxSpeed}\n", append: true);
                }
            }
        }

        private void PrintThrusterInfo()
        {
            List<IMyShipController> controllers = new List<IMyShipController>();
            GridTerminalSystem.GetBlocksOfType(controllers, c => c.IsSameConstructAs(Me));
            var referenceController = controllers[0];
            var thrusterInfo = thrusterAnalysis.AnalyzeThrusters(referenceController);
            //var controller = GridTerminalSystem.GetBlockWithName("Control Seat 2") as IMyShipController;
            //var connector = GridTerminalSystem.GetBlockWithName("Inset Connector 6");
            //var thrusterInfo = thrusterAnalysis.AnalyzeThrusters(controller, connector);
            var provider = GridTerminalSystem.GetBlockWithName("TestLCD") as IMyTextSurfaceProvider;
            var surface = provider.GetSurface(0);
            surface.ContentType = ContentType.TEXT_AND_IMAGE;
            surface.WriteText(""); //clear

            Echo($"Left: {thrusterInfo[Base6Directions.Direction.Left].MaxThrust}");

            foreach(var stats in thrusterInfo)
            {
                surface.WriteText($"{stats.Key}: current {stats.Value.CurrentThrustRatio:0.00}%, max: {stats.Value.MaxThrust:0.00}N\n", append: true);
                surface.WriteText($"      maxAcceleration {stats.Value.MaxAccelerationPerTick:0.00}ms^2, velocity: {stats.Value.CurrentVelocity:0.00}m/s\n", append: true);
                surface.WriteText($"      velocityAfterGravity {stats.Value.CurrentVelocityAfterGravity:0.00}m/s\n\n", append: true);
            }
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

        class DockingApproachesFetcher
        {
            private Program Program { get; set; }
            public List<DockingApproach> Approaches { get; set; } = new List<DockingApproach>();

            public DockingApproachesFetcher(Program program)
            {
                Program = program;                
            }

            public void RequestApproaches()
            {
                Program.IGC.SendBroadcastMessage("DockingQuery", "Query");
                Approaches.Clear();
            }

            public void Run()
            {
                while (Program.IGC.UnicastListener.HasPendingMessage)
                {
                    var message = Program.IGC.UnicastListener.AcceptMessage();
                    Approaches.Add(DockingApproach.Deserialize(message));
                }
            }
        }     

        class OrientationAutopilot
        {
            private Program Program;
            private GyroControl GyroControl;
            private PID YawPid;
            private PID PitchPid;

            public bool EnableManualOverride { get; set; } = true;
            public IMyTerminalBlock ReferenceBlock { get; set; }
            public Vector3D? DesiredOrientation { get; set; }

            public OrientationAutopilot(Program program, GyroControl gyroControl, PID yawPid, PID pitchPid)
            {
                Program = program;
                GyroControl = gyroControl;
                ReferenceBlock = Program.Me;
                YawPid = yawPid;
                PitchPid = pitchPid;
            }     

            public void Actuate()
            {
                if (DesiredOrientation == null) return;                

                Vector3 reference = ReferenceBlock.WorldMatrix.Forward;
                Vector3 target = DesiredOrientation.Value;
                
                Vector3 up = ReferenceBlock.WorldMatrix.Up;
                Vector3 left = ReferenceBlock.WorldMatrix.Left;
                float yaw = ComputeAngleAndMoveReference(ref up, ref reference, ref target);
                float pitch = ComputeAngleAndMoveReference(ref left, ref reference, ref target);                
                
                if (float.IsNaN(yaw))
                {
                    yaw = (float)Math.PI;
                }

                if (float.IsNaN(pitch))
                {
                    pitch = 0;
                }

                GyroControl.Yaw = YawPid.Control(yaw);
                GyroControl.Pitch = PitchPid.Control(pitch);
            }

            private float ComputeAngleAndMoveReference(ref Vector3 planeNormal, ref Vector3 reference, ref Vector3 target)
            {
                Vector3 referenceProjection = Vector3.ProjectOnPlane(ref reference, ref planeNormal).Normalized();
                Vector3 targetProjection = Vector3.ProjectOnPlane(ref target, ref planeNormal).Normalized();
                float angle = (float)Math.Acos(Vector3.Dot(referenceProjection, targetProjection));

                Vector3 cross = Vector3.Cross(referenceProjection, targetProjection);

                float dot = Vector3.Dot(planeNormal, cross);

                if (dot > 0)
                {
                    angle = -angle;
                }

                Matrix rotation = Matrix.CreateFromAxisAngle(planeNormal, angle);
                reference = Vector3.Transform(reference, ref rotation);
                
                return angle;                
            }
        }

        class GyroControl
        {
            public float Roll { get; set; }
            public float Pitch { get; set; }
            public float Yaw { get; set; }
            public IMyTerminalBlock ReferenceBlock { get; set; }

            private Program Program;

            public GyroControl(Program program)
            {
                Program = program;
                ReferenceBlock = Program.Me;
            }

            public void Actuate()
            {
                List<IMyGyro> gyros = new List<IMyGyro>();
                Program.GridTerminalSystem.GetBlocksOfType(gyros, f => f.IsSameConstructAs(Program.Me));
                
                Quaternion baseOrientation;
                ReferenceBlock.Orientation.GetQuaternion(out baseOrientation);
                baseOrientation.W *= -1;

                foreach (var gyro in gyros)
                {                    
                    gyro.GyroOverride = true;

                    Quaternion gyroOrientation;
                    gyro.Orientation.GetQuaternion(out gyroOrientation);
                    Quaternion relativeOrientationQ = baseOrientation * gyroOrientation;
                    MyBlockOrientation relativeOrientation = new MyBlockOrientation(ref relativeOrientationQ);

                    if (relativeOrientation.Up == Base6Directions.Direction.Forward)
                    {
                        gyro.Yaw = -Roll;
                        if (relativeOrientation.Forward == Base6Directions.Direction.Left)
                        {                            
                            gyro.Pitch = Yaw;
                            gyro.Roll = -Pitch;
                        }
                        else if (relativeOrientation.Forward == Base6Directions.Direction.Up)
                        {
                            gyro.Pitch = -Pitch;
                            gyro.Roll = -Yaw;
                        }
                        else if (relativeOrientation.Forward == Base6Directions.Direction.Right)
                        {
                            gyro.Pitch = -Yaw;
                            gyro.Roll = Pitch;
                        }
                        else if (relativeOrientation.Forward == Base6Directions.Direction.Down)
                        {
                            gyro.Pitch = Pitch;
                            gyro.Roll = Yaw;
                        }                        
                    } 
                    else if (relativeOrientation.Up == Base6Directions.Direction.Backward)
                    {
                        gyro.Yaw = Roll;
                        if (relativeOrientation.Forward == Base6Directions.Direction.Left)
                        {                            
                            gyro.Pitch = -Yaw;
                            gyro.Roll = -Pitch;
                        }
                        else if (relativeOrientation.Forward == Base6Directions.Direction.Up)
                        {
                            gyro.Pitch = Pitch;
                            gyro.Roll = -Yaw;
                        }
                        else if (relativeOrientation.Forward == Base6Directions.Direction.Right)
                        {
                            gyro.Pitch = Yaw;
                            gyro.Roll = Pitch;
                        }
                        else if (relativeOrientation.Forward == Base6Directions.Direction.Down)
                        {
                            gyro.Pitch = -Pitch;
                            gyro.Roll = Yaw;
                        }
                    }
                    else if (relativeOrientation.Up == Base6Directions.Direction.Right)
                    {
                        gyro.Yaw = -Pitch;
                        if (relativeOrientation.Forward == Base6Directions.Direction.Forward)
                        {                            
                            gyro.Pitch = Yaw;
                            gyro.Roll = Roll;
                        }
                        else if (relativeOrientation.Forward == Base6Directions.Direction.Up)
                        {
                            gyro.Pitch = Roll;
                            gyro.Roll = -Yaw;
                        }
                        else if (relativeOrientation.Forward == Base6Directions.Direction.Backward)
                        {
                            gyro.Pitch = -Yaw;
                            gyro.Roll = -Roll;
                        }
                        else if (relativeOrientation.Forward == Base6Directions.Direction.Down)
                        {
                            gyro.Pitch = -Roll;
                            gyro.Roll = Yaw;
                        }
                    } 
                    else if (relativeOrientation.Up == Base6Directions.Direction.Left)
                    {
                        gyro.Yaw = Pitch;
                        if (relativeOrientation.Forward == Base6Directions.Direction.Forward)
                        {                            
                            gyro.Pitch = -Yaw;
                            gyro.Roll = Roll;
                        }
                        else if (relativeOrientation.Forward == Base6Directions.Direction.Up)
                        {
                            gyro.Pitch = -Roll;
                            gyro.Roll = -Yaw;
                        }
                        else if (relativeOrientation.Forward == Base6Directions.Direction.Backward)
                        {
                            gyro.Pitch = Yaw;
                            gyro.Roll = -Roll;
                        }
                        else if (relativeOrientation.Forward == Base6Directions.Direction.Down)
                        {
                            gyro.Pitch = Roll;
                            gyro.Roll = Yaw;
                        }
                    }
                    else if (relativeOrientation.Up == Base6Directions.Direction.Up)
                    {
                        gyro.Yaw = Yaw;
                        if (relativeOrientation.Forward == Base6Directions.Direction.Forward)
                        {                            
                            gyro.Pitch = Pitch;
                            gyro.Roll = Roll;
                        }
                        else if (relativeOrientation.Forward == Base6Directions.Direction.Left)
                        {
                            gyro.Pitch = Roll;
                            gyro.Roll = -Pitch;
                        }
                        else if (relativeOrientation.Forward == Base6Directions.Direction.Backward)
                        {
                            gyro.Pitch = -Pitch;
                            gyro.Roll = -Roll;
                        }
                        else if (relativeOrientation.Forward == Base6Directions.Direction.Right)
                        {
                            gyro.Pitch = -Roll;
                            gyro.Roll = Pitch;
                        }
                    }
                    else if (relativeOrientation.Up == Base6Directions.Direction.Down)
                    {
                        gyro.Yaw = -Yaw;
                        if (relativeOrientation.Forward == Base6Directions.Direction.Forward)
                        {
                            gyro.Pitch = -Pitch;
                            gyro.Roll = Roll;
                        }
                        else if (relativeOrientation.Forward == Base6Directions.Direction.Left)
                        {
                            gyro.Pitch = -Roll;
                            gyro.Roll = -Pitch;
                        }
                        else if (relativeOrientation.Forward == Base6Directions.Direction.Backward)
                        {
                            gyro.Pitch = Pitch;
                            gyro.Roll = -Roll;
                        }
                        else if (relativeOrientation.Forward == Base6Directions.Direction.Right)
                        {
                            gyro.Pitch = Roll;
                            gyro.Roll = Pitch;
                        }
                    }
                }
            }
        }

        class LogScreenControl
        {
            public string DisplayName { get; set; }
            public int DisplayId { get; set; }
            public Color BackgroundColor { get; set; } = Color.Black;
            public Color TextColor { get; set; } = Color.White;
            public float Scale { get; set; } = 1f;

            private Queue<string> Messages = new Queue<string>();
            private Program Program;

            public LogScreenControl(Program program)
            {
                Program = program;
            }

            public void Render()
            {
                var display = Program.GridTerminalSystem.GetBlockWithName(DisplayName) as IMyTextSurfaceProvider;
                var screen = display?.GetSurface(DisplayId);
                if (screen == null)
                {
                    Program.Echo($"Screen {DisplayName}[{DisplayId}] does not exist");
                    return;
                }

                screen.ContentType = ContentType.TEXT_AND_IMAGE;
                screen.BackgroundColor = BackgroundColor;
                screen.FontColor = TextColor;
                screen.FontSize = Scale;

                screen.WriteText(string.Empty, append: false);
                foreach(var message in Messages)
                {
                    screen.WriteText(message, append: true);
                }
            }

            public void AddMessage(string message)
            {
                if (Messages.Count > 20)
                {
                    Messages.Dequeue();
                }
                Messages.Enqueue(message + "\n");
            }
        }

        class TableScreenControl
        {
            public class Cell
            {
                public string Text { get; set; }
                public Color Color { get; set; }
                public float WidthRatio { get; set; }
                public int Margin { get; set; }
                public float Scale { get; set; }
            }

            public Color BackgroundColor { get; set; } = Color.Black;
            public int DefaultMargin { get; set; } = 4;
            public Color DefaultColor { get; set; } = Color.Blue;
            public float DefaultScale { get; set; } = 1f;
            public string DisplayName { get; set; }
            public int DisplayId { get; set; }
            public int RowGap { get; set; } = 4;
            private int DefaultRowSize = 24;

            private Program Program;
            private List<Cell> Headers = new List<Cell>();
            private List<List<Cell>> Data = new List<List<Cell>>();            

            public TableScreenControl(Program program)
            {
                Program = program;
            }

            public void Render()
            {
                var display = Program.GridTerminalSystem.GetBlockWithName(DisplayName) as IMyTextSurfaceProvider;
                var screen = display?.GetSurface(DisplayId);
                if (screen == null)
                {
                    Program.Echo($"Screen {DisplayName}[{DisplayId}] does not exist");
                    return;
                }

                RectangleF viewPort = new RectangleF((screen.TextureSize - screen.SurfaceSize) / 2f, screen.SurfaceSize);
                screen.ContentType = ContentType.SCRIPT;
                screen.Script = string.Empty;
                screen.ScriptBackgroundColor = BackgroundColor;

                var frame = screen.DrawFrame();
                int offsetX = 0;
                int offsetY = 0;
                int headerHeight = (int)(Headers[0].Scale * DefaultRowSize + RowGap);
                foreach (var header in Headers)
                {                        
                    int columnWidth = (int)(header.WidthRatio * viewPort.Width -  2 * header.Margin);
                    RenderCell(ref frame, offsetX, offsetY, header, columnWidth, headerHeight, viewPort.Position);                    
                    offsetX += columnWidth;
                }                    

                offsetY += headerHeight;
                    
                foreach(var row in Data)
                {
                    int rowHeight = (int)(row[0].Scale * DefaultRowSize + RowGap);
                    offsetX = 0;

                    for (int i = 0; i < row.Count; i++)
                    {
                        var cell = row[i];
                        int columnWidth = (int)(cell.WidthRatio * viewPort.Width - 2 * cell.Margin);
                        RenderCell(ref frame, offsetX, offsetY, cell, columnWidth, rowHeight, viewPort.Position);                            
                        offsetX += columnWidth;
                    }

                    offsetY += rowHeight;
                }

                frame.Dispose();
            }

            private void RenderCell(ref MySpriteDrawFrame frame, int offsetX, int offsetY, Cell cell, int columnWidth, int rowHeight, Vector2 basePosition)
            {
                using (frame.Clip(offsetX + cell.Margin, offsetY, columnWidth - 2 * cell.Margin, rowHeight))
                {
                    frame.Add(new MySprite()
                    {
                        Type = SpriteType.TEXT,
                        Data = cell.Text,
                        Position = basePosition + new Vector2(offsetX + cell.Margin, offsetY),
                        RotationOrScale = cell.Scale,
                        Color = cell.Color,
                        Alignment = TextAlignment.LEFT,
                        FontId = "White"
                    });
                }
            }

            public void ClearHeaders()
            {
                Headers.Clear();
            }

            public void AddHeader(string text, float widthRatio, int? margin = null, Color? color = null, float? scale = null)
            {
                Headers.Add(new Cell()
                {
                    Text = text,
                    WidthRatio = widthRatio,
                    Margin = margin ?? DefaultMargin,
                    Color = color ?? DefaultColor,
                    Scale = scale ?? DefaultScale
                });
            }

            public void ClearData()
            {
                Data.Clear();
            }

            public void AddCell(string text, Color? color = null, float? scale = null)
            {
                int row;
                int column;
                if (Data.Count == 0)
                {
                    row = 0; 
                    column = 0;
                    Data.Add(new List<Cell>());
                }
                else if (Data[Data.Count - 1].Count == Headers.Count)
                {
                    row = Data.Count;
                    column = 0;
                    Data.Add(new List<Cell>());
                }
                else
                {
                    row = Data.Count - 1;
                    column = Data[Data.Count - 1].Count;
                }

                Data[row].Add(new Cell()
                {
                    Text = text,
                    Color = color ?? DefaultColor,
                    Margin = Headers[column].Margin,
                    Scale = scale ?? Headers[column].Scale,
                    WidthRatio = Headers[column].WidthRatio
                });
            }
        }
        
        class CameraControl
        {
            public struct PointF
            {
                public float X;
                public float Y;

                public static PointF Zero = new PointF();

                public PointF(float x, float y)
                {
                    X = x;
                    Y = y;
                }

                public static bool operator ==(PointF a, PointF b)
                {
                    return a.X == b.X && a.Y == b.Y;
                }

                public static bool operator !=(PointF a, PointF b) {
                    return !(a == b); 
                }
            }

            public string CameraName { get; set; } = "Camera";
            // offsets from the center of camera in meters, positive (both of them)
            public PointF TopLeftCorner { get; set; } = PointF.Zero;
            public PointF BottomRightCorner { get; set; } = PointF.Zero;
            public float AngularResolution { get; set; }
            public float MaxDistance { get; set; } = 3000;            
            public Dictionary<PointF, MyDetectedEntityInfo?> DetectedObjects { get; } = new Dictionary<PointF, MyDetectedEntityInfo?>();
            public PointF LastCheckedOffset { get; private set; } = PointF.Zero;

            private Program Program;
            private IEnumerator<PointF> offsets { get; set; } = Enumerable.Empty<PointF>().GetEnumerator();

            public CameraControl(Program program)
            {
                Program = program;
            }

            public void RunRaycast()
            {
                if (LastCheckedOffset == offsets.Current)
                {
                    if (!offsets.MoveNext())
                    {
                        offsets = GenerateOffsets().GetEnumerator();
                        offsets.MoveNext();
                    }
                }                

                PointF offset = offsets.Current;

                var camera = Program.GridTerminalSystem.GetBlockWithName(CameraName) as IMyCameraBlock;
                camera.EnableRaycast = true;
                var distance = ComputeMaxDistance(offset);
                if (camera.AvailableScanRange >= distance)
                {
                    var entity = camera.Raycast(distance, offset.Y, offset.X);
                    LastCheckedOffset = offset;
                    DetectedObjects[offset] = entity;                    
                }
            }

            public float GetClosestObjectDistance()
            {
                var camera = Program.GridTerminalSystem.GetBlockWithName(CameraName) as IMyCameraBlock;
                var cameraPosition = camera.GetPosition();

                double minDistanceSquared = double.MaxValue;
                foreach (var detectedObject in DetectedObjects.Values)
                {
                    if (detectedObject == null || detectedObject.Value.IsEmpty()) continue;

                    var hitPosition = detectedObject.Value.HitPosition.Value;
                    var distanceSquared = (hitPosition - cameraPosition).LengthSquared();
                    minDistanceSquared = MathHelper.Min(minDistanceSquared, distanceSquared);
                }

                return (float)Math.Sqrt(minDistanceSquared);
            }

            private float ComputeMaxDistance(PointF offset)
            {
                float xDistance, yDistance;
                float width = offset.X > 0 ? BottomRightCorner.X : TopLeftCorner.X;
                float height = offset.Y > 0 ? TopLeftCorner.Y : BottomRightCorner.Y;

                if (offset.X != 0)
                {
                    xDistance = width / (float)Math.Cos(offset.X);
                }
                else
                {
                    xDistance = float.MaxValue;
                }

                if (offset.Y != 0)
                {
                    yDistance = height / (float)Math.Cos(offset.Y);
                }
                else
                {
                    yDistance = float.MaxValue;
                }

                return MathHelper.Min(xDistance, yDistance);
            }

            private IEnumerable<PointF> GenerateOffsets()
            {
                for(float yawOffset = -45; yawOffset <= 45; yawOffset += AngularResolution)
                {
                    for(float pitchOffset = -45; pitchOffset <= 45; pitchOffset += AngularResolution)
                    {
                        yield return new PointF(yawOffset, pitchOffset);
                    }
                }
                
            }
        }



        class PositionAutopilot
        {
            private ThrusterAutopilot ThrusterAutopilot;
            private ThrusterAnalysis ThrusterAnalysis;
            private Program Program;
            public Vector3? TargetPosition { get; set; }
            public float MaxSpeed { get; set; }
            public string ReferenceBlockName { get; set; }
            public float Distance
            {
                get
                {
                    var referenceBlock = Program.GridTerminalSystem.GetBlockWithName(ReferenceBlockName);
                    return referenceBlock != null && TargetPosition != null ? float.NaN : Vector3.Subtract(referenceBlock.GetPosition(), TargetPosition.Value).Length();
                }
            }

            public PositionAutopilot(ThrusterAutopilot thrusterAutopilot, Program program, ThrusterAnalysis thrusterAnalysis)
            {
                ThrusterAutopilot = thrusterAutopilot;
                Program = program;
                ThrusterAnalysis = thrusterAnalysis;
            }

            public void Actuate()
            {
                var provider = Program.GridTerminalSystem.GetBlockWithName("TestLCD") as IMyTextSurfaceProvider;
                var surface = provider.GetSurface(0);
                surface.ContentType = ContentType.TEXT_AND_IMAGE;

                if (ReferenceBlockName == null) return;

                var referenceBlock = Program.GridTerminalSystem.GetBlockWithName(ReferenceBlockName);

                if (referenceBlock == null || TargetPosition == null || MaxSpeed == 0f) return;

                List<IMyShipController> controllers = new List<IMyShipController>();
                Program.GridTerminalSystem.GetBlocksOfType(controllers, c => c.IsSameConstructAs(Program.Me));
                var controller = controllers[0];

                var thrusterInfo = ThrusterAnalysis.AnalyzeThrusters(controller);

                var diff = TargetPosition.Value - (Vector3)referenceBlock.GetPosition();
                var relativeDiff = Vector3.TransformNormal(diff, MatrixD.Transpose(controller.WorldMatrix));

                surface.WriteText($"bwd: {relativeDiff.Z:0.0}\nup: {relativeDiff.Y:0.0}\nright: {relativeDiff.X:0.0}\n");
                Program.Echo($"RelativeDiff: {relativeDiff}");

                float right = SetThrusters(thrusterInfo, Base6Directions.Direction.Right, relativeDiff.X);
                float up = SetThrusters(thrusterInfo, Base6Directions.Direction.Up, relativeDiff.Y);
                float backward = SetThrusters(thrusterInfo, Base6Directions.Direction.Backward, relativeDiff.Z);                
                var velocity = new Vector3(right, up, backward);
                if (velocity.Length() > MaxSpeed)
                {
                    velocity *= MaxSpeed / velocity.Length();
                }               

                surface.WriteText($"bwd: {velocity.Z:0.0}\nup: {velocity.Y:0.0}\nright:{-velocity.X:0.0}", true);

                ThrusterAutopilot.Velocity = velocity;
                /*                ActuateThrusters2(thrusterInfo, Base6Directions.Direction.Right, Velocity.X);
                ActuateThrusters2(thrusterInfo, Base6Directions.Direction.Up, Velocity.Y);
                ActuateThrusters2(thrusterInfo, Base6Directions.Direction.Backward, Velocity.Z);*/
            }

            private float SetThrusters(Dictionary<Base6Directions.Direction, ThrusterAnalysis.ThrusterInfo> thrusterInfo, Base6Directions.Direction direction, float relativePosition)
            {
                if (relativePosition < 0)
                {
                    direction = Base6Directions.GetOppositeDirection(direction);
                }

                var currentVelocity = thrusterInfo[direction].CurrentVelocity;
                var fps = 1 / Program.Runtime.LastRunTimeMs;
                var acceleration = thrusterInfo[direction].MaxAccelerationPerTick * fps;
                var deceleration = thrusterInfo[Base6Directions.GetOppositeDirection(direction)].MaxAccelerationPerTick * fps;

                var maxVelocity = (float)Math.Sqrt((currentVelocity * currentVelocity * deceleration + 2 * Math.Abs(relativePosition) * acceleration * deceleration) / (acceleration + deceleration));

                var result = maxVelocity > currentVelocity ? maxVelocity : 0;
                if (relativePosition < 0) result *= -1;

                return result;
            }
        }

        class ThrusterAutopilot
        {
            public Vector3 Velocity { get; set; } = Vector3.Zero;            
            public List<ThrusterOutput> LastThrusterOutputs { get; } = new List<ThrusterOutput>();

            private Program Program;
            private ThrusterAnalysis ThrusterAnalysis;
            private IMyGridTerminalSystem GridTerminalSystem => Program.GridTerminalSystem;
            private IMyProgrammableBlock Me => Program.Me;

            public struct ThrusterOutput
            {
                public Base6Directions.Direction Direction { get; set; }
                public float ThrusterOutputRatio { get; set; }
            }

            public ThrusterAutopilot(Program program, ThrusterAnalysis thrusterAnalysis)
            {
                Program = program;
                ThrusterAnalysis = thrusterAnalysis;
            }

            public void Actuate()
            {
                List<IMyShipController> controllers = new List<IMyShipController>();
                GridTerminalSystem.GetBlocksOfType(controllers, c => c.IsSameConstructAs(Me));
                var referenceController = controllers[0];
                var thrusterInfo = ThrusterAnalysis.AnalyzeThrusters(referenceController);
             
                LastThrusterOutputs.Clear();
                ActuateThrusters2(thrusterInfo, Base6Directions.Direction.Right, Velocity.X);
                ActuateThrusters2(thrusterInfo, Base6Directions.Direction.Up, Velocity.Y);
                ActuateThrusters2(thrusterInfo, Base6Directions.Direction.Backward, Velocity.Z);
            }

            public void DisableAllOverrides()
            {
                List<IMyThrust> thrusters = new List<IMyThrust>();
                GridTerminalSystem.GetBlocksOfType(thrusters, t => t.IsSameConstructAs(Me) && t.IsFunctional && t.Enabled);
                foreach(var thruster in thrusters)
                {
                    thruster.ThrustOverridePercentage = 0;
                }
            }

            private void ActuateThrusters2(Dictionary<Base6Directions.Direction, ThrusterAnalysis.ThrusterInfo> thrusterInfo, Base6Directions.Direction direction, float desiredVelocity)
            {
                var desiredChange = desiredVelocity - thrusterInfo[direction].CurrentVelocityAfterGravity;

                if (desiredChange < 0f)
                {
                    desiredChange = -desiredChange;
                    direction = Base6Directions.GetFlippedDirection(direction);
                }

                var ratio = desiredChange / thrusterInfo[direction].MaxAccelerationPerTick;

                foreach(var thruster in thrusterInfo[direction].Thrusters)
                {
                    thruster.ThrustOverridePercentage = MathHelper.Min(ratio, 1);
                    LastThrusterOutputs.Add(new ThrusterOutput
                    {
                        Direction = direction,
                        ThrusterOutputRatio = thruster.ThrustOverridePercentage
                    });
                }
                foreach(var reverseThruster in thrusterInfo[Base6Directions.GetFlippedDirection(direction)].Thrusters)
                {
                    reverseThruster.ThrustOverridePercentage = 0f;
                }
            }
        }

        public class ThrusterAnalysis
        {
            private Program Program;

            public class ThrusterInfo
            {
                public Base6Directions.Direction Direction { get; set; }
                public float MaxThrust { get; set; }
                public float MaxAccelerationPerTick { get; set; }
                public float CurrentThrustRatio { get; set; }
                public float CurrentVelocity { get; set; }
                public float CurrentVelocityAfterGravity { get; set; }
                public List<IMyThrust> Thrusters { get; set; }
            }

            public ThrusterAnalysis(Program program)
            {
                Program = program;
            }

            public Dictionary<Base6Directions.Direction, ThrusterInfo> AnalyzeThrusters(IMyShipController referenceController)
            {
                List<IMyThrust> thrusters = new List<IMyThrust>();
                Program.GridTerminalSystem.GetBlocksOfType(thrusters, t => t.IsFunctional && t.Enabled && t.IsSameConstructAs(Program.Me));

                var shipMass = referenceController.CalculateShipMass().TotalMass;
                var currentGravity = referenceController.GetNaturalGravity();
                var gravityVector = (Vector3)(currentGravity * Program.Runtime.TimeSinceLastRun.TotalSeconds);                
                var currentWorldVelocity = (Vector3)(referenceController.GetShipVelocities().LinearVelocity + currentGravity * Program.Runtime.TimeSinceLastRun.TotalSeconds);
                var relativeVelocity = Vector3.TransformNormal((Vector3)referenceController.GetShipVelocities().LinearVelocity, MatrixD.Transpose(referenceController.WorldMatrix));
                var relativeVelocityAfterGravity = Vector3.TransformNormal(currentWorldVelocity, MatrixD.Transpose(referenceController.WorldMatrix));

                var result = new Dictionary<Base6Directions.Direction, ThrusterInfo>();
                AnalyzeThrusterDirection(thrusters, result, Base6Directions.Direction.Forward, referenceController, shipMass, -relativeVelocity.Z, -relativeVelocityAfterGravity.Z);
                AnalyzeThrusterDirection(thrusters, result, Base6Directions.Direction.Backward, referenceController, shipMass, relativeVelocity.Z, relativeVelocityAfterGravity.Z);
                AnalyzeThrusterDirection(thrusters, result, Base6Directions.Direction.Left, referenceController, shipMass, -relativeVelocity.X, -relativeVelocityAfterGravity.X);
                AnalyzeThrusterDirection(thrusters, result, Base6Directions.Direction.Right, referenceController, shipMass, relativeVelocity.X, relativeVelocityAfterGravity.X);
                AnalyzeThrusterDirection(thrusters, result, Base6Directions.Direction.Up, referenceController, shipMass, relativeVelocity.Y, relativeVelocityAfterGravity.Y);
                AnalyzeThrusterDirection(thrusters, result, Base6Directions.Direction.Down, referenceController, shipMass, -relativeVelocity.Y, -relativeVelocityAfterGravity.Y);                
                return result;
            }

            private void AnalyzeThrusterDirection(List<IMyThrust> thrusters,
                Dictionary<Base6Directions.Direction, ThrusterInfo> result, 
                Base6Directions.Direction direction,
                IMyTerminalBlock referenceBlock,
                float shipMass,
                float currentVelocity,
                float currentVelocityAfterGravity)
            {
                var relevantThrusters = thrusters.Where(t => Base6Directions.GetOppositeDirection(t.Orientation.Forward) == referenceBlock.Orientation.TransformDirection(direction)).ToList();
                var maxThrust = relevantThrusters.Sum(t => t.MaxEffectiveThrust);
                var currentThrust = relevantThrusters.Sum(t => t.CurrentThrust);
                var maxAccelerationPerTick = (maxThrust / shipMass) * (float)Program.Runtime.TimeSinceLastRun.TotalSeconds;

                result[direction] = new ThrusterInfo
                {
                    Direction = direction,
                    MaxThrust = maxThrust,
                    MaxAccelerationPerTick = maxAccelerationPerTick,
                    CurrentThrustRatio = currentThrust / maxThrust,
                    CurrentVelocity = currentVelocity,
                    CurrentVelocityAfterGravity = currentVelocityAfterGravity,
                    Thrusters = relevantThrusters
                };
            }
        }

        public class PID
        {
            public float P { get; set; } = 0;
            public float I { get; set; } = 0;
            public float D { get; set; } = 0;

            float errorSum = 0;
            float lastError = 0;
            bool firstRun = true;
            Program program;
            
            protected float timeSinceLastRunMs
            {
                get
                {
                    return (float)program.Runtime.TimeSinceLastRun.TotalMilliseconds;
                }
            }

            public PID(float p, float i, float d, Program program)
            {
                P = p;
                I = i;
                D = d;
                this.program = program;
            }

            protected virtual float GetIntegral(float currentError, float errorSum)
            {
                return errorSum + currentError * timeSinceLastRunMs;
            }

            public float Control(float error)
            {
                float errorDerivative = (error - lastError) / timeSinceLastRunMs;

                if (firstRun)
                {
                    errorDerivative = 0;
                    firstRun = false;
                }

                errorSum = GetIntegral(error, errorSum);
                lastError = error;

                program.Echo($"P: {P * error},\n I: {I * errorSum},\n D: {D * errorDerivative}\n");

                return P * error + I * errorSum + D * errorDerivative;
            }

            public virtual void Reset()
            {
                errorSum = 0;
                lastError = 0;
                firstRun = true;
            }
        }
    }
}
