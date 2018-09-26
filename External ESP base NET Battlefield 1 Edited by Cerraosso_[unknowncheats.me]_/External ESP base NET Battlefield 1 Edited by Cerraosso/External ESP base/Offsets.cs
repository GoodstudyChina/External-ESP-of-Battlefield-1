/////////////XTREME HACK////////////////
///////////unknowncheats.me/////////////

using System;

namespace External_ESP_Base
{
    struct Offsets
    {
        public static Int64 OFFSET_DXRENDERER = 0x14360b618;
        public static Int64 OFFSET_GAMECONTEXT = 0x14341b650;
        public static Int64 OFFSET_GAMERENDERER = 0x14360b120;
        public static Int64 OFFSET_MAIN = 0x143128218;

        public struct ClientGameContext
        {
            public static Int64 m_pPlayerManager = 0x68;  // ClientPlayerManager

            public static Int64 GetInstance()
            {
                return OFFSET_GAMECONTEXT;
            }
        }

        public struct ClientPlayerManager
        {
            public static Int64 m_MaxPlayerCount = 0x0010; // INT32
            public static Int64 m_pLocalPlayer = 0x578;    // ClientPlayer
            public static Int64 m_ppPlayer = 0x100;        // ClientPlayer
        }

        public struct ClientPlayer
        {
            public static Int64 szName = 0x40;            // 10 CHARS
            public static Int64 m_isSpectator = 0xCE9;   // BYTE
            public static Int64 m_teamId = 0x1C34;        // INT32
            public static Int64 m_character = 0x1D28;     // ClientSoldierEntity 
            public static Int64 m_pAttachedControllable = 0x1D38;   // ClientSoldierEntity (ClientVehicleEntity)
            public static Int64 m_pControlledControllable = 0x1D48; // ClientSoldierEntity
            public static Int64 m_attachedEntryId = 0x1D40; // INT32
        }

        public struct ClientVehicleEntity
        {
            public static Int64 m_data = 0x0030;           // VehicleEntityData
            public static Int64 m_pPhysicsEntity = 0x0238;  // DynamicPhysicsEntity
            public static Int64 m_Velocity = 0x0310;       // D3DXVECTOR3 
            public static Int64 m_prevVelocity = 0x0320;   // D3DXVECTOR3 
            public static Int64 m_Chassis = 0x03E0;        // ClientChassisComponent
            public static Int64 m_childrenAABB = 0x02D0;   // AxisAlignedBox
        }

        public struct AxisAlignedBox
        {
            public static Int64 m_Min = 0x00; // D3DXVECTOR3 
            public static Int64 m_Max = 0x10; // D3DXVECTOR3 
        }

        public struct DynamicPhysicsEntity
        {
            public static Int64 m_EntityTransform = 0xA0;  // PhysicsEntityTransform
        }

        public struct PhysicsEntityTransform
        {
            public static Int64 m_Transform = 0x00;       // D3DXMATRIX
        }

        public struct VehicleEntityData
        {
            public static Int64 m_FrontMaxHealth = 0x148; // FLOAT
            public static Int64 m_NameSid = 0x0288;       // char* ID_P_VNAME_9K22
        }

        public struct ClientChassisComponent
        {
            public static Int64 m_Velocity = 0x01C0; // D3DXVECTOR4
        }

        public struct ClientSoldierEntity
        {
            public static Int64 m_data = 0x0030;         // VehicleEntityData
            public static Int64 m_pPlayer = 0x0260;          // ClientPlayer
            public static Int64 m_pHealthComponent = 0x01C0; // HealthComponent
            public static Int64 m_authorativeYaw = 0x05B4;   // FLOAT
            public static Int64 m_authorativePitch = 0x05E4; // FLOAT 
            public static Int64 m_poseType = 0x05E8;         // INT32
            public static Int64 m_pPredictedController = 0x0598;    // ClientSoldierPrediction
            public static Int64 m_soldierWeaponsComponent = 0x0648; // ClientSoldierWeaponsComponent
            public static Int64 m_ragdollComponent = 0x0460;        // ClientRagDollComponent 
            public static Int64 m_breathControlHandler = 0x0658;    // BreathControlHandler 
            public static Int64 m_sprinting = 0x698;  // BYTE 
            public static Int64 m_occluded = 0x069B;  // BYTE
        }

        public struct HealthComponent
        {
            public static Int64 m_Health = 0x0020;        // FLOAT
            public static Int64 m_MaxHealth = 0x0024;     // FLOAT
            public static Int64 m_vehicleHealth = 0x0040; // FLOAT (pLocalSoldier + 0x1E0 + 0x14C0 + 0x140 + 0x38)
        }

        public struct ClientSoldierPrediction
        {
            public static Int64 m_Position = 0x0040; // D3DXVECTOR3
            public static Int64 m_Velocity = 0x0060; // D3DXVECTOR3
        }

        public struct ClientSoldierWeaponsComponent
        {
            public enum WeaponSlot
            {
                M_PRIMARY = 0,
                M_SECONDARY = 1,
                M_GADGET = 2,
                M_GRENADE = 6,
                M_KNIFE = 7
            };

            public static Int64 m_handler = 0x08A8;      // m_handler + m_activeSlot * 0x8 = ClientSoldierWeapon
            public static Int64 m_activeSlot = 0x0960;   // INT32 (WeaponSlot)
        }

        public struct UpdatePoseResultData
        {
            public enum BONES
            {
                BONE_HEAD = 53,
                BONE_NECK = 51,
                BONE_SPINE2 = 7,
                BONE_SPINE1 = 6,
                BONE_SPINE = 5,
                BONE_LEFTSHOULDER = 8,
                BONE_RIGHTSHOULDER = 163,
                BONE_LEFTELBOWROLL = 14,
                BONE_RIGHTELBOWROLL = 169,
                BONE_LEFTHAND = 16,
                BONE_RIGHTHAND = 171,
                BONE_LEFTKNEEROLL = 285,
                BONE_RIGHTKNEEROLL = 299,
                BONE_LEFTFOOT = 277,
                BONE_RIGHTFOOT = 291
            };

            public static Int64 m_ActiveWorldTransforms = 0x0028; // QuatTransform
            public static Int64 m_ValidTransforms = 0x0040;       // BYTE
        }

        public struct ClientRagDollComponent
        {
            public static Int64 m_ragdollTransforms = 0x0088; // UpdatePoseResultData
            public static Int64 m_Transform = 0x05D0;         // D3DXMATRIX
        }

        public struct QuatTransform
        {
            public static Int64 m_TransAndScale = 0x0000; // D3DXVECTOR4
            public static Int64 m_Rotation = 0x0010;      // D3DXVECTOR4
        }

        public struct ClientSoldierWeapon
        {
            public static Int64 m_data = 0x0030;              // WeaponEntityData
            public static Int64 m_authorativeAiming = 0x4988; // ClientSoldierAimingSimulation
            public static Int64 m_pWeapon = 0x4A18;           // ClientWeapon
            public static Int64 m_pPrimary = 0x4A30;          // WeaponFiring
        }

        public struct ClientActiveWeaponHandler
        {
            public static Int64 m_activeWeapon = 0x038; // ClientSoldierWeapon
        }

        public struct WeaponEntityData
        {
            public static Int64 m_name = 0x0180; // char*
        }

        public struct ClientSoldierAimingSimulation
        {
            public static Int64 m_fpsAimer = 0x0028;  // AimAssist
            public static Int64 m_yaw = 0x0030;       // FLOAT
            public static Int64 m_pitch = 0x0034;     // FLOAT
            public static Int64 m_sway = 0x0040;      // D3DXVECTOR2
            public static Int64 m_zoomLevel = 0x34C;  // FLOAT
        }

        public struct ClientWeapon
        {
            public static Int64 m_pModifier =  0x0020; // WeaponModifier
            public static Int64 m_shootSpace = 0x0040; // D3DXMATRIX
        }

        public struct WeaponFiring
        {

            public static Int64 m_pSway = 0x0050;                  // WeaponSway
            public static Int64 m_pPrimaryFire = 0x0130;           // PrimaryFire 
            public static Int64 m_projectilesLoaded = 0x01D8;      // INT32 
            public static Int64 m_projectilesInMagazines = 0x01DC; // INT32 
            public static Int64 m_overheatPenaltyTimer = 0x01E8;   // FLOAT
            public static Int64 m_ReloadTimer = 0x0190; // float;
            public static Int64 m_WeaponModifier = 0x01F0; // WeaponModifier //
            public static Int64 m_pVehicleWeaponName = 0x01C8; // Char* //
            public static Int64 m_RecoilTimer = 0x019C; // Float
        }

        public struct WeaponSway
        {
            public static Int64 m_pSwayData = 0x0008;      // GunSwayData
            public static Int64 m_deviationPitch = 0x0148; // FLOAT 
            public static Int64 m_deviationYaw = 0x014C;   // FLOAT 
        }

        public struct GunSwayData
        {


            public static Int64 m_DeviationScaleFactorZoom = 0x03D0;           // FLOAT 
            public static Int64 m_DeviationScaleFactorNoZoom = 0x3D4;         // FLOAT 
            public static Int64 m_ShootingRecoilDecreaseScale = 0x03C8; // FLOAT 
            public static Int64 m_FirstShotRecoilMultiplier = 0x03D4;   // FLOAT 

        }

        public struct PrimaryFire
        {
             public static Int64 m_shotConfigData = 0x0010; // ShotConfigData
        }

        public struct ShotConfigData
        {
            public static Int64 m_initialSpeed = 0x0088;    // FLOAT 
            public static Int64 m_pProjectileData = 0x00B0; // BulletEntityData
        }

        public struct BulletEntityData
        {
            public static Int64 m_Gravity = 0x0130;     // FLOAT
            public static Int64 m_StartDamage = 0x0154; // FLOAT
            public static Int64 m_EndDamage = 0x0158;   // FLOAT
        }

        public struct AimAssist
        {
            public static Int64 m_yaw = 0x0024;   // FLOAT
            public static Int64 m_pitch = 0x0028; // FLOAT
        }

        public struct BreathControlHandler
        {


            public static Int64 m_breathControlTimer = 0x0038; // FLOAT
            public static Int64 m_breathControlMultiplier = 0x003C; // FLOAT  
            public static Int64 m_breathControlPenaltyTimer = 0x0040; // FLOAT  
            public static Int64 m_breathControlpenaltyMultiplier = 0x0044; // FLOAT  
            public static Int64 m_breathControlActive = 0x0048; // FLOAT  
            public static Int64 m_breathControlInput = 0x004C; // FLOAT  
            public static Int64 m_breathActive = 0x0050; // FLOAT  
            public static Int64 m_Enabled = 0x0058; // FLOAT  
        }

        public struct GameRenderer
        {
            public static Int64 m_pRenderView = 0x60; // RenderView

            public static Int64 GetInstance()
            {
                return OFFSET_GAMERENDERER;
            }
        }

        public struct RenderView
        {

            public static Int64 m_Transform = 0x0040;         // D3DXMATRIX
            public static Int64 m_FovY = 0x00E4;              // FLOAT 0x0114
            public static Int64 m_FovX = 0x0420;              // FLOAT 0x02C0
            public static Int64 m_ViewProj = 0x0460;          // D3DXMATRIX 0x0490
            public static Int64 m_ViewMatrixInverse = 0x0320; // D3DXMATRIX 0x0350
            public static Int64 m_ViewProjInverse = 0x04E0;   // D3DXMATRIX 0x0510

        }
    }
}
