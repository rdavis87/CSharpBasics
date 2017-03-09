//
// Project Name:  CSharpBasics         - Sample code showing some C# basic concepts.  The best way to learn a new 
//                                       language is to write code.  This is my first C# program.
//
// Copyright (C) <2017> <Robert A. Davis>
// All rights reserved.
//
// This software may be modified and distributed under the terms
// of the MIT license.  See the LICENSE.TXT file for details.
//

// symbol to determine whether the Male class overrides the Human WhatAmI function, uncomment it to have it override
#define OVERRIDE_MALE_WhatAmI

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;

namespace CSharpBasics
{
    class Program
    {
        static void Main(string[] args)
        {
            // start by printing out any command line arguments if there are any 
            if (args.Length > 0)
            {
                Console.WriteLine("Number of command line arguments is {0}", args.Length);
                foreach (var value in args)
                {
                    Console.WriteLine(value);
                }
            }
            else Console.WriteLine("no command line arguments");

            // output current application and user settings
            Console.WriteLine("Application setting");
            Console.WriteLine("AppSetting1 = {0}", CSharpBasics.Properties.Settings.Default.AppSetting1);
            Console.WriteLine("AppSetting2 = {0}", CSharpBasics.Properties.Settings.Default.AppSetting2);
            Console.WriteLine("AppSetting3 = {0}", CSharpBasics.Properties.Settings.Default.AppSetting3);
            Console.WriteLine("UserSetting1 = {0}", CSharpBasics.Properties.Settings.Default.UserSetting1);
            Console.WriteLine("UserSetting2 = {0}", CSharpBasics.Properties.Settings.Default.UserSetting2);
            Console.WriteLine("UserSetting3 = {0}", CSharpBasics.Properties.Settings.Default.UserSetting3);

            // increment the integer user setting each time run and save it.
            // the change should show up in the output next invocation of the program
            CSharpBasics.Properties.Settings.Default.UserSetting2++;
            Properties.Settings.Default.Save();

            // use a FileSystemWatcher object as an example of an event handler function
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName;
                // watch all files in the current directory
                watcher.Path = Directory.GetCurrentDirectory();
                watcher.Filter = "*.*";

                // add event handlers for create and delete files
                watcher.Created += new FileSystemEventHandler(OnChanged);
                watcher.Deleted += new FileSystemEventHandler(OnChanged);

                // begin watching
                watcher.EnableRaisingEvents = true;

                // test some directory functions
                MyDirectory md = new MyDirectory();
                md.TestDirectory();

                // perform text file i/o tests            
                MyTextFile mtf = new MyTextFile();
                mtf.TestTextFiles();

                // perform binary file i/o tests
                MyBinaryFile mbf = new MyBinaryFile();
                mbf.TestBinaryFiles();

                // end watching since we are done doing file I/O
                watcher.EnableRaisingEvents = false;
            }

            // perform class defintion test
            MyClassTest mct = new MyClassTest();
            mct.TestClasses();

            // perform some threading tests
            MyThreadTest mtt = new MyThreadTest();
            mtt.ThreadTest();
        }

        // define the event handler for the file system watcher
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // print file name and action done
            Console.WriteLine("\nfilewatcher File: " + e.Name + " " + e.ChangeType);
        }
    }
}

public class MyBinaryFile
{
    // class to define some data to be written to a binary file and read back
    public class Worker
    {
        public string name;
        public byte age;
        public uint height;
        public int weight;
        public double netWorth;
        public decimal salary;

        public Worker() { name = ""; age = 0; height = 0; weight = 0; netWorth = 0; salary = 0; }
        public void SetAttributes(string n, byte a, uint h, int w, double nw, decimal s) { name = n; age = a; height = h; weight = w; netWorth = nw; salary = s; }
        public void Display()
        {
            Console.WriteLine("Worker: name {0}, age {1}, height {2}\"{3}', weight {4}, net worth {5:C2}, salary {6:C2}\n",
                                name, age, height / 12, height % 12, weight, netWorth, salary);
        }

        public void Save(string fn)
        {
            try
            {
                // open the file for writing, creating it if it does not exist
                using (FileStream fs = new FileStream(fn, FileMode.OpenOrCreate | FileMode.Append, FileAccess.Write))
                {
                    fs.Seek(0L, SeekOrigin.End);  // opening with append attribute makes this unnecessary, but wanted to exercise Seek
                    // create a binary writer and write the object's data to the file
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        bw.Write(name);
                        bw.Write(age);
                        bw.Write(height);
                        bw.Write(weight);
                        bw.Write(netWorth);
                        bw.Write(salary);
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }

        public bool ReadNextWorker(BinaryReader br, out Worker w)
        {
            // w is output from this function so get a new worker and read in the data
            w = new Worker();
            try
            {
                w.name = br.ReadString();
                w.age = br.ReadByte();
                w.height = br.ReadUInt32();
                w.weight = br.ReadInt32();
                w.netWorth = br.ReadDouble();
                w.salary = br.ReadDecimal();
                return true;
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }

    // write data to a binary file and read it back using the Worker structure as an example
    public void TestBinaryFiles()
    {
        // write a few different object's to a binary file
        Console.WriteLine("\nwriting object data to binary file\n");
        string fileName = Constants.BinaryFileName;
        Worker w = new Worker();
        w.SetAttributes("Tommy", 23, 68, 180, 1234d, 45890.87m);
        w.Display();
        w.Save(fileName);
        w.SetAttributes("Jenny", 45, 66, 130, 123456d, 67426.32m);
        w.Display();
        w.Save(fileName);
        w.SetAttributes("Billy", 55, 72, 250, 1234567d, 120675.00m);
        w.Display();
        w.Save(fileName);

        // now read the data from the binary file
        Console.WriteLine("\nreading object data from binary file\n");
        try
        {
            // open the file for reading
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                // get a binary reader to read and display the data
                using (BinaryReader br = new BinaryReader(fs))
                {
                    Worker w1;
                    while (fs.Position != fs.Length)
                    {
                        w.ReadNextWorker(br, out w1);
                        w1.Display();
                    }
                }
            }
        }
        catch (IOException e)
        {
            Console.WriteLine(e.Message);
            return;
        }
        // delete the binary file
        Console.WriteLine("\ndeleting binary file\n");
        File.Delete(fileName);
    }
}

public class MyTextFile
{
        // Exercise the built in file objects to process a text file
        public void TestTextFiles()
        {
        Console.WriteLine("\ncreating text file");
        // create a text file and write some data to it
        string fileName = Constants.TextFileName;
        using (FileStream fs = new FileStream(fileName, FileMode.Create))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                for (int i = 0; i < 5; i++)
                {
                   sw.WriteLine("this is line number {0} text", i);
                }
            }
        }

      // now open the file for reading and read the text back out
      using (StreamReader sr = new StreamReader(fileName))
        {
            string line;
            for(int i = 0; i< 5; i++)
            {
                while((line = sr.ReadLine()) != null) 
                {
                    Console.WriteLine(line);
                }
            }
        }

        // copy text.txt to a temp file
        string tempFile = Path.GetRandomFileName();
        Console.WriteLine("\ncopying text from {0} to a temp file {1}", fileName, tempFile);
        File.Copy(fileName, tempFile);

        Console.WriteLine("\nadding text to temp file");
        // open temp file for append to put new text at the end
        using (FileStream fs = new FileStream(tempFile, FileMode.Append))
        {
             using (StreamWriter sw = new StreamWriter(fs))
             {
                 for (int i = 5; i < 11; i++)
                 {
                     sw.WriteLine("this is line number {0} text", i);
                 }
             }
         }

        // now open the file for reading and read the text back out
        Console.WriteLine("\nreading text from temp file");
        using (StreamReader sr = new StreamReader(tempFile))
        {
            string line;
            for (int i = 0; i < 5; i++)
            {
                while ((line = sr.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }
        }

        // delete the text files
        File.Delete(fileName);
        File.Delete(tempFile);
        Console.WriteLine("\nfiles deleted");
    }
}

public class MyDirectory
{
    // Function to get all the subdirectories of the directory in di
    void EnumerateSubDirectories(DirectoryInfo di)
    {
        Console.WriteLine("\nEnumerating sub directories");
        DirectoryInfo[] diArr = di.GetDirectories();
        if (diArr.Count() > 0)
        {
            foreach (DirectoryInfo dri in diArr)
            {
                Console.WriteLine("directory : " + dri.Name);
            }
        }
        else Console.WriteLine("\nno sub directories found");
    }

    // Function to get all the files of the directory in di
    void EnumerateFiles(DirectoryInfo di)
    {
        Console.WriteLine("\nEnumerating files");
        FileInfo[] fiArr = di.GetFiles();
        if (fiArr.Count() > 0)
        {
            foreach (FileInfo fi in fiArr)
            {
                Console.WriteLine("file : {0}, created {1}, Length is {2:N0} bytes", fi.Name, fi.CreationTime, fi.Length);
            }
        }
        else Console.WriteLine("\nno files found");
    }

    // function to exercise some of the directory functions
    public void TestDirectory()
    {
        // get he current directory and create a DirectoryInfo structure with it
        string path = Directory.GetCurrentDirectory();
        Console.WriteLine(path);

        // get the directory info for the current directory
        DirectoryInfo di = new DirectoryInfo(path);

        // print any directories that exist
        EnumerateSubDirectories(di);

        // create a subdirectory and show that it was created
        string subDir = "TestDirectory";
        Console.WriteLine("\ncreating subdirectory " + subDir);
        di.CreateSubdirectory(subDir);
        EnumerateSubDirectories(di);

        // delete the directory and show it was deleted
        string subDirPath = path + "\\" + subDir;
        DirectoryInfo di2 = new DirectoryInfo(subDirPath);
        Console.WriteLine("\ndeleting subdirectory " + subDir);
        di2.Delete();
        EnumerateSubDirectories(di);

        // display the files in this directory
        EnumerateFiles(di);
    }
}

// interface to make sure all objects inheriting Human have display functions
public interface IHuman
{
    // interface members
    void Print();
    void WhatAmI();
}

// the following classes are not intended to be a "logical" design
// but to make sure I am familiar with the most common things that
// can be done with c# classes

// abstract class to define some basic human attributes 
public abstract class Human : IHuman
{
    // show use of various access specifiers 
	public string name;    // public since names can be legally changed to anything
    private int height;    // private but with a method to allow change as person grows
    protected int weight;  // protected just to show use, this can be set by class inheriting from Human
    public Constants.EyeColor EyeColor { get; }  // property with only a get function, cannot be changed after creation
    // PascalCase for properties seems to be the standard
    private Constants.HairColor hairColor;  // internal variable to hold property value because of overriding default get/set functions to verify validity
    public Constants.HairColor HairColor // property to show the use of them
    {
        get
        {
            return hairColor;
        }
        set
        {   // make sure hair color is valid
            if (value == Constants.HairColor.Blonde || value == Constants.HairColor.Brunette || value == Constants.HairColor.Red)
                hairColor = value;
            else
                throw new Exception("Invalid hair color");
        }
    }

    // constructor with default values
    public Human(string n = "noname", int h = 68, int w = 140, Constants.HairColor hc = Constants.HairColor.Brunette, Constants.EyeColor ec = Constants.EyeColor.Brown)
    {
        name = n; height = h; weight = w;
        HairColor = hc;  // uses the property set function to validate data
        // validate eye color is valid
        if (ec == Constants.EyeColor.Blue || ec == Constants.EyeColor.Brown || ec == Constants.EyeColor.Green) EyeColor = ec;
        else throw new Exception("Invalid eye color");
    }

    public void SetHeight(int h) { height = h; Console.WriteLine("setting height from Human class"); } // base function
    public virtual void SetWeight(int w) { weight = w; Console.WriteLine("setting weight from Human class"); }  // function can be overridden
    public int GetHeight() { return height; }
  //  public Constants.EyeColor GetEyeColor() { return eyeColor; }

    public override string ToString()
    {
        return string.Format("name = {0}, Height = {1}'{2}\", weight = {3} lbs., hair color = {4}, \neye color = {5}",
                          name, height / 12, height % 12, weight, Constants.HairColorStrings[(int)HairColor], Constants.EyeColorStrings[(int)EyeColor]);
    }

    // required interface functions can be overridden
    public virtual void Print()
    {
        Console.Write(ToString());
    }
    public virtual void WhatAmI()
    {
        Console.WriteLine("I am human");
    }
}

public class Female : Human
{
    private decimal shoeSize;
    private int dressSize;
    // the constructor calls the base class constructor then sets the object specific fields
    public Female(string n, int h, int w, Constants.HairColor hc, Constants.EyeColor ec, decimal ss = 8, int ds = 8) : base(n, h, w, hc, ec)
    {
        // make sure shoe size if either a whole number or includes only .5 for half sizes
        decimal d = ss - Math.Truncate(ss);  // validate shoe size is even or half size
        if(d == 0 || d == .5M)
            shoeSize = ss;
        else
            throw new Exception("Invalid shoe size");
        dressSize = ds;
    }

    public void SetAttributes(string n, int h, int w, Constants.HairColor hc, decimal ss, int ds)  // cannot set eyecolor
    {
        name = n;           // public so we can access it here
        SetHeight(h);       // private must use human function to set it
        weight = w;         // protected so we can access it here 
        HairColor = hc;     // property so this ends up calling the set function
        shoeSize = ss;
        dressSize = ds;
    }

    public void GetAttributes(out string n, out int h, out int w, out Constants.HairColor hc, out Constants.EyeColor ec, out decimal ss, out int ds)
    {
        n = name;               // public so can access directly
        h = GetHeight();        // private so need access function
        w = weight;             // protected so can access directly
        hc = HairColor;         // property ends up calling the get function
        ec = EyeColor;          // property ends up calling the get function
        ss = shoeSize;
        ds = dressSize;
    }

    public override string ToString()
    {
        // call the base ToString function first then format the object specific fields
        string s = base.ToString();
        return s + string.Format(", shoe size = {0}, dress size = {1}", shoeSize, dressSize);
    }

    // required interface functions are overrides of the base
    public override void Print()
    {
        WhatAmI();
        Console.WriteLine(ToString());
    }

    public override void WhatAmI()
    {
        Console.WriteLine("I am female");
    }
}

public class Male : Human
{
    private decimal shoeSize;
    private int coatSize;
    // the constructor calls the base class constructor then sets the object specific fields
    public Male(string n, int h, int w, Constants.HairColor hc, Constants.EyeColor ec, decimal ss = 8, int cs = 42) : base(n, h, w, hc, ec)
    {
        // make sure shoe size if either a whole number or includes only .5 for half sizes
        decimal d = ss - Math.Truncate(ss);
        if (d == 0 || d == .5M)  // validate shoe size is even or half size
            shoeSize = ss;
        else
            throw new Exception("Invalid shoe size");
        coatSize = cs;
    }

    public void SetAttributes(string n, int h, int w, Constants.HairColor hc, decimal ss, int cs)  // cannot set eyecolor
    {
        name = n;           // public so we can access it here
        SetHeight(h);       // private must use human function to set it
        weight = w;         // private so we can access it here 
        HairColor = hc;     // property so this ends up calling the set function
        shoeSize = ss;
        coatSize = cs;
    }
    public void GetAttributes(out string n, out int h, out int w, out Constants.HairColor hc, out Constants.EyeColor ec, out decimal ss, out int cs)
    {
        n = name;               // public so can access directly
        h = GetHeight();        // private so need access function
        w = weight;             // protected so can access directly
        hc = HairColor;         // property ends up calling the get function
        ec = EyeColor;          // property ends up calling the get function
        ss = shoeSize;
        cs = coatSize;
    }
    public override string ToString()
    {
        // call the base ToString function first then format the object specific fields
        string s = base.ToString();
        return s + string.Format(", shoe size = {0}, coat size = {1}", shoeSize, coatSize);
    }

    // required interface functions are overrides of the base
    public override void Print()
    {
        WhatAmI();
        Console.WriteLine(ToString());
    }

#if OVERRIDE_MALE_WhatAmI   // #define statement at top to change how the male object works
    public override void WhatAmI()
    {
        Console.WriteLine("I am male");
    }
#endif
}

// the person object doesn't inherit anything, but creates instances of the female, or male object as needed
public class Person
{
    private int numKids;
    private bool married;
    private int idNum;  // sort key
    private static int nextIdNum = 1;  // give every person object a unique number for sorting
    private object gender;  // will be female or male set at instantiation
 
    public Person(Constants.Gender g, string n, int h, int w, Constants.HairColor hc, Constants.EyeColor ec, decimal ss, int dscs, bool m = false, int nk = 0)
    {
        // create a gender based object
        switch(g)
        {
            case Constants.Gender.Female:
                gender = new Female(n, h, w, hc, ec, ss, dscs);
                break;
            case Constants.Gender.Male:
                gender = new Male(n, h, w, hc, ec, ss, dscs);
                break;
            default:
                throw new Exception("invalid gender");
        }
        // set person specific fields
        married = m;
        numKids = nk;
        // get a new id
        idNum = nextIdNum++;
    }

    public void GetAttributes(out string n, out int h, out int w, out Constants.HairColor hc, out Constants.EyeColor ec, out decimal ss, out int dscs, out int nk, out bool m)
    {
        // call gender specific get function, human does not have get and set attribute functions so the gender
        // specific versions must be called unlike the WhatAmI and Print functions which are virtual functions in human
        Female f = gender as Female;
        if (f != null)
        {
            f.GetAttributes(out n, out h, out w, out hc, out ec, out ss, out dscs);
        }
        else
        {
            Male ma = gender as Male;
            ma.GetAttributes(out n, out h, out w, out hc, out ec, out ss, out dscs);
        }
        // get person specific fields
        nk = numKids;
        m = married;
    }

    public void SetAttributes(string n, int h, int w, Constants.HairColor hc, decimal ss, int dscs, int nk, bool m)
    {
        // call gender specific set function, human does not have get and set attribute functions so the gender
        // specific versions must be called unlike the WhatAmI and Print functions
        Female f = gender as Female;
        if (f != null)
        {
            f.SetAttributes(n, h, w, hc, ss, dscs);
        }
        else
        {
            Male ma = gender as Male;
            ma.SetAttributes(n, h, w, hc, ss, dscs);
        }
        // set person specific fields
        numKids = nk;
        married = m;
    }

    public int GetIdNum() { return idNum; }

    // these functions are not required as Person does not inherit the human interface 
    public void WhatAmI()
    {
        Human h = gender as Human;
        Console.Write("person: ");
        h.WhatAmI();    // virtual function so this will call the gender specic WhatAmI function

    }

    public override string ToString()
    {
        string s = "";
        if (married == true)
            s = string.Format("I am married with {0} kids", numKids);
        else
            s = string.Format("I am unmarried with {0} kids", numKids);
        s += string.Format(", my id number is {0}\n", idNum);
        return s;
    }

    public void Print()
    {
        Human h = gender as Human;
        h.Print();    // virtual function so this will call the gender specic print function
        // print person specific data
        Console.WriteLine(this.ToString());
    }
}

public class MyClassTest
{
    // this class will perform some tests with the human based classes 
    public void TestClasses()
    {
        // catch any exceptions thrown by the human based classes
        try
        {
            string name;
            int height;
            int weight;
            Constants.EyeColor eyeColor;
            Constants.HairColor HairColor;
            decimal shoeSize;
            int dressSize;
            int coatSize;
            bool married;
            int numKids;

            // test female object
            Female f = new Female("Sandra", 66, 120, Constants.HairColor.Blonde, Constants.EyeColor.Blue, 7.5M, 6);
            f.Print();
            Console.WriteLine("\nchanging name");
            f.name = "Debbie";  // show access to public name
            f.Print();
            Console.WriteLine("\nchanging weight and HairColor");
            f.GetAttributes(out name, out height, out weight, out HairColor, out eyeColor, out shoeSize, out dressSize);
            f.SetAttributes(name, height, 110, Constants.HairColor.Brunette, shoeSize, dressSize);
            f.Print();
            Console.WriteLine();

            // test male object
            Male m = new Male("Tom", 76, 220, Constants.HairColor.Blonde, Constants.EyeColor.Brown, 11M, 44);
            m.Print();
            Console.WriteLine("\nchanging name");
            m.name = "Charlie";  // show access to public name
            m.Print();
            Console.WriteLine("\nchanging weight and HairColor");
            m.GetAttributes(out name, out height, out weight, out HairColor, out eyeColor, out shoeSize, out coatSize);
            m.SetAttributes(name, height, 210, Constants.HairColor.Brunette, shoeSize, coatSize);
            m.Print();
            Console.WriteLine();

            // test person object
            Person p = new Person(Constants.Gender.Male, "Jerry", 74, 230, Constants.HairColor.Red, Constants.EyeColor.Brown, 10M, 46, true, 3);
            p.Print();
            p.GetAttributes(out name, out height, out weight, out HairColor, out eyeColor, out shoeSize, out coatSize, out numKids, out married);
            Console.WriteLine("changing weight and marrital status");
            p.SetAttributes(name, height, 240, Constants.HairColor.Brunette, shoeSize, coatSize, numKids, false);
            p.Print();
            Person p2 = new Person(Constants.Gender.Female, "Gail", 68, 130, Constants.HairColor.Red, Constants.EyeColor.Blue, 8.5M, 10, false, 0);
            p2.Print();
            Person p3 = new Person(Constants.Gender.Female, "Jennie", 62, 90, Constants.HairColor.Blonde, Constants.EyeColor.Green, 7M, 6, true, 2);
            p3.Print();
            Person p4 = new Person(Constants.Gender.Male, "David", 71, 250, Constants.HairColor.Brunette, Constants.EyeColor.Blue, 11M, 48, true, 1);
            p4.Print();

            // test binary tree functions
            Console.WriteLine("\ncreating binary tree\n");
            SimpleBTree<int, Person> tree = new SimpleBTree<int, Person>();
            tree.Add(p2.GetIdNum(), p2);  // add to b tree in unsorted order
            tree.Add(p.GetIdNum(), p);
            tree.Add(p4.GetIdNum(), p4);
            tree.Add(p3.GetIdNum(), p3);
            if (tree.Add(p3.GetIdNum(), p3) == false)
                Console.WriteLine("correctly could not add node already in tree\n");
            List<Person> list = tree.GetTree();
            foreach(Person ps in list)
            {
                ps.Print();
            }
            Console.WriteLine("\ngetting person with id 3 from the binary tree\n");
            p = tree.GetByKey(3);
            p.Print();

            Console.WriteLine("\nreversing binary tree\n");
            tree.Reverse();
            list = tree.GetTree();
            foreach(Person ps in list)
            {
                ps.Print();
            }

            Console.WriteLine("\ngetting person with id 1 from the binary tree\n");
            p = tree.GetByKey(1);
            p.Print();

            Console.WriteLine("\ndeleting person with id 2 from the binary tree\n");
            tree.Delete(2);
            list = tree.GetTree();
            foreach (Person ps in list)
            {
                ps.Print();
            }
            Console.WriteLine("\ndeleting person with id 3 from the binary tree\n");
            tree.Delete(3);
            list = tree.GetTree();
            foreach (Person ps in list)
            {
                ps.Print();
            }
            Console.WriteLine("\ndeleting person with id 4 from the binary tree\n");
            tree.Delete(4);
            list = tree.GetTree();
            foreach (Person ps in list)
            {
                ps.Print();
            }
            Console.WriteLine("\ndeleting person with id 1 from the binary tree\n");
            tree.Delete(1);
            list = tree.GetTree();
            foreach (Person ps in list)
            {
                ps.Print();
            }

        }
        catch (Exception e)
        {
            Console.WriteLine("exception caught {0}", e.Message);
        }
    }
}

// class implements a generic binary tree, the key parameter must be comparable
public class SimpleBTree<TKey, TValue> where TKey : IComparable
{
    // tree node structure 
    private class Node
    {
        public TKey key;
        public TValue value;
        public Node lLink;
        public Node rLink;
        // make sure links are null to start with
        public Node(TKey k, TValue v) { key = k; value = v; lLink = null; rLink = null; }
    }
    private Node head;      // base of tree
    private bool reversed;  // tracks which way the tree is sorted

    public SimpleBTree() { head = null; reversed = false; }  // default constructor
    public SimpleBTree(TKey k, TValue v) { head = new Node(k, v); reversed = false; } // allow creation with first node

    public bool Add(TKey k, TValue v)  // public function to add to tree
    {
        return AddNode(ref head, k, v);
    }

    private bool AddNode(ref Node n, TKey k, TValue v)  // internal function that performs the add function
    {
        if (n == null)
        {
            // at end of branch add the node to the tree
            n = new Node(k, v);
            return true;
        }
        else
        {
            // traverse tje tree recuresively to find the place to add
            int i = k.CompareTo(n.key);
            if (i == 0)
                return false; // node already exists return error
            else if (i == -1)
                return AddNode(ref n.lLink, k, v);  // add to left side of tree
            else
                return AddNode(ref n.rLink, k, v);  // add to right side of tree
        }
    }

    public void Reverse()           // publice function to reverse the tree
    {
        ReverseLinks(ref head);
        reversed = !reversed;       // indicate tree was reversed
    }

    private void ReverseLinks(ref Node n)   // internal function to perform the reversal
    {
        if (n.lLink != null)
            ReverseLinks(ref n.lLink);  // recverse the left branch
        if (n.rLink != null)
            ReverseLinks(ref n.rLink);  // reverse the right branch
        Node temp;  // now reveree the links on the current node
        temp = n.rLink;
        n.rLink = n.lLink;
        n.lLink = temp;
    }

    public List<TValue> GetTree()           // publice function to put the tree node values into a list
    {
        List<TValue> tree = new List<TValue>();
        if(head != null)
            GetTreeNode(tree, head);
        return tree;
    }

    private void GetTreeNode(List<TValue> t, Node n)        // private function to do the work of getting the nodes
    {
        // use recursion to get all the nodes
        if (n.lLink != null)
            GetTreeNode(t, n.lLink);
        t.Add(n.value);
        if (n.rLink != null)
            GetTreeNode(t, n.rLink);
    }

    public TValue GetByKey(TKey k)      // public function to get a particular value by its key
    {
        return FindKey(head, k);
    }

    private TValue FindKey(Node n, TKey k)          // internal function to traverse the tree to find the key
    {
        // traverse the tree looking for the key value
        if (n == null)
            return default(TValue);
        int i = k.CompareTo(n.key);
        if (i == 0)  // found match return the data
            return n.value;
        // check which way the tree is sorted so the search goes out the correct branch
        if (reversed == false)
        {
            if (i < 0)
                return FindKey(n.lLink, k);
            else
                return FindKey(n.rLink, k);
        }
        else
        {
            if (i > 0)
                return FindKey(n.lLink, k);
            else
                return FindKey(n.rLink, k);
        }
    }

    public bool Delete(TKey k)          // delete a node based on key
    {
        return DeleteNode(ref head, ref head, k);
    }

    private bool DeleteNode(ref Node parent, ref Node n, TKey k)        // internal function to do the deletion
    {
        if (n == null)
            return false;
        int i = k.CompareTo(n.key);
        if (i == 0)
        {
            // if no branches delete the leaf node
            if (n.lLink == null && n.rLink == null)
            {
                if (parent == n)  // check special case of deleting only node in the list
                    parent = null;
                else if (parent.lLink == n)  // determine if left branch of parent led us here
                    parent.lLink = null;
                else
                    parent.rLink = null;  // must have gotten here from right branch
            }
            else if (n.lLink != null && n.rLink == null)
            { // only left branch so copy date from that branch whcich effectively deletes the node
                n.key = n.lLink.key;
                n.value = n.lLink.value;
                n.rLink = n.lLink.rLink;
                n.lLink = n.lLink.lLink;

            }
            else if (n.lLink == null && n.rLink != null)
            { // only right branch so copy date from that branch whcich effectively deletes the node
                n.key = n.rLink.key;
                n.value = n.rLink.value;
                n.lLink = n.rLink.lLink;
                n.rLink = n.rLink.rLink;
            }
            else  // both links have branches, find a leaf node in right branch to replace deleted node
            {
                Node curNode, saveNode;
                curNode = n.rLink;
                saveNode = n;
                while (curNode.lLink != null)
                {
                    saveNode = curNode;  // save parent of curNode
                    curNode = curNode.lLink;
                }
                // curNode should now be the leaf node to replace the deleted node
                n.key = curNode.key;
                n.value = curNode.value;
                if (saveNode == n)
                {   // if the node to replace the deleted node is the first node in right branch just move up the link
                    n.rLink = curNode.rLink;
                }
                else
                {   // otherwise move the right branch of the node to replace deleted node to the parents left branch
                    saveNode.lLink = curNode.rLink;
                }
            }
            return true;
        }
        else
        {
            // current node not a match traverse the branches based on which way the tree is sorted
            if (reversed)
            {
                if(i > 0)
                    return DeleteNode(ref n, ref n.lLink, k);
                else
                    return DeleteNode(ref n, ref n.rLink, k);
            }
            else
            {
                if (i > 0)
                    return DeleteNode(ref n, ref n.rLink, k);
                else
                    return DeleteNode(ref n, ref n.lLink, k);
            }
        }
    }
}

public class MyThreadTest
{
    static int threadNum = 1;   // count of threads launched
    static readonly object lockObj = new object();  // used to lock resources in a thread
    // parameters to be passed to thread
    static string threadS = "static parameter string";
    static int threadI = 1;

    static void SimpleThreadFunc()  // static function used as a new thread
    {
        // acquire the lock before continuing on
        Console.WriteLine("acquiring lock");
        Thread.Sleep(200);  // take a short nap to let the other threads print their acquiring messages
        lock (lockObj)
        {
            Console.WriteLine("SimpleThreadFunc: thread number {0} started", threadNum++);
            // print current parameters from static fields
            Console.WriteLine("SimpleThreadFunc passed parameters: {0}, {1}", threadS, threadI);
        }
    }

    class Parameters
    {
        public int param1;
        public double param2;
        public string param3;
        public Parameters(int p1, double p2, string p3) { param1 = p1;  param2 = p2; param3 = p3; }
    }

    static void ParameterizedThreadFunc(object o)
    {
        Console.WriteLine("ParameterizedThreadFunc: thread number {0} started", threadNum++);
        Parameters p = o as Parameters;
        if(p != null)
            Console.WriteLine("parameters passed to thread function were {0}, {1}, {2}", p.param1, p.param2, p.param3);
    }

    // Passing parameters to a thread can be done by creating a class with fields for 
    // the paramters and function for the thread.  This class also uses an event to signal
    // when the thread completes and sets a return code from the thread
    public class MyThread : IDisposable
    {
        public int param1;
        public double param2;
        public string param3;
        // this system resource means this class should implement IDisposable
        public AutoResetEvent signal; // will be initialized to non signalled state
        public int retCode;
        // Flag: Has Dispose already been called?
        bool disposed = false;

        public MyThread(int i, double d, string s) { param1 = i; param2 = d; param3 = s; signal = new AutoResetEvent(false); } 

        public void MyThreadFunc()
        {
            Console.WriteLine("MyThreadFunc: thread number {0} started", threadNum++);
            Console.WriteLine("parameters passed to thread function were {0}, {1}, {2}", param1, param2, param3);
            retCode = param1;  // pass back a return code which in this case is the same as the int param passed in
            signal.Set();  // signal thread ending
        }
        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                signal.Dispose();
                Console.WriteLine("signal has been disposed");
            }
            // indicate disposed has been called
            disposed = true;
        }
    }

    static void ThreadPoolFunc(object parameter)  // function to be executed by thread pool
    {
        Console.WriteLine("ThreadPoolFunc: thread number {0} started", threadNum++);
        Console.WriteLine("parameter passed was {0}", parameter);
    }

    public void ThreadTest()
    {
        // start a thread using a static function
        ThreadStart simpleThread = new ThreadStart(SimpleThreadFunc);
        for (int i = 1; i < 6; i++)
        {
            // pass parameters to thread through static variables
            // useful to start 1 or more threads with the same parameters
            Thread thread = new Thread(simpleThread);
            // quickly fire off the threads, they will use the lock keyword to execute one at a time
            thread.Start();      // start a new thread
        }
        Thread.Sleep(1000);  // let the locking threads complete    
        Console.WriteLine();

        // pass parameters to thread using the parameterized method 
        for (int i = 1; i < 6; i++)
        {
            Thread pts = new Thread(new ParameterizedThreadStart(ParameterizedThreadFunc));
            pts.Start(new Parameters(i + 10, i * 10.3d, string.Format("parameter string{0}", i)));
            Thread.Sleep(500);  // let the thread run now
        }
        Console.WriteLine();

        // start a thread with parameters using a class provides an easy way to start threads with
        // a different set of parameters
        for (int i = 1; i < 6; i++)
        {
            using (MyThread mt = new MyThread(i, i * 2.5d, string.Format("string{0}", i)))
            {
                ThreadStart classThread = new ThreadStart(mt.MyThreadFunc);
                Thread thread = new Thread(classThread);
                thread.Start();      // start a new thread
                mt.signal.WaitOne();  // let the thread signal when done
                Console.WriteLine("thread return code was {0}", mt.retCode);
                mt.signal.Dispose(); // free up the event handle
            }
        }
        Console.WriteLine();

        // the .net framework provides a pool of threads ready to run skipping the overhead
        // of thread creation and can be used for small amounts of work to be done
        ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolFunc), "hello there");
        Thread.Sleep(1000);
        ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolFunc), 10);
        Thread.Sleep(1000);
        ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolFunc), 10.2d);
        Thread.Sleep(1000);
    }
}

// place to collect constants used in this program
public class Constants
{
    public const string TextFileName = "test.txt";
    public const string BinaryFileName = "object.dat";
    public enum HairColor { Blonde, Brunette, Red }; 
    public static readonly string[] HairColorStrings = { "Blonde", "Brunette", "Red" };
    public enum EyeColor { Blue, Green, Brown };
    public static readonly string[] EyeColorStrings = { "Blue", "Green", "Brown" };
    public enum Gender { Female, Male };
}