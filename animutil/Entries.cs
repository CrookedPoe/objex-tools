using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace animutil
{
    public class FileContainer
    {
        public string Fullpath { get; set; }
        public string Directory { get; set; }
        public string Filename { get; set; }
        public string FileExtension { get; set; }
        public byte[] Bytes { get; set; }
        public string Text { get; set; }

        public FileContainer() { }

        public FileContainer(string filePath, string type)
        {
            Fullpath = Path.GetFullPath(filePath);
            Directory = Path.GetDirectoryName(Fullpath);
            Filename = Path.GetFileNameWithoutExtension(Fullpath);
            FileExtension = Path.GetFileName(Fullpath).Replace(Filename, String.Empty);
            switch(type) {
                case "Binary": {
                    Bytes = File.ReadAllBytes(Fullpath);
                } break;
                case "Text": {
                    Text = File.ReadAllText(Fullpath);
                } break;
            }
        }
    }
    public class SegmentTokens
    {
        public string[] Segments { get; set; }
        public SegmentTokens() { }
        public SegmentTokens(JToken json)
        {
            JArray segments = JArray.FromObject(json["Segments"]);
            Segments = new string[segments.Count];
            for (int i = 0; i < segments.Count; i++)
            {
                Segments[i] = segments[i].ToString();
            }
        }
    }

    public class FileEntry
    {
        public string Name { get; set; }
        public string vromStart { get; set; }
        public string vromEnd { get; set; }

        public int Start { get; set; }

        public int End { get; set; }

        public FileEntry() { }

        public FileEntry(JToken json)
        {
            Name = json["Name"].ToString();
            vromStart = json["vromStart"].ToString();
            vromEnd = json["vromEnd"].ToString();
            Start = Convert.ToInt32(vromStart, 16);
            End = Convert.ToInt32(vromEnd, 16);
        }
    }
    public class ExtractionParameters
    {
        public bool isEnabled { get; set; }
        public string[] romParams { get; set; }
        public bool keepFiles { get; set; }
        public List<FileEntry> filesToExtract { get; set; }
        public ExtractionParameters() { }

        public ExtractionParameters(JToken json)
        {
            isEnabled = Convert.ToBoolean(json["isEnabled"]);
            romParams = new string[2] {
                json["romParams"][0].ToString(),
                json["romParams"][1].ToString()
            };
            keepFiles = Convert.ToBoolean(json["keepFiles"]);
            filesToExtract = new List<FileEntry>();
            JArray files = JArray.FromObject(json["filesToExtract"]);
            for (int i = 0; i < files.Count; i++) {
                filesToExtract.Add(new FileEntry(files[i]));
            }
        }
    }

    public class ConverterParameters
    {
        public bool isEnabled { get; set; }
        public string convertToType { get; set; }
        public JArray limbMapFromTo { get; set; }
        public JArray adjustDegrees { get; set; }

        public ConverterParameters() { }

        public ConverterParameters(JToken json)
        {
            isEnabled = Convert.ToBoolean(json["isEnabled"]);
            convertToType = json["convertToType"].ToString();

            limbMapFromTo = JArray.FromObject(json["limbMapFromTo"]);
            /*limbMapFromTo = new int[2, limbMap.Count / 2];
            for (int i = 0; i < limbMap.Count; i += 2) {
                limbMapFromTo[0, i] = Convert.ToInt32(limbMap[i]);
                limbMapFromTo[1, i + 1] = Convert.ToInt32(limbMap[i + 1]);
            }*/

            adjustDegrees = JArray.FromObject(json["adjustDegrees"]);
            /*adjustDegrees = new float[3, adjust.Count / 3];
            for (int i = 0; i < adjust.Count; i += 3) {
                adjustDegrees[0, i] = Convert.ToSingle(adjust[i]);
                adjustDegrees[1, i + 1] = Convert.ToSingle(adjust[i + 1]);
                adjustDegrees[1, i + 2] = Convert.ToSingle(adjust[i + 2]);
            }*/
        }

    }

    public class AnimationJSONEntry
    {
        public string Name { get; set; }
        public string Offset { get; set; }

        public AnimationJSONEntry() { }

        public AnimationJSONEntry(JToken animation)
        {
            Name = animation["Name"].ToString();
            Offset = animation["Offset"].ToString();
        }
    }
    public class SkeletonJSONEntry
    {
        public bool isFlex { get; set; }
        public bool isLOD { get; set; }
        public string Name { get; set; }
        public string Offset { get; set; }

        public SkeletonJSONEntry() { }

        public SkeletonJSONEntry(JToken skeleton)
        {
            isFlex = Convert.ToBoolean(skeleton["isFlex"]);
            isLOD = Convert.ToBoolean(skeleton["isLOD"]);
            Name = skeleton["Name"].ToString();
            Offset = skeleton["Offset"].ToString();
        }
    }

    public class ObjectJSONEntry
    {
        public string Type { get; set; }
        public List<AnimationJSONEntry> Animations { get; set; }
        public List<SkeletonJSONEntry> Skeletons { get; set; }

        public ObjectJSONEntry() { }

        public ObjectJSONEntry(JToken json)
        {
            Type = json["Type"].ToString();

            Animations = new List<AnimationJSONEntry>();
            JArray animations = JArray.FromObject(json["Animations"]);
            for (int i = 0; i < animations.Count; i++) {
                Animations.Add(new AnimationJSONEntry(animations[i]));
            }

            Skeletons = new List<SkeletonJSONEntry>();
            JArray skeletons = JArray.FromObject(json["Skeletons"]);
            for (int i = 0; i < skeletons.Count; i++) {
                Skeletons.Add(new SkeletonJSONEntry(skeletons[i]));
            }

        }
    }
    public class InputJSON
    {
        public SegmentTokens segmentDef { get; set; }
        public bool hasSegmentDef { get; set; }
        public ExtractionParameters extractParams { get; set; }
        public bool hasExtractParams { get; set; }
        public ConverterParameters convertParams { get; set; }
        public bool hasConvertParams { get; set; }

        public InputJSON() { }

        public InputJSON(JObject json)
        {

            if (json.ContainsKey("segmentDef")) {
                segmentDef = new SegmentTokens(json["segmentDef"]);
                hasSegmentDef = true;
            } else {
                segmentDef = new SegmentTokens();
                hasSegmentDef = false;
            }

            if (json.ContainsKey("extractParams")) {
                extractParams = new ExtractionParameters(json["extractParams"]);
                hasExtractParams = true;
            } else {
                extractParams = new ExtractionParameters();
                hasExtractParams = false;
            }

            if (json.ContainsKey("convertParams")) {
                convertParams = new ConverterParameters(json["convertParams"]);
                hasConvertParams = true;
            } else {
                convertParams = new ConverterParameters();
                hasConvertParams = false;
            }
        }
    }
}