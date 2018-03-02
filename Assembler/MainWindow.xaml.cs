using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Assembler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string curFile;
        int counter = 1;
        Dictionary<string, int> symTable = new Dictionary<string, int>();
        Dictionary<string, string> opcode = new Dictionary<string, string>()
        {
            {"AND", "0000" }, {"ADD", "1000" }, {"LDA", "2000" }, {"STA", "3000" }, {"BUN", "4000" },
            {"BSA", "5000" },{"ISZ", "6000" }, {"CLA", "7800" }, {"CLE", "7400" }, {"CMA", "7200" },
            {"CME", "7100" }, {"CIR", "7080" }, {"CIL", "7040" }, {"INC", "7020" }, {"SPA", "7010" },
            {"SNA", "7008" }, {"SZA", "7004" }, {"SZE", "7002" }, {"HLT", "7001" }, {"INP", "F800" },
            {"OUT", "F400" }, {"SKI", "F200" }, {"SKO", "F100" }, {"ION", "F080" }, {"IOF", "F040" },
            {"DEC", "0000" },{"HEX", "0000" }
        };


        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.  
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "ASM Files|*.asm";
            openFileDialog.Title = "Select an ASM File";

            // Show the Dialog.  
            // If the user clicked OK in the dialog and  
            // a .ASM file was selected, open it.
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                FilePath.Text = openFileDialog.FileName;
                curFile = System.IO.File.ReadAllText(openFileDialog.FileName, Encoding.UTF8);
                LoadedFile.Text = curFile;
                AssembleButton.IsEnabled = true;
            }
        }

        private void Assemble_Click(object sender, RoutedEventArgs e)
        {
            //first pass
            using (StringReader reader = new StringReader(curFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    
                    if(line.Substring(5,3) == "ORG")
                    {
                        //May have to account for varying number of memory lengths (i.e. 100 10 1) but won't worry about this for now
                        counter = int.Parse(line.Substring(9, 3), System.Globalization.NumberStyles.HexNumber);
                        counter--;
                    }
                    //Create symbol table
                    //Look for symbols
                    if(line[3] == ',')
                    {
                        symTable.Add(line.Substring(0, 3), counter);
                        SymbolTable.Text += line.Substring(0, 3) + " " + counter.ToString("X") + "\n"; //X is for hex
                    }
                    counter++;

                }
            }
            //reset counter
            counter = 1;
            //second pass
            using (StringReader reader = new StringReader(curFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    bool memRef = false;
                    if (line.Substring(5, 3) == "ORG")
                    {
                        //May have to account for varying number of memory lengths (i.e. 100 10 1) but won't worry about this for now
                        counter = int.Parse(line.Substring(9, 3), System.Globalization.NumberStyles.HexNumber);
                        counter--;
                    }
                    else
                    {
                        if(line.Substring(5,3) != "END")
                        {
                            string instruction = line.Substring(5, 3);
                            string hexOpcode = opcode[instruction];
                            int opcodeInt;
                            int operand;
                            string machineCode;

                            //get opcode
                            if(line.Length > 13 && line[13] == 'I')
                            {
                                    opcodeInt = int.Parse(hexOpcode, System.Globalization.NumberStyles.HexNumber) | 32768;
                                    memRef = true;
                            }
                            else
                            {
                                opcodeInt = int.Parse(hexOpcode, System.Globalization.NumberStyles.HexNumber);
                            }


                            //get operand
                            string num;
                            if (memRef) { num = line.Substring(9).Split('I')[0].Trim(); }
                            else { num = line.Substring(9).Split('/')[0].Trim(); }

                            if (symTable.ContainsKey(num))
                            {
                                operand = symTable[num];
                            }
                            else if (instruction == "DEC" || instruction == "HEX")
                            {
                                if (instruction == "DEC")
                                {
                                   
                                    operand = int.Parse(num);
                                }
                                else
                                {
                                    operand = int.Parse(num, System.Globalization.NumberStyles.HexNumber);
                                }
                            }
                            else
                            {
                                operand = 0000;
                            }

                            machineCode = Padding((operand | opcodeInt).ToString("X"));

                            AssembleFile.Text += counter.ToString("X") + ": " + machineCode + "\n";
                        }  

                    }
                    counter++;
                }
            }


        }

        private string Padding(String s)
        {
            string padded = "";
            int length = s.Length;
            if(length > 4)
            {
                //Get the last 4 characters
                padded = s.Substring(length - 4);
            }
            else
            {
                for(int i=length; i < 4; i++)
                {
                    padded += "0";
                }
                padded += s;
            }
            return padded;
        }
    }
}
