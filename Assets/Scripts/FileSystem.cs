using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class FileSystem
{
    public void SaveFile(string filename)
    {
        string path = Application.dataPath + "/" + filename;

        // either overwrites the old content, or creates a new file.
        FileMode mode = File.Exists(path) ? FileMode.Truncate : FileMode.CreateNew;

        // files get saved in the application directory
        Stream s = File.Open(path, mode);
        BinaryWriter bw = new BinaryWriter(s);

#if UNITY_EDITOR
        // only output this for debug purposes
        Debug.Log("Writing a file at: " + path);
#endif

        bw.Write(10);
        
        bw.Close();
        s.Close();
    }

    public bool LoadFile(string filename, out int output)
    {
        string path = Application.dataPath + "/" + filename;

        if (File.Exists(path)) {
            Stream s = File.Open(path, FileMode.Open);
            BinaryReader br = new BinaryReader(s);

#if UNITY_EDITOR
            // only output this for debug purposes
            Debug.Log("Reading a file at: " + path);
#endif

            output = br.ReadInt32();

            return true;
        } else { // failed to load the file as it doesn't exist
#if UNITY_EDITOR
            // only output this for debug purposes
            Debug.Log("Failed to find a file at: " + path);
#endif

            output = 0;
            return false; 
        }
    }
}
