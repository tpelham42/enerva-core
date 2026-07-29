using System;
using System.IO;
using UnityEngine;

namespace EnervaCore {
    public class ECSettings {

        public string ProjectName { get; set; } = "Enerva Game";

        public string CoreModID { get; set; } = "Core";

        public string PublicGameFolderPath { get; set; } = "";

        //Path to mod folder inside game the game folder. Primarily used for internal mods that ship with the game. This is the default mod path.
        public string InternalModPath { get; set; } = "";
        //Path to mod folder inside the Documents/Special folder. Primarily used for user mods that are downloaded or created by the user.
        public string PublicModPath { get; set; } = "";

        public ECSettings(string ProjectName) {

            if(string.IsNullOrEmpty(ProjectName)) {
                throw new ArgumentException("ProjectName cannot be null or empty.");
            }

            this.ProjectName = ProjectName;

            //Init Public Game Folder Path
            //Special folder is the Windows Documents folder. Documents/<ProjectName> will store runtime game data                
            PublicGameFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ProjectName);

            //Init Internal Mod Path
            if (Application.isEditor) {
                InternalModPath = Path.Combine(Path.GetFullPath("."), "mods");                
            }
            else {
                InternalModPath = Path.Combine(Application.dataPath, "mods");                
            }

            //Init Public Mod Path
            if (Application.isEditor == false || Application.isPlaying) {
                //Documents/<ProjectName>/mods will be where user mods get stored and loaded                
                PublicModPath = Path.Combine(PublicGameFolderPath, "mods");

                DirectoryInfo publicModsDI = new DirectoryInfo(PublicModPath);
                if (publicModsDI.Exists == false) {
                    publicModsDI.Create();
                }
            }
        }

    }
}
