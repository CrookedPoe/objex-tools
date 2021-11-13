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
    public class Vector2 {
        public float X {get; set;}
        public float Y {get; set;}

        public Vector2() { }

        public Vector2(dynamic x, dynamic y)
        {
            this.X = Convert.ToSingle(x);
            this.Y = Convert.ToSingle(y);
        }
    }

    public class Vector3 {
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
        private double _toDegrees = (65536 / 360);
        public Vector3 Degrees { get; set; }

        public Rotation3D() { }
        public Rotation3D(Int16 X, Int16 Y, Int16 Z)
        {
            Degrees = new Vector3(X, Y, Z);
            Degrees.X /= Convert.ToSingle(_toDegrees);
            Degrees.Y /= Convert.ToSingle(_toDegrees);
            Degrees.Z /= Convert.ToSingle(_toDegrees);
        }

        public Rotation3D(Vector3 vec, bool isDegrees)
        {
            if (isDegrees) {
                Degrees = new Vector3(vec.X, vec.Y, vec.Z);
            } else
            {
                Degrees = new Vector3(vec.X, vec.Y, vec.Z);
                Degrees.X /= Convert.ToSingle(_toDegrees);
                Degrees.Y /= Convert.ToSingle(_toDegrees);
                Degrees.Z /= Convert.ToSingle(_toDegrees);
            }
        }

        public static Rotation3D AdjustRotation(Rotation3D r, float X, float Y, float Z)
        {
            Rotation3D newRot = new Rotation3D(r.Degrees, true);
            newRot.Degrees.X += X;
            newRot.Degrees.Y += Y;
            newRot.Degrees.Z += Z;
            return newRot;
        }
    }
}