using System;
using System.IO;

namespace BaGet.Tests.Support
{
    public class TestResources
    {
        public static string GetFile(params string[] paths) {
            foreach(var file in paths) 
            {
                if(File.Exists(file))
                    return file;
            }
            throw new FileNotFoundException("Could not find file in any of the paths:\n" + string.Join('\n',paths));
        }

        public static string GetNupkgBagetTest1()
        {
            return GetFile(
                "baget-test1.1.0.0.nupkg",
                "e2e/input/baget-test1/bin/Debug/baget-test1.1.0.0.nupkg",
                "/ide/work/e2e/input/baget-test1/bin/Debug/baget-test1.1.0.0.nupkg"
            );
        }
    }
}