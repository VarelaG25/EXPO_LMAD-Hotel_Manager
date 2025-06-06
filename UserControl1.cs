using System;
using System.Windows.Forms;

namespace AAVD
{
    public partial class Menu : UserControl
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void CerrarVentanaActual()
        {
            Form ventanaActual = this.FindForm();
            ventanaActual.Hide();
        }

        private void BTN_CLIENTES_Click(object sender, EventArgs e)
        {
            var NuevoForm = new Clientes();
            NuevoForm.Show();
            CerrarVentanaActual();
        }

        private void BTN_REP_Click(object sender, EventArgs e)
        {
            if (Login.tipoUsuario == 1)
            {
                var NuevoForm = new Reportes();
                NuevoForm.Show();
                CerrarVentanaActual();
            }
            else
                MessageBox.Show("No tiene permisos para acceder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void BTN_USUARIO_Click(object sender, EventArgs e)
        {
            if (Login.tipoUsuario == 1)
            {
                var NuevoForm = new Usuario();
                NuevoForm.Show();
                CerrarVentanaActual();
            }
            else
                MessageBox.Show("No tiene permisos para acceder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void BTN_HOTEL_Click(object sender, EventArgs e)
        {
            if (Login.tipoUsuario == 1)
            {
                var NuevoForm = new Hoteles();
                NuevoForm.Show();
                CerrarVentanaActual();
            }
            else
                MessageBox.Show("No tiene permisos para acceder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void BTN_RESER_Click(object sender, EventArgs e)
        {
            var NuevoForm = new Reservaciones();
            NuevoForm.Show();
            CerrarVentanaActual();
        }

        private void Menu_Load(object sender, EventArgs e)
        {
        }

        private void BTN_INICIO_Click(object sender, EventArgs e)
        {
            var NuevoForm = new Factura();
            NuevoForm.Show();
            CerrarVentanaActual();
        }

        private void BTN_SALIR_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}