using System;
using System.Linq;
using System.Windows.Forms;

namespace AAVD
{
    public partial class Login : Form
    {
        public Guid IdUsuarioLogueado { get; private set; }

        public static int tipoUsuario { get; set; }
        public static int baseDatos = 2;

        public Login()
        {
            InitializeComponent();
        }

        public class LoginUsuario
        {
            public string Usuario { get; set; }
            public string Contrasena { get; set; }
        }

        private void Login_Usuario_TextChanged(object sender, EventArgs e)
        {
            label3.Visible = false;
        }

        private void Login_Contrasena_TextChanged(object sender, EventArgs e)
        {
            label2.Visible = false;
        }

        private void Login_Continuar_Click(object sender, EventArgs e)
        {
            //string usuario = Login_Usuario.Text.Trim();
            //string contrasena = Login_Contrasena.Text.Trim();

            string usuario = "juan@example.com";
            string contrasena = "1234";

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
            {
                MessageBox.Show("Por favor, complete todos los campos.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Login.baseDatos = 2;

            if (Login.baseDatos == 2)             {
                try
                {
                    var enlace = new EnlaceCassandra();
                    var usuarioCQL = enlace.Get_One_Login(usuario)?.FirstOrDefault();

                    if (usuarioCQL == null || usuarioCQL.contraseniaaUsuario != contrasena)
                    {
                        MessageBox.Show("Usuario o contraseña incorrectos", "Error de autenticación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    tipoUsuario = usuarioCQL.tipoUsuario;

                    var usuarioDatos = enlace.GetUsuarioPorCorreo(usuario);
                    if (usuarioDatos == null)
                    {
                        MessageBox.Show("No se encontró información adicional del usuario.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    SesionUsuario.IdUsuarioActual = usuarioDatos.Id_Usuario;
                    SesionUsuario.NombreUsuario = usuarioDatos.NombreUsuario;
                    this.IdUsuarioLogueado = usuarioDatos.Id_Usuario;

                    this.DialogResult = DialogResult.OK;
                    this.Hide();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}