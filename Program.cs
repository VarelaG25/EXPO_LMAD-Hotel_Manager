using System;
using System.Windows.Forms;

namespace AAVD
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Login login = new Login();
            if (login.ShowDialog() == DialogResult.OK)
            {
                Application.Run(new Clientes());
            }
            else
            {
                MessageBox.Show("El inicio de sesión fue cancelado o fallido.", "Fin del programa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
        }
    }
}