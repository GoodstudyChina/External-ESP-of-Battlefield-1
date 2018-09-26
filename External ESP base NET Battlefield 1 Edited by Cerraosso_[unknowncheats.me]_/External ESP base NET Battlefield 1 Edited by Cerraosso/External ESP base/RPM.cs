/////////////XTREME HACK////////////////
///////////unknowncheats.me/////////////

using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace External_ESP_Base
{
    class RPM
    {
        private static IntPtr pHandle = IntPtr.Zero;

        public static IntPtr OpenProcess(int pId)
        {
            pHandle = Managed.OpenProcess(Managed.PROCESS_VM_READ | Managed.PROCESS_VM_WRITE | Managed.PROCESS_VM_OPERATION, false, pId);
            return pHandle;
        }

        public static IntPtr GetHandle()
        {
            return pHandle;
        }

        public static void CloseProcess()
        {
            Managed.CloseHandle(pHandle);
        }

        public static Int64 ReadInt64(Int64 _lpBaseAddress)
        {
            byte[] Buffer = new byte[8];
            IntPtr ByteRead;
            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, Buffer, 8, out ByteRead);
            return BitConverter.ToInt64(Buffer, 0);
        }

        public static Int32 ReadInt32(Int64 _lpBaseAddress)
        {
            byte[] Buffer = new byte[4];
            IntPtr ByteRead;
            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, Buffer, 4, out ByteRead);
            return BitConverter.ToInt32(Buffer, 0);
        }

        public static float ReadFloat(Int64 _lpBaseAddress)
        {
            byte[] Buffer = new byte[sizeof(float)];
            IntPtr ByteRead;
            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, Buffer, sizeof(float), out ByteRead);
            return BitConverter.ToSingle(Buffer, 0);
        }

        public static bool WriteMemory(Int64 MemoryAddress, byte[] Buffer)
        {
            uint oldProtect;
            Managed.VirtualProtectEx(pHandle, (IntPtr)MemoryAddress, (uint)Buffer.Length, Managed.PAGE_READWRITE, out oldProtect);
            IntPtr ptrBytesWritten;
            return Managed.WriteProcessMemory(pHandle, MemoryAddress, Buffer, (uint)Buffer.Length, out ptrBytesWritten);
        }

        public static bool WriteFloat(Int64 _lpBaseAddress, float _Value)
        {
            byte[] Buffer = BitConverter.GetBytes(_Value);
            return WriteMemory(_lpBaseAddress, Buffer);
        }

        public static bool WriteInt32(Int64 _lpBaseAddress, int _Value)
        {
            byte[] Buffer = BitConverter.GetBytes(_Value);
            return WriteMemory(_lpBaseAddress, Buffer);
        }

        public static bool WriteByte(Int64 _lpBaseAddress, byte _Value)
        {
            byte[] Buffer = BitConverter.GetBytes(_Value);
            return WriteMemory(_lpBaseAddress, Buffer);
        }

        public static byte ReadByte(Int64 _lpBaseAddress)
        {
            byte[] Buffer = new byte[sizeof(byte)];
            IntPtr ByteRead;
            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, Buffer, sizeof(byte), out ByteRead);
            return Buffer[0];
        }

        public static string ReadString(Int64 _lpBaseAddress, UInt64 _Size)
        {
            byte[] buffer = new byte[_Size];
            IntPtr BytesRead;

            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, buffer, _Size, out BytesRead);

            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == 0)
                {
                    byte[] _buffer = new byte[i];
                    Buffer.BlockCopy(buffer, 0, _buffer, 0, i);
                    return Encoding.ASCII.GetString(_buffer);
                }
            }
            return Encoding.ASCII.GetString(buffer);
        }

        public static string ReadString2(Int64 _lpBaseAddress, UInt64 _Size)
        {
            byte[] buffer = new byte[_Size];
            IntPtr BytesRead;

            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, buffer, _Size, out BytesRead);
            return Encoding.ASCII.GetString(buffer);
        }

        public static Vector2 ReadVector2(Int64 _lpBaseAddress)
        {
            Vector2 tmp = new Vector2();

            byte[] Buffer = new byte[8];
            IntPtr ByteRead;

            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, Buffer, 8, out ByteRead);
            tmp.X = BitConverter.ToSingle(Buffer, (0 * 4));
            tmp.Y = BitConverter.ToSingle(Buffer, (1 * 4));
            return tmp;
        }

        public static Vector3 ReadVector3(Int64 _lpBaseAddress)
        {
            Vector3 tmp = new Vector3();

            byte[] Buffer = new byte[12];
            IntPtr ByteRead;

            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, Buffer, 12, out ByteRead);
            tmp.X = BitConverter.ToSingle(Buffer, (0 * 4));
            tmp.Y = BitConverter.ToSingle(Buffer, (1 * 4));
            tmp.Z = BitConverter.ToSingle(Buffer, (2 * 4));
            return tmp;
        }

        public static AxisAlignedBox ReadAABB(Int64 _lpBaseAddress)
        {
            AxisAlignedBox tmp = new AxisAlignedBox();
            byte[] Buffer = new byte[32];
            IntPtr ByteRead;

            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, Buffer, 32, out ByteRead);
            tmp.Min.X = BitConverter.ToSingle(Buffer, (0 * 4));
            tmp.Min.Y = BitConverter.ToSingle(Buffer, (1 * 4));
            tmp.Min.Z = BitConverter.ToSingle(Buffer, (2 * 4));
            tmp.Max.X = BitConverter.ToSingle(Buffer, (4 * 4));
            tmp.Max.Y = BitConverter.ToSingle(Buffer, (5 * 4));
            tmp.Max.Z = BitConverter.ToSingle(Buffer, (6 * 4));
            return tmp;
        }

        public static Vector4 ReadVector4(Int64 _lpBaseAddress)
        {
            Vector4 tmp = new Vector4();

            byte[] Buffer = new byte[16];
            IntPtr ByteRead;

            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, Buffer, 16, out ByteRead);
            tmp.X = BitConverter.ToSingle(Buffer, (0 * 4));
            tmp.Y = BitConverter.ToSingle(Buffer, (1 * 4));
            tmp.Z = BitConverter.ToSingle(Buffer, (2 * 4));
            tmp.W = BitConverter.ToSingle(Buffer, (3 * 4));
            return tmp;
        }

        public static Matrix ReadMatrix(Int64 _lpBaseAddress)
        {
            Matrix tmp = new Matrix();

            byte[] Buffer = new byte[64];
            IntPtr ByteRead;

            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, Buffer, 64, out ByteRead);

            tmp.M11 = BitConverter.ToSingle(Buffer, (0 * 4));
            tmp.M12 = BitConverter.ToSingle(Buffer, (1 * 4));
            tmp.M13 = BitConverter.ToSingle(Buffer, (2 * 4));
            tmp.M14 = BitConverter.ToSingle(Buffer, (3 * 4));

            tmp.M21 = BitConverter.ToSingle(Buffer, (4 * 4));
            tmp.M22 = BitConverter.ToSingle(Buffer, (5 * 4));
            tmp.M23 = BitConverter.ToSingle(Buffer, (6 * 4));
            tmp.M24 = BitConverter.ToSingle(Buffer, (7 * 4));

            tmp.M31 = BitConverter.ToSingle(Buffer, (8 * 4));
            tmp.M32 = BitConverter.ToSingle(Buffer, (9 * 4));
            tmp.M33 = BitConverter.ToSingle(Buffer, (10 * 4));
            tmp.M34 = BitConverter.ToSingle(Buffer, (11 * 4));

            tmp.M41 = BitConverter.ToSingle(Buffer, (12 * 4));
            tmp.M42 = BitConverter.ToSingle(Buffer, (13 * 4));
            tmp.M43 = BitConverter.ToSingle(Buffer, (14 * 4));
            tmp.M44 = BitConverter.ToSingle(Buffer, (15 * 4));
            return tmp;
        }

        public static bool IsValid(Int64 Address)
        {
            return (Address >= 0x10000 && Address < 0x000F000000000000);
        } 
    }
}
