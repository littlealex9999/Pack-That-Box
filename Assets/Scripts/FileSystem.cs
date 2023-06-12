using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileSystem
{
    #region Helper Functions
    string DeterminePath(string filename) => Application.dataPath + "/" + filename;

    BinaryWriter OpenWriter(string filename, out Stream stream)
    {
        string path = DeterminePath(filename);

        // either overwrites the old content, or creates a new file.
        FileMode mode = File.Exists(path) ? FileMode.Truncate : FileMode.CreateNew;

        // files get saved in the application directory
        stream = File.Open(path, mode);
        BinaryWriter bw = new BinaryWriter(stream);

#if UNITY_EDITOR
        // only output this for debug purposes
        Debug.Log("Writing a file at: " + path);
#endif

        return bw;
    }

    BinaryReader OpenReader(string filename, out Stream stream)
    {
        string path = DeterminePath(filename);

        if (File.Exists(path)) {
            stream = File.Open(path, FileMode.Open);
            BinaryReader br = new BinaryReader(stream);

#if UNITY_EDITOR
            // only output this for debug purposes
            Debug.Log("Reading a file at: " + path);
#endif

            return br;
        } else { // failed to load the file as it doesn't exist
#if UNITY_EDITOR
            // only output this for debug purposes
            Debug.Log("Failed to find a file at: " + path);
#endif
        }

        stream = null;
        return null;
    }

    void CloseSystem(BinaryReader binaryIO, Stream stream)
    {
        binaryIO.Close();
        stream.Close();
    }
    void CloseSystem(BinaryWriter binaryIO, Stream stream)
    {
        binaryIO.Close();
        stream.Close();
    }
    #endregion

    public void SaveFile(string filename)
    {
        BinaryWriter bw = OpenWriter(filename, out Stream s);

        bw.Write(10);

        CloseSystem(bw, s);
    }

    public bool LoadFile(string filename, out int output)
    {
        BinaryReader br = OpenReader(filename, out Stream s);
        if (br != null) {
            output = br.ReadInt32();

            CloseSystem(br, s);
            return true;
        } else {
            output = 0;
            return false;
        }
    }
}
