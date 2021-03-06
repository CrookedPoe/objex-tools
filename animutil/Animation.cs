using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace animutil
{
    public class BaseAnimationHeader {
        public string Name { get; set; }
        public SegmentAddress Offset { get; set; }
        public int FrameCount { get; set; }

        public BaseAnimationHeader() { }

        public BaseAnimationHeader(string name, SegmentAddress o, byte[] b)
        {
            Name = name;
            Offset = o;
            FrameCount = Convert.ToInt32(b.BEReadInt16(0));
        }
    }

    public class NPCFrame
    {
        public static int Size { get; set; }
        public Vector3 RootTranslation;
        public Rotation3D[] LimbRotations;

        public NPCFrame() { }

        public NPCFrame(Int16[] lut, int f, Int16 limit, byte[] key)
        {
            LimbRotations = new Rotation3D[(key.Length / 6) - 1];
            for (int i = 0; i < key.Length; i += 6) {
                Int16 x = key.BEReadInt16(i + 0);
                Int16 y = key.BEReadInt16(i + 2);
                Int16 z = key.BEReadInt16(i + 4);
                if (i < 6){
                    RootTranslation = new Vector3(
                        (x >= limit) ? lut[x + f] : lut[x + 0],
                        (y >= limit) ? lut[y + f] : lut[y + 0],
                        (z >= limit) ? lut[z + f] : lut[z + 0]
                    );
                } else {
                    LimbRotations[(i / 6) - 1] = new Rotation3D(
                        (x >= limit) ? lut[x + f] : lut[x + 0],
                        (y >= limit) ? lut[y + f] : lut[y + 0],
                        (z >= limit) ? lut[z + f] : lut[z + 0]
                    );
                }
            }
        }
    }

    public class NPCAnimation : BaseAnimationHeader
    {
        public Skeleton Skeleton { get; set; }
        public SegmentAddress RotationLookupPtr { get; set; }
        public SegmentAddress LimbKeyPtr { get; set;}
        public Int16 Limit { get; set; }
        public Int16[] RotationLookupData { get; set; }
        public NPCFrame[] AnimationFrames { get; set; }

        public NPCAnimation() { }

        public NPCAnimation(AnimationJSONEntry animation, Skeleton skeleton, byte[] zobjBuffer)
        {
            Name = animation.Name;
            Skeleton = skeleton;
            Offset = new SegmentAddress(animation.Offset, 16);
            byte[] animation_header = zobjBuffer.BlockCopy(Offset.Address, 16);
            FrameCount = animation_header.BEReadInt16(0);
            RotationLookupPtr = new SegmentAddress(animation_header.BEReadUInt32(4));
            LimbKeyPtr = new SegmentAddress(animation_header.BEReadUInt32(8));
            Limit = animation_header.BEReadInt16(12);

            int keySize = ((skeleton.Header.LimbCount) + 1) * 6;
            byte[] keyData = zobjBuffer.BlockCopy(LimbKeyPtr.Address, keySize);

            int lutSize = (LimbKeyPtr.Address - RotationLookupPtr.Address);
            byte[] lutData = zobjBuffer.BlockCopy(RotationLookupPtr.Address, lutSize);
            RotationLookupData = new Int16[lutSize];
            for (int i = 0; i < lutData.Length; i += 2) {
                RotationLookupData[i / 2] = lutData.BEReadInt16(i);
            }

            AnimationFrames = new NPCFrame[FrameCount];
            for (int i = 0; i < FrameCount; i++) {
                AnimationFrames[i] = new NPCFrame(RotationLookupData, i, Limit, keyData);
            }
        }

        public static List<AnimationJSONEntry> Find(byte[] zobjBuffer, int segment)
        {
            List<AnimationJSONEntry> animations = new List<AnimationJSONEntry>();
            for (int i = 0; i < zobjBuffer.Length - 16; i += 4) {
                if (zobjBuffer[i + 2] == 0 && zobjBuffer[i + 3] == 0) {
                    SegmentAddress[] check = new SegmentAddress[2] {
                        new SegmentAddress(zobjBuffer.BEReadUInt32(i + 4)),
                        new SegmentAddress(zobjBuffer.BEReadUInt32(i + 8))
                    };

                    if (check[0].Segment == segment && check[1].Segment == segment) {
                        if (check[0].Address < zobjBuffer.Length && check[0].Address % 4 == 0) {
                            if ((check[1].Address < zobjBuffer.Length && check[1].Address % 4 == 0)) {
                                /* Likely an animation header. */
                                int header_offset = i;
                                MyConsole.WriteLine("NOTE", $"New animation at 0x{header_offset.ToString("X6")} ({zobjBuffer.BEReadInt16(header_offset)} frames)");
                                AnimationJSONEntry animation = new AnimationJSONEntry {
                                    Name = $"anim_0x{header_offset.ToString("X6")}",
                                    Offset = $"0x{segment.ToString("X2")}{header_offset.ToString("X6")}"
                                };
                                animations.Add(animation);
                            }
                        }
                    }
                }
            }
            return animations;
        }

        public static void Export(int objexVersion, NPCAnimation anim, string filePath)
        {
            int v = objexVersion;
check_objex_v:
            if (v == 1)
            {
                using (StreamWriter f = new StreamWriter(File.Create(filePath))) {
                    f.WriteLine("anim_total 1");
                    f.WriteLine($"frames {anim.FrameCount} \"{anim.Name}\"");
                    for (int i = 0; i < anim.AnimationFrames.Length; i++) {
                        NPCFrame frame = anim.AnimationFrames[i];
                        f.WriteLine($"l {frame.RootTranslation.X} {frame.RootTranslation.Y} {frame.RootTranslation.Z}");
                        for (int j = 0; j < frame.LimbRotations.Length; j++) {
                            f.WriteLine($"r {frame.LimbRotations[j].Radians.X.ToString("F3")} {frame.LimbRotations[j].Radians.Y.ToString("F3")} {frame.LimbRotations[j].Radians.Z.ToString("F3")}");
                        }
                    }
                }
            } else if (v == 2)
            {
                using (StreamWriter f = new StreamWriter(File.Create(filePath))) {
                    f.WriteLine($"newskel \"{anim.Skeleton.Header.Name}\" \"{anim.Name}\" {anim.FrameCount}");
                    for (int i = 0; i < anim.AnimationFrames.Length; i++) {
                        f.WriteLine($"# Frame {i + 1}");
                        NPCFrame frame = anim.AnimationFrames[i];
                        f.WriteLine($"loc {frame.RootTranslation.X} {frame.RootTranslation.Y} {frame.RootTranslation.Z}");
                        for (int j = 0; j < frame.LimbRotations.Length; j++) {
                            f.WriteLine($"rot {frame.LimbRotations[j].Degrees.X.ToString("F2")} {frame.LimbRotations[j].Degrees.Y.ToString("F2")} {frame.LimbRotations[j].Degrees.Z.ToString("F2")}");
                        }
                    }
                }
            } else {
                MyConsole.WriteLine("WARN", "Invalid OBJEX version specified, defaulting to OBJEX Version 2.");
                v = 2;
                goto check_objex_v;
            }
        }
    }
    public class LinkFrame
    {
        public static int Size = 0x86;
        private static string[] EyeIndex = {
            "Automatic",
            "Open",
            "Half Open",
            "Closed",
            "Looking Left",
            "Looking Right",
            "Surprised / Shocked",
            "Looking Down",
            "Tightly Closed"
        };

        private static string[] MouthIndex = {
            "Automatic",
            "Closed",
            "Slightly Opened",
            "Open Wide / Shouting",
            "Smile"
        };

        public Vector3 RootTranslation;
        public Rotation3D[] LimbRotations;
        public string Face;

        public LinkFrame() { }

        public LinkFrame(byte[] frameData)
        {
            LimbRotations = new Rotation3D[21];
            for (int i = 0; i < frameData.Length - 2; i += 6) {
                if (i < 6){
                    RootTranslation = new Vector3(
                        frameData.BEReadInt16(i + 0),
                        frameData.BEReadInt16(i + 2),
                        frameData.BEReadInt16(i + 4)
                    );
                } else {
                    LimbRotations[(i / 6) - 1] = new Rotation3D(
                        frameData.BEReadInt16(i + 0),
                        frameData.BEReadInt16(i + 2),
                        frameData.BEReadInt16(i + 4)
                    );
                }
            }
            Face = $"# Eyes: {EyeIndex[frameData[0x85] & 0x0F]}, Mouth: {MouthIndex[(frameData[0x85] & 0xF0) >> 4]}";
        }

        public LinkFrame(JArray limbMapFromTo, JArray Adjustment, NPCFrame frameData)
        {
            LimbRotations = new Rotation3D[21];
            RootTranslation = frameData.RootTranslation;
            for (int i = 0; i < LimbRotations.Length; i++) {
                int from = Convert.ToInt32(limbMapFromTo[i * 2]);
                if (from > -1) {
                    LimbRotations[i] = Rotation3D.AdjustRotation(
                        frameData.LimbRotations[from],
                        Convert.ToSingle(Adjustment[(i * 3) + 0]),
                        Convert.ToSingle(Adjustment[(i * 3) + 1]),
                        Convert.ToSingle(Adjustment[(i * 3) + 2])
                    );
                } else
                    LimbRotations[i] = new Rotation3D(0, 0, 0);
            }
            Face = $"# Eyes: {EyeIndex[0]}, Mouth: {MouthIndex[0]}";
        }
    }
    public class LinkAnimation : BaseAnimationHeader
    {
        public LinkFrame[] AnimationFrames { get; set; }

        public LinkAnimation() { }

        public LinkAnimation(AnimationJSONEntry animation, byte[] gameplay_keep, byte[] link_animetion)
        {
            Offset = new SegmentAddress(animation.Offset, 16);
            Name = animation.Name;
            byte[] animation_header = gameplay_keep.BlockCopy(Offset.Address, 8);
            FrameCount = animation_header.BEReadInt16(0);
            SegmentAddress DataOffset = new SegmentAddress(animation_header.BEReadUInt32(4));

            AnimationFrames = new LinkFrame[FrameCount];
            for (int i = 0; i < AnimationFrames.Length; i++) {
                AnimationFrames[i] = new LinkFrame(link_animetion.BlockCopy(DataOffset.Address + (i * LinkFrame.Size), LinkFrame.Size));
            }
        }

        public LinkAnimation(ConverterParameters convertParams, NPCAnimation animation)
        {
            Offset = animation.Offset;
            Name = $"link_{animation.Name}";
            FrameCount = animation.FrameCount;
            AnimationFrames = new LinkFrame[FrameCount];
            for (int i = 0; i < AnimationFrames.Length; i++) {
                AnimationFrames[i] = new LinkFrame(convertParams.limbMapFromTo, convertParams.adjustDegrees, animation.AnimationFrames[i]);
            }
        }

        public static void Export(int objexVersion, LinkAnimation anim, string filePath)
        {
            int v = objexVersion;
check_objex_v:
            if (v == 1)
            {
                using (StreamWriter f = new StreamWriter(File.Create(filePath))) {
                f.WriteLine("anim_total 1");
                f.WriteLine($"frames {anim.FrameCount} \"{anim.Name}\"");
                for (int i = 0; i < anim.AnimationFrames.Length; i++) {
                    LinkFrame frame = anim.AnimationFrames[i];
                    f.WriteLine($"l {frame.RootTranslation.X} {frame.RootTranslation.Y} {frame.RootTranslation.Z}");
                    for (int j = 0; j < frame.LimbRotations.Length; j++) {
                        f.WriteLine($"r {frame.LimbRotations[j].Radians.X.ToString("F3")} {frame.LimbRotations[j].Radians.Y.ToString("F3")} {frame.LimbRotations[j].Radians.Z.ToString("F3")}");
                    }
                    //f.WriteLine($"{frame.Face}");
                }
            }
            } else if (v == 2)
            {
                using (StreamWriter f = new StreamWriter(File.Create(filePath))) {
                f.WriteLine($"newskel \"skeleton_name\" \"{anim.Name}\" {anim.FrameCount}");
                for (int i = 0; i < anim.AnimationFrames.Length; i++) {
                    f.WriteLine($"# Frame {i + 1}");
                    LinkFrame frame = anim.AnimationFrames[i];
                    f.WriteLine($"loc {frame.RootTranslation.X} {frame.RootTranslation.Y} {frame.RootTranslation.Z}");
                    for (int j = 0; j < frame.LimbRotations.Length; j++) {
                        f.WriteLine($"rot {frame.LimbRotations[j].Degrees.X.ToString("F2")} {frame.LimbRotations[j].Degrees.Y.ToString("F2")} {frame.LimbRotations[j].Degrees.Z.ToString("F2")}");
                    }
                    f.WriteLine($"{frame.Face}");
                }
            }
            } else {
                MyConsole.WriteLine("WARN", "Invalid OBJEX version specified, defaulting to OBJEX Version 2.");
                v = 2;
                goto check_objex_v;
            }
        }

    }
}