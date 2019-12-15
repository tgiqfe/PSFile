﻿using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using PSFile.Serialize;

namespace PSFile.Cmdlet
{
    /// <summary>
    /// レジストリ情報をエクスポートしてDirectorySummaryインスタンスをシリアライズ
    /// </summary>
    [Cmdlet(VerbsData.Export, "Registry")]
    public class ExportRegistry : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0), Alias("Path")]
        public string RegistryPath { get; set; }
        [Parameter, Alias("File")]
        //public string OutputFile { get; set; }
        //[Parameter, ValidateSet(Item.REG, Item.DAT, Item.XML, Item.JSON, Item.YML)]
        public string DataType { get; set; } = Item.JSON;

        protected override void BeginProcessing()
        {
            DataType = Item.CheckCase(DataType);
        }

        protected override void ProcessRecord()
        {
            switch (DataType)
            {
                /*
                case Item.REG:
                    OutputReg();
                    break;
                case Item.DAT:
                    OutputDat();
                    break;
                */
                case Item.XML:
                    WriteObject(DataSerializer.Serialize<List<RegistrySummary>>(GetPRegList(), Serialize.DataType.Xml));
                    break;
                case Item.JSON:
                    WriteObject(DataSerializer.Serialize<List<RegistrySummary>>(GetPRegList(), Serialize.DataType.Json));
                    break;
                case Item.YML:
                    WriteObject(DataSerializer.Serialize<List<RegistrySummary>>(GetPRegList(), Serialize.DataType.Yml));
                    /*
                    if (OutputFile == null)
                    {
                        switch (DataType)
                        {
                            case Item.XML:
                                DataSerializer.Serialize<List<FileSummary>>(GetPRegList(), Console.Out, PSFile.Serialize.DataType.Xml);
                                break;
                            case Item.JSON:
                                DataSerializer.Serialize<List<FileSummary>>(GetPRegList(), Console.Out, PSFile.Serialize.DataType.Json);
                                break;
                            case Item.YML:
                                DataSerializer.Serialize<List<FileSummary>>(GetPRegList(), Console.Out, PSFile.Serialize.DataType.Yml);
                                break;
                        }
                    }
                    else
                    {
                        DataSerializer.Serialize<List<RegistrySummary>>(GetPRegList(), OutputFile);
                    }
                    */
                    break;
            }
        }

        /// <summary>
        /// RegistrySummaryのリストを取得
        /// </summary>
        /// <returns>RegistrySummaryのList</returns>
        private List<RegistrySummary> GetPRegList()
        {
            List<RegistrySummary> pregList = new List<RegistrySummary>();
            Action<RegistryKey> getPReg = null;
            getPReg = (targetKey) =>
            {
                pregList.Add(new RegistrySummary(targetKey, true));
                foreach (string keyName in targetKey.GetSubKeyNames())
                {
                    using (RegistryKey subTargetKey = targetKey.OpenSubKey(keyName, false))
                    {
                        getPReg(subTargetKey);
                    }
                }
            };
            using (RegistryKey startKey = RegistryControl.GetRegistryKey(RegistryPath, false, false))
            {
                getPReg(startKey);
            }
            return pregList;
        }

        /*
        /// <summary>
        /// reg exportコマンドによるエクスポート
        /// </summary>
        private void OutputReg()
        {

            if (OutputFile == null)
            {
                //  reg export一時出力先
                string tempDir = Path.Combine(
                    Environment.ExpandEnvironmentVariables("%TEMP%"),
                    Item.APPLICATION_NAME);
                OutputFile = Path.Combine(tempDir, "Reg_Export.reg");
                if (!Directory.Exists(tempDir)) { Directory.CreateDirectory(tempDir); ; }

                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = "reg.exe";
                    proc.StartInfo.Arguments = $"export \"{RegistryPath}\" \"{OutputFile}\" /y";
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.Start();
                    proc.WaitForExit();
                }
                using (StreamReader sr = new StreamReader(OutputFile, Encoding.UTF8))
                {
                    Console.WriteLine(sr.ReadToEnd());
                }
            }
            else
            {

                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = "reg.exe";
                    proc.StartInfo.Arguments = $"export \"{RegistryPath}\" \"{OutputFile}\" /y";
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.Start();
                    proc.WaitForExit();
                }
            }
        }
        */

        /*
        /// <summary>
        /// reg saveコマンドによるエクスポート
        /// </summary>
        private void OutputDat()
        {
            //  管理者実行確認
            Functions.CheckAdmin();

            if (OutputFile == null)
            {
                string tempDir = Path.Combine(
                    Environment.ExpandEnvironmentVariables("%TEMP%"),
                    Item.APPLICATION_NAME);
                OutputFile = Path.Combine(tempDir, "Reg_Export.dat");
                if (!Directory.Exists(tempDir)) { Directory.CreateDirectory(tempDir); ; }
            }
            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = "reg.exe";
                proc.StartInfo.Arguments = $"save \"{RegistryPath}\" \"{OutputFile}\" /y";
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.Start();
                proc.WaitForExit();
            }
        }
        */
    }
}
