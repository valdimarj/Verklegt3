﻿using System;
using System.Web;
using System.Collections.Generic;
using MooshakPP.Models.Entities;
using System.Configuration;
using System.IO;
using System.Diagnostics;


namespace MooshakPP.Services
{
    public class SubmissionTester
    {
        public void InitCompiler(ref Process compiler, string workingFolder)
        {
            compiler.StartInfo.FileName = "cmd.exe";
            compiler.StartInfo.WorkingDirectory = workingFolder;
            compiler.StartInfo.RedirectStandardInput = true;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.StartInfo.UseShellExecute = false;
        }

        // Compile C++ code
        public result CompileCPP(ref Process compiler, string cppFileName)
        {
            try
            {
                // Provide the directory containing vcvars32.bat and cl.exe
                string compilerFolder = ConfigurationManager.AppSettings["C++CompilerFolder"];
                compiler.Start();
                compiler.StandardInput.WriteLine("\"" + compilerFolder + "vcvars32.bat" + "\"");
                compiler.StandardInput.WriteLine("cl.exe /nologo /EHsc " + cppFileName);
                compiler.StandardInput.WriteLine("exit");
                compiler.WaitForExit();
                compiler.Close();
            }
            catch(Exception)
            {
                // Any uncaught exception in the compilation process will be caught here
                return result.compError;
            }
            return result.none;
        }

        // Compile C# code
        // Currently incompatible with System.Linq
        public result CompileCS(ref Process compiler, string csFileName)
        {
            try
            {
                // Provide the vsvars32.bat plugin path
                string vsvarsPath = ConfigurationManager.AppSettings["C#PluginPath"];
                string cscPath = ConfigurationManager.AppSettings["C#CompilerPathRelative"];
                // Make the path absolute
                cscPath = HttpContext.Current.Server.MapPath(cscPath);
                compiler.Start();
                compiler.StandardInput.WriteLine("\"" + vsvarsPath + "\"");
                compiler.StandardInput.WriteLine(cscPath + " /nologo /out:Program.exe " + csFileName);
                compiler.StandardInput.WriteLine("exit");
                compiler.WaitForExit();
                compiler.Close();
            }
            catch(Exception)
            {
                // Any uncaught exception in the compilation process will be caught here
                return result.compError;
            }
            
            return result.none;
        }

        public void InitTester(ref ProcessStartInfo processInfoExe)
        {
            processInfoExe.UseShellExecute = false;
            processInfoExe.RedirectStandardOutput = true;
            processInfoExe.RedirectStandardInput = true;
            processInfoExe.RedirectStandardError = true;
            processInfoExe.CreateNoWindow = false;
        }

        public result TestSubmission(ref ProcessStartInfo processInfoExe, ref List<TestCase> testCases, string workingDir)
        {
            try
            {
                //count all tests and passed tests
                int passCount = 0;
                int testCount = 0;
                foreach (TestCase test in testCases)
                {
                    testCount++;
                    string input;
                    // Load test case input file
                    using (StreamReader sr = new StreamReader(test.inputUrl))
                    {
                        input = sr.ReadToEnd();
                    }
                    string output = "";
                    // Create a new process with a limited lifespan
                    using (Process processExe = new Process())
                    {
                        processExe.StartInfo = processInfoExe;
                        processExe.Start();
                        processExe.StandardInput.WriteLine(input);

                        // Read the program output
                        while (!processExe.StandardOutput.EndOfStream)
                        {
                            output = processExe.StandardOutput.ReadToEnd();
                        }
                        // Close the process
                        processExe.Close();
                    }

                    //Read the expected output of current test case
                    using (StreamReader sr = new StreamReader(test.outputUrl))
                    {
                        string expected = "";
                        while (!sr.EndOfStream)
                        {
                            expected = sr.ReadToEnd();
                        }

                        // Compare expected and obtained output
                        if (expected == output)
                        {   // Test passed
                            passCount++;
                        }
                        else
                        {   // Save all wrong outputs
                            string outputDir = workingDir + "\\Wrong outputs\\";
                            Directory.CreateDirectory(outputDir);
                            File.WriteAllText(outputDir + "test " + testCount + " output.txt", output);
                        }
                    }
                }
                // All tests passsed
                if (passCount == testCases.Count)
                {
                    return result.Accepted;
                }
                else
                {
                    return result.wrongAnswer;
                }
            }
            catch(Exception)
            {
                return result.runError;
            }

        }
    }
}