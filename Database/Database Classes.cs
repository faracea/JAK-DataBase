﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
namespace Databaser
{

    #region Database Manager
    public class DatabaseManager
    {
        Database TheDatabase = new Database();
        public string LastSave
        {
            get
            {
                return Converter.ByteToString(TheDatabase.DateOfLastSave, typeof(DateTime));
            }
        }
        public DatabaseManager()
        {
            if (!Directory.Exists(Database.FolderPath))
                Directory.CreateDirectory(Database.FolderPath);
            bool o = true;
            bool bts = false;
            #region Level 1
            while (o)
            {
                TheDatabase = new Database();
                Console.WriteLine("Hello and welcome to JAK Database! Pick one of the following to begin...:");//intro
                int res = CPExtensionMethods.TryToAskQuestion("0.Load Database\n1.New Database\n2.Import Database From .csv File\n3.Close", 3);
                Console.Clear();
                switch (res)
                {
                    case 0:

                        TheDatabase = Load_Database();
                        if (TheDatabase == null) bts = true;
                        break;
                    case 1:
                        //int dec = CPExtensionMethods.TryToAskQuestion("Would you like to create a new database from an existing csv file or through our quick entry system? (1 for csv file/0 for quick entry)", 1);
                        TheDatabase = AskNewDatabase();
                        break;
                    case 2:
                        try
                        {
                            if (TheDatabase.Read_csv_IntoDatabase())
                            {
                                Console.Clear(); View_Database(TheDatabase, TheDatabase.Data[0].Data.Count); Console.WriteLine("Press any key to continue...");
                                Console.ReadKey();
                                Console.Clear();
                            }
                            else { Console.Clear(); continue; }
                        }
                        catch (Exception)
                        {
                            Console.Clear();
                            Console.WriteLine("Unable to Import due to wrong formating or other issues");
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();
                            Console.Clear();
                            continue;
                        }

                        break;
                    case 3:
                        o = false;
                        break;

                }
                if (bts) { bts = false; continue; }
                if (res == 0 || res == 1 || res == 2)
                {
                    Console.Clear();
                    bool o2 = true;
                    #region Level 2
                    while (o2)
                    {
                        Console.WriteLine($"You are running JAK Database Solutions\nWelcome to {TheDatabase.FileName}!" +
                        $"\nThe last edit was made {LastSave}.\n");
                        int resl2 = CPExtensionMethods.TryToAskQuestion("0.Display\n1.Edit,Add,Remove\n2.Query\n3.Save\n4.Close", 4);
                        Console.Clear();
                        switch (resl2)
                        {
                            case 0:
                                if (TheDatabase.Data.Count == 0)
                                {
                                    Console.WriteLine("No columns have been added to this database yet, so there is nothing to view.\nPick another option\nPlease press any key to continue....");
                                    Console.ReadKey();
                                    Console.Clear();
                                    break;
                                }
                                View_Database(TheDatabase, TheDatabase.Data[0].Data.Count);
                                Console.WriteLine("Press any key to continue...");
                                Console.ReadKey();
                                Console.Clear();

                                break;
                            case 1:
                                MasterEditor();
                                Console.Clear();
                                break;
                            case 2:
                                if (TheDatabase.HasARecord)
                                {
                                    QueryCopyOfDatabase();
                                }
                                else
                                {
                                    Console.WriteLine("No columns have been added to this database yet, so there is nothing to view.\nPick another option");

                                    Console.WriteLine("Press any key...");
                                    Console.ReadKey();
                                    Console.Clear();
                                }
                                break;
                            case 3:
                                if (CPExtensionMethods.TryToAskQuestion("Insert 0 for Save, 1 for Save as:", 1) == 1)
                                {

                                    Console.WriteLine("Input a file name to store the database: ");
                                    string newFilePath = Console.ReadLine();
                                    if (!newFilePath.Contains(".bin")) newFilePath += ".bin";
                                    try
                                    {
                                        if (!File.Exists(Database.FolderPath + "\\" + newFilePath))
                                        {
                                            TheDatabase.FilePath = (Database.FolderPath + "\\" + newFilePath);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Invalid Path ... Reprompting");
                                            continue;
                                        }
                                    }
                                    catch
                                    {
                                        Console.WriteLine("Invalid Path ... Reprompting");
                                        continue;
                                    }
                                }
                                TheDatabase.SaveDatabase();
                                Console.WriteLine("Save complete");
                                Console.WriteLine("Press any key...");
                                Console.ReadKey();
                                Console.Clear();
                                break;
                            case 4:
                                o2 = false;
                                break;

                        }
                    }
                    #endregion
                }
            }
            #endregion
        }

        private void QueryCopyOfDatabase()

        {
            Database copy = TheDatabase.CopyDatabase();
            bool o = true;
            #region Level 3
            while (o)
            {
                int res = CPExtensionMethods.TryToAskQuestion("0.Sort\n1.Filter\n2.Display\n3.Save File\n4.Replace Database\n5.Return to Menu", 6);
                Console.Clear();
                switch (res)
                {
                    case 0:
                        #region Sorter
                        for (int col = 0; col < copy.Data.Count; col++)
                        {
                            Console.WriteLine($"{col}.{copy[col].Name}");
                        }
                        int column = CPExtensionMethods.TryToAskQuestion("", copy.Data.Count);
                        copy.ApplyPattern(copy[column].GenerateSortPattern((SortStyle)
                            CPExtensionMethods.TryToAskQuestion("0.Ascending\n1.Descending", 2)));
                        Console.Clear();
                        #endregion
                        break;
                    case 1:
                        #region Filterer
                        for (int col = 0; col < copy.Data.Count; col++)
                        {
                            Console.WriteLine($"{col}.{copy[col].Name}");
                        }
                        int columnfilter = CPExtensionMethods.TryToAskQuestion("", copy.Data.Count);
                        FilterStyle res2 = (FilterStyle)CPExtensionMethods.TryToAskQuestion("0.Equal To\n1.Greater Than Or Equal To\n2.Less Than Or Equal To\n3.Greater Than\n4.Less Than\n5 Not Equal To", 5);
                        Console.WriteLine("Please insert your comparetor");
                        byte[] input = RequestData(copy[columnfilter].type);
                        copy.ApplyPattern(copy[columnfilter].GenerateFilterPattern(res2, new Record(input, copy[columnfilter].type)));
                        Console.Clear();
                        #endregion
                        break;
                    case 2:
                        #region Display
                        View_Database(copy, CPExtensionMethods.TryToAskQuestion($"How many records would you like to view from the database out of {copy.Data[0].Data.Count}?", copy.Data[0].Data.Count));
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        Console.Clear();
                        #endregion
                        break;
                    case 3:
                        #region Save File
                        if (CPExtensionMethods.TryToAskQuestion("Insert 0 for Save, 1 for Save as:", 1) == 1)
                        {

                            Console.WriteLine("Input a file name to store the database: ");
                            string newFilePath = Console.ReadLine();
                            if (!newFilePath.Contains(".bin")) newFilePath += ".bin";
                            try
                            {
                                if (!File.Exists(Database.FolderPath + "\\" + newFilePath))
                                {
                                    copy.FilePath = (Database.FolderPath + "\\" + newFilePath);
                                }
                                else
                                {
                                    Console.WriteLine("Invalid Path ... Reprompting");
                                    continue;
                                }
                            }
                            catch
                            {
                                Console.WriteLine("Invalid Path ... Reprompting");
                                continue;
                            }
                        }
                        copy.SaveDatabase();
                        Console.WriteLine("Save complete");
                        Console.WriteLine("Press any key...");
                        Console.ReadKey();
                        Console.Clear();
                        #endregion
                        break;
                    case 4:
                        #region Replace Database
                        TheDatabase = copy;
                        Console.WriteLine("Database replaced");
                        #endregion
                        break;
                    case 5:
                        o = false;
                        break;

                }
            }
            #endregion
        }

        private byte[] RequestData(Type type)
        {
            byte[] output;
            while (!Converter.TryParseToByte(Console.ReadLine(), type, out output))
            {
                Console.WriteLine("Invalid Input, Reprompting...");
            }
            return output;
        }
        /// <summary>
        /// returns the length of the longest record of the column specified by the zero-based column number argument
        /// </summary>
        /// <param name="ColNum"></param>
        /// <returns></returns>
        private int LengthOfLongestRecord(int ColNum, Database DB)
        {
            List<Record> Records = DB.Data[ColNum].Data;
            int high = Converter.ByteToString(Records[0].Data, DB.Data[ColNum].type).Length;
            for (int i = 1; i < Records.Count; i++)
                if (Converter.ByteToString(Records[i].Data, DB.Data[ColNum].type).Length > high)
                    high = Converter.ByteToString(Records[i].Data, DB.Data[ColNum].type).Length;
            return high;
        }
        private void View_Database(Database DB_ToView, int NumberOfRecordsToDisplay)
        {
            if (NumberOfRecordsToDisplay > DB_ToView.Data[0].Data.Count /*Number of records in database*/)
            {
                Console.WriteLine("There are less than " + NumberOfRecordsToDisplay + " records in the database... displaying entire database instead:\n\n");
                NumberOfRecordsToDisplay = DB_ToView.Data[0].Data.Count /*Number of records in database*/;
            }
            if (DB_ToView.Data.Count == 0 || DB_ToView[0].Data.Count == 0)
            {
                Console.WriteLine("The Database is empty, there is nothing to display.");
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.White;
                List<string> ColumnHeadings = DB_ToView.sColumnHeadings();
                #region Column Width
                List<int> width = new List<int>();
                int maxwidth = 2;
                maxwidth = 2 >= DB_ToView.Data.Count.ToString().Length ? 2 : DB_ToView.Data.Count.ToString().Length;
                width.Add(maxwidth);
                for (int col = 0; col < ColumnHeadings.Count; col++)
                {
                    maxwidth = DB_ToView[col].Name.Length;
                    for (int rec = 0; rec < NumberOfRecordsToDisplay; rec++)
                    {


                        if (maxwidth < Converter.ByteToString(DB_ToView[col][rec].Data, DB_ToView[col][rec].type).Length)
                            maxwidth = Converter.ByteToString(DB_ToView[col][rec].Data, DB_ToView[col][rec].type).Length;
                    }
                    width.Add(maxwidth);
                }


                #endregion
                string nameToPrint = "ID" + " ".Times(width[0] - 2) + "|";
                Console.Write(nameToPrint);
                for (int r = 0; r < ColumnHeadings.Count; r++)
                {
                    Console.Write(ColumnHeadings[r] + " ".Times(width[r + 1] - ColumnHeadings[r].Length) + "|");

                }
                Console.ResetColor();

                //BABY, BABY BABY, OOOHHHHHH  #jonahisabelieber
                Console.WriteLine();

                for (int RecordNum = 0; RecordNum < NumberOfRecordsToDisplay; RecordNum++)
                {
                    //display current record


                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write(RecordNum + " ".Times(width[0] - RecordNum.ToString().Length) + "|");
                    for (int r = 0; r < ColumnHeadings.Count; r++)
                    {
                        Console.Write(Converter.ByteToString(DB_ToView[r][RecordNum].Data, DB_ToView[r][RecordNum].type)
                            + " ".Times(width[r + 1] - Converter.ByteToString(DB_ToView[r][RecordNum].Data, DB_ToView[r][RecordNum].type).Length) + "|");

                    }
                    //LEAVE THIS ALONE!!!!
                    Console.WriteLine();
                }
            }
            Console.ResetColor();

        }
        private Database Load_Database()
        {
            string[] DbPaths = AvailblDbPaths();
            if (DbPaths.Length == 0)
            {
                Console.WriteLine("No databases were found on this device, press any key to go back to the main menu...");
                Console.ReadKey();
                Console.Clear();
                return null;
            }
            Console.WriteLine("Here are the databases on this computer\n");
            for (int NumOfDbs = 0; NumOfDbs < DbPaths.Length; NumOfDbs++)
                Console.WriteLine((NumOfDbs + 1) + ". " + DbPaths[NumOfDbs]);
            Console.WriteLine("\nIf you wouldn’t like to open any of these and would like to return to the main menu, type anything else");
            Console.WriteLine("Pick the menu number of the database that you’d like to load: ");
            string Entry = Console.ReadLine();
            int Choice;
            if (!Int32.TryParse(Entry, out Choice) || DbPaths.Length < Choice || Choice <= 0) return null;
            string path = DbPaths[--Choice];
            return Database.LoadDatabase(path);
        }
        string[] AvailblDbPaths()
        {
            return Directory.GetFiles(Database.FolderPath, "*.bin");
        }
        string GetFileName(string EnteredPath)
        {
            if (!EnteredPath.Contains(".bin")) EnteredPath += ".bin";
            string FilePath = EnteredPath;
            if (EnteredPath.Length < 13 || EnteredPath.Split(new char[] { EnteredPath.ToCharArray()[12] })[0] != Database.FolderPath)//checking if folderpath is included in the entered directory of not
                FilePath = Path.Combine(Database.FolderPath, EnteredPath);
            //FilePath is now in correct format to be used by functions
            Database database;
            try
            {
                database = Database.LoadDatabase(FilePath);
            }
            catch
            {
                return "\\";
            }
            return database.FileName;
        }
        private void Create_Column()
        {
            string HappyWithCreation = "";
            string iName;
            Type itype;
            do
            {
                Console.WriteLine("What would you like to name this field?");
                iName = Console.ReadLine();
                for (int pro = 0; pro < Converter.types.Length; pro++)
                {
                    Console.WriteLine($"{pro} - {Converter.types[pro].TypeName()}");
                }
                while (true)
                {
                    try
                    {
                        itype = Converter.types[Int32.Parse(Console.ReadLine())];

                        break;
                    }
                    catch
                    {
                        Console.WriteLine("Invalid input, please enter a valid menu number:");
                    }
                }

                Console.WriteLine("Are you sure you’d like to create a " + TypeName(itype) + " field called " + iName + "? (yes/no)");

                while ((HappyWithCreation = Console.ReadLine()) != "yes" && HappyWithCreation != "no")
                {
                    Console.WriteLine("Enter either ‘yes’ or ‘no’");
                }
            } while (HappyWithCreation == "no");
            TheDatabase.Data.Add(new Column(iName, itype));
            if (TheDatabase.Data.Count > 0)//Database has some columns
            {
                if (TheDatabase.Data[0].Data.Count > 0)//Database isn’t empty
                {
                    Console.Clear();
                    #region Column Width
                    List<int> width = new List<int>();
                    int maxwidth = 2;
                    maxwidth = 2 >= TheDatabase.Data.Count.ToString().Length ? 2 : TheDatabase.Data.Count.ToString().Length;
                    width.Add(maxwidth);
                    for (int col = 0; col < TheDatabase.Data.Count; col++)
                    {
                        maxwidth = TheDatabase[col].Name.Length;
                        foreach (var item in TheDatabase[col].Data)
                        {
                            if (maxwidth < Converter.ByteToString(item.Data, item.type).Length)
                                maxwidth = Converter.ByteToString(item.Data, item.type).Length;
                        }
                        width.Add(maxwidth);
                    }


                    #endregion

                    Console.WriteLine("There are existing records in the database for which the new field must be filled out.\nPlease fill out the following information for the records already in the database:\n");
                    List<string> ColHeadings = TheDatabase.sColumnHeadings();   //Displaying column titles
                    string nameToPrint = "ID" + " ".Times(width[0] - 2) + "|";
                    Console.Write(nameToPrint);
                    for (int r = 0; r < ColHeadings.Count; r++)
                    {

                        Console.Write(ColHeadings[r] + " ".Times(width[r + 1] - ColHeadings[r].Length) + "|");

                    }       //column titles displayed
                    Console.Write("\n");//next line
                    for (int RecordNum = 0; RecordNum < TheDatabase.Data[0].Data.Count; RecordNum++)
                    {
                        //display current record


                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write(RecordNum + " ".Times(width[0] - RecordNum.ToString().Length) + "|");
                        //Console.Write(RecordNum + " ".Times(width[0] - RecordNum.ToString().Length) + "|");
                        byte[] tempB;
                        for (int r = 0; r < TheDatabase.Data.Count - 1; r++)
                        {
                            Console.Write(Converter.ByteToString(TheDatabase[r][RecordNum].Data, TheDatabase[r][RecordNum].type)
                                + " ".Times(width[r + 1] - Converter.ByteToString(TheDatabase[r][RecordNum].Data, TheDatabase[r][RecordNum].type).Length) + "|");

                        }
                        while (!Converter.TryParseToByte(Console.ReadLine(), itype, out tempB))
                        {
                            Console.WriteLine("Invalid Input...Reprompting");
                            Console.Write(RecordNum + " ".Times(width[0] - RecordNum.ToString().Length) + "|");
                            for (int r = 0; r < TheDatabase.Data.Count - 1; r++)
                            {
                                Console.Write(Converter.ByteToString(TheDatabase[r][RecordNum].Data, TheDatabase[r][RecordNum].type)
                                    + " ".Times(width[r + 1] - Converter.ByteToString(TheDatabase[r][RecordNum].Data, TheDatabase[r][RecordNum].type).Length) + "|");

                            }
                        }
                        TheDatabase[TheDatabase.Data.Count - 1].Data.Add(new Record(tempB, itype));

                    }

                }
            }
            Console.ResetColor();
            Console.WriteLine("Your new column has now been added to the database!\tPress any key to continue...");
            Console.ReadKey();
            return;
        }
        string TypeName(Type T)
        {
            switch (T.ToString())
            {
                case "string":
                    return "text";
                case "Byte":
                    return "positive byte";
                case "SByte":
                    return "byte";
                case "ushort":
                    return "positive short integer";
                case "short":
                    return "short integer";
                case "uint":
                    return "positive integer";
                case "int":
                    return "integer";
                case "ulong":
                    return "positive long integer";
                case "long":
                    return "long integer";
                case "float":
                    return "decimal";
                case "double":
                    return "long decimal";
                case "DateTime":
                    return "date-time";
                default:
                    return "general";//was thinking we could have a construct which allows a general field which can store many things at once... dont really know the use of it but i have to enter something in the default case otherwise all paths wouldn’t return a value
            }
        }
        private Database AskNewDatabase()
        {
            Database newDatabase = new Database();

            do
            {
                Console.WriteLine("Input a new database name: ");
                string newDatabaseName = Console.ReadLine();

                if (newDatabaseName.Contains("\\"))
                {
                    newDatabaseName.Replace("\\", "");
                }

                newDatabase.FileName = newDatabaseName;

                Console.WriteLine("Input a file name to store the new database: ");
                string newFilePath = Console.ReadLine();
                if (!newFilePath.Contains(".bin")) newFilePath += ".bin";
                try
                {
                    if (!File.Exists(Database.FolderPath + "\\" + newFilePath))
                    {
                        newDatabase.FilePath = newFilePath;
                    }
                    else
                    {
                        Console.WriteLine("File Already Exists ... Reprompting");
                        continue;
                    }
                }
                catch
                {
                    Console.WriteLine("Invalid Path ... Reprompting");
                    continue;
                }

                Console.WriteLine("To confirm that {0} is your new database's name and {1} is the file path where you would like to store the databse, type 'yes' or 'no': ", newDatabaseName, newFilePath);
                string confirmation = Console.ReadLine();
                do
                {
                    if (confirmation == "yes")
                    {
                        Console.WriteLine("The new database has successfully been created");
                        break;
                    }
                    else if (confirmation == "no")
                    {
                        Console.WriteLine("Please try again.");
                    }
                    else
                    {
                        Console.WriteLine("Input was invalid, please try again.");
                        confirmation = Console.ReadLine();
                    }
                }
                while (confirmation != "yes" && confirmation != "no");
                if (confirmation == "yes") break;
            } while (true);
            newDatabase.FilePath = Database.FolderPath + "\\" + newDatabase.FilePath;
            newDatabase.DateOfLastSave = Converter.StringToByte(DateTime.Now.ToString(), typeof(DateTime));
            return newDatabase;
        }

        private void MasterEditor()
        {
            Console.WriteLine(@"Please enter the corresponding menu number for the desired type:
    0 - Edit Records
    1 - Edit Columns
    2 - Return to Main Menu");
            string response;
            do
            {
                response = Console.ReadLine();
                switch (response)
                {
                    case "0":
                        if (RecordEditor()) break;
                        break;
                    case "1":
                        if (ColumnEditor()) break;
                        break;
                    case "2":
                        break;
                    default:
                        Console.WriteLine("Invalid input.");
                        break;
                }
            }
            while (response != "0" && response != "1" && response != "2");

        }
        private bool RecordEditor()
        {
            Console.Clear();
            Console.WriteLine("Welcome to the Record Editor!");
            Console.WriteLine("Please enter the corresponding menu number for the desired type:");
            int res = CPExtensionMethods.TryToAskQuestion("0 - Edit Record\n1 - Add Record\n2 - Delete Record\n3 - Return To Menu", 3);
            Console.Clear();
            switch (res)
            {
                case 0:
                    #region Edit Record
                    if (TheDatabase.HasARecord)
                    {
                        View_Database(TheDatabase, TheDatabase.Data[0].Data.Count);
                        Console.WriteLine("To go back, enter ‘back’");
                        string GoBack = Console.ReadLine();
                        if (GoBack == "back") return true;
                        int rec = CPExtensionMethods.TryToAskQuestion("Please insert the ID of the item you want to edit", TheDatabase[0].Data.Count - 1);
                        switch (CPExtensionMethods.TryToAskQuestion("0 - Edit all Record\n1 - Edit a field of Record", 1))
                        {
                            case 0:
                                Console.WriteLine("Please add data for each required value:");
                                for (int col = 0; col < TheDatabase.Data.Count; col++)
                                {
                                    do
                                    {
                                        Console.Write($"{TheDatabase[col].Name} of type {TheDatabase[col].type.TypeName()}:");
                                        byte[] tempdat0;
                                        if (!Converter.TryParseToByte(Console.ReadLine(), TheDatabase[col].type, out tempdat0))
                                        {
                                            Console.WriteLine("Invalid Input...Reprompting");
                                        }
                                        else
                                        {
                                            TheDatabase[col].Data[rec] = new Record(tempdat0, TheDatabase[col].type);
                                            break;
                                        }
                                    } while (true);

                                }
                                Console.WriteLine("Record Replaced");
                                Console.WriteLine("Press any key...");
                                Console.ReadKey();
                                Console.Clear();
                                break;
                            case 1:
                                for (int col = 0; col < TheDatabase.Data.Count; col++)
                                {
                                    Console.WriteLine($"{col} - {TheDatabase[col].Name}");
                                }
                                int colToEdit = CPExtensionMethods.TryToAskQuestion("Please insert the ID of the column to edit", TheDatabase.Data.Count - 1);
                                do
                                {
                                    Console.Write($"{TheDatabase[colToEdit].Name} of type {TheDatabase[colToEdit].type.TypeName()}:");
                                    byte[] tempdat;
                                    if (!Converter.TryParseToByte(Console.ReadLine(), TheDatabase[colToEdit].type, out tempdat))
                                    {
                                        Console.WriteLine("Invalid Input...Reprompting");
                                    }
                                    else
                                    {
                                        TheDatabase[colToEdit].Data[rec] = (new Record(tempdat, TheDatabase[colToEdit].type));
                                        break;
                                    }
                                } while (true);
                                Console.WriteLine("You have edited the record.");
                                Console.WriteLine("Press any key...");
                                Console.ReadKey();
                                Console.Clear();
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("You do not have a record to edit");
                        Console.WriteLine("Press any key...");
                        Console.ReadKey();
                        Console.Clear();
                    }
                    break;
                #endregion
                case 1:
                    if (TheDatabase.HasAColumn)
                    {
                        Console.WriteLine("Please add data for each required value:");
                        for (int col = 0; col < TheDatabase.Data.Count; col++)
                        {
                            do
                            {
                                Console.Write($"{TheDatabase[col].Name} of type {TheDatabase[col].type.TypeName()}:");
                                byte[] tempdat;
                                if (!Converter.TryParseToByte(Console.ReadLine(), TheDatabase[col].type, out tempdat))
                                {
                                    Console.WriteLine("Invalid Input...Reprompting");
                                }
                                else
                                {
                                    TheDatabase[col].Data.Add(new Record(tempdat, TheDatabase[col].type));
                                    break;
                                }
                            } while (true);

                        }
                    }
                    else
                    {
                        Console.WriteLine("Sorry. You must have at least one Column to add a record");
                    }
                    Console.WriteLine("Press any key...");
                    Console.ReadKey();
                    Console.Clear();
                    break;
                case 2:
                    if (TheDatabase.HasARecord)
                    {
                        View_Database(TheDatabase, TheDatabase.Data[0].Data.Count);
                        TheDatabase.RemoveEntry(CPExtensionMethods.TryToAskQuestion("Please insert the ID of the item you want to delete", TheDatabase[0].Data.Count - 1));
                    }
                    else Console.WriteLine("There are no records to delete");
                    Console.WriteLine("Press any key...");
                    Console.ReadKey();
                    Console.Clear();
                    break;
                case 3:
                    return true;
            }
            return false;
        }
        private bool ColumnEditor()
        {
            Console.Clear();
            Console.WriteLine("Welcome to the Column Editor!");
            Console.WriteLine("Please enter the corresponding menu number for the desired type:");
            int res = CPExtensionMethods.TryToAskQuestion("0 - Edit Column\n1 - Add Column\n2 - Delete Column\n3 - Return To Menu", 3);
            switch (res)
            {
                case 0:
                    if (TheDatabase.HasAColumn)
                    {
                        for (int col = 0; col < TheDatabase.Data.Count; col++)
                        {
                            Console.WriteLine($"{col} - {TheDatabase[col].Name}");
                        }
                        int colToEdit = CPExtensionMethods.TryToAskQuestion("Please insert the column ID to edit", TheDatabase.Data.Count - 1);
                        Console.Write("Please type the new name: ");
                        TheDatabase[colToEdit].Name = Console.ReadLine();
                        Console.WriteLine("Column Edited");

                    }
                    else
                        Console.WriteLine("There are no columns to delete");
                    Console.WriteLine("Press any key...");
                    Console.ReadKey();
                    Console.Clear();

                    break;
                case 1:
                    Create_Column();
                    Console.Clear();
                    break;
                case 2:
                    if (TheDatabase.HasAColumn)
                    {
                        Console.WriteLine("Please insert the column number to delete from the following:");
                        for (int col = 0; col < TheDatabase.Data.Count; col++)
                        {
                            Console.WriteLine($"{col} - {TheDatabase[col].Name}");
                        }
                        TheDatabase.Data.RemoveAt(CPExtensionMethods.TryToAskQuestion("", TheDatabase.Data.Count - 1));
                    }
                    else Console.WriteLine("There are no columns to delete");
                    Console.WriteLine("Press any key...");
                    Console.ReadKey();
                    Console.Clear();
                    break;
                case 3:
                    return true;
            }
            return false;
        }
    }
    #endregion

    #region Column and Record Constructs

    public class Record
    {
        public Type type;
        public byte[] Data;
        public Record()
        {

        }
        public Record(byte[] d, Type t)
        {
            Data = d;
            type = t;
        }
        public Record CopyRecord()
        {
            Record output = new Record();
            output.type = type;
            byte[] temp = new byte[Data.Length];
            Array.Copy(Data, temp, Data.Length);
            output.Data = temp;
            return output;
        }
        public int CompareTo(Record other)
        {
            if (type.Equals(typeof(string)))
            {
                UTF8Encoding encoder = new UTF8Encoding();
                return encoder.GetString(Data, 0, Data.Length - 1).ToLower().CompareTo(encoder.GetString(other.Data, 0, other.Data.Length - 1).ToLower());
            }
            else if (type.Equals(typeof(byte)))
            {
                return Data[0].CompareTo(other.Data[0]);
            }
            else if (type.Equals(typeof(sbyte)))
            {
                return unchecked(((sbyte)Data[0])).CompareTo(unchecked(((sbyte)other.Data[0])));
            }
            else if (type.Equals(typeof(Int16)))
            {
                return BitConverter.ToInt16(Data, 0).CompareTo(BitConverter.ToInt16(other.Data, 0));
            }
            else if (type.Equals(typeof(UInt16)))
            {
                return BitConverter.ToUInt16(Data, 0).CompareTo(BitConverter.ToUInt16(other.Data, 0));
            }
            else if (type.Equals(typeof(Int32)))
            {
                return BitConverter.ToInt32(Data, 0).CompareTo(BitConverter.ToInt32(other.Data, 0));
            }
            else if (type.Equals(typeof(UInt32)))
            {
                return BitConverter.ToUInt32(Data, 0).CompareTo(BitConverter.ToUInt32(other.Data, 0));
            }
            else if (type.Equals(typeof(Int64)))
            {
                return BitConverter.ToInt64(Data, 0).CompareTo(BitConverter.ToInt64(other.Data, 0));
            }
            else if (type.Equals(typeof(UInt64)))
            {
                return BitConverter.ToUInt64(Data, 0).CompareTo(BitConverter.ToUInt64(other.Data, 0));
            }
            else if (type.Equals(typeof(Single)))
            {
                return BitConverter.ToSingle(Data, 0).CompareTo(BitConverter.ToSingle(other.Data, 0));
            }
            else if (type.Equals(typeof(Double)))
            {
                return BitConverter.ToDouble(Data, 0).CompareTo(BitConverter.ToDouble(other.Data, 0));
            }
            else if (type.Equals(typeof(DateTime)))
            {
                return Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}:{4}:{5}", Data[0], Data[1], BitConverter.ToUInt16(new byte[] { Data[2], Data[3] }, 0), Data[4], Data[5], Data[6]))
                    .CompareTo(Convert.ToDateTime(string.Format("{0}/{1}/{2} {3}:{4}:{5}", other.Data[0], other.Data[1], BitConverter.ToUInt16(new byte[] { other.Data[2], other.Data[3] }, 0), other.Data[4], other.Data[5], other.Data[6])));

            }
            return 0;
        }
    }
    public class Column
    {
        public Type type;
        public string Name;
        public List<Record> Data = new List<Record>();
        public Column(string name, Type type)
        {
            this.type = type;
            Name = name;
        }
        public Column()
        {

        }
        public List<int> GenerateSortPattern(SortStyle sortStyle) //Bubble Sort UNTESTED
        {
            List<int> output = new List<int>();
            for (int num = 0; num < Data.Count; num++) output.Add(num);
            bool switched = false;

            for (int endpos = output.Count - 1; endpos >= 1; endpos--)
            {
                for (int pos = 0; pos < endpos; pos++)
                {
                    int res = Data[output[pos]].CompareTo(Data[output[pos + 1]]);
                    if (sortStyle == SortStyle.Descending) res = -res;
                    if (res > 0)
                    {
                        int temp = output[pos];
                        output[pos] = output[endpos];
                        output[endpos] = temp;
                        switched = true;
                    }
                }
                if (!switched) break;
            }
            return output;
        }
        public List<int> GenerateFilterPattern(FilterStyle filterStyle, Record Comparison)
        {
            List<int> output = new List<int>();
            for (int item = 0; item < Data.Count; item++)
            {
                if (Comparison.CompareTo(Data[item]) == 0 && (filterStyle == FilterStyle.Equal
                    || filterStyle == FilterStyle.GreaterThanOrEqual || filterStyle == FilterStyle.LessThanOrEqual))
                    output.Add(item);
                else if (Comparison.CompareTo(Data[item]) < 0 && (filterStyle == FilterStyle.GreaterThan || filterStyle == FilterStyle.GreaterThanOrEqual))
                    output.Add(item);
                else if (Comparison.CompareTo(Data[item]) > 0 && (filterStyle == FilterStyle.LessThan || filterStyle == FilterStyle.LessThanOrEqual))
                    output.Add(item);
                else if (Comparison.CompareTo(Data[item]) != 0 && (filterStyle == FilterStyle.NotEqualTo))
                    output.Add(item);
            }
            return output;
        }
        public Column CopyColumn()
        {
            Column output = new Column();
            output.type = type;
            output.Name = Name;
            List<Record> temp = new List<Record>();
            foreach (var item in Data)
            {
                temp.Add(item.CopyRecord());
            }
            output.Data = temp;
            return output;
        }
        public Record this[int index]
        {
            get { return Data[index]; }
        }

    }
    #endregion

    #region Database Constructs
    public enum SortStyle { Ascending, Descending };
    public enum FilterStyle { Equal, GreaterThanOrEqual, LessThanOrEqual, GreaterThan, LessThan, NotEqualTo }
    public class Database
    {
        public List<Column> Data = new List<Column>();
        public byte[] DateOfLastSave;
        public static string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "JAK");
        public string FilePath = "NameOfDatabase.bin";
        public string FileName = "TestFile";
        const byte ETX = 3;  //byte	00000011	ETX end of text
        const ulong FormatID = 18263452859329828488L;

        public bool HasAColumn { get { return Data.Count > 0; } }
        public bool HasARecord { get { return HasAColumn && Data[0].Data.Count > 0; } }
        /// <summary>
        /// Loads a Database from a binary file
        /// </summary>
        /// <param name="FilePath">File Location</param>
        /// <returns></returns>
        public static Database LoadDatabase(string FilePath)
        {
            if (!FilePath.Contains(".bin")) FilePath += ".bin";
            BinaryReader binaryReader = new BinaryReader(new FileStream(Path.Combine(FolderPath, FilePath), FileMode.Open));

            //Checks to see if in correct format
            if (binaryReader.BaseStream.Length < 8) throw new NotValidFormatException("Less than 8 bytes");
            else
            {
                if (binaryReader.ReadUInt64() != FormatID) throw new NotValidFormatException("Not Correct Format ID");
            }
            Database database = new Database();
            byte CCount = binaryReader.ReadByte();
            for (int col = 0; col < CCount; col++)
            {
                Column column = new Column();
                column.type = Converter.ByteToType(binaryReader.ReadByte());
                column.Name = Converter.ByteToString(binaryReader.ReadNextRecord(typeof(String)), typeof(String));
                database.Data.Add(column);
            }
            database.DateOfLastSave = binaryReader.ReadNextRecord(typeof(DateTime));
            database.FileName = Converter.ByteToString(binaryReader.ReadNextRecord(typeof(string)), typeof(string));
            //Reads in and creates the Empty Columns
            while (!binaryReader.EOF())
            {
                for (int col = 0; col < database.Data.Count; col++)
                {
                    database.Data[col].Data.Add(new Record(binaryReader.ReadNextRecord(database.Data[col].type),
                        database.Data[col].type));
                }
            }
            database.FilePath = FilePath;
            binaryReader.Close();
            return database;
        }
        public bool Read_csv_IntoDatabase()
        {

            string sPath = "";
            string dec;
            Console.Clear();
            int ans = CPExtensionMethods.TryToAskQuestion("Does your csv file have any column names in it? (1 - yes/0 - no)", 1);
            bool ColNamesInside = (ans == 1) ? true : false;
            DateTime InitialWait;
            TimeSpan FewSecs = new TimeSpan(4 * 10000);
            OpenFileDialog BrowseBox = new OpenFileDialog();
            BrowseBox.Multiselect = false;
            do
            {
                dec = "not no";
                InitialWait = DateTime.Now;
                Console.WriteLine("Please choose the .csv file that you would like to import... ");
                while (DateTime.Now < InitialWait.Add(FewSecs)) {; }
                if (BrowseBox.ShowDialog() == DialogResult.OK)
                    sPath = BrowseBox.FileName;
                else
                {
                    dec = CPExtensionMethods.TryToAskQuestion("Would you like to return to the main menu (1 - yes/0 - no)", 1).ToString();
                }
            } while (dec == "0");
            if (dec == "1")
                return false;
            string[] Lines = File.ReadAllLines(sPath);
            string[,] AllDataOnly = new string[Lines[0].Split(',').Length, Lines.Length - ((ColNamesInside) ? 1 : 0)];
            for (int i = (ColNamesInside) ? 1 : 0; i < Lines.Length; i++)//for every line
            {
                for (int j = 0; j < Lines[i].Split(',').Length; j++)//for every record in the line
                {
                    //add it to everything
                    AllDataOnly[j, i - ((ColNamesInside) ? 1 : 0)] = Lines[i].Split(',')[j];
                }
            }
            //AllDataOnly - ready
            for (int i = 0; i < Lines[0].Split(',').Length; i++)
                Data.Add(new Column(ColNamesInside ? Lines[0].Split(',')[i] : "", CPExtensionMethods.FindTypeOfColumn(AllDataOnly, i, Lines.Length - ((ColNamesInside) ? 1 : 0))));
            //Column types assigned
            for (int cols = 0; cols < Lines[0].Split(',').Length; cols++)//for every column
            {
                for (int recs = 0; recs < Lines.Length - ((ColNamesInside) ? 1 : 0); recs++)//add all records to List<Record> in current column
                {
                    Data[cols].Data.Add(new Record(Converter.StringToByte(AllDataOnly[cols, recs], Data[cols].type), Data[cols].type));
                }
            }

            //Get names of columns
            if (!ColNamesInside)
            {
                Console.WriteLine("Please enter the column names: ");
                for (int names = 0; names < Data.Count; names++)
                {
                    Console.WriteLine("Here is the column: \n");
                    for (int rec = 0; rec < Data[names].Data.Count; rec++)
                        Console.WriteLine(Converter.ByteToString(Data[names].Data[rec].Data, Data[names].type));
                    Console.WriteLine("\nEnter the name that you would like for this column: ");
                    Data[names].Name = Console.ReadLine();
                }
            }
            else
            {
                for (int index = 0; index < Data.Count; index++)
                    Data[index].Name = Lines[0].Split(',')[index];
            }
            Console.Clear();
            Console.WriteLine("And finally, what would you like to name this new database?");
            FileName = Console.ReadLine();
            Console.WriteLine("Enter a name under which the file will be stored..");
            FilePath = Console.ReadLine();
            if (!FilePath.Contains(".bin")) FilePath += ".bin";
            FilePath = FolderPath + "\\" + FilePath;
            while (Directory.Exists(FilePath))
            {
                Console.WriteLine("\nSorry, another database is already stored under this file name, please enter another suitable one");
                FilePath = Console.ReadLine();
                FilePath = FolderPath + "\\" + FilePath;
            }
            Console.Clear();
            SaveDatabase();
            Console.Write("And that’s it! All done, the database has now been saved in the JAK format and can now be accessed from JAK Database whenever you like!");
            return true;
        }
        public void SaveDatabase()
        {
            DateOfLastSave = Converter.StringToByte((Convert.ToString(DateTime.Now)), typeof(DateTime));
            BinaryWriter binaryWriter;
            if (!File.Exists(FilePath)) { binaryWriter = new BinaryWriter(new FileStream(FilePath, FileMode.Create)); }
            else binaryWriter = new BinaryWriter(new FileStream(FilePath, FileMode.Truncate));
            binaryWriter.Write(FormatID);
            binaryWriter.Write((byte)Data.Count);
            foreach (var col in Data)
            {
                binaryWriter.Write(Converter.TypeToByte(col.type));
                binaryWriter.Write(Converter.StringToByte(col.Name, typeof(string)));

            }
            binaryWriter.Write(DateOfLastSave);
            binaryWriter.Write(Converter.StringToByte(FileName, typeof(string)));
            if (Data.Count != 0)
            {
                for (int record = 0; record < Data[0].Data.Count; record++)
                {
                    for (int col = 0; col < Data.Count; col++)
                    {
                        binaryWriter.Write(Data[col].Data[record].Data);
                    }
                }
            }
            binaryWriter.Close();
        }
        public void ApplyPattern(List<int> pattern)
        {
            for (int col = 0; col < Data.Count; col++)
            {
                List<Record> temp = new List<Record>();
                foreach (var item in pattern)
                {
                    temp.Add(Data[col].Data[item]);
                }
                Data[col].Data = temp;
            }
        }
        public Database CopyDatabase()
        {
            Database output = new Database();
            output.DateOfLastSave = DateOfLastSave;
            output.FileName = FileName;
            output.FilePath = FilePath;
            List<Column> temp = new List<Column>();
            foreach (var item in Data)
            {
                temp.Add(item.CopyColumn());
            }
            output.Data = temp;
            return output;
        }

        public List<string> sRecord(int RecordNumber_startingat0)
        {
            int RecordNumber = RecordNumber_startingat0;//is this superfluous?
            List<string> ToReturn = new List<string>();
            for (int ColNum = 0; ColNum < Data.Count; ColNum++)
            {
                if (this[ColNum].Data.Count <= RecordNumber)//why not throw an exception? //new column being created, so no data in the record of that column
                    ToReturn.Add("");//Adding empty string so that indexing of ToReturn doesn’t get messed up
                else
                    ToReturn.Add(Converter.ByteToString(this[ColNum][RecordNumber].Data, this[ColNum].type));
            }
            return ToReturn;
        }
        public List<string> sColumnHeadings()
        {
            List<string> ToReturn = new List<string>();
            for (int i = 0; i < this.Data.Count; i++)
            {
                ToReturn.Add(this.Data[i].Name);
            }
            return ToReturn;
        }

        public void RemoveEntry(int v)
        {
            foreach (var item in Data)
            {
                item.Data.RemoveAt(v);
            }
        }

        [Serializable]
        public class NotValidFormatException : Exception
        {
            public NotValidFormatException() { }
            public NotValidFormatException(string message) : base(message) { }
            public NotValidFormatException(string message, Exception inner) : base(message, inner) { }
            protected NotValidFormatException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        public Column this[int index]
        {
            get { return Data[index]; }

        }

    }

    #endregion

    #region BinaryFile Constrcuts

    public static class StreamExtensionMethods
    {
        /// <summary>
        /// Checks to see if EndOfFile
        /// </summary>
        /// <param name="binaryReader">Binary Reader</param>
        /// <returns></returns>
        public static bool EOF(this BinaryReader binaryReader)
        {
            var bs = binaryReader.BaseStream;
            return (bs.Position == bs.Length);
        }
    }
    public static class Converter
    {
        public static Type[] types = new Type[] {
            typeof(String),
            typeof(Byte),
            typeof(SByte),
            typeof(UInt16),
            typeof(Int16),
            typeof(UInt32),
            typeof(Int32),
            typeof(UInt64),
            typeof(Int64),
            typeof(Single),
            typeof(Double),
            typeof(DateTime)};
        public static byte[] ReadNextRecord(this BinaryReader binaryReader, Type type) //ADD MORE DATATYPES
        {
            if (type.Equals(typeof(string)))
            {
                List<byte> output = new List<byte>();
                while (true)
                {
                    byte test = binaryReader.ReadByte();
                    output.Add(test);
                    if (test == 3)
                    {
                        return output.ToArray();
                    }
                }
            }
            else if (type.Equals(typeof(byte)) || type.Equals(typeof(sbyte)))
            {
                return binaryReader.ReadBytes(1);
            }
            else if (type.Equals(typeof(Int16)) || type.Equals(typeof(UInt16)))
            {
                return binaryReader.ReadBytes(2);
            }
            else if (type.Equals(typeof(Int32)) || type.Equals(typeof(UInt32)) || type.Equals(typeof(Single)))
            {
                return binaryReader.ReadBytes(4);
            }
            else if (type.Equals(typeof(DateTime)))
            {
                return binaryReader.ReadBytes(7);
            }
            else if (type.Equals(typeof(Int64)) || type.Equals(typeof(UInt64)) || type.Equals(typeof(Double)))
            {
                return binaryReader.ReadBytes(8);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public static string TypeName(this Type type)
        {

            if (type.Equals(typeof(string)))
            {
                return "String";
            }
            else if (type.Equals(typeof(byte)))
            {
                return "Unsigned Byte";
            }
            else if (type.Equals(typeof(sbyte)))
            {
                return "Signed Byte";
            }
            else if (type.Equals(typeof(Int16)))
            {
                return "Signed Short";
            }
            else if (type.Equals(typeof(UInt16)))
            {
                return "Unsigned Short";
            }
            else if (type.Equals(typeof(Int32)))
            {
                return "Signed Integer";
            }
            else if (type.Equals(typeof(UInt32)))
            {
                return "Unsigned Integer";
            }
            else if (type.Equals(typeof(Int64)))
            {
                return "Signed Long";
            }
            else if (type.Equals(typeof(UInt64)))
            {
                return "Unsigned Long";
            }
            else if (type.Equals(typeof(Single)))
            {
                return "Float";
            }
            else if (type.Equals(typeof(Double)))
            {
                return "Double";
            }
            else if (type.Equals(typeof(DateTime)))
            {
                return "Date Time";
            }
            return "Unsupported Type";
        }

        public static Type ByteToType(byte B)
        {
            if (B > types.Length - 1 || B < 0) throw new InvalidDataException("Note a type ID");
            return types[B];
        }
        public static Byte TypeToByte(Type T)
        {
            return (byte)Array.IndexOf<Type>(types, T);
        }

        public static string ByteToString(byte[] B, Type type) //ADD MORE DATATYPES
        {
            if (type.Equals(typeof(string)))
            {
                UTF8Encoding encoder = new UTF8Encoding();
                return encoder.GetString(B, 0, B.Length - 1);
            }
            else if (type.Equals(typeof(byte)))
            {
                return B[0].ToString();
            }
            else if (type.Equals(typeof(sbyte)))
            {
                return unchecked(((sbyte)B[0])).ToString();
            }
            else if (type.Equals(typeof(Int16)))
            {
                return BitConverter.ToInt16(B, 0).ToString();
            }
            else if (type.Equals(typeof(UInt16)))
            {
                return BitConverter.ToUInt16(B, 0).ToString();
            }
            else if (type.Equals(typeof(Int32)))
            {
                return BitConverter.ToInt32(B, 0).ToString();
            }
            else if (type.Equals(typeof(UInt32)))
            {
                return BitConverter.ToUInt32(B, 0).ToString();
            }
            else if (type.Equals(typeof(Int64)))
            {
                return BitConverter.ToInt64(B, 0).ToString();
            }
            else if (type.Equals(typeof(UInt64)))
            {
                return BitConverter.ToUInt64(B, 0).ToString();
            }
            else if (type.Equals(typeof(Single)))
            {
                return BitConverter.ToSingle(B, 0).ToString();
            }
            else if (type.Equals(typeof(Double)))
            {
                return BitConverter.ToDouble(B, 0).ToString();
            }
            else if (type.Equals(typeof(DateTime)))
            {
                return string.Format("{0:00}/{1:00}/{2:0000} {3:00}:{4:00}:{5:00}", B[0], B[1], BitConverter.ToUInt16(new byte[] { B[2], B[3] }, 0), B[4], B[5], B[6]);

            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public static bool TryParseToByte(string s, Type type, out byte[] B)
        {
            try
            {
                B = StringToByte(s, type);
            }
            catch (Exception)
            {
                B = null;
                return false;
            }
            return true;
        }
        public static byte[] StringToByte(string s, Type type) //ADD MORE DATATYPES
        {
            if (type.Equals(typeof(string)))
            {
                UTF8Encoding encoder = new UTF8Encoding();
                return encoder.GetBytes(s).Add(3);
            }
            else if (type.Equals(typeof(byte)))
            {
                return new byte[] { byte.Parse(s) };
            }
            else if (type.Equals(typeof(sbyte)))
            {
                return new byte[] { (byte)sbyte.Parse(s) };
            }
            else if (type.Equals(typeof(Int16)))
            {
                return BitConverter.GetBytes(Int16.Parse(s));
            }
            else if (type.Equals(typeof(UInt16)))
            {
                return BitConverter.GetBytes(UInt16.Parse(s));
            }
            else if (type.Equals(typeof(Int32)))
            {
                return BitConverter.GetBytes(Int32.Parse(s));
            }
            else if (type.Equals(typeof(UInt32)))
            {
                return BitConverter.GetBytes(UInt32.Parse(s));
            }
            else if (type.Equals(typeof(Int64)))
            {
                return BitConverter.GetBytes(Int64.Parse(s));
            }
            else if (type.Equals(typeof(UInt64)))
            {
                return BitConverter.GetBytes(UInt64.Parse(s));
            }
            else if (type.Equals(typeof(Single)))
            {
                return BitConverter.GetBytes(Single.Parse(s));
            }
            else if (type.Equals(typeof(Double)))
            {
                return BitConverter.GetBytes(Double.Parse(s));
            }
            else if (type.Equals(typeof(DateTime)))
            {
                DateTime tester = DateTime.Parse(s);
                byte[] output = new byte[7]{
                    (byte)(tester.Day),
                    (byte)(tester.Month),
                    BitConverter.GetBytes((short)(tester.Year))[0],
                    BitConverter.GetBytes((short)(tester.Year))[1],
                    (byte)(tester.Hour),
                (byte)(tester.Minute),
                (byte)(tester.Second)
                };
                return output;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    #endregion

    #region GeneralPurpose Extension Methods

    public static class CPExtensionMethods
    {
        public static string Times(this string x, int times)
        {
            string e = "";
            for (int l = 0; l < times; l++)
            {
                e += x;
            }
            return e;

        }
        public static byte[] Add(this byte[] b, byte b2)
        {
            byte[] output = new byte[b.Length + 1];
            b.CopyTo(output, 0);
            output[b.Length] = b2;
            return output;
        }
        public static int TryToAskQuestion(string Q, int maxValue)
        {
            Console.WriteLine(Q);
            int res;
            string i = Console.ReadLine();
            while (!Int32.TryParse(i, out res) || res < 0 || res > maxValue)
            {
                Console.WriteLine("Incorrect Input. Reprompting");
                i = Console.ReadLine();
            }
            return res;

        }
        /// <summary>
        /// Returns the most suitable type of a column of text in Table. The column is specified by the zero-based value of ColumnNumber
        /// </summary>
        /// <param name=""></param>
        /// <param name="ColumnNumber"></param>
        /// <returns></returns>
        public static Type FindTypeOfColumn(string[,] Table, int ColumnNumber, int NumOfRecords)
        {
            string[] ColSpecified = new string[NumOfRecords];
            for (int i = 0; i < NumOfRecords; i++)
                ColSpecified[i] = Table[ColumnNumber, i];
            try
            {
                DateTime DT;
                for (int i = 0; i < ColSpecified.Length; i++)
                    DT = Convert.ToDateTime(ColSpecified[i]);
                return typeof(DateTime);
            }
            catch
            {
                try
                {
                    UInt64 UI;
                    for (int i = 0; i < ColSpecified.Length; i++)
                        UI = Convert.ToUInt64(ColSpecified[i]);
                    return typeof(UInt64);
                }
                catch
                {
                    try
                    {
                        Int64 I;
                        for (int i = 0; i < ColSpecified.Length; i++)
                            I = Convert.ToInt64(ColSpecified[i]);
                        return typeof(Int64);
                    }
                    catch
                    {
                        try
                        {
                            double D;
                            for (int i = 0; i < ColSpecified.Length; i++)
                                D = Convert.ToDouble(ColSpecified[i]);
                            return typeof(Double);
                        }
                        catch
                        {
                            try
                            {
                                char c;
                                for (int i = 0; i < ColSpecified.Length; i++)
                                    c = Convert.ToChar(ColSpecified[i]);
                                return typeof(char);
                            }
                            catch
                            {
                                return typeof(string);
                            }
                        }
                    }
                }
            }
        }
    }

    #endregion
}
