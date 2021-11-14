# animutil JSON spec
---

The way the magic works with animutil is that it relies on a paired JSON file to define how an object file should be processed. One that's automatically generated only creates what is required for basic extraction of assets, but it's more than acceptable to modify it maually. This is a basic rundown of each valid section of the file.

---
## `segmentDef`
`segmentDef` is an object that contains a single array of strings with a length of 16. (See `/samples/Saria/oot_u10.json` for an example) Each index in the array `Segments` refers to an object or file that the json is intended to process. Though the program is only intended to process one object at a time, **it's just broken enough to process more than one.** (See `/samples/Link/oot_u10.json` for an example).

The intention is that you will be able to include other segments when an object relies on data from it, but this is an unsupported feature.

---

## `extractParams`
`extractParams` as denoted by the name is to define parameters used for ROM extraction. Even though you can process an object file individually using `-i`, you can instead of the program extract the assets and optionally remove them later keeping your workspace clean.

#### `isEnabled`
This is a boolean. If this is set to `false`, extraction will be ignored. 

#### `romParams`
This is another string array. Index `0` is a path to your ROM file. This can be either relative or absolute, but if the path is wrong or the file doesn't exist, extraction will fail. If the ROM does exist, the program will proceed to check it against the MD5 hash (calculated with md5sum included with Linux in index `1`. This is for further protection against extraction failing--it's a guarentee that the files defined are compatible with the ROM provided.

#### `keepFiles`
This is a boolean. If this is set to `false`, the files that are extracted from the ROM will be deleted once the program is finished using them. In addition, this could make the program double as a general-purpose file extractor for any type of binary file.

#### `filesToExtract`
This is an array of objects (internally `FileEntry`) that define the filename, the start offset and end offset of the file you want to extract from whatever you define in `romParams`. The filename does not default to an extension, so one needs to be included, otherwise it will be extracted as an extensionless file.

---
## `convertParams`
This was a thrown-together feature that I was very interested in supporting. The inspiration behind it was being able to convert NPC-formatted animations to Link's format.

#### `isEnabled`
This is a boolean. If this is set to `false`, conversion will be ignored.

#### `convertToType`
This defines the type of animation you're wanting to convert to. This is a string. Valid options are `"Link"`, `"NPC"*`, and `"Legacy"*`.
`*` Currently Unspported

#### `limbMapFromTo`
This is a simple array of numbers where for every pair, it maps a 0-index limb number from the object being processed to the intended "destination" format. Limb `-1` is the same as `"No Limb"`. In other words, it does not exist. (See `/samples/Saria/oot_u10.json` for an example). The total number of entries should be exactly twice that of the number of limbs for the destination object.

#### `adjustDegrees`
Limb rotations are not always 1:1, so some adjustments may have to be made. This integer array is processed as a set of XYZ rotations that are applied to the final converted animation. There should be one for each limb, even if no adjustment is to be made. (See `/samples/Saria/oot_u10.json` for an example)

---

## `exportParams`
This will define the parameters for exporting animations and skeletons.

#### `objexVersion`
This is an integer where you will define which version of the OBJEX spec you would like to use. Valid options are `1` (for the old version) or `2` for the most recent version.

#### `exportSkel`
This is a boolean to determine whether or not the skeleton will be exported.

#### `exportAnim`
This is a boolean to determine whether or not `.anim` files will be exported.

#### `exportBinary`
This is a boolean to determine whether or not the animations and skeleton will be exported as a binary file.

#### `exportCObject`
This is a boolean to determine whether or not the animations and skeleton will be exported as a C-compatible `.o` file.

---
Every entry following these parameters should be that of an object named the same as it is defined in `segmentDef`.

#### `Type`
The explains what kind of file is being processed. Valid options are `"Link"`, `"NPC"`, `"Legacy"*`, and `"Data"*` When a file is automatically generated, it is defaulted to `"NPC"`.
`*` It doesn't actually matter what is put here if it's not an NPC or Link. The program doesn't care yet.

#### `Animations`
This is an array of objects (internally `AnimationJSONEntry`) to define the animations that are to be extracted. Each entry should contain a `Name` field and an `Offset` field, though it doesn't matter which order these are in.

#### `Skeletons`
This is an array of objects (internally `SkeletonJSONEntry`) to define the skeletons that are to be extracted. Each entry should contain an `isFlex` boolean, an `isLOD` boolean, a `Name` field and an `Offset` field, though it doesn't matter which order these are in. All of these parameters are detected automatically in a generated file, but if being written manually it's easy to judge in a hex editor.

---
## Future Plans and To-Do
- Binary Exporting (for those hardcore ROM hackers)
- C Exporting (for those hardcore decomp hackers)
- Legacy Animation / Skeleton Formats (for those hardcore beta enthusiasts)
- Converting Link animations to Modern NPC format (for those with hardcore NPC energy)
- Converting Link and Modern NPC animations to the Legacy format (for those beta-enthusiasts with hardcore NPC energy)
- External segments and animation data banks (For those hardcore modular projects)
- Animation tweening (For this [hardcore dude](https://www.youtube.com/watch?v=wA5iEVHP2os))