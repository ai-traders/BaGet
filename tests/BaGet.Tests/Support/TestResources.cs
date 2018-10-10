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

         public static string GetDirectory(params string[] paths) {
            foreach(var dir in paths) 
            {
                if(Directory.Exists(dir))
                    return dir;
            }
            throw new FileNotFoundException("Could not find directory in any of the paths:\n" + string.Join('\n',paths));
        }

        public static string GetE2eInputDirectory() {
            return GetDirectory(
                "e2e/input",
                "/ide/work/e2e/input"
            );            
        }

        public static string GetNupkgBagetTest1()
        {
            return GetFile(
                "baget-test1.1.0.0.nupkg",
                "e2e/input/baget-test1/bin/Debug/baget-test1.1.0.0.nupkg",
                "/ide/work/e2e/input/baget-test1/bin/Debug/baget-test1.1.0.0.nupkg"
            );
        }

        public static string GetNupkgBagetTwoV1()
        {
            string filename = "baget-two.1.0.0.nupkg";
            return GetFile(
                filename,                
                "e2e/input/baget-two/bin/Debug/" + filename,
                "/ide/work/e2e/input/baget-two/bin/Debug/" + filename
            );
        }

        public static string GetNupkgBagetTwoV2()
        {
            string filename = "baget-two.2.1.0.nupkg";
            return GetFile(
                filename,                
                "e2e/input/baget-two/bin/Debug/" + filename,
                "/ide/work/e2e/input/baget-two/bin/Debug/" + filename
            );
        }
    }
}