using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AAVD
{
    public partial class Reservaciones : Form
    {
        private Guid idUsuario = SesionUsuario.IdUsuarioActual;
        private List<Hotel1> listaHoteles = new List<Hotel1>();
        private List<ResumenHoteles> listaResumenHoteles = new List<ResumenHoteles>();
        private List<TipoHabitacion> listaTipoHabitaciones = new List<TipoHabitacion>();
        private List<Reservacion1> reservas = new List<Reservacion1>();
        private List<ClienteLigero> clientes;

        public bool habitacionDisponible = false;

        public Reservaciones()
        {
            InitializeComponent();
        }

        public class ServicioAdicional
        {
            public int Id_ServicioAdicional { get; set; }
            public string nombreServicio { get; set; }
            public decimal precio { get; set; }
        }

        private void AbrirControlEnPanel(System.Windows.Forms.UserControl control)
        {
            MenuContenedor.Controls.Clear();
            control.Dock = DockStyle.Fill;
            MenuContenedor.Controls.Add(control);
            control.BringToFront();
        }

        private void Reservaciones_Load(object sender, EventArgs e)
        {
            AbrirControlEnPanel(new Menu());
            var NuevoForm = new Login();
            TotalTXT.Text = "0";
            AnticipoTXT.Text = "0";

            if (Login.baseDatos == 2)
            {
                UsuarioActualTXT.Text = SesionUsuario.NombreUsuario;
                CargarTablasCass();
                LlenarCombosClientes();
                MostrarClientesEnTabla();
                LlenarReservaCheck();
                MostrarCheckIn();
                CargarCheckOuts();
            }
            this.FindForm().Size = NuevoForm.Size;
            this.FindForm().StartPosition = FormStartPosition.Manual;
            this.FindForm().Location = NuevoForm.Location;
        }

        private void HotelCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Login.baseDatos == 2)
            {
                if (HotelCB.SelectedItem is Hotel1 hotelSeleccionado)
                {
                    Guid idHotel = hotelSeleccionado.id_hotel;
                    var resumen = listaResumenHoteles.FirstOrDefault(r => r.Id_Hotel == hotelSeleccionado.id_hotel);

                    if (resumen != null)
                    {
                        PaisTXT.Text = resumen.Pais;
                        EstadoTXT.Text = resumen.Estado;
                        CiudadTXT.Text = resumen.Ciudad;
                    }
                    else
                    {
                        PaisTXT.Text = "";
                        EstadoTXT.Text = "";
                        CiudadTXT.Text = "";
                    }

                    EnlaceCassandra enlace = new EnlaceCassandra();
                    var tipos = enlace.GetDistribucionPorHotel(idHotel);

                    TipoHabCB.DataSource = tipos;
                    TipoHabCB.DisplayMember = "Nombre_Tipo";
                    TipoHabCB.ValueMember = "Id_tipo_hab";

                    TipoHabReservaCB.DataSource = new BindingSource(tipos, null);
                    TipoHabReservaCB.DisplayMember = "nombre_tipo";
                    TipoHabReservaCB.ValueMember = "id_tipo_hab";

                    CargarServiciosAdicionales(idHotel);
                }
            }
        }

        private void TipoHabCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Login.baseDatos == 2)
            {
                if (TipoHabCB.SelectedValue != null)
                    TipoHabReservaCB.SelectedValue = TipoHabCB.SelectedValue;

                if (TipoHabCB.SelectedValue == null)
                    return;

                Guid idTipoHab;
                if (TipoHabCB.SelectedValue != null && Guid.TryParse(TipoHabCB.SelectedValue.ToString(), out idTipoHab))
                {
                    EnlaceCassandra enlace = new EnlaceCassandra();
                    var habitacion = enlace.ObtenerTipoHabitacionPorId(idTipoHab);

                    if (habitacion != null)
                    {
                        AmenidadTXT.Text = habitacion.Amenidades;
                        CaracteristicaTXT.Text = habitacion.Caracteristicas;
                        PrecioXNocheTXT.Text = habitacion.Precio.ToString("F2");
                        PrecioTXT.Text = habitacion.Precio.ToString("F2");
                        PersonasTXT.Text = habitacion.NumeroPersonas.ToString();
                    }
                    else
                    {
                        MessageBox.Show("No se encontró información para esta habitación.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ReservarBTN_Click(object sender, EventArgs e)
        {
            if (habitacionDisponible == false)
            {
                MessageBox.Show("No hay habitaciones disponibles, pruebe intentar otro tipo o fecha.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Guid guid = Guid.NewGuid();
            string guidStr = guid.ToString("N");
            string codigoReserva = guidStr.Substring(guidStr.Length - 15).ToUpper();

            int personasPermitidas = Convert.ToInt32(PersonasTXT.Text);
            int personasIngresadas = Convert.ToInt32(NumPersonas.Text);

            string anticipoLimpio = Regex.Replace(AnticipoTXT.Text, @"[^\d.,\-]", "");
            string restanteLimpio = Regex.Replace(RestanteTXT.Text, @"[^\d.,\-]", "");
            string Totalimpio = Regex.Replace(TotalTXT.Text, @"[^\d.,\-]", "");
            string TotalServicios = Regex.Replace(PrecioServiciosTXT.Text, @"[^\d.,\-]", "");

            if (Login.baseDatos == 2)
            {
                Guid tipoHab1 = (Guid)TipoHabReservaCB.SelectedValue;
                Guid id_hotel1 = (Guid)HotelCB.SelectedValue;
                string nombreHotel = HotelCB.Text;
                Guid id_cliente1 = (Guid)ApellidosTXT.SelectedValue;
                string Apellidos = ApellidosTXT.Text;
                string RFC = RFCCB.Text;
                string Correo = CorreoCB.Text;

                decimal subtotal = decimal.Parse(Totalimpio, CultureInfo.CurrentCulture);
                decimal iva = subtotal * 0.16m; decimal total = subtotal + iva;
                if (personasPermitidas < personasIngresadas)
                {
                    MessageBox.Show("No se admiten más de: " + "'" + personasPermitidas.ToString() + "'" + " personas por habitación", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!DateTime.TryParse(FechaLlegada.Text, out DateTime llegada) ||
                !DateTime.TryParse(FechaSalida.Text, out DateTime salida) ||
                !DateTime.TryParse(FechaActualRegistro.Text, out DateTime registro))
                {
                    MessageBox.Show("Fechas inválidas. Verifica el formato.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Reservacion1 RegistrarReservacion = new Reservacion1
                {
                    Id_Reservacion = Guid.NewGuid(),
                    Id_Usuario = idUsuario,
                    Id_Hotel = id_hotel1,
                    Id_TipoHab = tipoHab1,
                    NombreHotel = nombreHotel,
                    Id_Cliente = id_cliente1,
                    Apellidos = Apellidos,
                    Rfc = RFC,
                    Correo = Correo,
                    CodigoReserva = Guid.NewGuid(),
                    FechaLlegada = DateTime.Parse(FechaLlegada.Text),
                    FechaSalida = DateTime.Parse(FechaSalida.Text),
                    Total = decimal.Parse(Totalimpio, CultureInfo.CurrentCulture),
                    Total_servicios = decimal.Parse(TotalServicios, CultureInfo.CurrentCulture),
                    Anticipo = decimal.Parse(anticipoLimpio, CultureInfo.CurrentCulture),
                    Restante = decimal.Parse(restanteLimpio, CultureInfo.CurrentCulture),
                    MetodoPago = MetPagoCB.Text,
                    PersonasHospedadas = personasIngresadas,
                    Estatus = "Activo",
                    CheckIn = false,
                    CheckOut = false,
                    FechaCheckIn = DateTime.Parse(FechaLlegada.Text),
                    FechaCheckOut = DateTime.Parse(FechaSalida.Text),
                    FechaRegistro = DateTime.Parse(FechaActualRegistro.Text)
                };
                EnlaceCassandra enlace1 = new EnlaceCassandra();
                enlace1.insertaReservacion(RegistrarReservacion);

                ResumenReservacion RegistroResumenRes = new ResumenReservacion
                {
                    Id_Cliente = id_cliente1,
                    RFC = RFC,
                    MetodoPago = MetPagoCB.Text,
                    TotalServicios = decimal.Parse(TotalServicios, CultureInfo.CurrentCulture),
                    Subtotal = decimal.Parse(Totalimpio, CultureInfo.CurrentCulture),
                    IVA = iva,
                    Total = total,
                    Anticipo = decimal.Parse(anticipoLimpio, CultureInfo.CurrentCulture),
                    Pendiente = decimal.Parse(restanteLimpio, CultureInfo.CurrentCulture),
                };

                EnlaceCassandra enlace2 = new EnlaceCassandra();
                enlace2.insertaResumenRes(RegistroResumenRes);

                MessageBox.Show("Reservación registrada correctamente.\nCódigo de reserva: " + codigoReserva, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LlenarReservaCheck();

                LimpiarFormulario();
            }
        }

        private void TipoHabReservaCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Login.baseDatos == 2)
            {
                if (TipoHabReservaCB.SelectedValue != null)
                    TipoHabCB.SelectedValue = TipoHabReservaCB.SelectedValue;
            }
        }

        private void FechaLlegada_ValueChanged(object sender, EventArgs e)
        {
            CargarPrecios();
        }

        private void FechaSalida_ValueChanged(object sender, EventArgs e)
        {
            CargarPrecios();
        }

        private void AnticipoTXT_TextChanged(object sender, EventArgs e)
        {
            CargarPrecios();

            if (!string.IsNullOrEmpty(AnticipoTXT.Text) && AnticipoTXT.Text.All(char.IsDigit))
            {
                decimal anticipo = Convert.ToDecimal(AnticipoTXT.Text);

                decimal total;
                if (decimal.TryParse(TotalTXT.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out total))
                {
                    decimal restante = total - anticipo;
                    RestanteTXT.Text = restante.ToString("C2");
                }
                else
                {
                    RestanteTXT.Text = "Total inválido";
                }
            }
            else
            {
                RestanteTXT.Text = "";
            }
        }

        private void ComprobarCB_Click(object sender, EventArgs e)
        {
            if (Login.baseDatos == 2)
            {
                Guid idHotel = (Guid)HotelCB.SelectedValue;
                Guid idTipoHab = (Guid)TipoHabReservaCB.SelectedValue;
                DateTime fechaLlegada = DateTime.Parse(FechaLlegada.Text);
                DateTime fechaSalida = DateTime.Parse(FechaSalida.Text);

                EnlaceCassandra enlace = new EnlaceCassandra();

                int reservadas = enlace.ObtenerReservacionesTraslapadas(idHotel, idTipoHab, fechaLlegada, fechaSalida);
                int disponibles = enlace.ObtenerCantidadHabitaciones(idHotel, idTipoHab);

                if (reservadas == -1 || disponibles == -1)
                {
                    return;
                }

                if (reservadas < disponibles)
                {
                    MessageBox.Show("Hay disponibilidad de habitaciones.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    habitacionDisponible = true;
                }
                else
                {
                    MessageBox.Show("No hay habitaciones disponibles para esas fechas.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ServicioAdicionalCB_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this.BeginInvoke(new Action(() =>
            {
                decimal total = 0;

                for (int i = 0; i < ServicioAdicionalCB.Items.Count; i++)
                {
                    var item = ServicioAdicionalCB.Items[i] as ServicioAdicionalView;
                    if (item == null) continue;

                    bool isChecked = ServicioAdicionalCB.GetItemChecked(i);

                    if (i == e.Index)
                    {
                        isChecked = (e.NewValue == CheckState.Checked);
                    }

                    if (isChecked)
                    {
                        total += item.Costo;
                    }
                }

                PrecioServiciosTXT.Text = total.ToString("C2");
                CargarPrecios();
            }));
        }

        public void CargarPrecios()
        {
            if (!decimal.TryParse(PrecioTXT.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal precio))
            {
                TotalTXT.Text = "Precio inválido";
                return;
            }

            if (!decimal.TryParse(PrecioServiciosTXT.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal precioServicios))
            {
                TotalTXT.Text = "Precio servicios inválido";
                return;
            }

            DateTime fechaInicio = FechaLlegada.Value.Date;
            DateTime fechaFin = FechaSalida.Value.Date;

            int dias = (fechaFin - fechaInicio).Days;

            if (dias <= 0)
            {
                TotalTXT.Text = "Rango de fechas inválido.";
                return;
            }

            decimal total = (precio * dias) + precioServicios;

            TotalTXT.Text = total.ToString("C2");
        }

        private void ConfirmarBTN_Click(object sender, EventArgs e)
        {
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show("¿Está seguro que desea confirmar asistencia?", "Advertencia", buttons, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                DateTime FechaLlegada;
                DateTime FechaActual;

                bool parseLlegada = DateTime.TryParse(FechaLlegadaTXT.Text, out FechaLlegada);
                bool parseConfirmacion = DateTime.TryParse(FechaConfirmacionDTG.Text, out FechaActual);

                if (!parseLlegada || !parseConfirmacion)
                {
                    MessageBox.Show("Formato de fecha inválido. Verifique los campos de fecha.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (FechaActual < FechaLlegada)
                {
                    MessageBox.Show("La fecha de confirmación no puede ser anterior a la fecha de llegada.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (Login.baseDatos == 2)
                {
                    Guid idReservacion = (Guid)ApellidosCB.SelectedValue;
                    Guid idCliente = (Guid)RFCCombo.SelectedValue;

                    Guid codigoReserva = Guid.Parse(CodigoReserva.Text);
                    var FechaCheck = FechaConfirmacionDTG.Value;

                    EnlaceCassandra enlace2 = new EnlaceCassandra();

                    if (enlace2.ExisteCheckInPorCodigoReserva(codigoReserva))
                    {
                        MessageBox.Show("Ya se ha registrado un check-in con este código de reserva.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        ReservacionCheckInDatos CheckInRes = new ReservacionCheckInDatos
                        {
                            IdReservacion = idReservacion,
                            IdCliente = idCliente,
                            Apellidos = ApellidosCB.Text,
                            RFC = RFCCombo.Text,
                            Correo = CorreoCombo.Text,
                            CodigoReserva = Guid.Parse(CodigoReserva.Text),
                            Estatus = "check-in",
                            FechaCheckIn = FechaCheck,
                            FechaRegistro = DateTime.Now
                        };

                        EnlaceCassandra enlace1 = new EnlaceCassandra();
                        enlace1.InsertarReservacionCheckIn(CheckInRes);
                        enlace1.ActualizarCheckIn(CheckInRes);
                        MessageBox.Show("Check-in confirmado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        MostrarCheckIn();
                    }
                }
            }
        }

        private void MostrarCheckIn()
        {
            EnlaceCassandra enlace = new EnlaceCassandra();
            var listaCheckIns = enlace.ObtenerTodosCheckIns();

            TablaCheckInDTG.DataSource = listaCheckIns;

            foreach (DataGridViewColumn col in TablaCheckInDTG.Columns)
            {
                col.Visible = false;
            }

            TablaCheckInDTG.Columns["CodigoReserva"].Visible = true;
            TablaCheckInDTG.Columns["Apellidos"].Visible = true;
            TablaCheckInDTG.Columns["Correo"].Visible = true;
            TablaCheckInDTG.Columns["FechaCheckIn"].Visible = true;
        }

        private void SalidaBTN_Click(object sender, EventArgs e)
        {
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show("¿Está seguro que desea confirmar salida de la estancia?", "Advertencia", buttons, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (Login.baseDatos == 2)
                {
                    FechaSalidaTXT.Text = FechaConfirmacionDTG.Value.ToString("dd-MM-yyyy");
                    Guid codigoReserva = Guid.Parse(CodigoReserva.Text);

                    EnlaceCassandra enlace = new EnlaceCassandra();

                    DateTime? fechaCheckIn = enlace.ObtenerFechaCheckInPorCodigoReserva(codigoReserva);
                    DateTime? fechaCheckOut = enlace.ObtenerFechaCheckOutPorCodigoReserva(codigoReserva);
                    if (fechaCheckOut != null)
                    {
                        MessageBox.Show("Ya se ha registrado un check-out con este código de reserva.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else if (fechaCheckIn == null)
                    {
                        MessageBox.Show("No se puede realizar check-out porque no se ha registrado un check-in para esta reservación.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        Guid idReservacion = (Guid)ApellidosCB.SelectedValue;
                        Guid idCliente = (Guid)RFCCombo.SelectedValue;
                        var FechaCheck = FechaConfirmacionDTG.Value;

                        ReservacionCheckOutDatos CheckOutRes = new ReservacionCheckOutDatos
                        {
                            IdReservacion = idReservacion,
                            IdCliente = idCliente,
                            Apellidos = ApellidosCB.Text,
                            RFC = RFCCombo.Text,
                            Correo = CorreoCombo.Text,
                            CodigoReserva = codigoReserva,
                            Estatus = "check-out",
                            FechaCheckOut = FechaCheck,
                            FechaRegistro = DateTime.Now
                        };

                        EnlaceCassandra enlace1 = new EnlaceCassandra();
                        enlace1.InsertarReservacionCheckOut(CheckOutRes);
                        enlace1.ActualizarCheckOut(CheckOutRes);
                        MessageBox.Show("Check-out confirmado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        CargarCheckOuts();
                    }
                }
            }
        }

        private void CargarCheckOuts()
        {
            EnlaceCassandra enlace1 = new EnlaceCassandra();
            var listaCheckOuts = enlace1.ObtenerTodosCheckOuts();
            TablaCheckOutDTG.DataSource = listaCheckOuts;

            foreach (DataGridViewColumn col in TablaCheckOutDTG.Columns)
                col.Visible = false;

            TablaCheckOutDTG.Columns["CodigoReserva"].Visible = true;
            TablaCheckOutDTG.Columns["Apellidos"].Visible = true;
            TablaCheckOutDTG.Columns["Correo"].Visible = true;
            TablaCheckOutDTG.Columns["FechaCheckOut"].Visible = true;
        }

        public bool PuedeCancelar(DateTime fechaLlegada, DateTime fechaConfirmacion)
        {
            return fechaConfirmacion < fechaLlegada.AddDays(-4);
        }

        private void CancelarBTN_Click(object sender, EventArgs e)
        {
            if (Login.tipoUsuario == 0)
            {
                MessageBox.Show("No tiene permisos para acceder a esta función.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show("¿Está seguro que desea cancelar la estadía?", "Advertencia", buttons, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (Login.baseDatos == 2)
                {
                    EnlaceCassandra enlace = new EnlaceCassandra();
                    Guid codigoReserva = Guid.Parse(CodigoReserva.Text);
                    Guid idReservacion = (Guid)ApellidosCB.SelectedValue;
                    Guid idCliente = (Guid)RFCCombo.SelectedValue;
                    DateTime fechaConfirmacion = FechaConfirmacionDTG.Value;

                    DateTime? fechaLlegada = enlace.ObtenerFechaLlegadaPorCodigoReserva(idReservacion);

                    if (fechaLlegada != null)
                    {
                        if (PuedeCancelar(fechaLlegada.Value, fechaConfirmacion))
                        {
                            enlace.EliminarReservacion(idReservacion);

                            CancelacionDatos datos = new CancelacionDatos
                            {
                                CodigoReserva = codigoReserva,
                                IdReservacion = idReservacion,
                                IdCliente = idCliente,
                                Apellidos = CorreoCombo.Text,
                                RFC = RFCCombo.Text,
                                Correo = CorreoCombo.Text,
                                FechaCancelacion = fechaConfirmacion
                            };

                            enlace.InsertarCancelacion(datos);
                            MessageBox.Show("Cancelación registrada con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("No se puede cancelar la reservación: solo se permite hasta 3 días antes de la fecha de llegada.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No se encontró la fecha de llegada para esta reservación.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private decimal precioTotalServicios = 0;


        public void CargarTablasCass()
        {
            EnlaceCassandra enlace = new EnlaceCassandra();
            listaHoteles = enlace.GetAllHoteles();
            listaResumenHoteles = enlace.GetAllResumenHoteles();

            CiudadCB.DataSource = listaHoteles;
            CiudadCB.DisplayMember = "ciudad";
            CiudadCB.ValueMember = "id_hotel";

            var ciudades = listaHoteles
                .Select(h => h.ciudad)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            CiudadCB.DataSource = ciudades;
            listaTipoHabitaciones = new EnlaceCassandra().GetAllTipoHabitaciones();
        }

        private void CargarServiciosAdicionales(Guid idHotel)
        {
            ServicioAdicionalCB.Items.Clear();
            EnlaceCassandra enlace = new EnlaceCassandra();

            List<ServicioAdicionalView> servicios = enlace.GetServiciosAdicionales(idHotel);

            foreach (var servicio in servicios)
            {
                ServicioAdicionalCB.Items.Add(servicio);
            }
        }

        private void MostrarClientesEnTabla()
        {
            var enlace = new EnlaceCassandra();
            var clientes = enlace.Get_Clientes_Ligeros();

            var datosTabla = clientes.Select(c => new
            {
                Apellidos = $"{c.PrimerApellido} {c.SegundoApellido}",
                RFC = c.RFC,
                Correo = c.Correo?.FirstOrDefault() ?? "Sin correo"
            }).ToList();

            TablaClientesDTG.DataSource = datosTabla;

            TablaClientesDTG.Columns["Apellidos"].HeaderText = "Apellidos";
            TablaClientesDTG.Columns["RFC"].HeaderText = "RFC";
            TablaClientesDTG.Columns["Correo"].HeaderText = "Correo";

            TablaReservacionesDTG.DataSource = datosTabla;
            TablaReservacionesDTG.Columns["Apellidos"].HeaderText = "Apellidos";
            TablaReservacionesDTG.Columns["RFC"].HeaderText = "RFC";
            TablaReservacionesDTG.Columns["Correo"].HeaderText = "Correo";
        }

        private void LimpiarFormulario()
        {

            TotalTXT.Clear();
            AnticipoTXT.Clear();
            NumPersonas.Clear();
            RestanteTXT.Clear();
            PrecioServiciosTXT.Clear();
            PrecioTXT.Clear();
            habitacionDisponible = false;

            MetPagoCB.SelectedIndex = -1;

            FechaLlegada.Value = DateTime.Now;
            FechaSalida.Value = DateTime.Now;
        }

        private void LlenarReservaCheck()
        {
            var enlace = new EnlaceCassandra();
            reservas = enlace.GetAllReservacion();

            RFCCombo.DataSource = new BindingSource(reservas, null);
            ApellidosCB.DataSource = reservas;
            ApellidosCB.DisplayMember = "Apellidos";
            ApellidosCB.ValueMember = "Id_Reservacion";

            ApellidosCB.SelectedIndex = -1;

            RFCCombo.DataSource = new BindingSource(reservas, null);
            RFCCombo.DisplayMember = "RFC";
            RFCCombo.ValueMember = "Id_Cliente";

            RFCCombo.SelectedIndex = -1;

            CorreoCombo.DataSource = new BindingSource(
    reservas.Select(c => new
    {
        Id_Cliente = c.Id_Cliente,
        Correo = c.Correo ?? ""
    }).ToList(), null);
            CorreoCombo.DisplayMember = "Correo";
            CorreoCombo.ValueMember = "Id_Cliente";

            CorreoCombo.SelectedIndex = -1;
        }

        private void LlenarCombosClientes()
        {
            var enlace = new EnlaceCassandra();
            clientes = enlace.Get_Clientes_Ligeros();

            RFCCB.DataSource = new BindingSource(clientes, null);
            RFCCB.DisplayMember = "RFC";
            RFCCB.ValueMember = "Id_Cliente";

            ApellidosTXT.DataSource = new BindingSource(clientes.Select(c => new
            {
                Id_Cliente = c.Id_Cliente,
                Apellidos = $"{c.PrimerApellido} {c.SegundoApellido}"
            }).ToList(), null);
            ApellidosTXT.DisplayMember = "Apellidos";
            ApellidosTXT.ValueMember = "Id_Cliente";

            CorreoCB.DataSource = new BindingSource(
    clientes.Select(c => new
    {
        Id_Cliente = c.Id_Cliente,
        Correo = c.Correo?.FirstOrDefault() ?? ""
    }).ToList(), null);
            CorreoCB.DisplayMember = "Correo";
            CorreoCB.ValueMember = "Id_Cliente";
        }

        private void RFCCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (RFCCB.SelectedValue is Guid id)
                SincronizarComboBoxes(id);
        }

        private void CorreoCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CorreoCB.SelectedValue is Guid id)
                SincronizarComboBoxes(id);
        }

        private void ApellidosTXT_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ApellidosTXT.SelectedValue is Guid id)
                SincronizarComboBoxes(id);
        }

        private void SincronizarComboBoxes(Guid idCliente)
        {
            var cliente = clientes.FirstOrDefault(c => c.Id_Cliente == idCliente);
            if (cliente != null)
            {
                RFCCB.SelectedValue = cliente.Id_Cliente;

                string apellidos = $"{cliente.PrimerApellido} {cliente.SegundoApellido}";
                var apellidosItem = ApellidosTXT.Items.Cast<dynamic>().FirstOrDefault(i => i.Apellidos == apellidos);
                if (apellidosItem != null)
                    ApellidosTXT.SelectedValue = apellidosItem.Id_Cliente;

                string primerCorreo = cliente.Correo?.FirstOrDefault() ?? "";
                var correoItem = CorreoCB.Items.Cast<dynamic>().FirstOrDefault(i => i.Correo == primerCorreo);
                if (correoItem != null)
                    CorreoCB.SelectedValue = correoItem.Id_Cliente;
            }
        }

        private void ApellidosCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Login.baseDatos == 2)
            {
                if (ApellidosCB.SelectedValue is Guid idReservacion)
                {
                    var enlace = new EnlaceCassandra();
                    var reserva = enlace.ObtenerInfoReservacionPorId(idReservacion);

                    if (reserva != null)
                    {
                        RFCCombo.Text = reserva.Rfc;
                        CorreoCombo.Text = reserva.Correo;
                        MetodoPagoTXT.Text = reserva.MetodoPago;
                        PrecioRestanteTXT.Text = reserva.Restante.ToString("C2");
                        PrecioTotal.Text = reserva.Total.ToString("C2");
                        CodigoReserva.Text = reserva.CodigoReserva.ToString();
                        FechaRegistroTXT.Text = reserva.FechaRegistro.ToString("yyyy-MM-dd");
                        FechaLlegadaTXT.Text = reserva.FechaLlegada.ToString("yyyy-MM-dd");
                        FechaSalidaTXT.Text = reserva.FechaSalida.ToString("yyyy-MM-dd");
                    }
                }
            }
        }

        private void CodigoReserva_TextChanged(object sender, EventArgs e)
        {
            if (Login.baseDatos == 2)
            {
                EnlaceCassandra enlace = new EnlaceCassandra();
                Guid codigoReserva = Guid.Parse(CodigoReserva.Text);

                DateTime? fecha = enlace.ObtenerFechaCheckInPorCodigoReserva(codigoReserva);

                if (fecha != null)
                {
                    FechaLlegadaTXT.Text = fecha.Value.ToString("dd-MM-yyyy");
                }
                else
                {
                    FechaLlegadaTXT.Clear();
                }

                EnlaceCassandra enlace1 = new EnlaceCassandra();

                DateTime? fecha1 = enlace1.ObtenerFechaCheckOutPorCodigoReserva(codigoReserva);

                if (fecha1 != null)
                {
                    FechaSalidaTXT.Text = fecha1.Value.ToString("dd-MM-yyyy");
                }
                else
                {
                    FechaSalidaTXT.Clear();
                }
            }
        }

        private void CiudadCB_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            string ciudadSeleccionada = CiudadCB.Text;

            if (Login.baseDatos == 2)
            {
                string ciudadSeleccionada1 = CiudadCB.SelectedItem?.ToString();

                if (!string.IsNullOrEmpty(ciudadSeleccionada1))
                {
                    var hotelesFiltrados = listaHoteles
                        .Where(h => h.ciudad == ciudadSeleccionada1)
                        .ToList();

                    HotelCB.DataSource = hotelesFiltrados;
                    HotelCB.DisplayMember = "nombre_hotel";
                    HotelCB.ValueMember = "id_hotel";
                }
            }
        }
    }
}