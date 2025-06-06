using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace AAVD
{
    public partial class Reportes : Form
    {
        public Reportes()
        {
            InitializeComponent();
        }

        private List<Hotel1> listaHoteles = new List<Hotel1>();
        private List<Hotel1> listaHoteles1 = new List<Hotel1>();
        private List<ResumenHoteles> listaResumenHoteles = new List<ResumenHoteles>();
        private List<ClienteLigero> clientes;
        private DataTable historialCompleto;

        public class filtros
        {
            public string pais { get; set; }
            public string ciudad { get; set; }
            public string hotel { get; set; }
            public string primerApellido { get; set; }
            public string segundoApellido { get; set; }
            public string correo { get; set; }
            public string rfc { get; set; }
            public int? anio { get; set; }
            public int Id_Cliente { get; set; }
        }

        private void AbrirControlEnPanel(System.Windows.Forms.UserControl control)
        {
            MenuContenedor.Controls.Clear();
            control.Dock = DockStyle.Fill;
            MenuContenedor.Controls.Add(control);
            control.BringToFront();
        }

        public List<HistorialReservacion> ConstruirHistorialReservaciones()
        {
            var lista = new List<HistorialReservacion>();
            EnlaceCassandra enlace = new EnlaceCassandra();
            var reservaciones = enlace.ObtenerReservacionesDesdeCassandra();

            foreach (var row in reservaciones)
            {
                Guid codigoReserva = row.GetValue<Guid>("codigo_reserva");
                var (fechaCheckIn, estatusCheckIn) = enlace.ObtenerCheckIn(codigoReserva);
                var (fechaCheckOut, estatusCheckOut) = enlace.ObtenerCheckOut(codigoReserva);

                var item = new HistorialReservacion
                {
                    Hotel = row.GetValue<string>("nombre_hotel"),
                    NombreCompleto = row.GetValue<string>("apellidos"),
                    PersonasHospedadas = row.GetValue<int>("personas_hospedadas"),
                    CodigoReserva = codigoReserva,
                    Anticipo = row.GetValue<decimal>("anticipo"),
                    Total = row.GetValue<decimal>("total"),
                    MontoServicios = row.GetValue<decimal>("total_servicios"),
                    MontoHospedaje = row.GetValue<decimal>("anticipo") + row.GetValue<decimal>("restante"),
                    FechaCheckIn = fechaCheckIn,
                    FechaCheckOut = fechaCheckOut,
                    EstatusCheckIn = estatusCheckIn,
                    EstatusCheckOut = estatusCheckOut,
                    FechaReservacion = row.GetValue<DateTime>("fecha_registro"),
                };

                lista.Add(item);
            }

            return lista;
        }

        private void CargarCassandraRportes()
        {
            LlenarCombosClientes();
            UsuarioActualTXT.Text = SesionUsuario.NombreUsuario;
            EnlaceCassandra enlace = new EnlaceCassandra();
            EnlaceCassandra enlace2 = new EnlaceCassandra();
            listaHoteles = enlace.GetAllHoteles();
            listaHoteles1 = enlace2.GetAllHoteles();
            listaResumenHoteles = enlace.GetAllResumenHoteles();

            PaisCB3.DataSource = listaHoteles;
            PaisCB3.DisplayMember = "pais";
            PaisCB3.ValueMember = "id_hotel";

            var paises = listaHoteles
                .Select(h => h.pais)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            PaisCB3.DataSource = paises;

            PaisCB1.DataSource = listaHoteles;
            PaisCB1.DisplayMember = "pais";
            PaisCB1.ValueMember = "id_hotel";

            var paises1 = listaHoteles
                .Select(h => h.pais)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            PaisCB1.DataSource = paises1;

            PaisCB2.DataSource = listaHoteles1;
            PaisCB2.DisplayMember = "pais";
            PaisCB2.ValueMember = "id_hotel";

            var paises2 = listaHoteles
                .Select(h => h.pais)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            PaisCB2.DataSource = paises2;
            EnlaceCassandra enlace1 = new EnlaceCassandra();
            DataTable datos = enlace1.ObtenerDatosReporte();
            ReporteVentasDTG.DataSource = datos;

            ReporteVentasDTG.Columns["Mes"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            ReporteVentasDTG.Columns["Año"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            ReporteVentasDTG.Columns["Ingresos por hospedaje"].DefaultCellStyle.Format = "C2";
            ReporteVentasDTG.Columns["Ingresos por servicios adicionales"].DefaultCellStyle.Format = "C2";
            ReporteVentasDTG.Columns["Ingresos totales"].DefaultCellStyle.Format = "C2";
            LlenarFiltrosDesdeDatos();
            MostrarHistorialEnDataGrid();
            MostrarReporteOcupacionEnDataGrid();
            MostrarReporteOcupacionResumenEnDataGrid();
        }

        public void MostrarReporteOcupacionResumenEnDataGrid()
        {
            EnlaceCassandra enlace = new EnlaceCassandra();
            List<ReporteOcupacionHotel> datos = enlace.ObtenerListaOcupacionPorHotel();

            if (datos == null || datos.Count == 0)
            {
                MessageBox.Show("No hay datos de resumen de ocupación por hotel para mostrar.");
                return;
            }

            ReporteOcupacionResumen.DataSource = new BindingSource { DataSource = datos };
            ReporteOcupacionResumen.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            ReporteOcupacionResumen.Columns["Mes"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            ReporteOcupacionResumen.Columns["Anio"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            ReporteOcupacionResumen.Columns["Anio"].HeaderText = "Año";
            ReporteOcupacionResumen.Columns["HabitacionesOcupadas"].HeaderText = "Habitaciones ocupadas";
            ReporteOcupacionResumen.Columns["TotalHabitaciones"].HeaderText = "Total de habitaciones";
        }

        public void MostrarReporteOcupacionEnDataGrid()
        {
            EnlaceCassandra enlace = new EnlaceCassandra();
            List<ReporteOcupacion> datos = enlace.ObtenerListaOcupacion();

            if (datos == null || datos.Count == 0)
            {
                MessageBox.Show("No hay datos de ocupación para mostrar.");
                return;
            }

            ReporteOcupacionDTG.DataSource = new BindingSource { DataSource = datos };
            ReporteOcupacionDTG.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            ReporteOcupacionDTG.Columns["Mes"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            ReporteOcupacionDTG.Columns["Anio"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            ReporteOcupacionDTG.Columns["Anio"].HeaderText = "Año";
            ReporteOcupacionDTG.Columns["TipoHabitacion"].HeaderText = "Nivel de habitacion";
            ReporteOcupacionDTG.Columns["HabitacionesOcupadas"].HeaderText = "Habitaciones ocupadas";
            ReporteOcupacionDTG.Columns["TotalHabitaciones"].HeaderText = "Total de habitaciones";
            ReporteOcupacionDTG.Columns["PorcentajeOcupacion"].HeaderText = "% de ocupacion";
        }
        private void ReporteOcupacionDTG_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (ReporteOcupacionDTG.Columns[e.ColumnIndex].Name == "PorcentajeOcupacion" && e.Value != null)
            {
                if (double.TryParse(e.Value.ToString(), out double valor))
                {
                    e.Value = valor.ToString("0.00") + "%";
                    e.FormattingApplied = true;
                }
            }
        }
        private void ReporteOcupacionResumen_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (ReporteOcupacionResumen.Columns[e.ColumnIndex].Name == "PorcentajeOcupacion" && e.Value != null)
            {
                if (double.TryParse(e.Value.ToString(), out double valor))
                {
                    e.Value = valor.ToString("0.00") + "%";
                    e.FormattingApplied = true;
                }
            }
        }

        public void MostrarHistorialEnDataGrid()
        {
            var historial = ConstruirHistorialReservaciones();

            DataTable tabla = new DataTable();
            tabla.Columns.Add("Hotel", typeof(string));
            tabla.Columns.Add("Nombre Completo", typeof(string));
            tabla.Columns.Add("Personas Hospedadas", typeof(int));
            tabla.Columns.Add("Código de Reserva", typeof(Guid));
            tabla.Columns.Add("Fecha de Reservación", typeof(DateTime));
            tabla.Columns.Add("Anticipo", typeof(decimal));
            tabla.Columns.Add("Total", typeof(decimal));
            //tabla.Columns.Add("Año", typeof(int));
            tabla.Columns.Add("Monto Servicios", typeof(decimal));
            tabla.Columns.Add("Monto Hospedaje", typeof(decimal));
            tabla.Columns.Add("Fecha de llegada", typeof(DateTime));
            tabla.Columns.Add("Estatus Check-In", typeof(string));
            tabla.Columns.Add("Fecha de salida", typeof(DateTime));
            tabla.Columns.Add("Estatus Check-Out", typeof(string));
            tabla.Columns.Add("Año de reservacion", typeof(string));

            foreach (var item in historial)
            {
                tabla.Rows.Add(
                    item.Hotel,
                    item.NombreCompleto,
                    item.PersonasHospedadas,
                    item.CodigoReserva,
                    item.FechaReservacion,
                    item.Anticipo,
                    item.Total,
                    item.MontoServicios,
                    item.MontoHospedaje,
                    item.FechaCheckIn ?? (object)DBNull.Value,
                    item.EstatusCheckIn ?? (object)DBNull.Value,
                    item.FechaCheckOut ?? (object)DBNull.Value,
                    item.EstatusCheckOut ?? (object)DBNull.Value,
                    item.FechaReservacion.Year
                );
            }

            HistorialDTG.DataSource = tabla;
            HistorialDTG.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            HistorialDTG.Columns["Anticipo"].DefaultCellStyle.Format = "C2";
            HistorialDTG.Columns["Total"].DefaultCellStyle.Format = "C2";
            HistorialDTG.Columns["Monto Servicios"].DefaultCellStyle.Format = "C2";
            HistorialDTG.Columns["Monto Hospedaje"].DefaultCellStyle.Format = "C2";
            historialCompleto = tabla;
            HistorialDTG.DataSource = historialCompleto;
            HistorialDTG.Columns["Estatus Check-In"].Visible = false;
            HistorialDTG.Columns["Estatus Check-Out"].Visible = false;
        }

        private void LlenarFiltrosDesdeDatos()
        {
            EnlaceCassandra enlace = new EnlaceCassandra();
            DataTable datos = enlace.ObtenerDatosReporte();

            var paises = datos.AsEnumerable()
                .Select(row => row.Field<string>("Pais"))
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            var hoteles = datos.AsEnumerable()
                .Select(row => row.Field<string>("Hotel"))
                .Distinct()
                .OrderBy(h => h)
                .ToList();

            var ciudades = datos.AsEnumerable()
                .Select(row => row.Field<string>("Ciudad"))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            PaisCB3.DataSource = paises;
            HotelCB3.DataSource = hoteles;
            CiudadCB3.DataSource = ciudades;

            AnioCB3.Value = DateTime.Today;
        }

        private void LlenarCombosClientes()
        {
            var enlace = new EnlaceCassandra();
            clientes = enlace.Get_Clientes_Ligeros();

            RfcCB.DataSource = new BindingSource(clientes, null);
            RfcCB.DisplayMember = "RFC";
            RfcCB.ValueMember = "Id_Cliente";

            ApellidoPCB.DataSource = new BindingSource(clientes.Select(c => new
            {
                Id_Cliente = c.Id_Cliente,
                Apellidos = $"{c.PrimerApellido} {c.SegundoApellido}"
            }).ToList(), null);
            ApellidoPCB.DisplayMember = "Apellidos";
            ApellidoPCB.ValueMember = "Id_Cliente";

            CorreoCB.DataSource = new BindingSource(clientes.Select(c => new
            {
                Id_Cliente = c.Id_Cliente,
                Correo = c.Correo?.FirstOrDefault() ?? ""
            }).ToList(), null);
            CorreoCB.DisplayMember = "Correo";
            CorreoCB.ValueMember = "Id_Cliente";
        }

        private void AplicarFiltroHistorial()
        {
            if (historialCompleto == null) return;

            DataView vista = new DataView(historialCompleto);
            List<string> filtros = new List<string>();

            if (!string.IsNullOrEmpty(ApellidoPCB.Text))
            {
                string apellidos = ApellidoPCB.Text.Replace("'", "''");
                filtros.Add($"[Nombre Completo] LIKE '%{apellidos}%'");
            }

            int añoSeleccionado = AnioDTP.Value.Year;
            filtros.Add($"[Año de reservacion] = {añoSeleccionado}");

            vista.RowFilter = string.Join(" AND ", filtros);
            HistorialDTG.DataSource = vista;
        }

        private void Reportes_Load(object sender, EventArgs e)
        {
            AbrirControlEnPanel(new Menu());
            var NuevoForm = new Login();

            if (Login.baseDatos == 2)
            {
                label22.Text = "Apellidos";
                label14.Visible = false;
                ApellidoMCB.Visible = false;
                CargarCassandraRportes();
            }
            this.FindForm().Size = NuevoForm.Size;
            this.FindForm().StartPosition = FormStartPosition.Manual;
            this.FindForm().Location = NuevoForm.Location;
        }

        private void LimpiarFiltrosBTN_Click(object sender, EventArgs e)
        {
            CargarCassandraRportes();
        }

        private void AplicarBTN1_Click(object sender, EventArgs e)
        {
            filtros filtro = new filtros();

            var paisSeleccionado = PaisCB1.SelectedValue?.ToString();
            filtro.pais = string.IsNullOrWhiteSpace(paisSeleccionado) ? null : paisSeleccionado;

            var ciudadSeleccionada = CiudadCB1.SelectedValue?.ToString();
            filtro.ciudad = string.IsNullOrWhiteSpace(ciudadSeleccionada) ? null : ciudadSeleccionada;

            var hotelSeleccionado = HotelCB1.SelectedValue?.ToString();
            filtro.hotel = string.IsNullOrWhiteSpace(hotelSeleccionado) ? null : hotelSeleccionado;

            if (AnioCB1.Checked)
            {
                filtro.anio = AnioCB1.Value.Year;
            }
            else
            {
                filtro.anio = null;
            }

            if (Login.baseDatos == 2)
            {
                EnlaceCassandra enlace = new EnlaceCassandra();

                List<ReporteOcupacionHotel> listaCompleta = enlace.ObtenerListaOcupacionPorHotel();

                string ciudad = CiudadCB1.Text;
                string hotel = HotelCB1.Text;
                string año = AnioCB1.Value.Year.ToString();

                var listaFiltrada = listaCompleta.Where(item =>
                (string.IsNullOrEmpty(ciudad) || item.Ciudad.ToLower().Contains(ciudad.ToLower())) &&
                (string.IsNullOrEmpty(hotel) || item.Hotel.Equals(hotel, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(año) || item.Anio.ToString() == año)
                    ).ToList();

                ReporteOcupacionDTG.DataSource = listaFiltrada;
            }
        }

        private void AplicarBTN2_Click(object sender, EventArgs e)
        {
            filtros filtro = new filtros();

            var paisSeleccionado = PaisCB2.SelectedValue?.ToString();
            filtro.pais = string.IsNullOrWhiteSpace(paisSeleccionado) ? null : paisSeleccionado;

            var ciudadSeleccionada = CiudadCB2.SelectedValue?.ToString();
            filtro.ciudad = string.IsNullOrWhiteSpace(ciudadSeleccionada) ? null : ciudadSeleccionada;

            var hotelSeleccionado = HotelCB2.SelectedValue?.ToString();
            filtro.hotel = string.IsNullOrWhiteSpace(hotelSeleccionado) ? null : hotelSeleccionado;

            if (AnioCB2.Checked)
            {
                filtro.anio = AnioCB2.Value.Year;
            }
            else
            {
                filtro.anio = null;
            }

            if (Login.baseDatos == 2)
            {
                EnlaceCassandra enlace = new EnlaceCassandra();

                List<ReporteOcupacion> datos = enlace.ObtenerListaOcupacion();

                string ciudad = CiudadCB2.Text;
                string hotel = HotelCB2.Text;
                string año = AnioCB2.Value.Year.ToString();

                var listaFiltrada = datos.Where(item =>
                (string.IsNullOrEmpty(ciudad) || item.Ciudad.ToLower().Contains(ciudad.ToLower())) &&
                (string.IsNullOrEmpty(hotel) || item.Hotel.Equals(hotel, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(año) || item.Anio.ToString() == año)
                    ).ToList();

                ReporteOcupacionResumen.DataSource = listaFiltrada;
            }
        }

        private void LimpiarBTN_Click(object sender, EventArgs e)
        {
            if (Login.baseDatos == 2) CargarCassandraRportes();
        }

        private void Aplicar_Click(object sender, EventArgs e)
        {
            filtros filtro = new filtros();

            var paisSeleccionado = PaisCB3.SelectedValue?.ToString();
            filtro.pais = string.IsNullOrWhiteSpace(paisSeleccionado) ? null : paisSeleccionado;

            var ciudadSeleccionada = CiudadCB3.SelectedValue?.ToString();
            filtro.ciudad = string.IsNullOrWhiteSpace(ciudadSeleccionada) ? null : ciudadSeleccionada;

            var hotelSeleccionado = HotelCB3.SelectedValue?.ToString();
            filtro.hotel = string.IsNullOrWhiteSpace(hotelSeleccionado) ? null : hotelSeleccionado;

            filtro.anio = AnioCB3.Value.Year;

            if (AnioCB3.Checked)
            {
                filtro.anio = AnioCB3.Value.Year;
            }
            else
            {
                filtro.anio = null;
            }

            if (Login.baseDatos == 2)
            {
                EnlaceCassandra enlace = new EnlaceCassandra();
                DataTable datosCompletos = enlace.ObtenerDatosReporte();
                DataView vista = new DataView(datosCompletos);

                string filtro1 = "";

                string pais = PaisCB3.SelectedItem?.ToString();
                string hotel = HotelCB3.Text;
                string ciudad = CiudadCB3.Text;
                string año = AnioCB3.Value.Year.ToString();
                if (!string.IsNullOrEmpty(pais))
                    filtro1 += $"Convert([Pais], 'System.String') LIKE '%{pais}%'";

                if (!string.IsNullOrEmpty(hotel))
                {
                    if (!string.IsNullOrEmpty(filtro1)) filtro1 += " AND ";
                    filtro1 += $"[Hotel] = '{hotel.Replace("'", "''")}'";
                }

                if (!string.IsNullOrEmpty(ciudad))
                {
                    if (!string.IsNullOrEmpty(filtro1)) filtro1 += " AND ";
                    filtro1 += $"[Ciudad] = '{ciudad.Replace("'", "''")}'";
                }

                if (!string.IsNullOrEmpty(año))
                {
                    if (!string.IsNullOrEmpty(filtro1)) filtro1 += " AND ";
                    filtro1 += $"[Año] = {año}";
                }

                vista.RowFilter = filtro1;
                ReporteVentasDTG.DataSource = vista;
            }
        }

        private void LimpiarHistorial_Click(object sender, EventArgs e)
        {
            if (Login.baseDatos == 2) MostrarHistorialEnDataGrid();
        }

        private void AplicarHistorial_Click(object sender, EventArgs e)
        {
            filtros filtro = new filtros();

            var apellidoPaterno = ApellidoPCB.SelectedValue?.ToString();
            filtro.primerApellido = string.IsNullOrWhiteSpace(apellidoPaterno) ? null : apellidoPaterno;

            var apellidoMaterno = ApellidoMCB.SelectedValue?.ToString();
            filtro.segundoApellido = string.IsNullOrWhiteSpace(apellidoMaterno) ? null : apellidoMaterno;

            var correo = CorreoCB.SelectedValue?.ToString();
            filtro.correo = string.IsNullOrWhiteSpace(correo) ? null : correo;

            var rfc = RfcCB.SelectedValue?.ToString();
            filtro.rfc = string.IsNullOrWhiteSpace(rfc) ? null : rfc;

            if (AnioDTP.Checked)
            {
                filtro.anio = AnioDTP.Value.Year;
            }
            else
            {
                filtro.anio = null;
            }
            if (Login.baseDatos == 2)
            {
                AplicarFiltroHistorial();
            }
        }

        private void PaisCB3_SelectedIndexChanged(object sender, EventArgs e)
        {
            string paisSeleccionada1 = PaisCB3.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(paisSeleccionada1))
            {
                var hotelesFiltrados = listaHoteles
                    .Where(h => h.pais == paisSeleccionada1)
                .ToList();

                HotelCB3.DataSource = hotelesFiltrados;
                HotelCB3.DisplayMember = "nombre_hotel";
                HotelCB3.ValueMember = "id_hotel";

                CiudadCB3.DataSource = hotelesFiltrados;
                CiudadCB3.DisplayMember = "ciudad";
                CiudadCB3.ValueMember = "id_hotel";
            }
        }

        private void ApellidoPCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Login.baseDatos == 2)
            {
                if (ApellidoPCB.SelectedValue is Guid id)
                    SincronizarComboBoxes(id);
            }
        }

        private void SincronizarComboBoxes(Guid idCliente)
        {
            var cliente = clientes.FirstOrDefault(c => c.Id_Cliente == idCliente);
            if (cliente != null)
            {
                RfcCB.SelectedValue = cliente.Id_Cliente;

                string apellidos = $"{cliente.PrimerApellido} {cliente.SegundoApellido}";
                var apellidosItem = ApellidoPCB.Items.Cast<dynamic>().FirstOrDefault(i => i.Apellidos == apellidos);
                if (apellidosItem != null)
                    ApellidoPCB.SelectedValue = apellidosItem.Id_Cliente;

                string primerCorreo = cliente.Correo?.FirstOrDefault() ?? "";
                var correoItem = CorreoCB.Items.Cast<dynamic>().FirstOrDefault(i => i.Correo == primerCorreo);
                if (correoItem != null)
                    CorreoCB.SelectedValue = correoItem.Id_Cliente;
            }
        }

        private void PaisCB1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string paisSeleccionada1 = PaisCB1.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(paisSeleccionada1))
            {
                var hotelesFiltrados = listaHoteles
                    .Where(h => h.pais == paisSeleccionada1)
                .ToList();

                HotelCB1.DataSource = hotelesFiltrados;
                HotelCB1.DisplayMember = "nombre_hotel";
                HotelCB1.ValueMember = "id_hotel";

                CiudadCB1.DataSource = hotelesFiltrados;
                CiudadCB1.DisplayMember = "ciudad";
                CiudadCB1.ValueMember = "id_hotel";
            }
        }

        private void PaisCB2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string paisSeleccionada2 = PaisCB2.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(paisSeleccionada2))
            {
                var hotelesFiltrados1 = listaHoteles1
                    .Where(h => h.pais == paisSeleccionada2)
                .ToList();

                HotelCB2.DataSource = hotelesFiltrados1;
                HotelCB2.DisplayMember = "nombre_hotel";
                HotelCB2.ValueMember = "id_hotel";

                CiudadCB2.DataSource = hotelesFiltrados1;
                CiudadCB2.DisplayMember = "ciudad";
                CiudadCB2.ValueMember = "id_hotel";
            }
        }

        private void RfcCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (RfcCB.SelectedValue is Guid id)
                SincronizarComboBoxes(id);
        }

        private void CorreoCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CorreoCB.SelectedValue is Guid id)
                SincronizarComboBoxes(id);
        }
    }
}