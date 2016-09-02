using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

namespace test5
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Form1 F1 = new Form1();
            F1.StartPosition = FormStartPosition.Manual;
            F1.Top = 220;
            F1.Left = 650;
            //Size s = new Size(200,200);   
            //F1.Size = s;
            F1.ShowDialog();
        }
    }
}
