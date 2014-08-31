using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Diagnostics;

namespace Walrus__The_CD_DVD_Merger_
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void dataGridView1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            dataGridView1.Rows.Clear();
            listBox1.Items.Clear();
            foreach(string s in (string[])e.Data.GetData(DataFormats.FileDrop))
            {
                dataGridView1.Rows.Add(s);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] riff = { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x41, 0x56, 0x45, 0x66, 0x6d, 0x74, 0x20, 0x10, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0x00, 0x44, 0xac, 0x00, 0x00, 0x10, 0xb1, 0x02, 0x00, 0x04, 0x00, 0x10, 0x00, 0x64, 0x61, 0x74, 0x61, 0x00, 0x00, 0x00, 0x00 };
            byte[] synchro = { 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x00 };
            MD5 md5_hash = MD5.Create();

            string hash = string.Empty;
            byte[] buffer2048 = new byte[2048];
            byte[] buffer2352 = new byte[2352];
            int temp = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                string entry = row.Cells[0].Value.ToString();
                FileInfo entry_info = new FileInfo(entry);

                XmlDocument xml_control = new XmlDocument();
                xml_control.LoadXml("<root>" + "</root>");

                XmlElement xml_entries = xml_control.CreateElement("entries");
                XmlElement xml_partitions = xml_control.CreateElement("partitions");

                xml_control.DocumentElement.AppendChild(xml_entries);
                xml_control.DocumentElement.AppendChild(xml_partitions);

                List<string> files = new List<string>();
                //string[] files = new string[0];
                switch (entry_info.Attributes)
                {
                    case FileAttributes.Directory:
                        files.AddRange(Directory.GetFiles(entry_info.FullName));
                        files.AddRange(Directory.GetDirectories(entry_info.FullName));
                        break;
                    default:
                        files.Add(entry_info.FullName);
                        break;
                }

                
                //string[] entry_files = new string[0];//Directory.GetFiles(row.Cells[0].Value.ToString(), "*.*", SearchOption.AllDirectories);
                //FileInfo dir_info = new FileInfo(row.Cells[0].Value.ToString());
                string save_path = string.Empty;
                //string save_path = entry_info.FullName.Replace(entry_info.Extension, "");
                switch (entry_info.Attributes)
                {
                    default:
                        save_path = entry_info.FullName.Replace(entry_info.Extension, "");
                        break;
                    case FileAttributes.Directory:
                        save_path = entry_info.FullName;
                        break;
                }

                //FileStream file_form2 = new FileStream(save_path + ".form2", FileMode.Create, FileAccess.ReadWrite);

                BinaryWriter master_form1 = new BinaryWriter(File.OpenWrite(save_path + " [MERGED].cdm"));
                BinaryWriter master_form2 = new BinaryWriter(File.Open(save_path + ".form2", FileMode.Create, FileAccess.ReadWrite));
                BinaryWriter master_subheader = new BinaryWriter(File.Open(save_path + ".subheader", FileMode.Create, FileAccess.ReadWrite));
                //BinaryWriter master_form2 = new BinaryWriter(file_form2);
                BinaryWriter master_cdda = new BinaryWriter(File.Open(save_path + ".wav", FileMode.Create, FileAccess.ReadWrite));
                master_cdda.Write(riff);
                //BinaryWriter master_subheader = new BinaryWriter(File.Open(save_path + ".subheader", FileMode.Create, FileAccess.ReadWrite));

                BinaryWriter master_map = new BinaryWriter(File.Open(save_path + ".map", FileMode.Create, FileAccess.ReadWrite));
                int sector_counter_form1 = 0;
                int sector_counter_form2 = 0;
                int sector_counter_subheader = 0;
                int sector_counter_cdda = 0;
                //int sector_counter_subheader = 0;

                Dictionary<string, int> duplicate_form1 = new Dictionary<string, int>();
                Dictionary<string, int> duplicate_form2 = new Dictionary<string, int>();
                Dictionary<string, int> duplicate_subheader = new Dictionary<string, int>();
                Dictionary<string, int> duplicate_cdda = new Dictionary<string, int>();
                //Dictionary<string, int> duplicate_subheader = new Dictionary<string, int>();



                foreach (string file in files)
                {
                    List<string> entry_files = new List<string>();
                    FileInfo file_info = new FileInfo(file);
                    XmlElement xml_entry = xml_control.CreateElement("entry");
                    XmlElement xml_files = xml_control.CreateElement("files");
                    XmlElement xml_directories = xml_control.CreateElement("directories");
                    //xml_entry.AppendChild(xml_files);

                    bool is_dir = false;
                    switch (file_info.Attributes)
                    {
                        default:
                            xml_entry.SetAttribute("type", "file");
                            xml_entry.SetAttribute("name", file_info.Name);
                            xml_entry.SetAttribute("date", file_info.LastWriteTime.Ticks.ToString());
                            is_dir = false;
                            entry_files.Add(file);
                            break;
                        case FileAttributes.Directory:
                            xml_entry.SetAttribute("type", "dir");
                            xml_entry.SetAttribute("name", file_info.Name);
                            xml_entry.SetAttribute("date", file_info.LastWriteTime.Ticks.ToString());
                            is_dir = true;
                            break;
                    }

                    xml_entries.AppendChild(xml_entry);

                    


                    /*
                    switch (file_info.Attributes)
                    {
                        default:
                            xml_entry.SetAttribute("name", file_info.Name.Replace(file_info.Extension, ""));
                            xml_entry.SetAttribute("directory", "false");
                            xml_entry.SetAttribute("date", file_info.LastWriteTime.Ticks.ToString());
                            xml_entries.AppendChild(xml_entry);

                            entry_files.Add(file);

                            
                            break;
                        case FileAttributes.Directory:
                            xml_entry.SetAttribute("name", file_info.Name);
                            xml_entry.SetAttribute("directory", "true");
                            xml_entry.SetAttribute("date", file_info.LastWriteTime.Ticks.ToString());
                            xml_entries.AppendChild(xml_entry);

                            //List<string> dirs = new List<string>();
                            string[] dirs = Directory.GetDirectories(file_info.FullName, "*.*", SearchOption.AllDirectories);
                            if (dirs.Length != 0)
                            {
                                foreach (string dir in dirs)
                                {
                                    XmlElement directory = xml_control.CreateElement("directory");
                                    //directory.SetAttribute("path", 
                                }
                            }


                            entry_files.AddRange(Directory.GetFiles(file, "*.*", SearchOption.AllDirectories));
                            break;
                    }
                    */



                    foreach (string single_file in entry_files)
                    {
                        int normal_file = 0;
                        FileInfo sfile_info = new FileInfo(single_file);
                        int msf_counter = 150;
                        //int write_offset = 0;

                        switch (sfile_info.Extension)
                        {
                            default:
                                normal_file = 0;
                                listBox1.Items.Add(sfile_info.Name.ToString() + " normal file");
                                break;
                            case ".iso":
                                normal_file = 0;
                                listBox1.Items.Add(sfile_info.Name.ToString() + " ISO file");
                                break;
                            case ".bin":
                            case ".raw":
                            case ".img":
                                if (sfile_info.Length % 2352 == 0) normal_file = 1;
                            /*    
                            BinaryReader temp_reader = new BinaryReader(File.OpenRead(single_file));
                                byte[] temp_synchro = temp_reader.ReadBytes(12);
                                int raw_counter = 0;
                                for (int i = 0; i < 12; i++)
                                {
                                    if (temp_synchro[i] == synchro[i]) raw_counter++;
                                }
                                switch (raw_counter)
                                {
                                    default:
                                        //normal_file = 2;
                                        break;
                                    case 12:
                                        normal_file = 1;
                                        break;
                                }
                                //if (raw_counter == 12) normal_file = 1;
                             */
                                listBox1.Items.Add(sfile_info.Name.ToString());
                                break;
                        }

                        FileStream file_md5 = new FileStream(single_file, FileMode.Open);
                        hash = BitConverter.ToString(md5_hash.ComputeHash(file_md5)).Replace("-", "").ToLower();
                        file_md5.Close();

                        XmlElement xml_file = xml_control.CreateElement("file");
                        //xml_file.SetAttribute("name", sfile_info.Name);

                        switch (normal_file)
                        {
                            case 0:
                                xml_file.SetAttribute("type", "file");
                                break;
                            case 1:
                                xml_file.SetAttribute("type", "raw");
                                break;
                        }

                        switch (is_dir)
                        {
                            case false:
                                xml_entry.SetAttribute("md5", hash);
                                xml_entry.SetAttribute("size", sfile_info.Length.ToString());
                                //xml_entries.AppendChild(xml_entry);
                                break;
                            case true:
                                xml_file.SetAttribute("name", sfile_info.FullName.Replace(sfile_info.DirectoryName + "\\", ""));
                                xml_file.SetAttribute("date", sfile_info.LastWriteTime.Ticks.ToString());
                                xml_file.SetAttribute("md5", hash);
                                xml_file.SetAttribute("size", sfile_info.Length.ToString());

                                xml_files.AppendChild(xml_file);
                                break;
                        }


                        

                        //BinaryReader raw_reader = new BinaryReader(File.OpenRead(single_file));
                        BinaryReader raw_reader = new BinaryReader(File.Open(single_file, FileMode.Open, FileAccess.Read, FileShare.Read));

                        MemoryStream map = new MemoryStream();
                        BinaryWriter map_write = new BinaryWriter(map);

                        /*
                        //offset start
                        BinaryReader offset_checker = new BinaryReader(File.OpenRead(single_file));
                        byte[] header_check = offset_checker.ReadBytes(12);
                        offset_checker.BaseStream.Position = 0;

                        int audio_check = 0;
                        int audio_offset = 0;

                        for (int i = 0; i < 12; i++)
                        {
                            if (header_check[i] == synchro[i]) audio_check++;
                        }

                        if (audio_check != 12 && offset_checker.BaseStream.Length % 2352 == 0)
                        {
                            byte[] temp_audio = new byte[2352];

                            while (offset_checker.BaseStream.Position != offset_checker.BaseStream.Length)
                            {
                                temp_audio = offset_checker.ReadBytes(2352);
                                for (int a = 0; a < 2352; a++)
                                {
                                    if (temp_audio[a] == 0)
                                    {
                                        audio_offset++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                if (audio_offset % 2352 != 0) break;

                            }

                            /*
                            while (offset_checker.ReadByte() == 0)
                            {
                                audio_offset++;
                                if (offset_checker.BaseStream.Position == offset_checker.BaseStream.Length) break;
                            }
                             
                        }

                        offset_checker.Close();
                        //offset end
                    */
                        //raw_reader.BaseStream.Position = audio_offset; for offset
                        switch (normal_file)
                        {
                            case 0:
                                long complete_blocks = sfile_info.Length / 2048;
                                long incomplete_block = sfile_info.Length % 2048;

                                for (int i = 0; i < complete_blocks; i++)
                                {
                                    buffer2048 = raw_reader.ReadBytes(2048);
                                    hash = BitConverter.ToString(md5_hash.ComputeHash(buffer2048));
                                    switch (duplicate_form1.TryGetValue(hash, out temp))
                                    {
                                        case false:
                                            temp = sector_counter_form1;
                                            sector_counter_form1++;
                                            master_form1.Write(buffer2048);
                                            duplicate_form1.Add(hash, temp);
                                            break;
                                    }
                                    map_write.Write(temp);
                                }

                                if (incomplete_block != 0)
                                {
                                    byte[] block = raw_reader.ReadBytes((int)incomplete_block);

                                    hash = BitConverter.ToString(md5_hash.ComputeHash(block));
                                    switch (duplicate_form1.TryGetValue(hash, out temp))
                                    {
                                        case false:
                                            temp = sector_counter_form1;
                                            sector_counter_form1++;
                                            master_form1.Write(block);
                                            duplicate_form1.Add(hash, temp);
                                            break;
                                    }
                                    map_write.Write(temp);

                                    while (master_form1.BaseStream.Position % 2048 != 0)
                                    {
                                        master_form1.Write((byte)0);
                                    }
                                }

                                /*
                                while (raw_reader.BaseStream.Position != raw_reader.BaseStream.Length)
                                {
                                    buffer2048 = raw_reader.ReadBytes(2048);
                                    hash = BitConverter.ToString(md5_hash.ComputeHash(buffer2048));
                                    switch (duplicate_form1.TryGetValue(hash, out temp))
                                    {
                                        case false:
                                            temp = sector_counter_form1;
                                            sector_counter_form1++;
                                            master_form1.Write(buffer2048);
                                            duplicate_form1.Add(hash, temp);
                                            break;
                                    }
                                    map_write.Write(temp);
                                }
                                */
                                xml_file.SetAttribute("map_offset", master_map.BaseStream.Position.ToString());
                                xml_file.SetAttribute("map_size", map.Length.ToString());
                                map.Position = 0;
                                map.CopyTo(master_map.BaseStream);
                                break;
                            case 1:
                                //long complete_raw_blocks = (sfile_info.Length - audio_offset) / 2352;
                                //long incomplete_raw_block = (sfile_info.Length - audio_offset) % 2352;

                                while (raw_reader.BaseStream.Position != raw_reader.BaseStream.Length)
                                //for (int ii = 0; ii < complete_raw_blocks; ii++)
                                {
                                    buffer2352 = raw_reader.ReadBytes(2352);

                                    int raw_counter = 0;
                                    int error_check = 0;
                                    int null_edc = 0;
                                    int msf_correction = 0;
                                    int temp_msf = 0;

                                    for (int i = 0; i < 12; i++)
                                    {
                                        if (buffer2352[i] == synchro[i]) raw_counter++;
                                    }

                                    if (raw_counter == 12)
                                    {
                                        if (buffer2352[12] == 0x10)
                                        {
                                            int j = 0;
                                        }
                                        //temp_msf = Convert.ToInt32(Convert.ToString(buffer2352[12], 16)) * 60 * 75;

                                        temp_msf = ((buffer2352[12] & 0xf) + ((buffer2352[12] >> 4) & 0xf) * 10) * 60 * 75;
                                        temp_msf += Convert.ToInt32(Convert.ToString(buffer2352[13], 16)) * 75;
                                        temp_msf += Convert.ToInt32(Convert.ToString(buffer2352[14], 16));

                                        if (msf_counter != temp_msf)
                                        {
                                            msf_correction = 0x20;
                                            msf_counter = temp_msf;
                                        }
                                        msf_counter++;
                                    }

                                    if (raw_counter == 12 && buffer2352[15] == 2)
                                    {
                                        for (int i = 0; i < 8; i++)
                                        {
                                            if (buffer2352[16 + i] == 0) error_check++;
                                        }

                                        if (error_check == 8)
                                        {
                                            error_check = 0;
                                            for (int i = 0; i < 276; i++)
                                            {
                                                if (buffer2352[2076 + i] == 0) error_check++;
                                            }
                                            if (error_check != 276)
                                            {
                                                error_check = 0x80;
                                            }
                                        }
                                        else
                                        {
                                            error_check = 0;
                                        }
                                    }

                                    if (raw_counter == 12 && buffer2352[15] == 2 && (buffer2352[18] & 0x20) == 0x20)
                                    {
                                        for (int i = 0; i < 4; i++)
                                        {
                                            if (buffer2352[2348 + i] == 0) null_edc++;
                                        }
                                        if (null_edc == 4)
                                        {
                                            null_edc = 0x40;
                                        }
                                        else
                                        {
                                            null_edc = 0;
                                        }
                                    }

                                    switch (raw_counter)
                                    {
                                        default:


                                            //for audio
                                            map_write.Write((byte)0);
                                            hash = BitConverter.ToString(md5_hash.ComputeHash(buffer2352));
                                                    switch (duplicate_cdda.TryGetValue(hash, out temp))
                                                    {
                                                        case false:
                                                            temp = sector_counter_cdda;
                                                            sector_counter_cdda++;
                                                            master_cdda.Write(buffer2352);
                                                            duplicate_cdda.Add(hash, temp);
                                                            break;
                                                    }
                                                    map_write.Write(temp);
                                            break;
                                        case 12:
                                            map_write.Write((byte)(buffer2352[15] | error_check | null_edc | msf_correction));
                                            //map_write.Write(buffer2352, 12, 3);
                                            if (msf_correction == 0x20) map_write.Write(temp_msf);
                                            switch (buffer2352[15])
                                            {
                                                case 1:
                                                    hash = BitConverter.ToString(md5_hash.ComputeHash(buffer2352, 16, 2048));
                                                    switch (duplicate_form1.TryGetValue(hash, out temp))
                                                    {
                                                        case false:
                                                            temp = sector_counter_form1;
                                                            sector_counter_form1++;
                                                            master_form1.Write(buffer2352, 16, 2048);
                                                            duplicate_form1.Add(hash, temp);
                                                            break;
                                                    }
                                                    map_write.Write(temp);
                                                    //map_write.Write(buffer2352, 2064, 4);
                                                    break;
                                                case 2:
                                                    //start
                                                    //map_write.Write(buffer2352, 16, 8); //!!!!!!!!!!!!!!!!!!!!!!!!
                                                    //map_write.Write((int)0);
                                                    hash = BitConverter.ToString(md5_hash.ComputeHash(buffer2352, 16, 8));
                                                    switch (duplicate_subheader.TryGetValue(hash, out temp))
                                                            {
                                                                case false:
                                                                    temp = sector_counter_subheader;
                                                                    sector_counter_subheader++;
                                                                    master_subheader.Write(buffer2352, 16, 8);
                                                                    duplicate_subheader.Add(hash, temp);
                                                                    break;
                                                            }
                                                            map_write.Write(temp);

                                                    switch (buffer2352[16 + 2] & 0x20)
                                                    {
                                                        default:
                                                            hash = BitConverter.ToString(md5_hash.ComputeHash(buffer2352, 24, 2048));
                                                            switch (duplicate_form1.TryGetValue(hash, out temp))
                                                            {
                                                                case false:
                                                                    temp = sector_counter_form1;
                                                                    sector_counter_form1++;
                                                                    master_form1.Write(buffer2352, 24, 2048);
                                                                    duplicate_form1.Add(hash, temp);
                                                                    break;
                                                            }
                                                            map_write.Write(temp);
                                                            //map_write.Write(buffer2352, 2072, 4);
                                                            break;
                                                        case 0x20:
                                                            hash = BitConverter.ToString(md5_hash.ComputeHash(buffer2352, 24, 2324));
                                                            switch (duplicate_form2.TryGetValue(hash, out temp))
                                                            {
                                                                case false:
                                                                    temp = sector_counter_form2;
                                                                    sector_counter_form2++;
                                                                    master_form2.Write(buffer2352, 24, 2324);
                                                                    duplicate_form2.Add(hash, temp);
                                                                    break;
                                                            }
                                                            map_write.Write(temp);
                                                            //map_write.Write(buffer2352, 2348, 4);
                                                            break;
                                                    }
                                                    //end
                                                    break;
                                            }
                                            

                                    


                                            break;
                                    }


                                    
                                }
                                ///incomplete
                                /*
                                if (incomplete_raw_block != 0)
                                {
                                    byte[] block = new byte[incomplete_raw_block];
                                    map_write.Write((byte)0);
                                    hash = BitConverter.ToString(md5_hash.ComputeHash(block));
                                    switch (duplicate_cdda.TryGetValue(hash, out temp))
                                    {
                                        case false:
                                            temp = sector_counter_cdda;
                                            sector_counter_cdda++;
                                            master_cdda.Write(block);
                                            duplicate_cdda.Add(hash, temp);
                                            break;
                                    }
                                    map_write.Write(temp);
                                }

                                while ((master_cdda.BaseStream.Position -44) % 2352 != 0)
                                {
                                    master_cdda.Write((byte)0);
                                }
                                */
                                //testing

                                xml_file.SetAttribute("map_offset", master_map.BaseStream.Position.ToString());
                                xml_file.SetAttribute("map_size", map.Length.ToString());

                                //if (audio_offset != 0) xml_file.SetAttribute("audio_offset", audio_offset.ToString());

                                map.Position = 0;
                                map.CopyTo(master_map.BaseStream);
                                break;
                        }
                        raw_reader.Close();
                    }
                    
                }
                XmlElement partition_form1 = xml_control.CreateElement("form1");
                partition_form1.SetAttribute("offset", "0");
                partition_form1.SetAttribute("size", master_form1.BaseStream.Length.ToString());
                xml_partitions.AppendChild(partition_form1);

                XmlElement partition_form2 = xml_control.CreateElement("form2");
                partition_form2.SetAttribute("offset", master_form1.BaseStream.Position.ToString());
                partition_form2.SetAttribute("size", master_form2.BaseStream.Length.ToString());
                xml_partitions.AppendChild(partition_form2);

                master_form2.BaseStream.Position = 0;
                master_form2.BaseStream.CopyTo(master_form1.BaseStream);
                master_form2.Close();
                File.Delete(save_path + ".form2");

                XmlElement partition_subheader = xml_control.CreateElement("subheader");
                partition_subheader.SetAttribute("offset", master_form1.BaseStream.Position.ToString());
                partition_subheader.SetAttribute("size", master_subheader.BaseStream.Length.ToString());
                xml_partitions.AppendChild(partition_subheader);

                master_subheader.BaseStream.Position = 0;
                master_subheader.BaseStream.CopyTo(master_form1.BaseStream);
                master_subheader.Close();
                File.Delete(save_path + ".subheader");

                XmlElement partition_cdda = xml_control.CreateElement("cdda");
                //partition_cdda.SetAttribute("offset", master_form1.BaseStream.Position.ToString());
                //partition_cdda.SetAttribute("size", master_cdda.BaseStream.Length.ToString());
                //xml_partitions.AppendChild(partition_cdda);

                
                if (master_cdda.BaseStream.Length != 44)
                {
                    master_cdda.BaseStream.Position = 4;
                    master_cdda.Write((int)master_cdda.BaseStream.Length - 8);
                    master_cdda.BaseStream.Position = 40;
                    master_cdda.Write((int)master_cdda.BaseStream.Length - 44);
                    //master_cdda.BaseStream.CopyTo(master_form1.BaseStream);
                    master_cdda.Close();


                    //flac coding
                    Process flac_exe = new Process();
                    flac_exe.StartInfo.FileName = "flac.exe";
                    flac_exe.StartInfo.Arguments = "--best --verify " + "\"" + save_path + ".wav" + "\"";

                    flac_exe.Start();
                    flac_exe.WaitForExit();
                    flac_exe.Close();

                    BinaryReader wav = new BinaryReader(File.OpenRead(save_path + ".flac"));
                    partition_cdda.SetAttribute("offset", master_form1.BaseStream.Position.ToString());
                    partition_cdda.SetAttribute("size", wav.BaseStream.Length.ToString());
                    xml_partitions.AppendChild(partition_cdda);

                    wav.BaseStream.CopyTo(master_form1.BaseStream);
                    wav.Close();

                    File.Delete(save_path + ".wav");
                    File.Delete(save_path + ".flac");
                }
                else
                {
                    master_cdda.Close();
                    File.Delete(save_path + ".wav");
                }
                
                //master_subheader.Close();

                XmlElement partition_map = xml_control.CreateElement("map");
                partition_map.SetAttribute("offset", master_form1.BaseStream.Position.ToString());
                partition_map.SetAttribute("size", master_map.BaseStream.Length.ToString());
                xml_partitions.AppendChild(partition_map);

                master_map.BaseStream.Position = 0;
                master_map.BaseStream.CopyTo(master_form1.BaseStream);
                master_map.Close();
                File.Delete(save_path + ".map");

                //char[] control = new char[0];
                char[] control = xml_control.InnerXml.ToCharArray();
                int control_size = control.Length;
                long control_offset = master_form1.BaseStream.Position;
                //control = xml_control.InnerXml.ToCharArray();
                //xml_control.InnerXml.CopyTo(0, control, 0, control_size);
                
                

                xml_control.Save(save_path + ".xml");
                //File.Delete(save_path + ".xml");
                master_form1.Write(control);
                master_form1.Write(control_offset);
                master_form1.Write(control_size);
                master_form1.Write("CDM3".ToCharArray());

                master_form1.Close();
            }
        }

        private void dataGridView2_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void dataGridView2_DragDrop(object sender, DragEventArgs e)
        {
            string cdm3 = "CDM3";

            foreach (string cdm_file in (string[])e.Data.GetData(DataFormats.FileDrop))
            {
                BinaryReader cdm_reader = new BinaryReader(File.Open(cdm_file, FileMode.Open, FileAccess.Read));
                cdm_reader.BaseStream.Position = cdm_reader.BaseStream.Length - 4;
                string cdm3_check = new string(cdm_reader.ReadChars(4));
                FileInfo cdm_info = new FileInfo(cdm_file);

                switch (cdm3 == cdm3_check)
                {
                    case true:
                        cdm_reader.BaseStream.Position = cdm_reader.BaseStream.Length - 16;
                        XmlDocument xml_control = new XmlDocument();
                        long xml_offset = cdm_reader.ReadInt64();
                        int xml_size = cdm_reader.ReadInt32();
                        cdm_reader.BaseStream.Position = xml_offset;
                        xml_control.InnerXml = new string(cdm_reader.ReadChars(xml_size));



                        foreach (XmlNode node in xml_control.SelectNodes("root//entries//entry"))
                        {
                            string entry = node.Attributes["name"].Value.ToString();
                            dataGridView2.Rows.Add(cdm_info.FullName, entry);
                        }

                        break;
                }
                cdm_reader.Close();
            }
        }
        
        private void calculate_edc(byte[] buffer, int mode, uint[] edc_lut)
        {
            UInt32 edc = 0;
            int count = 0;
            var i = 0;
            int offset = 0;

            switch (mode)
            {
                case 1:
                    count = 2064;
                    offset = 0;
                    break;
                case 21:
                    count = 2048 + 8;
                    offset = 16;
                    break;
                case 22:
                    count = 2324 + 8;
                    offset = 16;
                    break;
            }
            while (i != count)
            {
                edc = (UInt32)((edc >> 8) ^ edc_lut[(edc ^ (buffer[offset + i++])) & 0xff]);
            }
            byte[] ar_edc = BitConverter.GetBytes(edc);

            for (i = 0; i < 4; i++)
            {
                buffer[i + offset + count] = ar_edc[i];
            }
        }
        
        private void calculate_eccq(byte[] buffer, byte[] ecc_f_lut, byte[] ecc_b_lut)
        {
            UInt32 major_count, minor_count, major_mult, minor_inc;
            major_count = 52;
            minor_count = 43;
            major_mult = 86;
            minor_inc = 88;

            var eccsize = major_count * minor_count;
            UInt32 major, minor;
            for (major = 0; major < major_count; major++)
            {
                var index = (major >> 1) * major_mult + (major & 1);
                byte ecc_a = 0;
                byte ecc_b = 0;
                for (minor = 0; minor < minor_count; minor++)
                {
                    byte temp = buffer[12 + index];
                    index += minor_inc;
                    if (index >= eccsize) index -= eccsize;
                    ecc_a ^= temp;
                    ecc_b ^= temp;
                    ecc_a = ecc_f_lut[ecc_a];
                }
                ecc_a = ecc_b_lut[ecc_f_lut[ecc_a] ^ ecc_b];
                buffer[2076 + 172 + major] = ecc_a;
                buffer[2076 + 172 + major + major_count] = (byte)(ecc_a ^ ecc_b);
            }
        }

        private void calculate_eccp(byte[] buffer, byte[] ecc_f_lut, byte[] ecc_b_lut)
        {
            UInt32 major_count, minor_count, major_mult, minor_inc;
            major_count = 86;
            minor_count = 24;
            major_mult = 2;
            minor_inc = 86;

            var eccsize = major_count * minor_count;
            UInt32 major, minor;
            for (major = 0; major < major_count; major++)
            {
                var index = (major >> 1) * major_mult + (major & 1);
                byte ecc_a = 0;
                byte ecc_b = 0;
                for (minor = 0; minor < minor_count; minor++)
                {
                    byte temp = buffer[12 + index];
                    index += minor_inc;
                    if (index >= eccsize) index -= eccsize;
                    ecc_a ^= temp;
                    ecc_b ^= temp;
                    ecc_a = ecc_f_lut[ecc_a];
                }
                ecc_a = ecc_b_lut[ecc_f_lut[ecc_a] ^ ecc_b];
                buffer[2076 + major] = ecc_a;
                buffer[2076 + major + major_count] = (byte)(ecc_a ^ ecc_b);
            }
        }

        struct cdm_entry
        {
            public string type;
            public string name;
            public string date;
            public string md5;
            public long size;
            public long map_offset;
            public long map_size;
            //public int audio_offset;
        }

        struct cdm_partition
        {
            public long partition_offset;
            public long partition_size;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            uint[] edc_lut = new uint[256];
            byte[] ecc_f_lut = new byte[256];
            byte[] ecc_b_lut = new byte[256];

            UInt32 k, l, m;

            for (k = 0; k < 256; k++)
            {
                l = (UInt32)((k << 1) ^ ((k & 0x80) != 0 ? 0x11d : 0));
                ecc_f_lut[k] = (byte)l;
                ecc_b_lut[k ^ l] = (byte)k;
                m = k;

                for (l = 0; l < 8; l++)
                {
                    m = (m >> 1) ^ ((m & 1) != 0 ? 0xd8018001 : 0);
                }
                edc_lut[k] = m;
            }

            string cdm = dataGridView2[0, 0].Value.ToString();
            FileInfo cdm_info = new FileInfo(cdm);
            string save_path = Path.Combine(cdm_info.DirectoryName, cdm_info.Name.Replace(" [MERGED]", "").Replace(cdm_info.Extension, ""));
            Directory.CreateDirectory(save_path);

            BinaryReader form1_reader = new BinaryReader(File.Open(cdm, FileMode.Open, FileAccess.Read, FileShare.Read));
            form1_reader.BaseStream.Position = form1_reader.BaseStream.Length - 16;
            BinaryReader form2_reader = new BinaryReader(File.Open(cdm, FileMode.Open, FileAccess.Read, FileShare.Read));
            BinaryReader cdda_reader = new BinaryReader(File.Open(cdm, FileMode.Open, FileAccess.Read, FileShare.Read));

            XmlDocument xml_control = new XmlDocument();
            long xml_offset = form1_reader.ReadInt64();
            int xml_size = form1_reader.ReadInt32();
            form1_reader.BaseStream.Position = xml_offset;
            xml_control.InnerXml = new string(form1_reader.ReadChars(xml_size));

            cdm_partition[] parts = new cdm_partition[5];
            cdm_partition part = new cdm_partition();

            foreach (XmlNode node in xml_control.SelectSingleNode("root//partitions"))
            {
                switch (node.Name)
                {
                    case "form1":
                        part.partition_offset = Convert.ToInt64(node.Attributes["offset"].Value.ToString());
                        part.partition_size = Convert.ToInt64(node.Attributes["size"].Value.ToString());
                        parts[0] = part;
                        break;
                    case "form2":
                        part.partition_offset = Convert.ToInt64(node.Attributes["offset"].Value.ToString());
                        part.partition_size = Convert.ToInt64(node.Attributes["size"].Value.ToString());
                        parts[1] = part;
                        break;
                    case "subheader":
                        part.partition_offset = Convert.ToInt64(node.Attributes["offset"].Value.ToString());
                        part.partition_size = Convert.ToInt64(node.Attributes["size"].Value.ToString());
                        parts[2] = part;
                        break;
                    case "cdda":
                        part.partition_offset = Convert.ToInt64(node.Attributes["offset"].Value.ToString());
                        part.partition_size = Convert.ToInt64(node.Attributes["size"].Value.ToString());
                        parts[3] = part;
                        break;
                    case "map":
                        part.partition_offset = Convert.ToInt64(node.Attributes["offset"].Value.ToString());
                        part.partition_size = Convert.ToInt64(node.Attributes["size"].Value.ToString());
                        parts[4] = part;
                        break;
                }
            }

            if (parts[3].partition_size != 0)
            {
                FileStream temp_flac = new FileStream(Path.Combine(cdm_info.DirectoryName, "temp.flac"), FileMode.Create);
                form1_reader.BaseStream.Position = parts[3].partition_offset;
                //temp_flac.SetLength(parts[2].partition_size);
                //form1_reader.BaseStream.CopyTo(temp_flac);

                while (temp_flac.Position != parts[3].partition_size)
                {
                    temp_flac.WriteByte(form1_reader.ReadByte());
                }

                temp_flac.Close();

                Process flac_exe = new Process();
                flac_exe.StartInfo.FileName = "flac.exe";
                flac_exe.StartInfo.Arguments = "--decode " + Path.Combine(cdm_info.DirectoryName, "temp.flac");

                flac_exe.Start();
                flac_exe.WaitForExit();
                flac_exe.Close();
                File.Delete(Path.Combine(cdm_info.DirectoryName, "temp.flac"));
            }

            List<byte[]> subheaders = new List<byte[]>();
            if (parts[2].partition_size != 0)
            {
                form1_reader.BaseStream.Position = parts[2].partition_offset;
                while (parts[2].partition_size != 0)
                {
                    subheaders.Add(form1_reader.ReadBytes(8));
                    parts[2].partition_size -= 8;
                }
            }


            BinaryReader wav_reader = new BinaryReader(File.Open(Path.Combine(cdm_info.DirectoryName, "temp.wav"), FileMode.OpenOrCreate));




            foreach (DataGridViewCell cell in dataGridView2.SelectedCells)
            {
                //string cdm = dataGridView2[0, cell.RowIndex].Value.ToString();
                string entry = cell.Value.ToString();
                //FileInfo cdm_info = new FileInfo(cdm);

                //string save_path = Path.Combine(cdm_info.DirectoryName, cdm_info.Name.Replace(" [MERGED]", "").Replace(cdm_info.Extension, ""));
                //Directory.CreateDirectory(save_path);


                //BinaryReader form1_reader = new BinaryReader(File.Open(cdm, FileMode.Open, FileAccess.Read, FileShare.Read));
                //form1_reader.BaseStream.Position = form1_reader.BaseStream.Length - 16;
                //BinaryReader form2_reader = new BinaryReader(File.Open(cdm, FileMode.Open, FileAccess.Read, FileShare.Read));
                //BinaryReader cdda_reader = new BinaryReader(File.Open(cdm, FileMode.Open, FileAccess.Read, FileShare.Read));

                //XmlDocument xml_control = new XmlDocument();
                //long xml_offset = form1_reader.ReadInt64();
                //int xml_size = form1_reader.ReadInt32();
                //form1_reader.BaseStream.Position = xml_offset;
                //xml_control.InnerXml = new string(form1_reader.ReadChars(xml_size));

                //cdm_partition[] parts = new cdm_partition[5];
                //cdm_partition part = new cdm_partition();

                /*
                foreach (XmlNode node in xml_control.SelectSingleNode("root//partitions"))
                {
                    switch (node.Name)
                    {
                        case "form1":
                            part.partition_offset = Convert.ToInt64(node.Attributes["offset"].Value.ToString());
                            part.partition_size = Convert.ToInt64(node.Attributes["size"].Value.ToString());
                            parts[0] = part;
                            break;
                        case "form2":
                            part.partition_offset = Convert.ToInt64(node.Attributes["offset"].Value.ToString());
                            part.partition_size = Convert.ToInt64(node.Attributes["size"].Value.ToString());
                            parts[1] = part;
                            break;
                        case "subheader":
                            part.partition_offset = Convert.ToInt64(node.Attributes["offset"].Value.ToString());
                            part.partition_size = Convert.ToInt64(node.Attributes["size"].Value.ToString());
                            parts[2] = part;
                            break;
                        case "cdda":
                            part.partition_offset = Convert.ToInt64(node.Attributes["offset"].Value.ToString());
                            part.partition_size = Convert.ToInt64(node.Attributes["size"].Value.ToString());
                            parts[3] = part;
                            break;
                        case "map":
                            part.partition_offset = Convert.ToInt64(node.Attributes["offset"].Value.ToString());
                            part.partition_size = Convert.ToInt64(node.Attributes["size"].Value.ToString());
                            parts[4] = part;
                            break;
                    }
                }
                
                if (parts[3].partition_size != 0)
                {
                    FileStream temp_flac = new FileStream(Path.Combine(cdm_info.DirectoryName, "temp.flac"), FileMode.Create);
                    form1_reader.BaseStream.Position = parts[3].partition_offset;
                    //temp_flac.SetLength(parts[2].partition_size);
                    //form1_reader.BaseStream.CopyTo(temp_flac);

                    while (temp_flac.Position != parts[3].partition_size)
                    {
                        temp_flac.WriteByte(form1_reader.ReadByte());
                    }

                    temp_flac.Close();

                    Process flac_exe = new Process();
                    flac_exe.StartInfo.FileName = "flac.exe";
                    flac_exe.StartInfo.Arguments = "--decode " + Path.Combine(cdm_info.DirectoryName, "temp.flac");

                    flac_exe.Start();
                    flac_exe.WaitForExit();
                    flac_exe.Close();
                    File.Delete(Path.Combine(cdm_info.DirectoryName, "temp.flac"));
                }

                List<byte[]> subheaders = new List<byte[]>();
                if (parts[2].partition_size != 0)
                {
                    form1_reader.BaseStream.Position = parts[2].partition_offset;
                    while (parts[2].partition_size != 0)
                    {
                        subheaders.Add(form1_reader.ReadBytes(8));
                        parts[2].partition_size -= 8;
                    }
                }


                BinaryReader wav_reader = new BinaryReader(File.Open(Path.Combine(cdm_info.DirectoryName, "temp.wav"), FileMode.OpenOrCreate));
                */
                foreach (XmlNode node in xml_control.SelectNodes("root//entries//entry"))
                {
                    if (entry == node.Attributes["name"].Value.ToString())
                    {
                        List<cdm_entry> files = new List<cdm_entry>();
                        //cdm_entry file = new cdm_entry();

                        string entry_save_path = Path.Combine(save_path, entry);
                        Directory.CreateDirectory(entry_save_path);

                        foreach (XmlNode e_file in node.SelectNodes("files//file"))
                        {
                            cdm_entry file = new cdm_entry();
                            file.type = e_file.Attributes["type"].Value.ToString();
                            file.name = e_file.Attributes["name"].Value.ToString();
                            file.date = e_file.Attributes["date"].Value.ToString();
                            file.md5 = e_file.Attributes["md5"].Value.ToString();
                            file.size = Convert.ToInt64(e_file.Attributes["size"].Value.ToString());
                            file.map_offset = Convert.ToInt64(e_file.Attributes["map_offset"].Value.ToString());
                            file.map_size = Convert.ToInt64(e_file.Attributes["map_size"].Value.ToString());
                            //if (e_file.Attributes.Count == 8)
                                //file.audio_offset = Convert.ToInt32(e_file.Attributes["audio_offset"].Value.ToString());
                            files.Add(file);
                        }



                        foreach (cdm_entry f in files)
                        {
                            BinaryWriter out_file = new BinaryWriter(File.OpenWrite(Path.Combine(entry_save_path, f.name)));
                            form1_reader.BaseStream.Position = parts[4].partition_offset + f.map_offset;
                            byte[] map = form1_reader.ReadBytes((int)f.map_size);
                            BinaryReader map_reader = new BinaryReader(new MemoryStream(map));


                            int msf_counter = 150;
                            long offset = 0;
                            int sector = 0;
                            byte[] buffer2048 = new byte[2048];
                            byte[] buffer2324 = new byte[2324];
                            //byte[] buffer2352 = new byte[2352];
                            int subheader = 0;
                            byte[] msf = new byte[3];
                            byte[] mode2_subheader = new byte[8];
                            byte[] edc = new byte[4];
                            byte[] synchro = { 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x00 };

                            switch (f.type)
                            {
                                case "file":
                                    long complete_blocks = f.size / 2048;
                                    long last_block = f.size % 2048;

                                    for (long i = 0; i < complete_blocks; i++)
                                    {
                                        offset = (long)map_reader.ReadInt32() * 2048;
                                        if (form1_reader.BaseStream.Position != offset) form1_reader.BaseStream.Position = offset;
                                        out_file.Write(form1_reader.ReadBytes(2048));
                                    }
                                    if (last_block != 0)
                                    {
                                        offset = (long)map_reader.ReadInt32() * 2048;
                                        if (form1_reader.BaseStream.Position != offset) form1_reader.BaseStream.Position = offset;
                                        out_file.Write(form1_reader.ReadBytes((int)last_block));
                                    }
                                    break;
                                case "raw":
                                    //byte[] buffer2352 = new byte[2352];
                                    /*
                                    long complete_raw_block = 0;
                                    long incomplete_raw_block = 0;

                                    if (f.audio_offset != 0)
                                    {
                                        complete_raw_block = (f.size - f.audio_offset) / 2352;
                                        incomplete_raw_block = (f.size - f.audio_offset) % 2352;
                                        int counter = f.audio_offset;
                                        while (counter != 0)
                                        {
                                            out_file.Write((byte)0);
                                            counter--;
                                        }
                                    }
                                    else
                                    {
                                        complete_raw_block = f.size / 2352;
                                        incomplete_raw_block = f.size % 2352;
                                    }
                                    */
                                    //for (int ii = 0; ii < complete_raw_block; ii++)
                                    while (map_reader.BaseStream.Position != map_reader.BaseStream.Length)
                                    {
                                        byte[] buffer2352 = new byte[2352];
                                        byte sector_type = map_reader.ReadByte();

                                        switch (sector_type & 3)
                                        {
                                            case 0:
                                                sector = map_reader.ReadInt32();
                                                offset = 44 + (long)sector * 2352;
                                                if (wav_reader.BaseStream.Position != offset) wav_reader.BaseStream.Position = offset;
                                                out_file.Write(wav_reader.ReadBytes(2352));
                                                break;
                                            case 1:
                                                if ((sector_type & 0x20) == 0x20) msf_counter = map_reader.ReadInt32();
                                                sector = map_reader.ReadInt32();

                                                for (int i = 0; i < 12; i++)
                                                {
                                                    buffer2352[i] = synchro[i];
                                                }

                                                calculate_msf(buffer2352, msf_counter);
                                                msf_counter++;
                                                buffer2352[15] = 1;
                                                offset = parts[0].partition_offset + sector * 2048;
                                                if (form1_reader.BaseStream.Position != offset) form1_reader.BaseStream.Position = offset;
                                                buffer2048 = form1_reader.ReadBytes(2048);
                                                for (int i = 0; i < 2048; i++)
                                                {
                                                    buffer2352[16 + i] = buffer2048[i];
                                                }
                                                calculate_edc(buffer2352, 1, edc_lut);
                                                calculate_eccp(buffer2352, ecc_f_lut, ecc_b_lut);
                                                calculate_eccq(buffer2352, ecc_f_lut, ecc_b_lut);
                                                out_file.Write(buffer2352);
                                                break;
                                            case 2:
                                                if ((sector_type & 0x20) == 0x20) msf_counter = map_reader.ReadInt32();
                                                //msf = map_reader.ReadBytes(3);
                                                //mode2_subheader = map_reader.ReadBytes(8);
                                                subheader = map_reader.ReadInt32();
                                                sector = map_reader.ReadInt32();
                                                
                                                //edc = map_reader.ReadBytes(4);

                                                //switch (mode2_subheader[2] & 0x20)
                                                switch (subheaders[subheader][2] & 0x20)
                                                {
                                                    default:
                                                        for (int i = 0; i < 12; i++)
                                                        {
                                                            buffer2352[i] = synchro[i];
                                                        }
                                                        
                                                        for (int i = 0; i < 8; i++)
                                                        {
                                                            buffer2352[16 + i] = subheaders[subheader][i];
                                                        }
                                                        offset = parts[0].partition_offset + sector * 2048;
                                                        if (form1_reader.BaseStream.Position != offset) form1_reader.BaseStream.Position = offset;
                                                        buffer2048 = form1_reader.ReadBytes(2048);

                                                        for (int i = 0; i < 2048; i++)
                                                        {
                                                            buffer2352[24 + i] = buffer2048[i];
                                                        }

                                                        calculate_edc(buffer2352, 21, edc_lut);
                                                        /*
                                                        for (int i = 0; i < 4; i++)
                                                        {
                                                            buffer2352[2072 + i] = edc[i];
                                                        }
                                                        */
                                                        switch (sector_type & 0x80)
                                                        {
                                                            default:
                                                                calculate_eccp(buffer2352, ecc_f_lut, ecc_b_lut);
                                                                calculate_eccq(buffer2352, ecc_f_lut, ecc_b_lut);

                                                                calculate_msf(buffer2352, msf_counter);
                                                                msf_counter++;
                                                                /*
                                                                for (int i = 0; i < 3; i++)
                                                                {
                                                                    buffer2352[12 + i] = msf[i];
                                                                }
                                                                 */
                                                                buffer2352[15] = 2;
                                                                break;
                                                            case 0x80:
                                                                calculate_msf(buffer2352, msf_counter);
                                                                msf_counter++;
                                                                /*
                                                                for (int i = 0; i < 3; i++)
                                                                {
                                                                    buffer2352[12 + i] = msf[i];
                                                                }
                                                                 */
                                                                buffer2352[15] = 2;
                                                                calculate_eccp(buffer2352, ecc_f_lut, ecc_b_lut);
                                                                calculate_eccq(buffer2352, ecc_f_lut, ecc_b_lut);
                                                                break;
                                                        }
                                                        

                                                        out_file.Write(buffer2352);
                                                        break;
                                                    case 0x20:
                                                        for (int i = 0; i < 12; i++)
                                                        {
                                                            buffer2352[i] = synchro[i];
                                                        }
                                                        calculate_msf(buffer2352, msf_counter);
                                                                msf_counter++;
                                                        /*
                                                        for (int i = 0; i < 3; i++)
                                                        {
                                                            buffer2352[12 + i] = msf[i];
                                                        }
                                                         */
                                                        buffer2352[15] = 2;
                                                        for (int i = 0; i < 8; i++)
                                                        {
                                                            buffer2352[16 + i] = subheaders[subheader][i];
                                                        }
                                                        offset = parts[1].partition_offset + sector * 2324;
                                                        if (form2_reader.BaseStream.Position != offset) form2_reader.BaseStream.Position = offset;
                                                        buffer2324 = form2_reader.ReadBytes(2324);

                                                        for (int i = 0; i < 2324; i++)
                                                        {
                                                            buffer2352[24 + i] = buffer2324[i];
                                                        }
                                                        if ((sector_type & 0x40) != 0x40) calculate_edc(buffer2352, 22, edc_lut);
                                                        /*
                                                        for (int i = 0; i < 4; i++)
                                                        {
                                                            buffer2352[2348 + i] = edc[i];
                                                        }
                                                        */
                                                        out_file.Write(buffer2352);
                                                        break;
                                                }


                                                break;
                                        }
                                    }

                                    /*
                                    if (incomplete_raw_block != 0)
                                    {
                                        map_reader.ReadByte();
                                        sector = map_reader.ReadInt32();
                                        offset = 44 + (long)sector * 2352;
                                        if (wav_reader.BaseStream.Position != offset) wav_reader.BaseStream.Position = offset;
                                        out_file.Write(wav_reader.ReadBytes((int)incomplete_raw_block));
                                        break;

                                        
                                        offset = (long)map_reader.ReadInt32() * 2048;
                                        if (form1_reader.BaseStream.Position != offset) form1_reader.BaseStream.Position = offset;
                                        out_file.Write(form1_reader.ReadBytes((int)last_block));
                                         
                                    }
                                    */
                                    break;
                            }
                            /*
                            while (map_reader.BaseStream.Position != map_reader.BaseStream.Length)
                            {
                                offset = (long)map_reader.ReadInt32() * 2048;
                                if (cdm_reader.BaseStream.Position != offset) cdm_reader.BaseStream.Position = offset;
                                out_file.Write(cdm_reader.ReadBytes(2048));
                            }
                            */
                            map_reader.Close();
                            out_file.Close();

                            FileInfo f_info = new FileInfo(Path.Combine(entry_save_path, f.name));
                            //f_info.LastWriteTime.Ticks = new DateTime(Convert.ToInt64(f.date));
                            f_info.LastWriteTime = new DateTime(Convert.ToInt64(f.date));
                            FileStream file_md5 = new FileStream(Path.Combine(entry_save_path, f.name), FileMode.Open);
                            if(f.md5 == BitConverter.ToString(MD5.Create().ComputeHash(file_md5)).Replace("-", "").ToLower()) listBox1.Items.Add(f.name + " md5 is OK");
                            file_md5.Close();

                            //f.md5
                        }
                    }
                }
                //wav_reader.Close();
                //File.Delete(Path.Combine(cdm_info.DirectoryName, "temp.wav"));
                //form1_reader.Close();
                //form2_reader.Close();
            }//

            wav_reader.Close();
            File.Delete(Path.Combine(cdm_info.DirectoryName, "temp.wav"));
            form1_reader.Close();
            form2_reader.Close();
        }

        private void calculate_msf(byte[] buffer2352, int msf_counter)
        {

            //master.sector += 150;
            //master.sector += 150;

            byte[] msf = new byte[3];

            int minutes = msf_counter / 4500;

            int minutes_Hi = minutes / 10;
            int minutes_Lo = minutes - minutes_Hi * 10;


            //msf[0] = Convert.ToByte((master.sector / 4500).ToString(), 16);
            msf[0] = (byte)((minutes_Hi << 4) | minutes_Lo);
            msf[1] = Convert.ToByte((msf_counter % 4500 / 75).ToString(), 16);
            msf[2] = Convert.ToByte((msf_counter % 75).ToString(), 16);
            

            //master.buffer.msf[3] = (byte)p;

            for (int i = 0; i < 3; i++)
            {
                buffer2352[i + 12] = msf[i];
                //master.buffer.temp[i + 12] = master.buffer.msf[i];
            }

        }
    }
}
