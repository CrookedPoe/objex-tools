using System;

namespace animutil
{
    public class Vector
    {
        public static dynamic NewVector(params dynamic[] n)
        {
            dynamic returnClass = 0;
            if (n.Length < 2) {
                returnClass = 0;
            } else if (n.Length == 2) {
                returnClass = new Vector2(n[0], n[1]);
            } else if (n.Length == 3) {
                returnClass = new Vector3(n[0], n[1], n[2]);
            }

            return returnClass;
        }
    }
    
    public class Vector2
    {
        public float X {get; set;}
        public float Y {get; set;}

        public Vector2() { }

        public Vector2(dynamic x, dynamic y)
        {
            this.X = Convert.ToSingle(x);
            this.Y = Convert.ToSingle(y);
        }
    }

    public class Vector3
    {
        public float X {get; set;}
        public float Y {get; set;}
        public float Z {get; set;}

        public Vector3() { }

        public Vector3(dynamic X, dynamic Y, dynamic Z)
        {
            this.X = Convert.ToSingle(X);
            this.Y = Convert.ToSingle(Y);
            this.Z = Convert.ToSingle(Z);
        }
    }

    public class Rotation3D
    {
        private double _shortToDegrees = (65536 / 360);
        private double _degreesToRadians = (Math.PI / 180);
        private double _radiansToDegrees = (180 / Math.PI);
        public Int16[] Shorts { get; set; }
        public Vector3 Degrees { get; set; }
        public Vector3 Radians { get; set; }

        public Byte[] ShortBytes { get; set; }

        public Rotation3D() { }
        public Rotation3D(Int16 X, Int16 Y, Int16 Z)
        {
            Shorts = new Int16[3] {X, Y, Z};

            ShortBytes = new Byte[6];
            for (int i = 0; i < 6; i += 2)
            {
                byte[] s = BitConverter.GetBytes(Shorts[i / 2]);
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(s);
                }
                ShortBytes[i + 0] = s[0];
                ShortBytes[i + 1] = s[1];
            }

            Degrees = new Vector3(X, Y, Z);
            Degrees.X /= Convert.ToSingle(_shortToDegrees);
            Degrees.Y /= Convert.ToSingle(_shortToDegrees);
            Degrees.Z /= Convert.ToSingle(_shortToDegrees);

            Radians = Degrees;
            Radians.X *= Convert.ToSingle(_degreesToRadians);
            Radians.Y *= Convert.ToSingle(_degreesToRadians);
            Radians.Z *= Convert.ToSingle(_degreesToRadians);
        }

        public Rotation3D(float X, float Y, float Z, string type)
        {
            Shorts = new Int16[3] {0, 0, 0};
            Radians = new Vector3(0, 0, 0);
            Degrees = new Vector3(0, 0, 0);

            if (type.ToLower() == "degrees") {
                Degrees = new Vector3(X, Y, Z);
                Radians.X = Degrees.X * Convert.ToSingle(_degreesToRadians);
                Radians.Y = Degrees.Y * Convert.ToSingle(_degreesToRadians);
                Radians.Z = Degrees.Z * Convert.ToSingle(_degreesToRadians);
                Shorts[0] = Convert.ToInt16(WrapEuler(Degrees.X) * Convert.ToSingle(_shortToDegrees));
                Shorts[1] = Convert.ToInt16(WrapEuler(Degrees.Y) * Convert.ToSingle(_shortToDegrees));
                Shorts[2] = Convert.ToInt16(WrapEuler(Degrees.Z) * Convert.ToSingle(_shortToDegrees));
            } else if (type.ToLower() == "radians") {
                Radians = new Vector3(X, Y, Z);
                Shorts[0] = Convert.ToInt16(WrapEuler(Degrees.X) * Convert.ToSingle(_shortToDegrees));
                Shorts[1] = Convert.ToInt16(WrapEuler(Degrees.Y) * Convert.ToSingle(_shortToDegrees));
                Shorts[2] = Convert.ToInt16(WrapEuler(Degrees.Z) * Convert.ToSingle(_shortToDegrees));
            } else if (type.ToLower() == "shorts") {
                Shorts[0] = Convert.ToInt16(X);
                Shorts[1] = Convert.ToInt16(Y);
                Shorts[2] = Convert.ToInt16(Z);
                Degrees = new Vector3(Shorts[0], Shorts[1], Shorts[2]);
                Degrees.X /= Convert.ToSingle(_shortToDegrees);
                Degrees.Y /= Convert.ToSingle(_shortToDegrees);
                Degrees.Z /= Convert.ToSingle(_shortToDegrees);
                Radians = new Vector3(Degrees.X, Degrees.Y, Degrees.Z);
                Radians.X *= Convert.ToSingle(_degreesToRadians);
                Radians.Y *= Convert.ToSingle(_degreesToRadians);
                Radians.Z *= Convert.ToSingle(_degreesToRadians);
            } else if (type.ToLower() == "vector") {
                Radians = new Vector3(X, Y, Z);
                Degrees = new Vector3(X, Y, Z);
                Shorts = new Int16[3] {
                    Convert.ToInt16(X),
                    Convert.ToInt16(Y),
                    Convert.ToInt16(Z)
                };
            } else {
                Radians = new Vector3(X, Y, Z);
                Radians.X = Degrees.X * Convert.ToSingle(_degreesToRadians);
                Radians.Y = Degrees.Y * Convert.ToSingle(_degreesToRadians);
                Radians.Z = Degrees.Z * Convert.ToSingle(_degreesToRadians);
                Degrees = new Vector3(X, Y, Z);
                Degrees.X = Radians.X * Convert.ToSingle(_radiansToDegrees);
                Degrees.Y = Radians.Y * Convert.ToSingle(_radiansToDegrees);
                Degrees.Z = Radians.Z * Convert.ToSingle(_radiansToDegrees);
                Shorts[0] = Convert.ToInt16((Degrees.X * Convert.ToSingle(_shortToDegrees)));
                Shorts[1] = Convert.ToInt16((Degrees.Y * Convert.ToSingle(_shortToDegrees)));
                Shorts[2] = Convert.ToInt16((Degrees.Z * Convert.ToSingle(_shortToDegrees)));
            }

            ShortBytes = new Byte[6];
            for (int i = 0; i < 6; i += 2)
            {
                byte[] s = BitConverter.GetBytes(Shorts[i / 2]);
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(s);
                }
                ShortBytes[i + 0] = s[0];
                ShortBytes[i + 1] = s[1];
            }
        }

        public static Rotation3D AdjustRotation(Rotation3D r, float X, float Y, float Z)
        {
            return new Rotation3D(r.Degrees.X + X, r.Degrees.Y + Y, r.Degrees.Z + Z, "Degrees");
        }

        public static float Clamp(float n, int min, int max)
        {
            return ((n) < (min) ? (min) : (n) > (max) ? (max) : (n));
        }
        public static float WrapEuler(float n)
        {
            int min = -180;
            int max = 179;
            return Convert.ToSingle((n < min) ? (max + (n - min) + 1) : (n > max) ? (max - (n - max)) : n);
        }
    }
}