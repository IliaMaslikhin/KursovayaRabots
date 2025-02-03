using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace KursovayaRabots
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ConnectionForm());
        }
    }
}


