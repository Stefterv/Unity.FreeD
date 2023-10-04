using System;
using System.Linq;

namespace FreeD
{
    [Serializable]
    public class Packet
    {
        public int Id;
        public float Pan;
        public float Tilt;
        public float Roll;
        public float PosZ;
        public float PosX;
        public float PosY;
        public int Zoom;
        public int Focus;

        public static Packet Decode(byte[] data)
        {
            //if (Checksum(data) == data[28])
            {
                var trackingData = new Packet
                {
                    Id = data.Skip(1).First(),
                    Pan = GetRotation(data.Skip(2).Take(3).ToArray()),
                    Tilt = GetRotation(data.Skip(5).Take(3).ToArray()),
                    Roll = GetRotation(data.Skip(8).Take(3).ToArray()),
                    PosZ = GetPosition(data.Skip(11).Take(3).ToArray()),
                    PosX = GetPosition(data.Skip(14).Take(3).ToArray()),
                    PosY = GetPosition(data.Skip(17).Take(3).ToArray()),
                    Zoom = GetEncoder(data.Skip(20).Take(3).ToArray()),
                    Focus = GetEncoder(data.Skip(23).Take(3).ToArray())
                };

                return trackingData;
            }
            throw new Exception("Calculated checksum does not match provided data. Probably not FreeD.");
        }

        public static byte[] Encode(Packet data)
        {
            byte[] output = { 0xD1 };  // Identifier
            output = output.Concat(new byte[] { 0xFF }).ToArray();  // ID
            output = output.Concat(SetRotation(data.Pan)).ToArray();  // Pitch
            output = output.Concat(SetRotation(data.Tilt)).ToArray();  // Yaw
            output = output.Concat(SetRotation(data.Roll)).ToArray();  // Roll
            output = output.Concat(SetPosition(data.PosZ)).ToArray();  // X
            output = output.Concat(SetPosition(data.PosX)).ToArray();  // Y
            output = output.Concat(SetPosition(data.PosY)).ToArray();  // Z
            output = output.Concat(SetEncoder(data.Zoom)).ToArray();  // Zoom
            output = output.Concat(SetEncoder(data.Focus)).ToArray();  // Focus
            output = output.Concat(new byte[] { 0x00, 0x00 }).ToArray();  // Reserved
            output = output.Concat(new byte[] { (byte)Checksum(output) }).ToArray();  // Checksum

            return output;
        }

        public static byte[] SetPosition(float pos)
        {
            long position = (long)(pos * 64 * 256);
            byte[] data = BitConverter.GetBytes(position);
            Array.Reverse(data);
            return data.Skip(4).Take(3).ToArray();
        }

        public static byte[] SetRotation(float rot)
        {
            long rotation = (long)(rot * 32768 * 256);
            byte[] data = BitConverter.GetBytes(rotation);
            Array.Reverse(data);
            return data.Skip(4).Take(3).ToArray();
        }

        public static byte[] SetEncoder(int enc)
        {
            byte[] data = BitConverter.GetBytes(enc);
            Array.Reverse(data);
            return new byte[] { 0x00 }.Concat(data.Skip(5).Take(2)).ToArray();
        }

        public static float GetPosition(byte[] data)
        {
            return (float)(BitConverter.ToInt32(new byte[] { 0x00, data[2], data[1], data[0] }, 0) / 64 / 256);
        }

        public static float GetRotation(byte[] data)
        {
            return (float)(BitConverter.ToInt32(new byte[] { 0x00, data[2], data[1], data[0] }, 0) / 32768 / 256);
        }

        public static int GetEncoder(byte[] data)
        {
            byte[] value = new byte[] { 0x00 }.Concat(data).ToArray();
            Array.Reverse(value);
            return BitConverter.ToInt32(value, 0);
        }

        public static int Checksum(byte[] data)
        {
            int sum = 64;
            for (int i = 0; i < 28; i++)
            {
                sum -= data[i];
            }
            return Modulo(sum, 256);
        }

        public static int Modulo(int d, int m)
        {
            int res = d % m;
            if ((res < 0 && m > 0) || (res > 0 && m < 0))
            {
                return res + m;
            }
            return res;
        }
    }
}