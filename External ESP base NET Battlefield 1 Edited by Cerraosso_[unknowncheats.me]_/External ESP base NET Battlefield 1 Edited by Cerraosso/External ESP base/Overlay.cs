/////////////XTREME HACK////////////////
///////////unknowncheats.me/////////////

using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

using Factory = SharpDX.Direct2D1.Factory;
using FontFactory = SharpDX.DirectWrite.Factory;
using Format = SharpDX.DXGI.Format;

using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using System.Runtime.InteropServices;

namespace External_ESP_Base
{
    public partial class Overlay : Form
    {
        // Process
        private Process process = null;
        private Thread updateStream = null, windowStream = null;

        // Game Data
        private List<Player> players = null;
        private Player localPlayer = null;
        private Matrix viewProj, m_ViewMatrixInverse;
        private int spectatorCount = 0;

        // Keys Control
        private KeysManager manager;
        // Handle
        private IntPtr handle;

        // Color
        private Color enemyColor = new Color(255, 0, 0, 200),
            enemyColorVisible = new Color(255, 255, 0, 220),
            enemyColorVehicle = new Color(255, 129, 72, 200),
            enemySkeletonColor = new Color(245, 114, 0, 255),
            friendlyColor = new Color(0, 255, 0, 200),
            friendlyColorVehicle = new Color(64, 154, 200, 255),
            friendSkeletonColor = new Color(46, 228, 213, 255);

        // Settings
        private bool ESP_Box = true, 
            ESP_Bone = true,
            ESP_Health = false,
            ESP_Distance = false,
            ESP_Name = false;

        // SharpDX
        private WindowRenderTarget device;
        private HwndRenderTargetProperties renderProperties;
        private SolidColorBrush solidColorBrush;
        private Factory factory;
        private bool IsResize = false;
        private bool IsMinimized = false;

        // SharpDX Font
        private TextFormat font, fontSmall;
        private FontFactory fontFactory;
        private const string fontFamily = "Calibri";
        private const float fontSize = 18.0f;
        private const float fontSizeSmall = 14.0f;

        // Screen Size
        private Rectangle rect;

        // Init
        public Overlay(Process process)
        {
            this.process = process;
            this.handle = Handle;

            int initialStyle = Managed.GetWindowLong(this.Handle, -20);
            Managed.SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);

            IntPtr HWND_TOPMOST = new IntPtr(-1);
            const UInt32 SWP_NOSIZE = 0x0001;
            const UInt32 SWP_NOMOVE = 0x0002;
            const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

            Managed.SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);  
            OnResize(null);

            InitializeComponent();
        }

        // Set window style
        protected override void OnResize(EventArgs e)
        {
            int[] margins = new int[] { 0, 0, rect.Width, rect.Height };
            Managed.DwmExtendFrameIntoClientArea(this.Handle, ref margins);
        }

        // INIT
        private void DrawWindow_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.Visible = true;
            this.FormBorderStyle = FormBorderStyle.None;
            //this.WindowState = FormWindowState.Maximized;
            this.Width = rect.Width;
            this.Height = rect.Height;

            // Window name
            this.Name = Process.GetCurrentProcess().ProcessName + "~Overlay";
            this.Text = Process.GetCurrentProcess().ProcessName + "~Overlay";

            // Init factory
            factory = new Factory();
            fontFactory = new FontFactory();

            // Render settings
            renderProperties = new HwndRenderTargetProperties()
            {
                Hwnd = this.Handle,
                PixelSize = new Size2(rect.Width, rect.Height),
                PresentOptions = PresentOptions.None
            };

            // Init device
            device = new WindowRenderTarget(factory, new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)), renderProperties);

            // Init brush
            solidColorBrush = new SolidColorBrush(device, Color.White);

            // Init font's
            font = new TextFormat(fontFactory, fontFamily, fontSize);
            fontSmall = new TextFormat(fontFactory, fontFamily, fontSizeSmall);

            // Open process
            RPM.OpenProcess(process.Id);

            // Init player array
            players = new List<Player>();
            localPlayer = new Player();

            // Init update thread
            updateStream = new Thread(new ParameterizedThreadStart(Update));
            updateStream.Start();

            // Init window thread (resize / position)
            windowStream = new Thread(new ParameterizedThreadStart(SetWindow));
            windowStream.Start();

            // Init Key Listener
            manager = new KeysManager();
            manager.AddKey(Keys.F5);
            manager.AddKey(Keys.F6);
            manager.AddKey(Keys.F7);
            manager.AddKey(Keys.F8);
            manager.AddKey(Keys.F9);
            //manager.KeyUpEvent += new KeysManager.KeyHandler(KeyUpEvent);
            manager.KeyDownEvent += new KeysManager.KeyHandler(KeyDownEvent);
        }

        // Key Down Event
        private void KeyDownEvent(int keyId, string keyName)
        {
            switch ((Keys)keyId)
            {
                case Keys.F5:
                    this.ESP_Box = !this.ESP_Box;
                    break;
                case Keys.F6:
                    this.ESP_Bone = !this.ESP_Bone;
                    break;
                case Keys.F7:
                    this.ESP_Health = !this.ESP_Health;
                    break;
                case Keys.F8:
                    this.ESP_Distance = !this.ESP_Distance;
                    break;
                case Keys.F9:
                    this.ESP_Name = !this.ESP_Name;
                    break;      
            }
        }

        // FPS Stats
        private static int lastTick;
        private static int lastFrameRate;
        private static int frameRate;

        // Check is Game Run
        private bool IsGameRun()
        {
            foreach (Process p in Process.GetProcesses())
            {
                if (p.ProcessName == process.ProcessName)
                    return true;
            }
            return false;
        } 

        // Update Thread
        private void Update(object sender)
        {
            while (IsGameRun())
            {
                // Resize
                if (IsResize)
                {
                    device.Resize(new Size2(rect.Width, rect.Height));
                    //Console.WriteLine("Resize {0}/{1}", rect.Width, rect.Height);
                    IsResize = false;
                }

                // Begin Draw
                device.BeginDraw();
                device.Clear(new Color4(0.0f, 0.0f, 0.0f, 0.0f));

                // Check Window State
                if (!IsMinimized)
                {
                    // Read & Draw Players
                    Read();

                    // Draw Credits
                    DrawTextCenter(rect.Width / 2 - 125, 5, 250, (int)font.FontSize, "EXTERNAL ESP BASE BY XTREME2010 EDITED BY CERRAOSSO", new Color(255, 214, 0, 255), true);

                    // Draw Spectator Count
                    DrawTextCenter(rect.Width / 2 - 100, rect.Height - (int)font.FontSize, 200, (int)font.FontSize, spectatorCount + " SPECTATOR(S) ON A SERVER", new Color(255, 214, 0, 255), true);

                    // Draw Menu
                    DrawMenu(5, 5);
                }

                // End Draw
                device.EndDraw();
                CalculateFrameRate();
                //Thread.Sleep(Interval);
            }

            // Close Process
            RPM.CloseProcess();
            // Exit
            Environment.Exit(0);
        }

        // Read Game Memorry
        private void Read()
        {
            // Reset Old Data
            players.Clear();
            localPlayer = new Player();

            // Read Local
            #region Get Local Player
            Int64 pGContext = RPM.ReadInt64(Offsets.ClientGameContext.GetInstance());
            if (!RPM.IsValid(pGContext))
                return;

            Int64 pPlayerManager = RPM.ReadInt64(pGContext + Offsets.ClientGameContext.m_pPlayerManager);
            if (!RPM.IsValid(pPlayerManager))
                return;

            Int64 pLocalPlayer = RPM.ReadInt64(pPlayerManager + Offsets.ClientPlayerManager.m_pLocalPlayer);
            if (!RPM.IsValid(pLocalPlayer))
                return;

            //RPM.ReadInt64(pLocalPlayer + Offsets.ClientPlayer.m_pControlledControllable);
            Int64 pLocalSoldier = GetClientSoldierEntity(pLocalPlayer, localPlayer);
            if (!RPM.IsValid(pLocalSoldier))
                return;

            Int64 pHealthComponent = RPM.ReadInt64(pLocalSoldier + Offsets.ClientSoldierEntity.m_pHealthComponent);
            if (!RPM.IsValid(pHealthComponent))
                return;

            Int64 m_pPredictedController = RPM.ReadInt64(pLocalSoldier + Offsets.ClientSoldierEntity.m_pPredictedController);
            if (!RPM.IsValid(m_pPredictedController))
                return;

            // Health
            localPlayer.Health = RPM.ReadFloat(pHealthComponent + Offsets.HealthComponent.m_Health);
            localPlayer.MaxHealth = RPM.ReadFloat(pHealthComponent + Offsets.HealthComponent.m_MaxHealth);

            if (localPlayer.Health <= 0.1f) // YOU DEAD :D
                return;

            // Origin
            localPlayer.Origin = RPM.ReadVector3(m_pPredictedController + Offsets.ClientSoldierPrediction.m_Position);

            // Other
            localPlayer.Team = RPM.ReadInt32(pLocalPlayer + Offsets.ClientPlayer.m_teamId);
            //localPlayer.Name = RPM.ReadString(pLocalPlayer + Offsets.ClientPlayer.szName, 10);
            localPlayer.Pose = RPM.ReadInt32(pLocalSoldier + Offsets.ClientSoldierEntity.m_poseType);
            localPlayer.Yaw = RPM.ReadFloat(pLocalSoldier + Offsets.ClientSoldierEntity.m_authorativeYaw);
            localPlayer.IsOccluded = RPM.ReadByte(pLocalSoldier + Offsets.ClientSoldierEntity.m_occluded);

            // Weapon Ammo
            if (!localPlayer.InVehicle)
            {
                Int64 pClientWeaponComponent = RPM.ReadInt64(pLocalSoldier + Offsets.ClientSoldierEntity.m_soldierWeaponsComponent);
                if (RPM.IsValid(pClientWeaponComponent))
                {
                    Int64 pWeaponHandle = RPM.ReadInt64(pClientWeaponComponent + Offsets.ClientSoldierWeaponsComponent.m_handler);
                    Int32 ActiveSlot = RPM.ReadInt32(pClientWeaponComponent + Offsets.ClientSoldierWeaponsComponent.m_activeSlot);

                    if (RPM.IsValid(pWeaponHandle))
                    {
                        Int64 pSoldierWeapon = RPM.ReadInt64(pWeaponHandle + ActiveSlot * 0x8);
                        if (RPM.IsValid(pSoldierWeapon))
                        {
                            Int64 pCorrectedFiring = RPM.ReadInt64(pSoldierWeapon + Offsets.ClientSoldierWeapon.m_pPrimary);
                            if (RPM.IsValid(pCorrectedFiring))
                            {
                                // Ammo
                                localPlayer.Ammo = RPM.ReadInt32(pCorrectedFiring + Offsets.WeaponFiring.m_projectilesLoaded);
                                localPlayer.AmmoClip = RPM.ReadInt32(pCorrectedFiring + Offsets.WeaponFiring.m_projectilesInMagazines);
                            }
                        }
                    }
                }
            }  
            #endregion

            // Render View
            Int64 pGameRenderer = RPM.ReadInt64(Offsets.GameRenderer.GetInstance());
            Int64 pRenderView = RPM.ReadInt64(pGameRenderer + Offsets.GameRenderer.m_pRenderView);

            // Read Screen Matrix
            viewProj = RPM.ReadMatrix(pRenderView + Offsets.RenderView.m_ViewProj);
            m_ViewMatrixInverse = RPM.ReadMatrix(pRenderView + Offsets.RenderView.m_ViewMatrixInverse);

            // Pointer to Players Array
            Int64 m_ppPlayer = RPM.ReadInt64(pPlayerManager + Offsets.ClientPlayerManager.m_ppPlayer);
            if (!RPM.IsValid(m_ppPlayer))
                return;

            // Reset
            spectatorCount = 0;

            // Get Player by Id
            #region Get Player by Id
            for (uint i = 0; i < 70; i++)
            {
                // Create new Player
                Player player = new Player();

                // Pointer to ClientPlayer class (Player Array + (Id * Size of Pointer))
                Int64 pEnemyPlayer = RPM.ReadInt64(m_ppPlayer + (i * sizeof(Int64)));
                if (!RPM.IsValid(pEnemyPlayer))
                    continue;

                if (pEnemyPlayer == pLocalPlayer)
                    continue;

                player.IsSpectator = Convert.ToBoolean(RPM.ReadByte(pEnemyPlayer + Offsets.ClientPlayer.m_isSpectator));

                if (player.IsSpectator)
                    spectatorCount++;

                // Name
                player.Name = RPM.ReadString2(pEnemyPlayer + Offsets.ClientPlayer.szName, 10);

                // RPM.ReadInt64(pEnemyPlayer + Offsets.ClientPlayer.m_pControlledControllable);
                Int64 pEnemySoldier = GetClientSoldierEntity(pEnemyPlayer, player);
                if (!RPM.IsValid(pEnemySoldier))
                    continue;

                Int64 pEnemyHealthComponent = RPM.ReadInt64(pEnemySoldier + Offsets.ClientSoldierEntity.m_pHealthComponent);
                if (!RPM.IsValid(pEnemyHealthComponent))
                    continue;

                Int64 pEnemyPredictedController = RPM.ReadInt64(pEnemySoldier + Offsets.ClientSoldierEntity.m_pPredictedController);
                if (!RPM.IsValid(pEnemyPredictedController))
                    continue;
             
                // Health
                player.Health = RPM.ReadFloat(pEnemyHealthComponent + Offsets.HealthComponent.m_Health);
                player.MaxHealth = RPM.ReadFloat(pEnemyHealthComponent + Offsets.HealthComponent.m_MaxHealth);

                if (player.Health <= 0.1f) // DEAD
                    continue;

                // Origin (Position in Game X, Y, Z)
                player.Origin = RPM.ReadVector3(pEnemyPredictedController + Offsets.ClientSoldierPrediction.m_Position);

                // Other
                player.Team = RPM.ReadInt32(pEnemyPlayer + Offsets.ClientPlayer.m_teamId);
                player.Pose = RPM.ReadInt32(pEnemySoldier + Offsets.ClientSoldierEntity.m_poseType);
                player.Yaw = RPM.ReadFloat(pEnemySoldier + Offsets.ClientSoldierEntity.m_authorativeYaw);
                player.IsOccluded = RPM.ReadByte(pEnemySoldier + Offsets.ClientSoldierEntity.m_occluded);

                // Distance to You
                player.Distance = Vector3.Distance(localPlayer.Origin, player.Origin);

                if (player.IsValid())
                {
                    #region Bone ESP
                    if (ESP_Bone)
                    {
                        // Player Bone
                        if (GetBonyById(pEnemySoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_HEAD, out player.Bone.BONE_HEAD)
                            && GetBonyById(pEnemySoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_LEFTELBOWROLL, out player.Bone.BONE_LEFTELBOWROLL)
                            && GetBonyById(pEnemySoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_LEFTFOOT, out player.Bone.BONE_LEFTFOOT)
                            && GetBonyById(pEnemySoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_LEFTHAND, out player.Bone.BONE_LEFTHAND)
                            && GetBonyById(pEnemySoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_LEFTKNEEROLL, out player.Bone.BONE_LEFTKNEEROLL)
                            && GetBonyById(pEnemySoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_LEFTSHOULDER, out player.Bone.BONE_LEFTSHOULDER)
                            && GetBonyById(pEnemySoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_NECK, out player.Bone.BONE_NECK)
                            && GetBonyById(pEnemySoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_RIGHTELBOWROLL, out player.Bone.BONE_RIGHTELBOWROLL)
                            && GetBonyById(pEnemySoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_RIGHTFOOT, out player.Bone.BONE_RIGHTFOOT)
                            && GetBonyById(pEnemySoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_RIGHTHAND, out player.Bone.BONE_RIGHTHAND)
                            && GetBonyById(pEnemySoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_RIGHTKNEEROLL, out player.Bone.BONE_RIGHTKNEEROLL)
                            && GetBonyById(pEnemySoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_RIGHTSHOULDER, out player.Bone.BONE_RIGHTSHOULDER)
                            && GetBonyById(pEnemySoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_SPINE, out player.Bone.BONE_SPINE)
                            && GetBonyById(pEnemySoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_SPINE1, out player.Bone.BONE_SPINE1)
                            && GetBonyById(pEnemySoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_SPINE2, out player.Bone.BONE_SPINE2))
                        {
                            DrawBone(player);
                        }
                    }
                    #endregion

                    Vector3 Foot, Head;
                    if (WorldToScreen(player.Origin, out Foot) &&
                        WorldToScreen(player.Origin, player.Pose, out Head))
                    {
                        float HeadToFoot = Foot.Y - Head.Y;
                        float BoxWidth = HeadToFoot / 2;
                        float X = Head.X - (BoxWidth) / 2;

                        #region ESP Color
                        Color color;
                        if (player.Team == localPlayer.Team)  {
                            color = friendlyColor;
                        }
                        else {
                            color = player.IsVisible() ? enemyColorVisible : enemyColor;
                        }
                        #endregion

                        #region Draw ESP
                        // ESP Box
                        if (ESP_Box)
                        {
                            DrawAABB(player.GetAABB(), player.Origin, player.Yaw, color);
                            //DrawRect((int)X, (int)Head.Y, (int)BoxWidth, (int)HeadToFoot, color);
                        }

                        // ESP Distance
                        if (ESP_Distance)
                        {
                            DrawText((int)X, (int)Foot.Y, (int)player.Distance + "m", Color.White, true);
                        }

                        // ESP Name
                        if (ESP_Name)
                        {
                            if (player.InVehicle && player.IsDriver) {
                                DrawTextCenter((int)X + ((int)BoxWidth / 2) - 100, (int)Head.Y - 20, 200, 20, "[" + player.VehicleName + "]", Color.White, true);
                            }
                            else if (!player.InVehicle) {
                                DrawTextCenter((int)X + ((int)BoxWidth / 2) - 100, (int)Head.Y - 20, 200, 20, player.Name, Color.White, true);
                            }
                        }

                        // ESP Health
                        if (ESP_Health)
                        {
                            DrawHealth((int)X, (int)Head.Y - 6, (int)BoxWidth, 3, (int)player.Health, (int)player.MaxHealth);
                        }
                        #endregion
                    }
                }

                // ADD IN ARRAY
                players.Add(player);
            } 
            #endregion

            // Check Spectator Count
            if (spectatorCount > 0)
            {
                DrawWarn(rect.Center.X - 125, 25, 250, 55);
            }
        }

        // Get SoldierEntity
        private Int64 GetClientSoldierEntity(Int64 pClientPlayer, Player player)
        {
            Int64 pAttached = RPM.ReadInt64(pClientPlayer + Offsets.ClientPlayer.m_pAttachedControllable);
            if (RPM.IsValid(pAttached))
            {
                Int64 m_ClientSoldier = RPM.ReadInt64(RPM.ReadInt64(pClientPlayer + Offsets.ClientPlayer.m_character)) - sizeof(Int64);
                if (RPM.IsValid(m_ClientSoldier))
                {
                    player.InVehicle = true;

                    Int64 pVehicleEntity = RPM.ReadInt64(pClientPlayer + Offsets.ClientPlayer.m_pAttachedControllable);
                    if (RPM.IsValid(pVehicleEntity))
                    {
                        // Driver
                        if (RPM.ReadInt32(pClientPlayer + Offsets.ClientPlayer.m_attachedEntryId) == 0)
                        {
                            Int64 _EntityData = RPM.ReadInt64(pVehicleEntity + Offsets.ClientSoldierEntity.m_data);
                            if (RPM.IsValid(_EntityData))
                            {
                                Int64 _NameSid = RPM.ReadInt64(_EntityData + Offsets.VehicleEntityData.m_NameSid);

                                string strName = RPM.ReadString(_NameSid, 20);
                                if (strName.Length > 11)
                                {
                                    // AttachedControllable Name
                                    player.VehicleName = strName.Remove(0, 11);
                                    player.IsDriver = true;
                                }
                            }
                        }
                    }
                }
                return m_ClientSoldier;
            }
            return RPM.ReadInt64(pClientPlayer + Offsets.ClientPlayer.m_pControlledControllable);
        }

        // Get Window Rect
        private void SetWindow(object sender)
        {
            while (true)
            {
                IntPtr targetWnd = IntPtr.Zero;
                targetWnd = Managed.FindWindow(null, "Battlefield™ 1");

                if (targetWnd != IntPtr.Zero)
                {
                    RECT targetSize = new RECT();
                    Managed.GetWindowRect(targetWnd, out targetSize);

                    // Game is Minimized
                    if (targetSize.Left < 0 && targetSize.Top < 0 && targetSize.Right < 0 && targetSize.Bottom < 0)
                    {
                        IsMinimized = true;
                        continue;
                    }

                    // Reset
                    IsMinimized = false;

                    RECT borderSize = new RECT();
                    Managed.GetClientRect(targetWnd, out borderSize);

                    int dwStyle = Managed.GetWindowLong(targetWnd, Managed.GWL_STYLE);

                    int windowheight;
                    int windowwidth;
                    int borderheight;
                    int borderwidth;

                    if (rect.Width != (targetSize.Bottom - targetSize.Top)
                        && rect.Width != (borderSize.Right - borderSize.Left))
                        IsResize = true;

                    rect.Width = targetSize.Right - targetSize.Left;
                    rect.Height = targetSize.Bottom - targetSize.Top;

                    if ((dwStyle & Managed.WS_BORDER) != 0)
                    {
                        windowheight = targetSize.Bottom - targetSize.Top;
                        windowwidth = targetSize.Right - targetSize.Left;

                        rect.Height = borderSize.Bottom - borderSize.Top;
                        rect.Width = borderSize.Right - borderSize.Left;

                        borderheight = (windowheight - borderSize.Bottom);
                        borderwidth = (windowwidth - borderSize.Right) / 2; //only want one side
                        borderheight -= borderwidth; //remove bottom

                        targetSize.Left += borderwidth;
                        targetSize.Top += borderheight;

                        rect.Left = targetSize.Left;
                        rect.Top = targetSize.Top;
                    }
                    Managed.MoveWindow(handle, targetSize.Left, targetSize.Top, rect.Width, rect.Height, true);
                }
                Thread.Sleep(300);
            }
        } 

        // 3D In 2D
        private bool WorldToScreen(Vector3 _Enemy, int _Pose, out Vector3 _Screen)
        {
            _Screen = new Vector3(0, 0, 0);
            float HeadHeight = _Enemy.Y;

            #region HeadHeight
            if (_Pose == 0)
            {
                HeadHeight += 1.7f;
            }
            if (_Pose == 1)
            {
                HeadHeight += 1.15f;
            }
            if (_Pose == 2)
            {
                HeadHeight += 0.4f;
            }
            #endregion

            float ScreenW = (viewProj.M14 * _Enemy.X) + (viewProj.M24 * HeadHeight) + (viewProj.M34 * _Enemy.Z + viewProj.M44);

            if (ScreenW < 0.0001f)
                return false;

            float ScreenX = (viewProj.M11 * _Enemy.X) + (viewProj.M21 * HeadHeight) + (viewProj.M31 * _Enemy.Z + viewProj.M41);
            float ScreenY = (viewProj.M12 * _Enemy.X) + (viewProj.M22 * HeadHeight) + (viewProj.M32 * _Enemy.Z + viewProj.M42);

            _Screen.X = (rect.Width / 2) + (rect.Width / 2) * ScreenX / ScreenW;
            _Screen.Y = (rect.Height / 2) - (rect.Height / 2) * ScreenY / ScreenW;
            _Screen.Z = ScreenW;
            return true;
        }

        // 3D In 2D
        private bool WorldToScreen(Vector3 _Enemy, out Vector3 _Screen)
        {
            _Screen = new Vector3(0, 0, 0);
            float ScreenW = (viewProj.M14 * _Enemy.X) + (viewProj.M24 * _Enemy.Y) + (viewProj.M34 * _Enemy.Z + viewProj.M44);

            if (ScreenW < 0.0001f)
                return false;

            float ScreenX = (viewProj.M11 * _Enemy.X) + (viewProj.M21 * _Enemy.Y) + (viewProj.M31 * _Enemy.Z + viewProj.M41);
            float ScreenY = (viewProj.M12 * _Enemy.X) + (viewProj.M22 * _Enemy.Y) + (viewProj.M32 * _Enemy.Z + viewProj.M42);

            _Screen.X = (rect.Width / 2) + (rect.Width / 2) * ScreenX / ScreenW;
            _Screen.Y = (rect.Height / 2) - (rect.Height / 2) * ScreenY / ScreenW;
            _Screen.Z = ScreenW;
            return true;
        }

        // Get Roll
        private bool GetBonyById(Int64 pEnemySoldier, int Id, out Vector3 _World)
        {
            _World = new Vector3();

            Int64 pRagdollComp = RPM.ReadInt64(pEnemySoldier + Offsets.ClientSoldierEntity.m_ragdollComponent);
            if (!RPM.IsValid(pRagdollComp))
                return false;

            byte m_ValidTransforms = RPM.ReadByte(pRagdollComp + (Offsets.ClientRagDollComponent.m_ragdollTransforms + Offsets.UpdatePoseResultData.m_ValidTransforms));
            if (m_ValidTransforms != 1)
                return false;

            Int64 pQuatTransform = RPM.ReadInt64(pRagdollComp + (Offsets.ClientRagDollComponent.m_ragdollTransforms + Offsets.UpdatePoseResultData.m_ActiveWorldTransforms));
            if (!RPM.IsValid(pQuatTransform))
                return false;

            _World = RPM.ReadVector3(pQuatTransform + Id * 0x20);
            return true;
        } 

        // Get FPS
        public int CalculateFrameRate()
        {
            int tickCount = Environment.TickCount;
            if (tickCount - lastTick >= 1000)
            {
                lastFrameRate = frameRate;
                frameRate = 0;
                lastTick = tickCount;
            }
            frameRate++;
            return lastFrameRate;
        }

        // Close window event
        private void DrawWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            updateStream.Abort();
            windowStream.Abort();
            RPM.CloseProcess();

            // Close main process
            Environment.Exit(0);
        }

        // Multiply Vector's
        public Vector3 Multiply(Vector3 vector, Matrix mat)
        {
            return new Vector3(mat.M11 * vector.X + mat.M21 * vector.Y + mat.M31 * vector.Z,
                                   mat.M12 * vector.X + mat.M22 * vector.Y + mat.M32 * vector.Z,
                                   mat.M13 * vector.X + mat.M23 * vector.Y + mat.M33 * vector.Z);
        }

        // Draw Functions
        #region Draw Functions
        private void DrawRect(int X, int Y, int W, int H, Color color)
        {
            solidColorBrush.Color = color;
            device.DrawRectangle(new Rectangle(X, Y, W, H), solidColorBrush);
        }

        private void DrawRect(int X, int Y, int W, int H, Color color, float stroke)
        {
            solidColorBrush.Color = color;
            device.DrawRectangle(new Rectangle(X, Y, W, H), solidColorBrush, stroke);
        }

        private void DrawFillRect(int X, int Y, int W, int H, Color color)
        {
            solidColorBrush.Color = color;
            device.FillRectangle(new RectangleF(X, Y, W, H), solidColorBrush);
        }

        private void DrawText(int X, int Y, string text, Color color)
        {
            solidColorBrush.Color = color;
            device.DrawText(text, font, new RectangleF(X, Y, font.FontSize * text.Length, font.FontSize), solidColorBrush);
        }

        private void DrawText(int X, int Y, string text, Color color, bool outline)
        {
            if (outline)
            {
                solidColorBrush.Color = Color.Black;
                device.DrawText(text, font, new RectangleF(X + 1, Y + 1, font.FontSize * text.Length, font.FontSize), solidColorBrush);
            }

            solidColorBrush.Color = color;
            device.DrawText(text, font, new RectangleF(X, Y, font.FontSize * text.Length, font.FontSize), solidColorBrush);
        }

        private void DrawText(int X, int Y, string text, Color color, bool outline, TextFormat format)
        {
            if (outline)
            {
                solidColorBrush.Color = Color.Black;
                device.DrawText(text, format, new RectangleF(X + 1, Y + 1, format.FontSize * text.Length, format.FontSize), solidColorBrush);
            }

            solidColorBrush.Color = color;
            device.DrawText(text, format, new RectangleF(X, Y, format.FontSize * text.Length, format.FontSize), solidColorBrush);
        }

        private void DrawTextCenter(int X, int Y, int W, int H, string text, Color color)
        {
            solidColorBrush.Color = color;
            TextLayout layout = new TextLayout(fontFactory, text, fontSmall, W, H);
            layout.TextAlignment = TextAlignment.Center;
            device.DrawTextLayout(new Vector2(X, Y), layout, solidColorBrush);
            layout.Dispose();
        }

        private void DrawTextCenter(int X, int Y, int W, int H, string text, Color color, bool outline)
        {
            TextLayout layout = new TextLayout(fontFactory, text, fontSmall, W, H);
            layout.TextAlignment = TextAlignment.Center;

            if (outline)
            {
                solidColorBrush.Color = Color.Black;
                device.DrawTextLayout(new Vector2(X + 1, Y + 1), layout, solidColorBrush);
            }

            solidColorBrush.Color = color;
            device.DrawTextLayout(new Vector2(X, Y), layout, solidColorBrush);
            layout.Dispose();
        }

        private void DrawLine(int X, int Y, int XX, int YY, Color color)
        {
            solidColorBrush.Color = color;
            device.DrawLine(new Vector2(X, Y), new Vector2(XX, YY), solidColorBrush);
        }

        private void DrawLine(Vector3 w2s, Vector3 _w2s, Color color)
        {
            solidColorBrush.Color = color;
            device.DrawLine(new Vector2(w2s.X, w2s.Y), new Vector2(_w2s.X, _w2s.Y), solidColorBrush);
        }

        private void DrawCircle(int X, int Y, int W, Color color)
        {
            solidColorBrush.Color = color;
            device.DrawEllipse(new Ellipse(new Vector2(X, Y), W, W), solidColorBrush);
        }

        private void DrawFillCircle(int X, int Y, int W, Color color)
        {
            solidColorBrush.Color = color;
            device.FillEllipse(new Ellipse(new Vector2(X, Y), W, W), solidColorBrush);
        }

        private void DrawImage(int X, int Y, int W, int H, Bitmap bitmap)
        {
            device.DrawBitmap(bitmap, new RectangleF(X, Y, W, H), 1.0f, BitmapInterpolationMode.Linear);
        }

        private void DrawImage(int X, int Y, int W, int H, Bitmap bitmap, float angle)
        {
            device.Transform = Matrix3x2.Rotation(angle, new Vector2(X + (H / 2), Y + (H / 2)));
            device.DrawBitmap(bitmap, new RectangleF(X, Y, W, H), 1.0f, BitmapInterpolationMode.Linear);
            device.Transform = Matrix3x2.Rotation(0);
        }

        private void DrawSprite(RectangleF destinationRectangle, Bitmap bitmap, RectangleF sourceRectangle)
        {
            device.DrawBitmap(bitmap, destinationRectangle, 1.0f, BitmapInterpolationMode.Linear, sourceRectangle);
        }

        private void DrawSprite(RectangleF destinationRectangle, Bitmap bitmap, RectangleF sourceRectangle, float angle)
        {
            Vector2 center = new Vector2();
            center.X = destinationRectangle.X + destinationRectangle.Width / 2;
            center.Y = destinationRectangle.Y + destinationRectangle.Height / 2;

            device.Transform = Matrix3x2.Rotation(angle, center);
            device.DrawBitmap(bitmap, destinationRectangle, 1.0f, BitmapInterpolationMode.Linear, sourceRectangle);
            device.Transform = Matrix3x2.Rotation(0);
        }

        private void DrawBone(Player player)
        {
            Vector3 BONE_HEAD,
            BONE_NECK,
            BONE_SPINE2,
            BONE_SPINE1,
            BONE_SPINE,
            BONE_LEFTSHOULDER,
            BONE_RIGHTSHOULDER,
            BONE_LEFTELBOWROLL,
            BONE_RIGHTELBOWROLL,
            BONE_LEFTHAND,
            BONE_RIGHTHAND,
            BONE_LEFTKNEEROLL,
            BONE_RIGHTKNEEROLL,
            BONE_LEFTFOOT,
            BONE_RIGHTFOOT;

            if(WorldToScreen(player.Bone.BONE_HEAD, out BONE_HEAD) &&
            WorldToScreen(player.Bone.BONE_NECK, out BONE_NECK) &&
            WorldToScreen(player.Bone.BONE_SPINE2, out BONE_SPINE2) &&
            WorldToScreen(player.Bone.BONE_SPINE1, out BONE_SPINE1) &&
            WorldToScreen(player.Bone.BONE_SPINE, out BONE_SPINE) &&
            WorldToScreen(player.Bone.BONE_LEFTSHOULDER, out BONE_LEFTSHOULDER) &&
            WorldToScreen(player.Bone.BONE_RIGHTSHOULDER, out BONE_RIGHTSHOULDER) &&
            WorldToScreen(player.Bone.BONE_LEFTELBOWROLL, out BONE_LEFTELBOWROLL) &&
            WorldToScreen(player.Bone.BONE_RIGHTELBOWROLL, out BONE_RIGHTELBOWROLL) &&
            WorldToScreen(player.Bone.BONE_LEFTHAND, out BONE_LEFTHAND) &&
            WorldToScreen(player.Bone.BONE_RIGHTHAND, out BONE_RIGHTHAND) &&
            WorldToScreen(player.Bone.BONE_LEFTKNEEROLL, out BONE_LEFTKNEEROLL) &&
            WorldToScreen(player.Bone.BONE_RIGHTKNEEROLL, out BONE_RIGHTKNEEROLL) &&
            WorldToScreen(player.Bone.BONE_LEFTFOOT, out BONE_LEFTFOOT) &&
            WorldToScreen(player.Bone.BONE_RIGHTFOOT, out BONE_RIGHTFOOT))
            {
                int stroke = 3;
                int strokeW = stroke % 2 == 0 ? stroke / 2 : (stroke - 1) / 2;

                // Color
                Color skeletonColor = player.Team == localPlayer.Team ? friendSkeletonColor : enemySkeletonColor;

                // RECT's
		        DrawFillRect((int)BONE_HEAD.X - strokeW, (int)BONE_HEAD.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_NECK.X - strokeW, (int)BONE_NECK.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_LEFTSHOULDER.X - strokeW, (int)BONE_LEFTSHOULDER.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_LEFTELBOWROLL.X - strokeW, (int)BONE_LEFTELBOWROLL.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_LEFTHAND.X - strokeW, (int)BONE_LEFTHAND.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_RIGHTSHOULDER.X - strokeW, (int)BONE_RIGHTSHOULDER.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_RIGHTELBOWROLL.X - strokeW, (int)BONE_RIGHTELBOWROLL.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_RIGHTHAND.X - strokeW, (int)BONE_RIGHTHAND.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_SPINE2.X - strokeW, (int)BONE_SPINE2.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_SPINE1.X - strokeW, (int)BONE_SPINE1.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_SPINE.X - strokeW, (int)BONE_SPINE.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_LEFTKNEEROLL.X - strokeW, (int)BONE_LEFTKNEEROLL.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_RIGHTKNEEROLL.X - strokeW, (int)BONE_RIGHTKNEEROLL.Y - strokeW, 2, 2, skeletonColor);
                DrawFillRect((int)BONE_LEFTFOOT.X - strokeW, (int)BONE_LEFTFOOT.Y - strokeW, 2, 2, skeletonColor);
                DrawFillRect((int)BONE_RIGHTFOOT.X - strokeW, (int)BONE_RIGHTFOOT.Y - strokeW, 2, 2, skeletonColor);

                // Head -> Neck
		        DrawLine((int)BONE_HEAD.X, (int)BONE_HEAD.Y, (int)BONE_NECK.X, (int)BONE_NECK.Y, skeletonColor);

		        // Neck -> Left
		        DrawLine((int)BONE_NECK.X, (int)BONE_NECK.Y, (int)BONE_LEFTSHOULDER.X, (int)BONE_LEFTSHOULDER.Y, skeletonColor);
		        DrawLine((int)BONE_LEFTSHOULDER.X, (int)BONE_LEFTSHOULDER.Y,(int) BONE_LEFTELBOWROLL.X, (int)BONE_LEFTELBOWROLL.Y, skeletonColor);
                DrawLine((int)BONE_LEFTELBOWROLL.X, (int)BONE_LEFTELBOWROLL.Y, (int)BONE_LEFTHAND.X, (int)BONE_LEFTHAND.Y, skeletonColor);

		        // Neck -> Right
                DrawLine((int)BONE_NECK.X, (int)BONE_NECK.Y, (int)BONE_RIGHTSHOULDER.X, (int)BONE_RIGHTSHOULDER.Y, skeletonColor);
                DrawLine((int)BONE_RIGHTSHOULDER.X, (int)BONE_RIGHTSHOULDER.Y, (int)BONE_RIGHTELBOWROLL.X, (int)BONE_RIGHTELBOWROLL.Y, skeletonColor);
                DrawLine((int)BONE_RIGHTELBOWROLL.X, (int)BONE_RIGHTELBOWROLL.Y, (int)BONE_RIGHTHAND.X, (int)BONE_RIGHTHAND.Y, skeletonColor);

		        // Neck -> Center
                DrawLine((int)BONE_NECK.X, (int)BONE_NECK.Y, (int)BONE_SPINE2.X, (int)BONE_SPINE2.Y, skeletonColor);
                DrawLine((int)BONE_SPINE2.X, (int)BONE_SPINE2.Y, (int)BONE_SPINE1.X, (int)BONE_SPINE1.Y, skeletonColor);
                DrawLine((int)BONE_SPINE1.X, (int)BONE_SPINE1.Y, (int)BONE_SPINE.X, (int)BONE_SPINE.Y, skeletonColor);

		        // Spine -> Left
                DrawLine((int)BONE_SPINE.X, (int)BONE_SPINE.Y, (int)BONE_LEFTKNEEROLL.X, (int)BONE_LEFTKNEEROLL.Y, skeletonColor);
                DrawLine((int)BONE_LEFTKNEEROLL.X, (int)BONE_LEFTKNEEROLL.Y, (int)BONE_LEFTFOOT.X, (int)BONE_LEFTFOOT.Y, skeletonColor);

		        // Spine -> Right
                DrawLine((int)BONE_SPINE.X, (int)BONE_SPINE.Y, (int)BONE_RIGHTKNEEROLL.X, (int)BONE_RIGHTKNEEROLL.Y, skeletonColor);
                DrawLine((int)BONE_RIGHTKNEEROLL.X, (int)BONE_RIGHTKNEEROLL.Y, (int)BONE_RIGHTFOOT.X, (int)BONE_RIGHTFOOT.Y, skeletonColor);
            }
        }

        private void DrawHealth(int X, int Y, int W, int H, int Health, int MaxHealth)
        {
            if (Health <= 0)
                Health = 1;

            if (MaxHealth < Health) 
                MaxHealth = 100;

            int progress = (int)((float)Health / ((float)MaxHealth / 100));
            int w = (int)((float)W / 100 * progress);

            if (w <= 2)
                w = 3;

            Color color = new Color(255, 0, 0, 255);
            if (progress >= 20) color = new Color(255, 165, 0, 255);
            if (progress >= 40) color = new Color(255, 255, 0, 255);
            if (progress >= 60) color = new Color(173, 255, 47, 255);
            if (progress >= 80) color = new Color(0, 255, 0, 255);

            DrawFillRect(X, Y - 1, W + 1, H + 2, Color.Black);
            DrawFillRect(X + 1, Y, w - 1, H, color);
        }

        private void DrawProgress(int X, int Y, int W, int H, int Value, int MaxValue)
        {
            int progress = (int)((float)Value / ((float)MaxValue / 100));
            int w = (int)((float)W / 100 * progress);

            Color color = new Color(0, 255, 0, 255);
            if (progress >= 20) color = new Color(173, 255, 47, 255);
            if (progress >= 40) color = new Color(255, 255, 0, 255);
            if (progress >= 60) color = new Color(255, 165, 0, 255);
            if (progress >= 80) color = new Color(255, 0, 0, 255);

            DrawFillRect(X, Y - 1, W + 1, H + 2, Color.Black);
            if (w >= 2)
            {
                DrawFillRect(X + 1, Y, w - 1, H, color);
            }
        }

        private void DrawAABB(AxisAlignedBox aabb, Matrix tranform, Color color)
        {
            Vector3 m_Position = new Vector3(tranform.M41, tranform.M42, tranform.M43);
            Vector3 fld = Multiply(new Vector3(aabb.Min.X, aabb.Min.Y, aabb.Min.Z), tranform) + m_Position;
            Vector3 brt = Multiply(new Vector3(aabb.Max.X, aabb.Max.Y, aabb.Max.Z), tranform) + m_Position;
            Vector3 bld = Multiply(new Vector3(aabb.Min.X, aabb.Min.Y, aabb.Max.Z), tranform) + m_Position;
            Vector3 frt = Multiply(new Vector3(aabb.Max.X, aabb.Max.Y, aabb.Min.Z), tranform) + m_Position;
            Vector3 frd = Multiply(new Vector3(aabb.Max.X, aabb.Min.Y, aabb.Min.Z), tranform) + m_Position;
            Vector3 brb = Multiply(new Vector3(aabb.Max.X, aabb.Min.Y, aabb.Max.Z), tranform) + m_Position;
            Vector3 blt = Multiply(new Vector3(aabb.Min.X, aabb.Max.Y, aabb.Max.Z), tranform) + m_Position;
            Vector3 flt = Multiply(new Vector3(aabb.Min.X, aabb.Max.Y, aabb.Min.Z), tranform) + m_Position;

            #region WorldToScreen
            if (!WorldToScreen(fld, out fld) || !WorldToScreen(brt, out brt)
                || !WorldToScreen(bld, out bld) || !WorldToScreen(frt, out frt)
                || !WorldToScreen(frd, out frd) || !WorldToScreen(brb, out brb)
                || !WorldToScreen(blt, out blt) || !WorldToScreen(flt, out flt))
                return;
            #endregion

            #region DrawLines
            DrawLine(fld, flt, color);
            DrawLine(flt, frt, color);
            DrawLine(frt, frd, color);
            DrawLine(frd, fld, color);
            DrawLine(bld, blt, color);
            DrawLine(blt, brt, color);
            DrawLine(brt, brb, color);
            DrawLine(brb, bld, color);
            DrawLine(fld, bld, color);
            DrawLine(frd, brb, color);
            DrawLine(flt, blt, color);
            DrawLine(frt, brt, color);
            #endregion
        }

        private void DrawAABB(AxisAlignedBox aabb, Vector3 m_Position, float Yaw, Color color)
        {
            float cosY = (float)Math.Cos(Yaw);
            float sinY = (float)Math.Sin(Yaw);

            Vector3 fld = new Vector3(aabb.Min.Z * cosY - aabb.Min.X * sinY, aabb.Min.Y, aabb.Min.X * cosY + aabb.Min.Z * sinY) + m_Position; // 0
            Vector3 brt = new Vector3(aabb.Min.Z * cosY - aabb.Max.X * sinY, aabb.Min.Y, aabb.Max.X * cosY + aabb.Min.Z * sinY) + m_Position; // 1
            Vector3 bld = new Vector3(aabb.Max.Z * cosY - aabb.Max.X * sinY, aabb.Min.Y, aabb.Max.X * cosY + aabb.Max.Z * sinY) + m_Position; // 2
            Vector3 frt = new Vector3(aabb.Max.Z * cosY - aabb.Min.X * sinY, aabb.Min.Y, aabb.Min.X * cosY + aabb.Max.Z * sinY) + m_Position; // 3
            Vector3 frd = new Vector3(aabb.Max.Z * cosY - aabb.Min.X * sinY, aabb.Max.Y, aabb.Min.X * cosY + aabb.Max.Z * sinY) + m_Position; // 4
            Vector3 brb = new Vector3(aabb.Min.Z * cosY - aabb.Min.X * sinY, aabb.Max.Y, aabb.Min.X * cosY + aabb.Min.Z * sinY) + m_Position; // 5
            Vector3 blt = new Vector3(aabb.Min.Z * cosY - aabb.Max.X * sinY, aabb.Max.Y, aabb.Max.X * cosY + aabb.Min.Z * sinY) + m_Position; // 6
            Vector3 flt = new Vector3(aabb.Max.Z * cosY - aabb.Max.X * sinY, aabb.Max.Y, aabb.Max.X * cosY + aabb.Max.Z * sinY) + m_Position; // 7

            #region WorldToScreen
            if (!WorldToScreen(fld, out fld) || !WorldToScreen(brt, out brt)
                || !WorldToScreen(bld, out bld) || !WorldToScreen(frt, out frt)
                || !WorldToScreen(frd, out frd) || !WorldToScreen(brb, out brb)
                || !WorldToScreen(blt, out blt) || !WorldToScreen(flt, out flt))
                return;
            #endregion

            #region DrawLines
            DrawLine(fld, brt, color);
            DrawLine(brb, blt, color);
            DrawLine(fld, brb, color);
            DrawLine(brt, blt, color);

            DrawLine(frt, bld, color);
            DrawLine(frd, flt, color);
            DrawLine(frt, frd, color);
            DrawLine(bld, flt, color);

            DrawLine(frt, fld, color);
            DrawLine(frd, brb, color);
            DrawLine(brt, bld, color);
            DrawLine(blt, flt, color);
            #endregion
        }

        private void DrawMenu(int X, int Y)
        {
            Color selectedColor = new Color(255, 214, 0, 255);
            DrawText(X, Y, "F5: ESP Box", ESP_Box ? selectedColor : Color.White, true, fontSmall);
            DrawText(X + 100, Y, "F6: ESP Bone", ESP_Bone ? selectedColor : Color.White, true, fontSmall);
            DrawText(X + 200, Y, "F7: ESP Health", ESP_Health ? selectedColor : Color.White, true, fontSmall);
            DrawText(X + 310, Y, "F8: ESP Distance", ESP_Distance ? selectedColor : Color.White, true, fontSmall);
            DrawText(X + 430, Y, "F9: ESP Name", ESP_Name ? selectedColor : Color.White, true, fontSmall);
        }

        private void DrawWarn(int X, int Y, int W, int H)
        {
            RoundedRectangle rect = new RoundedRectangle();
            rect.RadiusX = 4;
            rect.RadiusY = 4;
            rect.Rect = new RectangleF(X, Y, W, H);

            solidColorBrush.Color = new Color(196, 26, 31, 210);
            device.FillRoundedRectangle(ref rect, solidColorBrush);

            DrawText(X + 20, Y + 5, "Spectator on the server.", Color.White, true);
            DrawText(X + 20, Y + 25, "Watching You!", Color.White, true);
        } 
        #endregion
    }
}
