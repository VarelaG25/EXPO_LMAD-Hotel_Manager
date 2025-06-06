using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AAVD
{
    public partial class Clientes : Form
    {
        private void AbrirControlEnPanel(System.Windows.Forms.UserControl control)
        {
            MenuContenedor.Controls.Clear();
            control.Dock = DockStyle.Fill;
            MenuContenedor.Controls.Add(control);
            control.BringToFront();
        }

        public Clientes()
        {
            InitializeComponent();
        }

        private void Clientes_Load_1(object sender, EventArgs e)
        {
            AbrirControlEnPanel(new Menu());
            var NuevoForm = new Login();

            if (Login.baseDatos == 2)
            {
                ReiniciarCliente();
                CargarTablasCassandra();
            }
            this.FindForm().Size = NuevoForm.Size;
            this.FindForm().StartPosition = FormStartPosition.Manual;
            this.FindForm().Location = NuevoForm.Location;
        }

        private void ReiniciarCliente()
        {
            PaisCB.Text = "";
            EstadoCB.Text = "";
            CiudadCB.Text = "";
            CodigoPostalTXT.Text = "";
            TelefonoCasaTXT.Text = "";
            TelefonoCelularTXT.Text = "";
            RFCTXT.Text = "";
            CorreoTXT.Text = "";
            NombreTXT.Text = "";
            PrimerApellidoTXT.Text = "";
            SegundoApellidoTXT.Text = "";
            EstadoCivilCB.SelectedIndex = 0;
            FechaNacimientoDTP.Value = DateTime.Now;

            NombreSeleccionado.Text = "";
            PrimerApellidoSeleccionado.Text = "";
            SegundoApellidoSeleccionado.Text = "";
            TelefonoCasaSeleccionado.Text = "";
            TelefonoCelularSeleccionado.Text = "";
            RFCSeleccionado.Text = "";
            CorreoSeleccionado.Text = "";

            FechaNacimientoSeleccionada.Value = DateTime.Now;
        }

        private void AceptarBTN_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PaisCB.Text) || string.IsNullOrEmpty(EstadoCB.Text) || string.IsNullOrEmpty(CiudadCB.Text)
                || string.IsNullOrEmpty(CodigoPostalTXT.Text) || string.IsNullOrEmpty(TelefonoCasaTXT.Text) || string.IsNullOrEmpty(TelefonoCelularTXT.Text)
                || string.IsNullOrEmpty(RFCTXT.Text) || string.IsNullOrEmpty(CorreoTXT.Text) || string.IsNullOrEmpty(NombreTXT.Text) || string.IsNullOrEmpty(PrimerApellidoTXT.Text)
                || string.IsNullOrEmpty(SegundoApellidoTXT.Text) || string.IsNullOrEmpty(FechaNacimientoDTP.Text) || string.IsNullOrEmpty(EstadoCivilCB.Text))
            {
                MessageBox.Show("Por favor, complete todos los campos.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (TelefonoCelularTXT.Text.All(char.IsDigit) == false || TelefonoCasaTXT.Text.All(char.IsDigit) == false)
            {
                MessageBox.Show("El número de teléfono debe contener solo números.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (TelefonoCasaTXT.Text.Length != 10 && TelefonoCelularTXT.Text.Length != 10)
            {
                MessageBox.Show("El número de teléfono debe tener exactamente 10 dígitos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!Regex.IsMatch(CorreoTXT.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("El correo electrónico no es válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var pais = PaisCB.Text;
            var estado = EstadoCB.Text;
            var ciudad = CiudadCB.Text;
            var codigoPostal = CodigoPostalTXT.Text;
            var telefonoCasa = TelefonoCasaTXT.Text;
            var telefonoCelular = TelefonoCelularTXT.Text;
            var rfc = RFCTXT.Text;
            var correo = CorreoTXT.Text;
            var nombre = NombreTXT.Text;
            var primerApellido = PrimerApellidoTXT.Text;
            var segundoApellido = SegundoApellidoTXT.Text;
            var fechaNacimiento = FechaNacimientoDTP.Value;
            var fechaRegistro = DateTime.Parse(FechaRegistroDTP.Text);
            var estadoCivil = EstadoCivilCB.Text;
            if (estadoCivil == "Soltero") estadoCivil = "S";
            else if (estadoCivil == "Casado") estadoCivil = "C";
            else if (estadoCivil == "Divorciado") estadoCivil = "D";
            else if (estadoCivil == "Viudo") estadoCivil = "V";

            if (Login.baseDatos == 2)
            {
                int CP;

                if (int.TryParse(codigoPostal, out CP)) Console.WriteLine("Número convertido: " + CP);
                else
                {
                    Console.WriteLine("No se pudo convertir.");
                }
                ClienteDatos1 RegistrarCliente = new ClienteDatos1
                {
                    Id_Cliente = Guid.NewGuid(),
                    Nombre = nombre,
                    PrimerApellido = primerApellido,
                    SegundoApellido = segundoApellido,
                    FechaNacimientoCliente = fechaNacimiento,
                    TelefonoCasa = new Dictionary<int, string> { { 1, telefonoCasa } },
                    TelefonoCelular = new Dictionary<int, string> { { 1, telefonoCelular } },
                    EstadoCivil = estadoCivil,
                    RFC = rfc,
                    Correo = new HashSet<string> { correo },
                    FechaRegistro = fechaRegistro,
                    FechaModificacion = fechaRegistro,
                    Id_Ubicacion = Guid.NewGuid(),
                    Ciudad = ciudad,
                    Estado = estado,
                    Pais = pais,
                    CodigoPostal = CP
                };
                EnlaceCassandra enlace = new EnlaceCassandra();
                enlace.insertaCliente(RegistrarCliente);
                MessageBox.Show("Empleado agregado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarTablasCassandra();
                ReiniciarCliente();
            }
        }

        private void ClienteCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Login.baseDatos == 2)
            {
                var clienteSeleccionado = ClienteCB.SelectedItem as ClienteDatos1;

                if (clienteSeleccionado != null)
                {
                    NombreSeleccionado.Text = clienteSeleccionado.Nombre;
                    PrimerApellidoSeleccionado.Text = clienteSeleccionado.PrimerApellido;
                    SegundoApellidoSeleccionado.Text = clienteSeleccionado.SegundoApellido;

                    TelefonoCasaSeleccionado.Text = clienteSeleccionado.TelefonoCasa?.FirstOrDefault().Value ?? "";
                    TelefonoCelularSeleccionado.Text = clienteSeleccionado.TelefonoCelular?.FirstOrDefault().Value ?? "";

                    RFCSeleccionado.Text = clienteSeleccionado.RFC;
                    CorreoSeleccionado.Text = clienteSeleccionado.Correo?.FirstOrDefault() ?? "";
                    FechaNacimientoSeleccionada.Value = clienteSeleccionado.FechaNacimientoCliente;

                    PaisSeleccionado.Text = clienteSeleccionado.Pais;
                    EstadoSeleccionado.Text = clienteSeleccionado.Estado;
                    CiudadSeleccionado.Text = clienteSeleccionado.Ciudad;
                    CodigoPostalSeleccionado.Text = clienteSeleccionado.CodigoPostal.ToString();
                }
            }
        }

        private void ModificarBTN_Click(object sender, EventArgs e)
        {
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show("¿Está seguro de que desea modificar el cliente?", "Modificar Cliente", buttons, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (string.IsNullOrEmpty(NombreSeleccionado.Text) || string.IsNullOrEmpty(PrimerApellidoSeleccionado.Text) || string.IsNullOrEmpty(SegundoApellidoSeleccionado.Text) ||
                    string.IsNullOrEmpty(TelefonoCasaSeleccionado.Text) || string.IsNullOrEmpty(TelefonoCelularSeleccionado.Text) || string.IsNullOrEmpty(RFCSeleccionado.Text) ||
                    string.IsNullOrEmpty(CorreoSeleccionado.Text))
                {
                    MessageBox.Show("Por favor, complete todos los campos.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!TelefonoCasaSeleccionado.Text.All(char.IsDigit) || !TelefonoCelularSeleccionado.Text.All(char.IsDigit))
                {
                    MessageBox.Show("El número de teléfono debe contener solo números.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (TelefonoCelularSeleccionado.Text.Length != 10 && TelefonoCasaSeleccionado.Text.Length != 10)
                {
                    MessageBox.Show("El número de teléfono debe tener exactamente 10 dígitos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!Regex.IsMatch(CorreoSeleccionado.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    MessageBox.Show("El correo electrónico no es válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var nombre = NombreSeleccionado.Text;
                var primerApellido = PrimerApellidoSeleccionado.Text;
                var segundoApellido = SegundoApellidoSeleccionado.Text;
                var telefonoCasa = TelefonoCasaSeleccionado.Text;
                var telefonoCelular = TelefonoCelularSeleccionado.Text;
                var rfc = RFCSeleccionado.Text;
                var correo = CorreoSeleccionado.Text;
                var fechaNacimiento = FechaNacimientoSeleccionada.Value;
                var fechaModificacion = FechaNacimientoSeleccionada.Value;

                if (Login.baseDatos == 2)
                {
                    var clienteSeleccionado = ClienteCB.SelectedItem as ClienteDatos1;

                    if (clienteSeleccionado != null)
                    {
                        clienteSeleccionado.Nombre = NombreSeleccionado.Text.Trim();
                        clienteSeleccionado.PrimerApellido = PrimerApellidoSeleccionado.Text.Trim();
                        clienteSeleccionado.SegundoApellido = SegundoApellidoSeleccionado.Text.Trim();
                        clienteSeleccionado.RFC = RFCSeleccionado.Text.Trim();
                        clienteSeleccionado.Correo = new HashSet<string> { CorreoSeleccionado.Text.Trim() };
                        clienteSeleccionado.FechaNacimientoCliente = FechaNacimientoSeleccionada.Value;
                        clienteSeleccionado.Pais = PaisSeleccionado.Text.Trim();
                        clienteSeleccionado.Estado = EstadoSeleccionado.Text.Trim();
                        clienteSeleccionado.Ciudad = CiudadSeleccionado.Text.Trim();
                        clienteSeleccionado.CodigoPostal = int.Parse(CodigoPostalSeleccionado.Text.Trim());

                        clienteSeleccionado.TelefonoCasa = new Dictionary<int, string> { { 1, TelefonoCasaSeleccionado.Text.Trim() } };
                        clienteSeleccionado.TelefonoCelular = new Dictionary<int, string> { { 1, TelefonoCelularSeleccionado.Text.Trim() } };
                        clienteSeleccionado.FechaModificacion = DateTime.Now;

                        EnlaceCassandra enlace = new EnlaceCassandra();
                        enlace.actualizarCliente(clienteSeleccionado);

                        MessageBox.Show("Cliente modificado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CargarTablasCassandra();
                        ReiniciarCliente();
                        ClienteCB.SelectedIndex = -1;
                    }
                }
            }
        }

        private void BorrarBTN_Click(object sender, EventArgs e)
        {
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show("¿Está seguro de que desea borrar al cliente?", "Borrar Cliente", buttons, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (Login.baseDatos == 2)
                {
                    var clienteSeleccionado = ClienteCB.SelectedItem as ClienteDatos1;
                    EnlaceCassandra enlace = new EnlaceCassandra();
                    enlace.eliminarCliente(clienteSeleccionado.Id_Cliente);

                    MessageBox.Show("Cliente eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    CargarTablasCassandra();
                    ReiniciarCliente();
                    ClienteCB.SelectedIndex = -1;
                }
            }
        }

        public void CargarTablasCassandra()
        {
            UsuarioActualTXT.Text = SesionUsuario.NombreUsuario;
            try
            {
                EnlaceCassandra enlace = new EnlaceCassandra();
                var listaClientes = enlace.Get_All_Clientes();

                EstadoCivilCB.SelectedIndex = 0;
                try
                {
                    ClienteCB.DataSource = listaClientes;
                    ClienteCB.DisplayMember = "NombreCompleto";
                    ClienteCB.ValueMember = "Id_Cliente";

                    CiudadCB.DataSource = listaClientes
                        .GroupBy(c => c.Ciudad)
                        .Select(g => g.First())
                        .ToList();

                    CiudadCB.DisplayMember = "Ciudad";
                    CiudadCB.ValueMember = "ciudad";

                    EstadoCB.DataSource = listaClientes
                    .GroupBy(c => c.Estado)
                    .Select(g => g.First())
                    .ToList();
                    EstadoCB.DisplayMember = "Estado";
                    EstadoCB.ValueMember = "estado";

                    PaisCB.DataSource = listaClientes
                    .GroupBy(c => c.Pais)
                    .Select(g => g.First())
                    .ToList();

                    PaisCB.DisplayMember = "Pais";
                    PaisCB.ValueMember = "pais";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar clientes en el ComboBox: " + ex.Message, "Eror", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // Cargar datos en ambos DataGridView
                TablaClientesDTG.DataSource = null;
                TablaClientesDTG.DataSource = listaClientes;

                TablaClientesRegistradosDTG.DataSource = null;
                TablaClientesRegistradosDTG.DataSource = listaClientes;

                // Configurar columnas personalizadas para TablaClientesDTG
                TablaClientesDTG.Columns["NombreCompleto"].HeaderText = "Nombre del cliente";
                TablaClientesDTG.Columns["fechanacimientocliente"].HeaderText = "Fecha de nacimiento";
                TablaClientesDTG.Columns["estadocivil"].HeaderText = "Estado Civil";
                TablaClientesDTG.Columns["rfc"].HeaderText = "RFC";
                TablaClientesDTG.Columns["fecharegistro"].HeaderText = "Fecha de registro";
                TablaClientesDTG.Columns["CorreoMostrar"].HeaderText = "Correo electrónico";
                TablaClientesDTG.Columns["TelefonoCasaMostrar"].HeaderText = "Telefono de casa";
                TablaClientesDTG.Columns["TelefonoCelularMostrar"].HeaderText = "Telefono celular";
                TablaClientesDTG.Columns["TelefonoCelularMostrar"].HeaderText = "Telefono celular";
                TablaClientesDTG.Columns["CodigoPostal"].HeaderText = "Codigo Postal";

                TablaClientesDTG.Columns["rfc"].DisplayIndex = 0;
                TablaClientesDTG.Columns["NombreCompleto"].DisplayIndex = 1;
                TablaClientesDTG.Columns["primerapellido"].DisplayIndex = 2;
                TablaClientesDTG.Columns["segundoapellido"].DisplayIndex = 3;
                TablaClientesDTG.Columns["estadocivil"].DisplayIndex = 4;
                TablaClientesDTG.Columns["fechanacimientocliente"].DisplayIndex = 5;
                TablaClientesDTG.Columns["CorreoMostrar"].DisplayIndex = 6;
                TablaClientesDTG.Columns["TelefonoCasaMostrar"].DisplayIndex = 7;
                TablaClientesDTG.Columns["TelefonoCelularMostrar"].DisplayIndex = 8;
                TablaClientesDTG.Columns["fechaRegistro"].DisplayIndex = 9;
                TablaClientesDTG.Columns["Ciudad"].DisplayIndex = 10;
                TablaClientesDTG.Columns["Estado"].DisplayIndex = 11;
                TablaClientesDTG.Columns["Pais"].DisplayIndex = 12;
                TablaClientesDTG.Columns["CodigoPostal"].DisplayIndex = 13;

                string[] columnasOcultas = new string[]
                {
                    "fechamodificacion", "id_cliente", "id_ubicacion",
                    "Correo", "nombre", "primerapellido", "segundoapellido",
                    "TelefonoCasa", "TelefonoCelular"
                };

                foreach (string col in columnasOcultas)
                {
                    if (TablaClientesDTG.Columns.Contains(col))
                        TablaClientesDTG.Columns[col].Visible = false;
                }

                TablaClientesDTG.Columns["fechanacimientocliente"].DefaultCellStyle.Format = "yyyy-MM-dd";

                foreach (DataGridViewColumn col in TablaClientesDTG.Columns)
                {
                    if (TablaClientesRegistradosDTG.Columns.Contains(col.Name))
                    {
                        var colDestino = TablaClientesRegistradosDTG.Columns[col.Name];
                        colDestino.HeaderText = col.HeaderText;
                        colDestino.DisplayIndex = col.DisplayIndex;
                        colDestino.Visible = col.Visible;
                        colDestino.DefaultCellStyle.Format = col.DefaultCellStyle.Format;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar clientes: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}