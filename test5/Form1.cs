using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Xml;
namespace test5
{
    public partial class Form1 : Form
    {

        //tq start

        public static ArrayList alist = new ArrayList();
        public static ArrayList prolist = new ArrayList();
        public static ArrayList cpp_list = new ArrayList();
        public static ArrayList include_list = new ArrayList();
        public static string vcx_rootpath = "";
        public static string vcx_user = "";
        public static Dictionary<string, string> cpp_dic = new Dictionary<string, string>();//tq
        public static Dictionary<string, string> dic = new Dictionary<string, string>();//QTDIR

        /*
            解析vcxprof.fileter
         */

        static void ParseVcxFilter(string vcxfilter)
        {
            FileInfo vcx_ = new FileInfo(vcxfilter);

            if (!vcx_.Exists)
                return;

            XmlDocument doc = new XmlDocument();
            doc.Load(vcxfilter);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");
            XmlNode root = doc.SelectSingleNode("ns:Project", nsmgr);

            XmlNodeList inlcudePathNodes = root.SelectNodes("ns:ItemGroup/ns:ClCompile", nsmgr);
            foreach (XmlNode node in inlcudePathNodes)
            {
                XmlElement xe = (XmlElement)node;
                string file = @xe.GetAttribute("Include").ToString();
                string type = file;
                int pos = type.LastIndexOf('.') + 1;
                if (pos < 0 || pos > type.Length)
                    continue;
                type = type.Substring(pos, type.Length - pos);
                if (type != "cpp" && type != "c")
                    continue;
                FileInfo finfo = new FileInfo(file);

                if (!finfo.Exists)
                    continue;
                string panter = file.Replace("\\","/");
                string pan = panter.Substring(1, 2);
                string cpppath = "";
                if (pan != ":/")
                {
                    cpppath = vcx_.DirectoryName + file.Replace("\\","/");
                }
                else
                {
                    cpppath = file;
                }
                string cpp_dir = new FileInfo(cpppath).DirectoryName;
                if (!cpp_list.Contains(cpp_dir))
                {
                    cpp_list.Add(cpp_dir);
                }
            }
        }

        /*
            解析 vcxprof.user
         */

        static void ParseVcxUser(string vcxuser)
        {
            FileInfo vcx_ = new FileInfo(vcxuser);

            if (!vcx_.Exists)
                return;

            XmlDocument doc = new XmlDocument();
            doc.Load(vcxuser);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");
            XmlNode root = doc.SelectSingleNode("ns:Project", nsmgr);

            XmlNodeList inlcudePathNodes = root.SelectNodes("ns:PropertyGroup", nsmgr);

            foreach (XmlNode node in inlcudePathNodes)
            {
                XmlNodeList list1 = node.ChildNodes;
                foreach(XmlNode node1 in list1)
                {
                    string s1 = node1.Name;
                    string s2 = node1.InnerText;
                    if (s2.Contains("$") || s2.Contains("%") || s2.Contains("("))
                        continue;
                    if (!dic.ContainsKey(s1))
                        dic.Add(s1,s2.Replace("\\","/"));
                }
            }
        }

        /*
            解析 vcxproj
        */

        static void ParseVS(string vcxproj)
        {
            if (prolist.Contains(vcxproj))
                return;
            else
                prolist.Add(vcxproj);

            ParseVcxUser(vcxproj + ".user");
            ParseVcxFilter(vcxproj + ".filters");
            FileInfo vcx_ = new FileInfo(vcxproj);

            if (!vcx_.Exists)
                return;

            XmlDocument doc = new XmlDocument();
            doc.Load(vcxproj);
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");
            XmlNode root = doc.SelectSingleNode("ns:Project", nsmgr);

            /*
                 查找  IncludePath
             */
            /*
            XmlNodeList inlcudePathNodes = root.SelectNodes("ns:PropertyGroup/ns:IncludePath", nsmgr);

            foreach (XmlNode node in inlcudePathNodes)
            {
                string[] v_include = node.InnerText.Split(new char[] { ';' });

                foreach (string item in v_include)
                {
                    string def = "";
                    if (item.Contains("("))
                    {
                        int pos1 = item.IndexOf("(")+1;
                        int pos2 = item.IndexOf(")");
                        def = item.Substring(pos1,pos2-pos1);

                        if (!dic.ContainsKey(def))
                            continue;

                        string current_include = item;
                        current_include = current_include.Replace("\\","/");
                        string[] includes = current_include.Split('/');
                        string  addinclude = "";
                        foreach (string e in includes)
                        {
                            if (e.Contains("("))
                            {
                                addinclude += dic[def] + "/";
                            }
                            else
                            {
                                addinclude += e + "/";
                            }
                        }

                        if (!alist.Contains(addinclude))
                            alist.Add(addinclude);
                        continue;
                    }

                    DirectoryInfo dinfo = new DirectoryInfo(item);

                    if (!dinfo.Exists)
                        continue;

                    string include_ = item;
                   include_ =  include_.Replace("\\","/");
                    if (include_[include_.Length - 1] != '/')
                        include_ += "/";

                    if (!alist.Contains(include_))
                        alist.Add(include_);
                }
            }
            */
            /*
                查找 ClInclude 
             */
            /*
            XmlNodeList clIncludePathNodes = root.SelectNodes("ns:ItemGroup/ns:ClInclude", nsmgr);

            foreach (XmlNode node in clIncludePathNodes)
            {
                XmlElement xe = (XmlElement)node;
                string file = @xe.GetAttribute("Include").ToString();
                FileInfo finfo = new FileInfo(vcx_rootpath+file.Replace("\\","/"));
                string item = finfo.DirectoryName;
                //----------------------
                string def = "";
                if (item.Contains("("))
                {
                    int pos1 = item.IndexOf("(") + 1;
                    int pos2 = item.IndexOf(")");
                    def = item.Substring(pos1, pos2 - pos1);

                    if (!dic.ContainsKey(def))
                        continue;

                    string current_include = item;
                    current_include = current_include.Replace("\\", "/");
                    string[] includes = current_include.Split('/');
                    string addinclude = "";
                    foreach (string e in includes)
                    {
                        if (e.Contains("("))
                        {
                            addinclude += dic[def] + "/";
                        }
                        else
                        {
                            addinclude += e + "/";
                        }
                    }

                    if (!alist.Contains(addinclude))
                        alist.Add(addinclude);
                    continue;
                }
                //------------------------
                DirectoryInfo dinfo = new DirectoryInfo(item);
                if (!dinfo.Exists)
                    continue;

                item = item.Replace("\\","/");
                if (item[item.Length - 1] != '/')
                    item += "/";

                if (!alist.Contains(item))
                    alist.Add(item);

            }
            */

            /*
                查找  ClCompile 
             */

            XmlNodeList inlcudePathNodes = root.SelectNodes("ns:ItemGroup/ns:ClCompile", nsmgr);
            foreach (XmlNode node in inlcudePathNodes)
            {
                XmlElement xe = (XmlElement)node;
                string file = @xe.GetAttribute("Include").ToString();
                string type = file;
                int pos = type.LastIndexOf('.') + 1;
                if (pos < 0 || pos > type.Length)
                    continue;
                type = type.Substring(pos, type.Length - pos);
                if (type != "cpp" && type != "c")
                    continue;

                string panter = file.Replace("\\", "/");
                string pan = panter.Substring(1, 2);
                string cpppath = "";
                if (pan != ":/")
                {
                    cpppath = vcx_.DirectoryName +"/"+ file.Replace("\\","/");
                }
                else
                {
                    cpppath = file;
                }
                FileInfo finfo = new FileInfo(cpppath);

                if (!finfo.Exists)
                    continue;
                string cpp_dir = new FileInfo(cpppath).DirectoryName;
                if (!cpp_list.Contains(cpp_dir))
                {
                    cpp_list.Add(cpp_dir);
                }
            }


            /*
                查找 AdditionalIncludeDirectories
             */

            XmlNodeList AID_includepath = root.SelectNodes("ns:ItemDefinitionGroup/ns:ClCompile/ns:AdditionalIncludeDirectories", nsmgr);

            foreach (XmlNode node in AID_includepath)
            {
                string[] v_include = node.InnerText.Split(new char[] { ';' });
                foreach (string item in v_include)
                {

                    //---------------------
                    string def = "";
                    if (item.Contains("("))
                    {
                        int pos1 = item.IndexOf("(") + 1;
                        int pos2 = item.IndexOf(")");
                        def = item.Substring(pos1, pos2 - pos1);

                        if (!dic.ContainsKey(def))
                            continue;

                        string current_include = item;
                        current_include = current_include.Replace("\\", "/");
                        string[] includes = current_include.Split('/');
                        string addinclude = "";
                        foreach (string e in includes)
                        {
                            if (e.Contains("("))
                            {
                                addinclude += dic[def] + "/";
                            }
                            else
                            {
                                addinclude += e + "/";
                            }
                        }

                            if (!include_list.Contains(addinclude))
                                include_list.Add(addinclude);
                        if (!alist.Contains(addinclude))
                        {
                            alist.Add(addinclude);
                        }
                        continue;
                    }
                    //------------
                    DirectoryInfo dinfo = new DirectoryInfo(item);
                    if (!dinfo.Exists)
                        continue;
                    string include_ = dinfo.FullName;
                    if (item == ".")
                        include_ = vcx_rootpath;
                    include_ = include_.Replace("\\","/");
                    if (include_[include_.Length - 1] != '/')
                    { 
                        include_ += "/";
                    }
                    if (!include_list.Contains(include_))
                    { 
                        include_list.Add(include_);
                    }    
                    if (!alist.Contains(include_))
                    {
                        alist.Add(include_);
                    }

                }
            }
            /*
                    查找vcxproj
             */
            XmlNodeList vcx_ProjectPath = root.SelectNodes("ns:ItemGroup/ns:ProjectReference", nsmgr);
            if (vcx_ProjectPath.Count == 0)
            {
                if (include_list.Count !=0  && cpp_list.Count != 0)
                {
                    //cpp_dic.Add(include_list, cpp_list);  
                    //=================
                    FileStream fs11 = new FileStream("test.txt", FileMode.Append, FileAccess.Write);//创建写入文件 
                    StreamWriter sw1 = new StreamWriter(fs11);
                    sw1.WriteLine("===================start==========");
                    sw1.WriteLine("工程名:");
                    sw1.WriteLine(vcxproj);
                    sw1.WriteLine("头文件:");
                    
                    //string[] v_include = include_vector.Split(new char[] { ';' });
                    string all_inlcude = "";
                    foreach (string item in include_list)
                    {
                        all_inlcude += item + ";";
                        sw1.WriteLine(item);
                    }
                   
                    sw1.WriteLine("源代码:");
                    
                    //string[] v_cpp = cpp_vector.Split(new char[] { ';' });
                    string all_cpp_path = "";
                    foreach (string item in cpp_list)
                    {
                        all_cpp_path += item + ";";
                        sw1.WriteLine(item);
                    }
                     
                    sw1.WriteLine("===================end==========");
                    sw1.Close();
                    fs11.Close();
                    cpp_dic.Add(all_inlcude,all_cpp_path);
                    include_list.Clear();
                    cpp_list.Clear();
                    dic.Clear();
                    //=======================
                }
            }
            foreach (XmlNode node in vcx_ProjectPath)
            {
                XmlElement xe = (XmlElement)node;
                string file = @xe.GetAttribute("Include").ToString();
                string type = file;
                int pos = type.LastIndexOf('.') + 1;
                if (pos < 0 || pos > type.Length)
                    continue;
                type = type.Substring(pos, type.Length - pos);
                if (type != "vcxproj")
                    continue;
                if ( include_list.Count != 0 && cpp_list.Count != 0)
                { 
                    //cpp_dic.Add(include_list, cpp_list);
                    //=================
                    FileStream fs11 = new FileStream("test.txt", FileMode.Append, FileAccess.Write);//创建写入文件 
                    StreamWriter sw1 = new StreamWriter(fs11);
                    sw1.WriteLine("===================start==========");
                    sw1.WriteLine("工程名:");
                    sw1.WriteLine(vcxproj);
                    sw1.WriteLine("头文件:");
                    //string[] v_include = include_vector.Split(new char[] { ';' });
                    string all_inlcude = "";
                    foreach(string item in include_list)
                    {
                        all_inlcude += item + ";";
                        sw1.WriteLine(item);
                    }
                    sw1.WriteLine("源代码:");
                    //string[] v_cpp = cpp_vector.Split(new char[] { ';' });
                    string all_cpp_path = "";
                    foreach(string item in cpp_list)
                    {
                        all_cpp_path += item + ";";
                        sw1.WriteLine(item);
                    }
                    sw1.WriteLine("===================end==========");
                    sw1.Close();
                    fs11.Close();
                    cpp_dic.Add(all_inlcude, all_cpp_path);
                    //=======================
                }
                include_list.Clear();
                cpp_list.Clear();
                dic.Clear();
                string vcxlocation = file.Replace("\\", "/");
                //vcx_user = vcxlocation + ".user";
                //ParseVcxUser(vcx_user);
                ParseVS(vcxlocation);

            }
        }

        /*
            查找VS工程的sln,并通过sln查找vcxproj
         */

        static int findSlnAndVcxproj(string pro_rootpath)
        {
            int flog = -1;
            DirectoryInfo dir = new DirectoryInfo(pro_rootpath);
            if (!dir.Exists)
            {
                flog = 0;
                return flog;
            }
            vcx_rootpath = dir.FullName;
            vcx_rootpath = vcx_rootpath.Replace("\\","/");
            if (vcx_rootpath[vcx_rootpath.Length - 1] != '/')
                vcx_rootpath += "/";
            /*
                 遍历当前文件夹下的文件
            */

            FileInfo[] allfile = dir.GetFiles();

            foreach (FileInfo file in allfile)
            {

                string type = file.Name;
                int pos = type.LastIndexOf('.') + 1;
                if (pos < 0 || pos > type.Length)
                    continue;
                type = type.Substring(pos, type.Length - pos);

                if (type == "sln")
                {
                    if (File.Exists(@file.FullName))
                    {
                        flog = 1;
                        StreamReader sr = new StreamReader(file.FullName, Encoding.Default);
                        String str;
                        while ((str = sr.ReadLine()) != null)
                        {
                            if (str.Length < 8)
                                continue;
                            if (str.Substring(0, 8) != "Project(")
                                continue;
                            int pos0 = str.IndexOf(",") + 3;
                            int pos1 = str.LastIndexOf(",") - 1;
                            string vcxpath = str.Substring(pos0, pos1 - pos0);

                            // System.Console.WriteLine(vcxpath);

                            pro_rootpath = pro_rootpath.Replace("\\", "/");
                            if (pro_rootpath[pro_rootpath.Length - 1] != '/')
                                pro_rootpath += "/";
                            pro_rootpath = pro_rootpath + vcxpath;

                            System.Console.WriteLine(pro_rootpath.Replace("\\", "/"));

                            //===============================
                            string vcxlocation = pro_rootpath.Replace("\\", "/");
                            vcx_user = vcxlocation + ".user";
                            ParseVcxUser(vcx_user);
                            //================================
                            ParseVS(vcxlocation);
                            break;
                        }
                        sr.Close();
                    }
                }
            }
            return flog;
        }

        //tq end
        //==============================================================
        //==============================================================
        public Form1()
        {
            InitializeComponent();
            if (File.Exists(@"param.txt"))
            {

                StreamReader sr = new StreamReader("param.txt", Encoding.Default);
                String str;
                int cnt = 0;
                while ((str = sr.ReadLine()) != null)
                {
                    if (cnt == 0)
                    {
                        /*
                        int FLAG = str.IndexOf(" ");
                        String path2 = str.Substring(0, FLAG);
                        String path1 = str.Substring(FLAG + 1, str.Length - FLAG - 1);
                        textBox1.Text = path2;
                        textBox2.Text = path1;
                        cnt++;
                         */
                        textBox1.Text = str;
                    }
                    if (cnt == 1)
                    {
                        textBox2.Text = str;
                    }
                        cnt++;
                }
                sr.Close();
            }
            comboBox1.Items.Add("choose");
            comboBox1.Items.Add("make");
            comboBox1.Items.Add("VS");
            comboBox1.Items.Add("other");
            //设置默认选择项，DropDownList会默认选择第一项。
            comboBox1.SelectedIndex = 0;//设置第一项为默认选择项。
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.Description = "请选择文件路径";
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string folderName = folderBrowserDialog1.SelectedPath;
                if (folderName != "")
                {
                    textBox1.Text = folderName;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DirectoryInfo dir = new DirectoryInfo(this.textBox1.Text);
            if (!dir.Exists)
            {
                this.label5.Text = "工程路径不存在,请重新输入";
                this.label5.Refresh();
                return;
            }
            if (comboBox1.SelectedItem.ToString() == "VS")
            {
                    this.label5.Text= "正在解析VS工程....";
                    this.label5.Refresh();
               int flag =  findSlnAndVcxproj(this.textBox1.Text);
                switch(flag)
                {
                    case -1 :
                        this.label5.Text = "没有找到.sln文件，请输入正确VS路径";
                        this.label5.Refresh();
                        return;
                    case 0 :
                        this.label5.Text = "工程路径不存在,请输入正确";
                        this.label5.Refresh();
                        return;
                    case 1 :
                        this.label5.Text = "解析完成VS工程,正在添加头文件...";
                        this.textBox2.Text = "";
                        foreach (string include in alist)
                        {
                            this.textBox2.Text += include+";";
                        }
                        this.textBox2.Text = this.textBox2.Text.Substring(0, this.textBox2.Text.Length-1);
                        this.label5.Refresh();
                        //==============
                        if (File.Exists(@"include.txt"))
                        {
                            File.Delete(@"include.txt");
                        }
                        FileStream fs11 = new FileStream("include.txt", FileMode.Create, FileAccess.Write);//创建写入文件 
                        StreamWriter sw1 = new StreamWriter(fs11);
                        foreach (string include in alist)
                        {
                            sw1.WriteLine(include);
                        }
                        sw1.Close();
                        fs11.Close();
                        //===================
                        break;
                }
            }
            if (comboBox1.SelectedItem.ToString() == "make")
            {
                if (File.Exists(@"make.txt"))
                {
                    File.Delete(@"make.txt");
                }
                FileStream fs11 = new FileStream("make.txt", FileMode.Create, FileAccess.Write);//创建写入文件 
                StreamWriter sw1 = new StreamWriter(fs11);
                sw1.Write("delete me !");
                sw1.Close();
                fs11.Close();
            }
            if (File.Exists(@"param.txt"))
            {
                File.Delete(@"param.txt");
            }
            FileStream fs1 = new FileStream("param.txt", FileMode.Create, FileAccess.Write);//创建写入文件 
            StreamWriter sw = new StreamWriter(fs1);
            //sw.WriteLine(this.textBox1.Text.Trim() + " " + this.textBox2.Text);//开始写入值
            sw.WriteLine(this.textBox1.Text+" ");//开始写入值
            sw.Write(this.textBox2.Text);//开始写入值
            sw.Close();
            fs1.Close();
            /*
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            //p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            //p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();
            Console.WriteLine("clang-check-CurrentProject.bat " + textBox1.Text);
            p.StandardInput.WriteLine("clang-check-CurrentProject.bat " + textBox1.Text);
            p.StandardInput.WriteLine("clang-check-CurrentProject.bat " + textBox1.Text);
            p.StandardInput.WriteLine("exit");
            p.WaitForExit();
            p.Close();
            StreamReader sr = new StreamReader("C:\\clangBugInfo\\bug.csv", Encoding.Default);
            String str;
            int cnt = 0;
            while ((str = sr.ReadLine()) != null)
            {
                if (cnt == 0)
                {
                    cnt++;
                    continue;
                }
                int FLAG = str.IndexOf(":line")+6;
                int NUM = str.IndexOf(",", FLAG);
                int LOC = str.IndexOf("Location") + 9;
                int DES = str.IndexOf("Description")+12;
                String path = str.Substring(str.IndexOf(":")+1,str.IndexOf(",")-5);
                String num = str.Substring(FLAG,NUM-FLAG);
                String location = str.Substring(LOC, DES - LOC - 14);
                String description = str.Substring(DES);
                if(num.Length==4)
                    Console.WriteLine(path+"("+num+")\t\t"+location+"\t\t"+description);
                else if(num.Length==3)
                    Console.WriteLine(path + "(" + num + ") \t\t" + location + "\t\t" + description);
                else if(num.Length==2)
                    Console.WriteLine(path + "(" + num + ")  \t\t" + location + "\t\t" + description);
                else
                    Console.WriteLine(path + "(" + num + ")   \t\t" + location + "\t\t" + description);
            }
             sr.Close();
             */
             this.Close();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.Description = "请选择文件路径";
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string folderName = folderBrowserDialog1.SelectedPath;
                if (folderName != "")
                {
                    textBox2.Text = folderName;
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.Description = "请选择头文件路径";
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string folderName = folderBrowserDialog1.SelectedPath;
                if (folderName != "")
                {
                    if (textBox2.Text.Length != 0)
                        textBox2.Text +=";"+ folderName;
                    else
                        textBox2.Text += folderName;
                }
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click_1(object sender, EventArgs e)
        {

        }
    }
}
