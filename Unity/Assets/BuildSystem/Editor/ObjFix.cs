using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace BuildSystem
{
    public class ObjFix
    {
        public static void Fix()
        {
            var objLocation = "Assets/ObjFix";
            var content = new List<string>();
            foreach (var objFile in Directory.GetFiles(objLocation, "*.obj", SearchOption.AllDirectories))
            {
                Debug.Log(objFile);
                content.Clear();
                using (var reader = new StreamReader(objFile, Encoding.UTF8))
                {
                    //#position => v
                    //#normal => vn
                    //#texcoord0 => vt
                    //#texcoord0 0.9112037 0.07348031 => #texcoord0 0.9112037 1-0.07348031
                    //#texcoord1 => vt1 => #texcoord1 0.9112037 1-0.07348031
                    while (reader.Peek() > 0)
                    {
                        var line = reader.ReadLine();
                        var normalized = "";

                        if (!string.IsNullOrEmpty(line))
                        {
                            var array = line.Split(' ');
                            var prefix = array[0];
                            var left = line.Remove(0, prefix.Length);
                            if (prefix == "#position")
                            {
                                normalized = "v" + " " + left;
                            }
                            else if (prefix == "#normal")
                            {
                                normalized = "vn" + " " + left;
                            }
                            else if (prefix == "#texcoord0")
                            {
                                if (array.Length == 3)
                                {
                                    var d = 1.0f - double.Parse(array[2]);
                                    normalized = "vt" + " " + array[1] + " " + d;
                                }
                                else
                                {
                                    normalized = line;
                                }
                            }
                            else if (prefix == "#texcoord1")
                            {
                                if (array.Length == 3)
                                {
                                    var d = 1.0f - double.Parse(array[2]);
                                    normalized = "vt1" + " " + array[2] + " " + d;
                                }
                                else
                                {
                                    normalized = line;
                                }
                            }
                            else
                            {
                                normalized = line;
                            }
                        }
                        content.Add(normalized);
                    }
                }

                // save
                using (var writer = new StreamWriter(objFile, false, Encoding.UTF8))
                {
                    foreach (var line in content)
                    {
                        writer.WriteLine(line);
                    }
                }
            }
        }
    }
}