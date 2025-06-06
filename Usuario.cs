using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AAVD
{
    public partial class Usuario : Form
    {
        private string contraseniaActual;

        public Usuario()
        {
            InitializeComponent();
        }

        private void AbrirControlEnPanel(System.Windows.Forms.UserControl control)
        {
            MenuContenedor.Controls.Clear();
            control.Dock = DockStyle.Fill;
            MenuContenedor.Controls.Add(control);
            control.BringToFront();
        }

        private void Usuario_Load(object sender, EventArgs e)
        {
            AbrirControlEnPanel(new Menu());
            var NuevoForm = new Login();

            if (Login.baseDatos == 2)
            {
                UsuarioActualTXT.Text = SesionUsuario.NombreUsuario;
                ReiniciarCampos();
                NumeroAleatorio();
                cargaTablaUsuarioCass();
            }
            
            this.FindForm().Size = NuevoForm.Size;
            this.FindForm().StartPosition = FormStartPosition.Manual;
            this.FindForm().Location = NuevoForm.Location;
        }

        private void ReiniciarCampos()
        {
            NombreTXT.Text = "";
            PrimerApellidoTXT.Text = "";
            SegundoApellidoTXT.Text  = "";
            TelefonoCelularTXT.Text = "";
            TelefonoCasaTXT.Text = "";
            CorreoTXT.Text = "";
            ContraseniaTXT.Text = "";
            ConfirmarContraseniaTXT.Text = "";
            TipoUsuario.SelectedIndex = -1;

            NumeroNominaSeleccionadoTXT.Text = "";
            NombreSeleccionadoTXT.Text = "";
            PrimerApellidoSeleccionadoTXT.Text = "";
            SegundoApellidoSeleccionadoTXT.Text = "";
            TelefonoCelularSeleccionadoTXT.Text = "";
            TelefonoCasaSeleccionadoTXT.Text = "";
            CorreoSeleccionadoTXT.Text = "";
            ContraseniaSeleccionadoTXT.Text = "";
            NombreCompletoCB.SelectedIndex = -1;
        }

        private void NumeroAleatorio()
        {
            Random rnd = new Random();
            int rand = rnd.Next(100000, 999999);
            NumeroNominaTXT.Text = rand.ToString();
        }

        private void AceptarBTN_Click(object sender, EventArgs e)
        {
            if (NumeroNominaTXT.Text == "" || NombreTXT.Text == "" || PrimerApellidoTXT.Text == "" || SegundoApellidoTXT.Text == "" ||
                TelefonoCelularTXT.Text == "" || TelefonoCasaTXT.Text == "" || CorreoTXT.Text == "" || ContraseniaTXT.Text == "")
            {
                MessageBox.Show("Por favor, complete todos los campos.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (TelefonoCelularTXT.Text.Length != 10 || TelefonoCasaTXT.Text.Length != 10)
            {
                MessageBox.Show("Cada número de teléfono debe tener exactamente 10 dígitos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!TelefonoCelularTXT.Text.All(char.IsDigit) || !TelefonoCasaTXT.Text.All(char.IsDigit))
            {
                MessageBox.Show("El número de teléfono debe contener solo números.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!Regex.IsMatch(CorreoTXT.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("El correo electrónico no es válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (ContraseniaTXT.Text != ConfirmarContraseniaTXT.Text)
            {
                MessageBox.Show("Las contraseñas no coinciden", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!Regex.IsMatch(ContraseniaTXT.Text, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[!""#$%&/=´?¡¿:;,\.\-_\+\*\{\}\[\]]).{8,}$"))
            {
                MessageBox.Show("La contraseña debe tener al menos 8 caracteres, incluyendo una mayúscula, una minúscula y un carácter especial.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var NumeroNomina = int.Parse(NumeroNominaTXT.Text);
            var Nombre = NombreTXT.Text;
            var PrimerApellido = PrimerApellidoTXT.Text;
            var SegundoApellido = SegundoApellidoTXT.Text;
            var TelefonoCelular = TelefonoCelularTXT.Text;
            var TelefonoCasa = TelefonoCasaTXT.Text;
            var Correo = CorreoTXT.Text;
            var Contrasenia = ContraseniaTXT.Text;
            var FechaRegistro = DateTime.Parse(FechaActualDTP.Text);
            var FechaModificacion = DateTime.Today;
            var Id_Admin = TipoUsuario.SelectedItem.ToString() == "Administrador" ? true : false;

            if (Login.baseDatos == 2)
            {
                int Tipo = Id_Admin ? 1 : 2;

                UsuarioDatos1 RegistrarUsuario = new UsuarioDatos1
                {
                    Id_Usuario = Guid.NewGuid(),
                    Id_Admin = Id_Admin,
                    NumeroNomina = NumeroNomina,
                    NombreUsuario = Nombre,
                    PrimerApellido = PrimerApellido,
                    SegundoApellido = SegundoApellido,
                    TelefonoCelular = TelefonoCelular,
                    TelefonoCasa = TelefonoCasa,
                    CorreoUsuario = Correo,
                    ContrasenaUsuario = Contrasenia,
                    FechaRegistroUsuario = FechaRegistro,
                    FechaModificacionUsuario = FechaModificacion
                };
                EnlaceCassandra enlace = new EnlaceCassandra();
                enlace.insertaUsuario(RegistrarUsuario);
                MessageBox.Show("Empleado agregado exitosamente.");

                InicioSesion RegistrarInicio = new InicioSesion
                {
                    correoUsuario = Correo,
                    contraseniaaUsuario = Contrasenia,
                    tipoUsuario = Tipo
                };
                EnlaceCassandra enlace1 = new EnlaceCassandra();
                enlace.insertaInicioSesion(RegistrarInicio);
                cargaTablaUsuarioCass();
            }
        }

        private void NombreCompletoCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            int seleccion = NombreCompletoCB.SelectedValue != null ? Convert.ToInt32(NombreCompletoCB.SelectedValue) : -1;
            if (seleccion < 0)
            {
                MessageBox.Show("Por favor, seleccione un usuario.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Login.baseDatos == 2)
            {
                var usuarioSeleccionado = NombreCompletoCB.SelectedItem as UsuarioDatos1;

                if (usuarioSeleccionado != null)
                {
                    NumeroNominaSeleccionadoTXT.Text = usuarioSeleccionado.NumeroNomina.ToString();
                    NombreSeleccionadoTXT.Text = usuarioSeleccionado.NombreUsuario;
                    PrimerApellidoSeleccionadoTXT.Text = usuarioSeleccionado.PrimerApellido;
                    SegundoApellidoSeleccionadoTXT.Text = usuarioSeleccionado.SegundoApellido;
                    TelefonoCelularSeleccionadoTXT.Text = usuarioSeleccionado.TelefonoCelular;
                    TelefonoCasaSeleccionadoTXT.Text = usuarioSeleccionado.TelefonoCasa;
                    CorreoSeleccionadoTXT.Text = usuarioSeleccionado.CorreoUsuario;
                    contraseniaActual = usuarioSeleccionado.ContrasenaUsuario;
                }
            }
        }

        private void EliminarBTN_Click(object sender, EventArgs e)
        {
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show("¿Está seguro de que desea eliminar el usuario?", "Eliminar Usuario", buttons, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                if (Login.baseDatos == 2)
                {
                    var usuarioSeleccionado = NombreCompletoCB.SelectedItem as UsuarioDatos1;

                    if (usuarioSeleccionado != null)
                    {
                        EnlaceCassandra enlace = new EnlaceCassandra();
                        enlace.eliminarUsuario(usuarioSeleccionado.NumeroNomina);

                        MessageBox.Show("Usuario eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        cargaTablaUsuarioCass();
                        ReiniciarCampos();
                    }
                    else
                    {
                        MessageBox.Show("Por favor, selecciona un usuario.", "Eliminar Usuario", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void ModificarBTN_Click(object sender, EventArgs e)
        {
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show("¿Está seguro de que desea modificar el usuario?", "Modificar Usuario", buttons, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                if (NumeroNominaSeleccionadoTXT.Text == "" || NombreSeleccionadoTXT.Text == "" || PrimerApellidoSeleccionadoTXT.Text == "" || SegundoApellidoSeleccionadoTXT.Text == "" ||
                    TelefonoCelularSeleccionadoTXT.Text == "" || TelefonoCasaSeleccionadoTXT.Text == "" || CorreoSeleccionadoTXT.Text == "" || ConfirmarContraseniaSeleccionadoTXT.Text == "")
                {
                    MessageBox.Show("Por favor, complete todos los campos.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!TelefonoCelularSeleccionadoTXT.Text.All(char.IsDigit) || !TelefonoCasaSeleccionadoTXT.Text.All(char.IsDigit))
                {
                    MessageBox.Show("El número de teléfono debe contener solo números.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (TelefonoCelularSeleccionadoTXT.Text.Length != 10 || TelefonoCasaSeleccionadoTXT.Text.Length != 10)
                {
                    MessageBox.Show("Cada número de teléfono debe tener exactamente 10 dígitos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var NumeroNomina = int.Parse(NumeroNominaSeleccionadoTXT.Text);
                var Nombre = NombreSeleccionadoTXT.Text;
                var PrimerApellido = PrimerApellidoSeleccionadoTXT.Text;
                var SegundoApellido = SegundoApellidoSeleccionadoTXT.Text;
                var TelefonoCelular = TelefonoCelularSeleccionadoTXT.Text;
                var TelefonoCasa = TelefonoCasaSeleccionadoTXT.Text;
                var Correo = CorreoSeleccionadoTXT.Text;
                var Contrasenia = ContraseniaSeleccionadoTXT.Text;
                var FechaModificacion = DateTime.Parse(FechaModificadoTXT.Text);

                if (Login.baseDatos == 2)
                {
                    var usuarioSeleccionado = NombreCompletoCB.SelectedItem as UsuarioDatos1;

                    if (usuarioSeleccionado != null)
                    {
                        usuarioSeleccionado.NumeroNomina = int.Parse(NumeroNominaSeleccionadoTXT.Text);
                        usuarioSeleccionado.NombreUsuario = NombreSeleccionadoTXT.Text;
                        usuarioSeleccionado.PrimerApellido = PrimerApellidoSeleccionadoTXT.Text;
                        usuarioSeleccionado.SegundoApellido = SegundoApellidoSeleccionadoTXT.Text;
                        usuarioSeleccionado.TelefonoCelular = TelefonoCelularSeleccionadoTXT.Text;
                        usuarioSeleccionado.TelefonoCasa = TelefonoCasaSeleccionadoTXT.Text;
                        usuarioSeleccionado.CorreoUsuario = CorreoSeleccionadoTXT.Text;
                        usuarioSeleccionado.ContrasenaUsuario = contraseniaActual; 
                        usuarioSeleccionado.FechaModificacionUsuario = FechaModificacion;

                        EnlaceCassandra enlace = new EnlaceCassandra();
                        enlace.actualizarUsuario(usuarioSeleccionado);

                        MessageBox.Show("Usuario actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        cargaTablaUsuarioCass();
                        ReiniciarCampos();
                    }
                }
            }
        }

        public void cargaTablaUsuarioCass()
        {
            EnlaceCassandra enlace = new EnlaceCassandra();
            var listaUsuarios = enlace.GetAllUsuarios();

            NombreCompletoCB.DisplayMember = "nombreCompleto";
            NombreCompletoCB.ValueMember = "numeroNomina";
            NombreCompletoCB.DataSource = listaUsuarios;

            TablaRegistroUsuarioDGV.DataSource = listaUsuarios;

            TablaRegistroUsuarioDGV.Columns["numeroNomina"].Visible = true;
            TablaRegistroUsuarioDGV.Columns["nombreUsuario"].Visible = true;
            TablaRegistroUsuarioDGV.Columns["primerApellido"].Visible = true;
            TablaRegistroUsuarioDGV.Columns["segundoApellido"].Visible = true;
            TablaRegistroUsuarioDGV.Columns["fechaRegistroUsuario"].Visible = true;
            TablaRegistroUsuarioDGV.Columns["fechaModificacionUsuario"].Visible = true;
            TablaRegistroUsuarioDGV.Columns["correoUsuario"].Visible = true;
            TablaRegistroUsuarioDGV.Columns["contrasenaUsuario"].Visible = false;
            TablaRegistroUsuarioDGV.Columns["telefonoCelular"].Visible = true;
            TablaRegistroUsuarioDGV.Columns["telefonoCasa"].Visible = true;
            TablaRegistroUsuarioDGV.Columns["nombreCompleto"].Visible = false;
            TablaRegistroUsuarioDGV.Columns["Id_Usuario"].Visible = false;
            TablaRegistroUsuarioDGV.Columns["Id_Admin"].Visible = false;

            TablaRegistroUsuarioDGV.Columns["numeroNomina"].HeaderText = "Numero de nomina";
            TablaRegistroUsuarioDGV.Columns["nombreUsuario"].HeaderText = "Nombre";
            TablaRegistroUsuarioDGV.Columns["primerApellido"].HeaderText = "Primer apellido";
            TablaRegistroUsuarioDGV.Columns["segundoApellido"].HeaderText = "Segundo apellido";
            TablaRegistroUsuarioDGV.Columns["fechaRegistroUsuario"].HeaderText = "Fecha de registro";
            TablaRegistroUsuarioDGV.Columns["fechaModificacionUsuario"].HeaderText = "Fecha de modificacion";
            TablaRegistroUsuarioDGV.Columns["correoUsuario"].HeaderText = "Correo electrónico";
            TablaRegistroUsuarioDGV.Columns["telefonoCelular"].HeaderText = "Telefono celular";
            TablaRegistroUsuarioDGV.Columns["telefonoCasa"].HeaderText = "Teléfono de casa";

            TablaRegistroUsuarioDGV.Columns["numeroNomina"].DisplayIndex = 0;
            TablaRegistroUsuarioDGV.Columns["nombreUsuario"].DisplayIndex = 1;
            TablaRegistroUsuarioDGV.Columns["primerApellido"].DisplayIndex = 2;
            TablaRegistroUsuarioDGV.Columns["segundoApellido"].DisplayIndex = 3;
            TablaRegistroUsuarioDGV.Columns["correoUsuario"].DisplayIndex = 4;
            TablaRegistroUsuarioDGV.Columns["contrasenaUsuario"].DisplayIndex = 5;
            TablaRegistroUsuarioDGV.Columns["telefonoCelular"].DisplayIndex = 6;
            TablaRegistroUsuarioDGV.Columns["telefonoCasa"].DisplayIndex = 7;
            TablaRegistroUsuarioDGV.Columns["fechaRegistroUsuario"].DisplayIndex = 8;
            TablaRegistroUsuarioDGV.Columns["fechaModificacionUsuario"].DisplayIndex = 9;

            TablaModificarUsuarioDGV.DataSource = listaUsuarios;

            TablaModificarUsuarioDGV.Columns["numeroNomina"].Visible = true;
            TablaModificarUsuarioDGV.Columns["nombreUsuario"].Visible = true;
            TablaModificarUsuarioDGV.Columns["primerApellido"].Visible = true;
            TablaModificarUsuarioDGV.Columns["segundoApellido"].Visible = true;
            TablaModificarUsuarioDGV.Columns["fechaRegistroUsuario"].Visible = true;
            TablaModificarUsuarioDGV.Columns["fechaModificacionUsuario"].Visible = true;
            TablaModificarUsuarioDGV.Columns["correoUsuario"].Visible = true;
            TablaModificarUsuarioDGV.Columns["contrasenaUsuario"].Visible = false;
            TablaModificarUsuarioDGV.Columns["telefonoCelular"].Visible = true;
            TablaModificarUsuarioDGV.Columns["telefonoCasa"].Visible = true;
            TablaModificarUsuarioDGV.Columns["nombreCompleto"].Visible = false;
            TablaModificarUsuarioDGV.Columns["Id_Usuario"].Visible = false;
            TablaModificarUsuarioDGV.Columns["Id_Admin"].Visible = false;

            TablaModificarUsuarioDGV.Columns["numeroNomina"].HeaderText = "Número de nómina";
            TablaModificarUsuarioDGV.Columns["nombreUsuario"].HeaderText = "Nombre";
            TablaModificarUsuarioDGV.Columns["primerApellido"].HeaderText = "Primer apellido";
            TablaModificarUsuarioDGV.Columns["segundoApellido"].HeaderText = "Segundo apellido";
            TablaModificarUsuarioDGV.Columns["fechaRegistroUsuario"].HeaderText = "Fecha de registro";
            TablaModificarUsuarioDGV.Columns["fechaModificacionUsuario"].HeaderText = "Fecha de modificación";
            TablaModificarUsuarioDGV.Columns["correoUsuario"].HeaderText = "Correo electrónico";
            TablaModificarUsuarioDGV.Columns["telefonoCelular"].HeaderText = "Teléfono celular";
            TablaModificarUsuarioDGV.Columns["telefonoCasa"].HeaderText = "Teléfono de casa";

            TablaModificarUsuarioDGV.Columns["numeroNomina"].DisplayIndex = 0;
            TablaModificarUsuarioDGV.Columns["nombreUsuario"].DisplayIndex = 1;
            TablaModificarUsuarioDGV.Columns["primerApellido"].DisplayIndex = 2;
            TablaModificarUsuarioDGV.Columns["segundoApellido"].DisplayIndex = 3;
            TablaModificarUsuarioDGV.Columns["correoUsuario"].DisplayIndex = 4;
            TablaModificarUsuarioDGV.Columns["contrasenaUsuario"].DisplayIndex = 5;
            TablaModificarUsuarioDGV.Columns["telefonoCelular"].DisplayIndex = 6;
            TablaModificarUsuarioDGV.Columns["telefonoCasa"].DisplayIndex = 7;
            TablaModificarUsuarioDGV.Columns["fechaRegistroUsuario"].DisplayIndex = 8;
            TablaModificarUsuarioDGV.Columns["fechaModificacionUsuario"].DisplayIndex = 9;
        }
    }
}