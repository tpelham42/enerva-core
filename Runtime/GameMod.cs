using System.Collections.Generic;
using System.IO;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif



namespace EnervaCore {

    public enum GameModSource { Internal, Public, SteamWorkshop }

    /// <summary>
    /// A GameMod holds a collection of gamedata that can be activated or deactivated. When activated gamedata
    /// is passed over to the DBManager so that it is accessible by the rest of the system.
    /// </summary>
    public class GameMod {
        string _id;
        ulong _steamId;
        ulong _steamOwnerId;
        string _displayName;
        string _author;
        string _url;
        string _summary;
        string _modFolderPath;
        string _version;
        float _versionNum;
        int _buildNumber;
        GameModSource _source;
        Texture2D _previewImage;

        bool _enabled;        
        int _index;
        
        public List<ECObjectData> GameDataTemplates;

        public GameMod(string modFolderPath, ModInfo info) {
            _displayName = info.Name;
            _author = info.Author;
            _url = info.URL;
            _version = info.Version;
            _versionNum = info.VersionNum;
            _buildNumber = info.BuildNumber;
            _summary = info.Summary;
            _modFolderPath = modFolderPath;

            _id = new DirectoryInfo(_modFolderPath).Name;

            GameDataTemplates = new List<ECObjectData>();
        }
        #region Properties

        public string ID {
            get { return _id; }
        }

        public string DisplayName {
            get { return _displayName; }
        }

        public string Author {
            get { return _author; }
        }

        public string URL {
            get { return _url; }
        }

        public string Summary {
            get { return _summary; }
        }

        public string Version {
            get { return _version; }
        }

        public float VersionNum {
            get { return _versionNum; }
        }

        public int BuildNumber {
            get { return _buildNumber; }
        }

        public Texture2D PreviewImage {
            get { return _previewImage; }
            set { _previewImage = value; }
        }

        public bool Enabled {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public int Index {
            get { return _index; }
            set { _index = value; }
        }

        public ulong SteamId {
            get { return _steamId; }
            set { _steamId = value; }
        }

        public ulong SteamOwnerId {
            get { return _steamOwnerId; }
            set { _steamOwnerId = value; }
        }

        public string ModFolderPath {
            get { return _modFolderPath; }
        }

        public GameModSource Source {
            get { return _source; }
            set { _source = value; }
        }

        #endregion

        public void Activate() {            
            foreach (ECObjectData item in GameDataTemplates) {
                ECM.DB.AddTemplate(this, item);

                item.OnDataActivated();
            }            
        }

        public void Deactivate() {
            foreach (ECObjectData item in GameDataTemplates) {
                item.OnDataDeactivated();

                ECM.DB.RemoveTemplate(item.ID);
            }

            ClearData();
        }

        public void AddData(ECObjectData GIData) {
            if(GIData == null) return;

            GameDataTemplates.Add(GIData);
        }

        public void ClearData() {
            GameDataTemplates.Clear();
        }        
    }
}