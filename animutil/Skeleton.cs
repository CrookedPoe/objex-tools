using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace animutil
{
    public class SkeletonHeader
    {
        public string Name { get; set; }
        public SegmentAddress Offset { get; set; }
        public SegmentAddress LimbIndexPtr { get; set;}
        public int LimbCount { get; set;}

        public SkeletonHeader() { }

        public SkeletonHeader(string name, SegmentAddress o, byte[] b)
        {  
            Name = name;
            Offset = o;
            LimbIndexPtr = new SegmentAddress(b.BEReadUInt32(0));
            LimbCount = Convert.ToInt32(b.ReadUByte(4));
        }

    }

    public class FlexSkeletonHeader : SkeletonHeader
    {
        public int GfxLimbCount { get; set; }

        public FlexSkeletonHeader() { }

        public FlexSkeletonHeader(string name, SegmentAddress o, byte[] b) : base (name, o, b)
        {
            GfxLimbCount = Convert.ToInt32(b.ReadUByte(8));
        }
    }

    public class Limb
    {
        public SegmentAddress Offset { get; set; }
        public Vector3 Position { get; set; }

        public int Child { get; set; }
        public int Sibling { get; set; }

        public SegmentAddress DisplayListNear { get; set; }

        public Limb() { }
        public Limb(SegmentAddress o, byte[] b)
        {
            Offset = o;
            Position = Vector.NewVector(
                b.BEReadInt16(0),
                b.BEReadInt16(2),
                b.BEReadInt16(4)
            );
            Child = Convert.ToInt32(b.ReadSByte(6));
            Sibling = Convert.ToInt32(b.ReadSByte(7));
            DisplayListNear = new SegmentAddress(b.BEReadUInt32(8));
        }
    }

    public class LODLimb : Limb
    {
        public SegmentAddress DisplayListFar { get; set; }

        public LODLimb() { }
        public LODLimb(SegmentAddress o, byte[] b) : base(o, b)
        {
            DisplayListFar = new SegmentAddress(b.BEReadUInt32(12));
        }
    }

    public class Skeleton
    {
        private static int _stack = 0;
        public dynamic Header { get; set; }

        public SegmentAddress[] LimbIndex { get; set; }

        public dynamic[] LimbTable { get; set; }

        public bool isLOD { get; set; }

        public bool isFlex { get; set; }

        public Skeleton() { }

        public Skeleton(SkeletonJSONEntry skeleton, byte[] zobjBuffer)
        {
            // Parse and Create Header
            isLOD = skeleton.isLOD;
            isFlex = skeleton.isFlex;
            SegmentAddress offset = new SegmentAddress(skeleton.Offset.ToString(), 16);
            byte[] skeleton_header = zobjBuffer.BlockCopy(offset.Address, ((isFlex) ? 12 : 8));
            Header = (isFlex) ? (new FlexSkeletonHeader(skeleton.Name, offset, skeleton_header)) : (new SkeletonHeader(skeleton.Name, offset, skeleton_header));

            // Populate Limb Index
            int temp = Header.LimbCount;
            int temp2 = Header.LimbIndexPtr.Address;
            byte[] limb_index = zobjBuffer.BlockCopy(temp2, temp * 4);
            LimbIndex = new SegmentAddress[temp];
            for (int i = 0; i < limb_index.Length; i += 4) {
                LimbIndex[i / 4] = new SegmentAddress(limb_index.BEReadUInt32(i));
            }

            // Populate Limb Table
            LimbTable = new dynamic[temp];
            int limb_size = (isLOD) ? 16 : 12;
            for (int i = 0; i < LimbIndex.Length; i++) {
                SegmentAddress limb = LimbIndex[i];
                LimbTable[i] = (isLOD) ? (new LODLimb(limb, zobjBuffer.BlockCopy(limb.Address, limb_size))) : (new Limb(limb, zobjBuffer.BlockCopy(limb.Address, limb_size)));
            }
        }

        public static List<SkeletonJSONEntry> Find(byte[] zobjBuffer, int segment)
        {
            List<SkeletonJSONEntry> skeletons = new List<SkeletonJSONEntry>();
            SkeletonJSONEntry skeleton = new SkeletonJSONEntry {
                isFlex = false,
                isLOD = false
            };
            
            for (int i = 0; i < zobjBuffer.Length - 16; i += 8) {
                SegmentAddress[] check = new SegmentAddress[5] {
                    new SegmentAddress(zobjBuffer.BEReadUInt32(i + 0)),
                    new SegmentAddress(zobjBuffer.BEReadUInt32(i + 4)),
                    new SegmentAddress(zobjBuffer.BEReadUInt32(i + 8)),
                    new SegmentAddress(zobjBuffer.BEReadUInt32(i + 12)),
                    new SegmentAddress(zobjBuffer.BEReadUInt32(i + 16))
                };

                /* Determine if a skeleton was found. */
                if (check[2].Segment == segment && (check[2].Address < zobjBuffer.Length)) {
                    if (check[3].Segment > 0 && check[3].Address == 0) {
                        /* Likely a skeleton. */
                        int header_offset = i + 8;
                        MyConsole.WriteLine("NOTE", $"New skeleton at 0x{header_offset.ToString("X6")}");
                        if (check[4].Segment <= check[3].Segment && check[4].Address == 0) {
                            skeleton.isFlex = true;
                        }
                        if ((check[1].Address - check[0].Address) > 12) {
                            skeleton.isLOD = true;
                        }
                        skeleton.Name = $"skl_0x{header_offset.ToString("X6")}";
                        skeleton.Offset = $"0x{segment.ToString("X2")}{header_offset.ToString("X6")}";
                        skeletons.Add(skeleton);
                    }
                }
            }

            return skeletons;
        }

        private static void WriteTree(int objexVersion, StreamWriter f, dynamic[] l, int n) {
            int v = objexVersion;
            string name = $"\"limb_{n.ToString("D2")}\"";
            string pos = String.Empty;
            if (v == 1) {
                pos = $"{{{l[n].Position.X.ToString("F2")} {l[n].Position.Y.ToString("F2")} {l[n].Position.Z.ToString("F2")}}}";
            } else if (v == 2) {
                pos = $"{l[n].Position.X.ToString("F2")} {l[n].Position.Y.ToString("F2")} {l[n].Position.Z.ToString("F2")}";
            } else {
                pos = $"{l[n].Position.X.ToString("F2")} {l[n].Position.Y.ToString("F2")} {l[n].Position.Z.ToString("F2")}";
            }

            f.WriteLine($"{new String('\t', _stack++)}+ {name} {pos}");
            if (l[n].Child > -1)
                WriteTree(v, f, l, l[n].Child);
            f.WriteLine($"{new String('\t', --_stack)}-");
            if (l[n].Sibling > -1)
                WriteTree(v, f, l, l[n].Sibling);
        }
        public static void Export(int objexVersion, Skeleton skeleton, string filePath)
        {
            int v = objexVersion;
check_objex_v:
            if (v == 1)
            {
                using (StreamWriter f = new StreamWriter(File.Create(filePath))) {
                    f.WriteLine($"limbs {skeleton.LimbTable.Length}");
                    WriteTree(v, f, skeleton.LimbTable, 0);
                }
            } else if (v == 2) {
                using (StreamWriter f = new StreamWriter(File.Create(filePath))) {
                    f.WriteLine($"newskel \"{skeleton.Header.Name}\" \"{((skeleton.isLOD) ? "z64player" : "z64npc")}\"");
                    WriteTree(v, f, skeleton.LimbTable, 0);
                }
            } else {
                MyConsole.WriteLine("WARN", "Invalid OBJEX version specified, defaulting to OBJEX Version 2.");
                v = 2;
                goto check_objex_v;
            }
        }
    }
}