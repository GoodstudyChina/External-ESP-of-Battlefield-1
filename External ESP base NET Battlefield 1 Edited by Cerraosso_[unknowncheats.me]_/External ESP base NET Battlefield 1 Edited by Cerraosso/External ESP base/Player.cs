/////////////XTREME HACK////////////////
///////////unknowncheats.me/////////////

using System;
using SharpDX;

namespace External_ESP_Base
{
    class Player
    {
        public string Name;
        public string VehicleName;
        public int Team;
        public Vector3 Origin;
        public RadDoll Bone;
        public bool InVehicle;
        public bool IsDriver;
        public bool IsSpectator;

        // Weapon Data
        public int Ammo, AmmoClip;

        public int Pose;
        public int IsOccluded;

        public float Yaw;
        public float Distance;

        // Soldier
        public float Health;
        public float MaxHealth;

        public bool IsValid()
        {
            return (Health > 0.1f && Health <= 100 && !Origin.IsZero);
        }

        public bool IsVisible()
        {
           return (IsOccluded == 0);
        }

        public AxisAlignedBox GetAABB()
        {
            AxisAlignedBox aabb = new AxisAlignedBox();
            if (this.Pose == 0) // standing
            {
                aabb.Min = new Vector3(-0.350000f, 0.000000f, -0.350000f);
                aabb.Max = new Vector3(0.350000f, 1.700000f, 0.350000f);
            }
            if (this.Pose == 1) // crouching
            {
                aabb.Min = new Vector3(-0.350000f, 0.000000f, -0.350000f);
                aabb.Max = new Vector3(0.350000f, 1.150000f, 0.350000f);
            }
            if (this.Pose == 2) // prone
            {
                aabb.Min = new Vector3(-0.350000f, 0.000000f, -0.350000f);
                aabb.Max = new Vector3(0.350000f, 0.400000f, 0.350000f);
            }
            return aabb;
        }
    }
}
