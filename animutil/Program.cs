using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace animutil
{
    class Program
    {
        static void PromptUsage()
        {
            Console.WriteLine("OBJEX Animation Utility <CrookedPoe 2021>");
            MyConsole.WriteLine("NOTE", "Standard Usage is as follows:");
            MyConsole.WriteLine("SPEC", "./animutil -i/--input object.zobj");
            MyConsole.WriteLine("NOTE", "With just an input file provided, this will do its");
            Console.WriteLine("    best to autodetect animations and skeletons in a");
            Console.WriteLine("    zelda64 object file. The default behavior of this");
            Console.WriteLine("    program is to process an object and write a .skel");
            Console.WriteLine("    and .anim for all of the detected skeletons and");
            Console.WriteLine("    animations.");
            MyConsole.WriteLine("NOTE", "There are additional, yet optional arguments.");
            MyConsole.WriteLine("SPEC", "-h / --help: Show this prompt.");
            MyConsole.WriteLine("SPEC", "-a / --autodetect: This will force autodetection, but");
            Console.WriteLine("    will prompt you if it wants to overwrite a file.");
            MyConsole.WriteLine("SPEC", "-j / --json: This will allow you to specify a json");
            Console.WriteLine("    file to use for object processing. If one is not");
            Console.WriteLine("    specified, the program will attempt to use one named");
            Console.WriteLine("    the same as your object file located in the object's");
            Console.WriteLine("    directory.");
            MyConsole.WriteLine("NOTE", "For more usage information and the JSON specification, please");
            Console.WriteLine("    investigate the included README.md either included or at");
            Console.WriteLine("    https://github.com/CrookedPoe/objex-tools/animutil");
            Environment.Exit(0);
        }

        static bool CheckArg(string c, string shorthand, string longhand) {
            return (c == $"-{shorthand}" || c == $"--{longhand}");
        }

        static string md5sum(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        static void Main(string[] args)
        {
            bool hasInput = false;
            FileContainer inputFile = new FileContainer();
            bool hasJSON = false;
            FileContainer jsonFile = new FileContainer();
            string autoJSONName = String.Empty;
            bool autoDetect = false;

            Console.Title = "OBJEX Animation Utility";

            if (args.Length < 1) {
                PromptUsage();
            } else {
                for (int i = 0; i < args.Length; i++) {
                    if (CheckArg(args[i], "h", "help")) {
                        PromptUsage();
                    }
                    if (CheckArg(args[i], "i", "input")) {
                        hasInput = true; i++;
                        inputFile = new FileContainer(args[i], "Binary");
                        autoJSONName = $"{inputFile.Directory}/{inputFile.Filename}.json";
                    }
                    if (CheckArg(args[i], "j", "json")) {
                        hasJSON = true; i++;
                        jsonFile = new FileContainer(args[i], "Text");
                    }
                    if (CheckArg(args[i], "a", "autodetect")) {
                        autoDetect = true;
                    }
                }
            }

            if (!hasInput && !hasJSON) {
                //Console.WriteLine("[!] Neither an input file nor JSON file were provided.");
                MyConsole.WriteLine("NOTE", "Neither an input file nor JSON file were provided.");
                PromptUsage();
            }

recheck_no_json:
            if (hasInput && !hasJSON) {
                if (!autoDetect) {
                    if (File.Exists(autoJSONName)) {
                        MyConsole.WriteLine("PROMPT", "A JSON file was detected for this input file. Would");
                        Console.WriteLine("    you like to use it? (Y/N)");
                        if (Console.ReadKey().Key == ConsoleKey.Y) {
                            Console.Write("\n");
                            jsonFile = new FileContainer(autoJSONName, "Text");
                            hasJSON = true;
                            ProcessJSON(inputFile, jsonFile);
                        } else {
                            Console.Write("\n");
                            autoJSONName = String.Empty;
                            goto recheck_no_json;
                        }
                    } else {
                        MyConsole.WriteLine("PROMPT", "An input file was provided, but a JSON file was not.");
                        Console.WriteLine("    one can be created through auto-detection. Would you");
                        Console.WriteLine("    like to try this feature? (Y/N)");
                        if (Console.ReadKey().Key == ConsoleKey.Y) {
                            Console.Write("\n");
                            autoDetect = true;
                            goto recheck_no_json;
                        } else {
                            Console.Write("\n");
                            MyConsole.WriteLine("NOTE", "Please provide a JSON to continue. Exiting...");
                            PromptUsage();
                        }
                    }
                } else {
                    if (File.Exists(autoJSONName)) {
                        MyConsole.WriteLine("PROMPT", "Auto-detection is currently enabled, but a json file");
                        Console.WriteLine("    was already detected in this directory. Continuing will");
                        Console.WriteLine("    overwrite the existing file. Would you like to continue? (Y/N)");
                        if (Console.ReadKey().Key == ConsoleKey.Y) {
                            Console.Write("\n");
                            AutoDetect(inputFile);
                        } else {
                            Console.Write("\n");
                            MyConsole.WriteLine("PROMPT", "Would you like to use the existing JSON file? (Y/N)");
                            if (Console.ReadKey().Key == ConsoleKey.Y) {
                                Console.Write("\n");
                                jsonFile = new FileContainer(autoJSONName, "Text");
                                hasJSON = true;
                                ProcessJSON(inputFile, jsonFile);
                            } else {
                                Console.Write("\n");
                                MyConsole.WriteLine("NOTE", "Please provide a JSON to continue. Exiting...");
                                PromptUsage();
                            }
                        }
                    } else {
                        AutoDetect(inputFile);
                    }
                }
            }

            if ((!hasInput && hasJSON) || hasJSON) {
                if (autoDetect) {
                    MyConsole.WriteLine("NOTE", "Auto-detection is enabled, but there was");
                    Console.WriteLine("    no input file provided. The program will continue,");
                    Console.WriteLine("    but nothing will be auto-detected.");
                }
                ProcessJSON(inputFile, jsonFile);
            }

            MyConsole.WriteLine("NOTE", "Done!");
        }

        static void AutoDetect(FileContainer inputFile)
        {
            string autoJSONName = $"{inputFile.Directory}/{inputFile.Filename}.json";
            SegmentTokens segments = new SegmentTokens {
                Segments = new string[16]
            };
            ObjectJSONEntry outputObject = new ObjectJSONEntry {
                Type = "NPC",
                Animations = new List<AnimationJSONEntry>(),
                Skeletons = new List<SkeletonJSONEntry>()
            };

            MyConsole.WriteLine("NOTE", "Looking for skeletons...");
            outputObject.Skeletons = Skeleton.Find(inputFile.Bytes, 0x06);
            MyConsole.WriteLine("NOTE", "Looking for animations...");
            outputObject.Animations = NPCAnimation.Find(inputFile.Bytes, 0x06);

            segments.Segments[6] = inputFile.Filename;
            for (int i = 0; i < 16; i++) {
                if (segments.Segments[i] == null) {
                    segments.Segments[i] = String.Empty;
                }
            }

            var newJSON = new {
                segmentDef = segments,
                renameMe = outputObject
            };

            using (StreamWriter f = new StreamWriter(File.Create(autoJSONName))) {
                string json = JsonConvert.SerializeObject(newJSON, Formatting.Indented);
                json = json.Replace("renameMe", inputFile.Filename);
                f.WriteLine(json);
            }

            MyConsole.WriteLine("ADD", $"Written {Path.GetFileName(autoJSONName)}");
        }

        static void ProcessJSON(FileContainer inputFile, FileContainer jsonFile)
        {
            List<FileContainer> extractedFiles = new List<FileContainer>();
            List<string> extractedList = new List<string>();
            List<ObjectJSONEntry> objectList = new List<ObjectJSONEntry>();
            List<string> objectNameList = new List<string>();
            List<Skeleton> skeletons = new List<Skeleton>();
            List<NPCAnimation> animations = new List<NPCAnimation>();
            List<LinkAnimation> link_animations = new List<LinkAnimation>();

            JObject json = JObject.Parse(jsonFile.Text);
            InputJSON JSON = new InputJSON(json);

            if (JSON.hasExtractParams) {
                if (JSON.extractParams.isEnabled) {
                    /* ROM extraction is enabled. */
                    if (File.Exists(JSON.extractParams.romParams[0])) {
                        /* ROM file Exists */
                        if (md5sum(JSON.extractParams.romParams[0]) == JSON.extractParams.romParams[1]) {
                            extractedFiles.Add(new FileContainer(JSON.extractParams.romParams[0], "Binary"));
                            if (JSON.extractParams.filesToExtract.Count > 0) {
                                for (int i = 0; i < JSON.extractParams.filesToExtract.Count; i++) {
                                    FileEntry file = JSON.extractParams.filesToExtract[i];
                                    using (BinaryWriter f = new BinaryWriter(File.Create($"{jsonFile.Directory}/{file.Name}"))) {
                                        f.Write(extractedFiles[0].Bytes.BlockCopy(file.Start, file.Start, file.End));
                                    }
                                    MyConsole.WriteLine("ADD", $"{file.Name}");
                                    extractedFiles.Add(new FileContainer($"{jsonFile.Directory}/{file.Name}", "Binary"));
                                    extractedList.Add(extractedFiles[extractedFiles.Count - 1].Fullpath);
                                }
                            } else {
                                MyConsole.WriteLine("WARN", "There were no files defined to extract. Aborting extraction...");
                            }
                        } else {
                            MyConsole.WriteLine("WARN", "The provided ROM did not match the defined hash. Aborting extraction...");
                        }
                    } else {
                        MyConsole.WriteLine("WARN", "The provided ROM did not match the defined hash. Aborting extraction...");
                    }
                }
            }

            if (extractedFiles.Count < 2) {
                if (inputFile.Bytes == null) {
                    MyConsole.WriteLine("ERR", "There were no binary files available to process. Exiting...");
                    Environment.Exit(1);
                }
            }

            if (JSON.hasSegmentDef) {
                for (int i = 0; i < 16; i++) {
                    if (JSON.segmentDef.Segments[i] != String.Empty) {
                        string token = JSON.segmentDef.Segments[i];
                        objectList.Add(new ObjectJSONEntry(json[$"{token}"]));
                        objectNameList.Add(token);
                    }
                }
            } else {
                MyConsole.WriteLine("ERR", "There were no segments defined. Please make sure your JSON file is valid. Exiting...");
                Environment.Exit(1);
            }

            if (objectList.Count > 0)
            {
                /* Skeletons and Animations */
                for (int i = 0; i < objectList.Count; i++) {

                    for (int j = 0; j < extractedFiles.Count; j++) {
                        if (objectNameList[i] == extractedFiles[j].Filename) {
                            inputFile = extractedFiles[j];
                            break;
                        }
                    }

                    for (int j = 0; j < objectList[i].Skeletons.Count; j++) {
                        skeletons.Add(new Skeleton(objectList[i].Skeletons[j], inputFile.Bytes));
                        MyConsole.WriteLine("NOTE", $"New skeleton from {objectNameList[i]}");
                    }

                    if (objectList[i].Type == "Link") {
                        FileContainer link_animetion = new FileContainer();
                        FileContainer gameplay_keep = new FileContainer();
                        for (int j = 0; j < extractedFiles.Count; j++) {
                            if (extractedFiles[j].Filename == "link_animetion") {
                                link_animetion = extractedFiles[j];
                            }
                            if (extractedFiles[j].Filename == "gameplay_keep") {
                                gameplay_keep = extractedFiles[j];
                            }
                        }
                        for (int j = 0; j < objectList[objectNameList.IndexOf("gameplay_keep")].Animations.Count; j++) {
                            AnimationJSONEntry animation = objectList[objectNameList.IndexOf("gameplay_keep")].Animations[j];
                            link_animations.Add(new LinkAnimation(animation, gameplay_keep.Bytes, link_animetion.Bytes));
                        }
                    }

                    if (objectList[i].Type == "NPC") {
                        for (int j = 0; j < objectList[i].Animations.Count; j++) {
                            if (objectList[i].Animations[j].Name.Contains("extern")) {
                                MyConsole.WriteLine("WARN", "NPCs with external animation segments are currently unsupported.");
                            } else {
                                animations.Add(new NPCAnimation(objectList[i].Animations[j], skeletons[skeletons.Count - 1], inputFile.Bytes));
                                MyConsole.WriteLine("NOTE", $"New animation from {objectNameList[i]}");
                            }
                        }

                        if (JSON.hasConvertParams) {
                            if (JSON.convertParams.isEnabled) {
                                if (JSON.convertParams.convertToType == "Link") {
                                    for (int j = 0; j < animations.Count; j++) {
                                        link_animations.Add(new LinkAnimation(JSON.convertParams, animations[j]));
                                        MyConsole.WriteLine("NOTE", $"New animation from {objectNameList[i]} converted to Link's format");
                                    }
                                } else {
                                    MyConsole.WriteLine("WARN", "Conversion to {JSON.convertParams.convertToType} animations is currently unimplemented. Aborting conversion...");
                                }
                            }
                        }
                    }
                }
            }

            /* Export */
            if (!JSON.hasExportParams) {
                MyConsole.WriteLine("WARN", "No export parameters were specified, defaulting...");
                JSON.exportParams.objexVersion = 2;
                JSON.exportParams.exportSkel = true;
                JSON.exportParams.exportAnim = true;
                JSON.exportParams.exportBinary = false;
                JSON.exportParams.exportCObject = false;
            }
            string subFolder = $"{jsonFile.Directory}/{jsonFile.Filename}";
            Directory.CreateDirectory(subFolder);

            if (JSON.exportParams.exportSkel)
            {
                if (skeletons.Count > 0)
                {
                    /* Skeletons */
                    Directory.CreateDirectory($"{subFolder}/skeletons");
                    for (int i = 0; i < skeletons.Count; i++) {
                        Skeleton.Export(JSON.exportParams.objexVersion, skeletons[i], $"{subFolder}/skeletons/{skeletons[i].Header.Name}.skel");
                        MyConsole.WriteLine("ADD", $"{skeletons[i].Header.Name}.skel");
                    }
                }
            }

            if (JSON.exportParams.exportAnim)
            {
                if (animations.Count > 0)
                {
                    /* NPC Animations */
                    Directory.CreateDirectory($"{subFolder}/animations");
                    for (int i = 0; i < animations.Count; i++) {
                        NPCAnimation.Export(JSON.exportParams.objexVersion, animations[i], $"{subFolder}/animations/{animations[i].Name}.anim");
                        MyConsole.WriteLine("ADD", $"{animations[i].Name}.anim");
                    }
                }

                if (link_animations.Count > 0)
                {
                    /* Link Animations */
                    Directory.CreateDirectory($"{subFolder}/link_animations");
                    for (int i = 0; i < link_animations.Count; i++) {
                        LinkAnimation.Export(JSON.exportParams.objexVersion, link_animations[i], $"{subFolder}/link_animations/{link_animations[i].Name}.anim");
                        MyConsole.WriteLine("ADD", $"{link_animations[i].Name}.anim");
                    }
                }
            }

            if (!JSON.extractParams.keepFiles) {
                for (int i = 0; i < extractedList.Count; i++) {
                    File.Delete(extractedList[i]);
                    MyConsole.WriteLine("DEL", $"Removed {Path.GetFileName(extractedList[i])}");
                }
            }
        }
    }
}
