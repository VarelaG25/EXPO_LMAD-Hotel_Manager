using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AAVD
{
    public partial class Hoteles : Form
    {
        public Hoteles()
        {
            InitializeComponent();
        }

        private Guid idUsuario = SesionUsuario.IdUsuarioActual;
        private List<Servicio> servicios = new List<Servicio>();

        public class Amenidad
        {
            public int Id_Amenidad { get; set; }
            public string nombreAmenidad { get; set; }

            public override string ToString()
            {
                return nombreAmenidad;
            }
        }

        public class TipoHab
        {
            public int Id_TipoHab { get; set; }
            public int Id_Hotel { get; set; }
            public string nivelHabitacion { get; set; }
            public int numeroCamas { get; set; }
            public string tipoCama { get; set; }
            public double precio { get; set; }
            public int numeroPersonas { get; set; }
            public string frenteA { get; set; }
            public override string ToString()
            {
                return nivelHabitacion;
            }
        }

        public class Caracteristica
        {
            public int Id_Caracteristica { get; set; }
            public string nombreCaracteristica { get; set; }

            public override string ToString()
            {
                return nombreCaracteristica;
            }
        }

        public class ServicioAdicional
        {
            public int Id_ServicioAdicional { get; set; }
            public string nombreServicio { get; set; }
            public double costo { get; set; }

            public override string ToString()
            {
                return nombreServicio;
            }
        }

        private void AbrirControlEnPanel(System.Windows.Forms.UserControl control)
        {
            MenuContenedor.Controls.Clear();
            control.Dock = DockStyle.Fill;
            MenuContenedor.Controls.Add(control);
            control.BringToFront();
        }

        private void Hoteles_Load(object sender, EventArgs e)
        {
            AbrirControlEnPanel(new Menu());
            var NuevoForm = new Login();

            if (Login.baseDatos == 2)
            {
                UsuarioActual1TXT.Text = SesionUsuario.NombreUsuario;
                CargarTablasTipoHabCass();
                CargarDistribucionHabitaciones();
            }
            this.FindForm().Size = NuevoForm.Size;
            this.FindForm().StartPosition = FormStartPosition.Manual;
            this.FindForm().Location = NuevoForm.Location;
        }

        private void BTN_Registrar_Click(object sender, EventArgs e)
        {
            Amenidad amenidad = new Amenidad();
            Caracteristica caracteristica = new Caracteristica();
            if (string.IsNullOrEmpty(NivelHabitacionTXT.Text) || string.IsNullOrEmpty(NumeroCamasTXT.Text) || string.IsNullOrEmpty(TipoCamaTXT.Text) || string.IsNullOrEmpty(PrecioTXT.Text) || string.IsNullOrEmpty(CantidadPersonasTXT.Text))
            {
                MessageBox.Show("Por favor, complete todos los campos.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!NumeroCamasTXT.Text.All(char.IsDigit))
            {
                MessageBox.Show("El número de camas debe contener solo números.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!PrecioTXT.Text.All(c => char.IsDigit(c) || c == '.') || PrecioTXT.Text.Count(c => c == '.') > 1)
            {
                MessageBox.Show("El precio debe contener solo números y un punto decimal.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (Convert.ToInt32(PrecioTXT.Text) < 800)
            {
                MessageBox.Show("El precio no puede ser menor a 800", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (Convert.ToInt32(NumeroCamasTXT.Text) < 1 || Convert.ToInt32(NumeroCamasTXT.Text) > 5)
            {
                MessageBox.Show("El número de camas no puede ser menor a 1 o mayor a 5", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (CheckJardinTXT.Checked && CheckPiscinaTXT.Checked && CheckPlayaTXT.Checked)
            {
                MessageBox.Show("Seleccione solo una opción donde estara mirando la habitacion", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var nivelHabitacion = NivelHabitacionTXT.Text;
            var tipoCama = TipoCamaTXT.Text;
            var numeroCamas = Convert.ToInt32(NumeroCamasTXT.Text);
            var precio = Convert.ToDouble(PrecioTXT.Text);
            var numeroPersonas = Convert.ToInt32(CantidadPersonasTXT.Text);
            var frenteA = string.Empty;
            if (CheckJardinTXT.Checked) frenteA = "Jardin";
            if (CheckPiscinaTXT.Checked) frenteA = "Piscina";
            if (CheckPlayaTXT.Checked) frenteA = "Playa";

            if (Login.baseDatos == 2)
            {
                nivelHabitacion = NivelHabitacionTXT.Text;
                tipoCama = TipoCamaTXT.Text;
                numeroCamas = Convert.ToInt32(NumeroCamasTXT.Text);
                Decimal precio1 = Convert.ToDecimal(PrecioTXT.Text); numeroPersonas = Convert.ToInt32(CantidadPersonasTXT.Text);
                var frenteA1 = CheckJardinTXT.Checked || CheckPiscinaTXT.Checked || CheckPlayaTXT.Checked;
                var idHotel1 = Guid.Parse(HotelTHCB.SelectedValue.ToString());
                TipoHabitacion RegistrarHab = new TipoHabitacion
                {
                    IdTipoHab = Guid.NewGuid(),
                    IdHotel = idHotel1,
                    NombreHotel = HotelTHCB.Text,
                    NivelHabitacion = nivelHabitacion,
                    NumeroCamas = numeroCamas,
                    TipoCama = tipoCama,
                    Precio = precio1,
                    NumeroPersonas = numeroPersonas,
                    FrenteA = frenteA1,
                    Caracteristicas = CaracteristicasTXT.Text,
                    Amenidades = AmenidadesTXT.Text,
                    Estatus = 1
                };
                EnlaceCassandra enlace = new EnlaceCassandra();
                enlace.insertaTipoHabitacion(RegistrarHab);
                MessageBox.Show("Empleado agregado exitosamente.");
            }
            NivelHabitacionTXT.Text = "";
            TipoCamaTXT.Text = "";
            NumeroCamasTXT.Text = "";
            PrecioTXT.Text = "";
            CantidadPersonasTXT.Text = "";
            if (CheckJardinTXT.Checked) CheckJardinTXT.Checked = false;
            if (CheckPiscinaTXT.Checked) CheckPiscinaTXT.Checked = false;
            if (CheckPlayaTXT.Checked) CheckPlayaTXT.Checked = false;
            AmenidadesTXT.Text = "";
            CaracteristicasTXT.Text = "";
            MessageBox.Show("Tipo de habitacion registrado correctamente: Nivel " + nivelHabitacion, "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            CargarTablasTipoHabCass();
        }

        private void AgregarAmenidadBTN7_Click(object sender, EventArgs e)
        {
            Amenidad amenidad = new Amenidad();
            if (string.IsNullOrEmpty(CampoAmenidadTXT.Text))
            {
                MessageBox.Show("Error al registrar la amenidad, no debe de estar vacio el campo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var nombreAmenidad = CampoAmenidadTXT.Text;

            if (Login.baseDatos == 2)
            {
                if (!string.IsNullOrEmpty(nombreAmenidad) && !AmenidadesCB.Items.Contains(nombreAmenidad))
                {
                    AmenidadesCB.Items.Add(nombreAmenidad);
                    CampoAmenidadTXT.Clear();
                }
                else
                {
                    MessageBox.Show("Introduce una amenidad válida o que no esté repetida.");
                }
            }

            CampoAmenidadTXT.Clear();
        }

        private void AgregarCaracteristicaBTN_Click(object sender, EventArgs e)
        {
            Caracteristica caracteristica = new Caracteristica();
            if (string.IsNullOrEmpty(CampoCaracteristicasTXT.Text))
            {
                MessageBox.Show("Error al registrar la caracteristica, no debe de estar vacio el campo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var nombreCaracteristica = CampoCaracteristicasTXT.Text;
            if (Login.baseDatos == 2)
            {
                string caracteristica1 = CampoCaracteristicasTXT.Text.Trim();

                if (!string.IsNullOrEmpty(caracteristica1) && !CaracteristicasCB.Items.Contains(caracteristica1))
                {
                    CaracteristicasCB.Items.Add(caracteristica1);
                    CampoCaracteristicasTXT.Clear();
                }
                else
                {
                    MessageBox.Show("Escribe una característica válida y que no esté repetida.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            CampoCaracteristicasTXT.Clear();
        }

        private void CaracteristicasCB_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this.BeginInvoke(new Action(() =>
            {
                var seleccionadas = CaracteristicasCB.CheckedItems.Cast<string>().ToList();

                if (e.NewValue == CheckState.Checked)
                {
                    string nueva = CaracteristicasCB.Items[e.Index].ToString();
                    if (!seleccionadas.Contains(nueva))
                    {
                        seleccionadas.Add(nueva);
                    }
                }
                else
                {
                    string quitar = CaracteristicasCB.Items[e.Index].ToString();
                    seleccionadas.Remove(quitar);
                }

                CaracteristicasTXT.Text = string.Join(", ", seleccionadas);
            }));
        }

        private void AgregarBTN_Click(object sender, EventArgs e)
        {
            ServicioAdicional servicioAdicional = new ServicioAdicional();

            if (string.IsNullOrEmpty(ServicioTXT.Text) || string.IsNullOrEmpty(CostoServicioTXT.Text) || !CostoServicioTXT.Text.All(char.IsDigit))
            {
                MessageBox.Show("Error al registrar el servicio adicional, no debe de estar vacíos los campos o el precio debe contener solo dígitos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var nombreServicio = ServicioTXT.Text;
            var costo = Convert.ToInt32(CostoServicioTXT.Text);

            if (Login.baseDatos == 2)
            {
                if (Login.baseDatos == 2)
                {
                    Servicio nuevo = new Servicio { Nombre = nombreServicio, Costo = costo };
                    servicios.Add(nuevo);
                    ServicioAdicionalCB.Items.Add(nuevo);

                    ServicioTXT.Clear();
                    CostoServicioTXT.Clear();
                }
            }

            ServicioTXT.Clear();
            CostoServicioTXT.Clear();
        }

        private void BTN_RegistrarHotel_Click(object sender, EventArgs e)
        {
            ServicioAdicional servicio = new ServicioAdicional();
            if (string.IsNullOrEmpty(NombreHotelTXT.Text) || string.IsNullOrEmpty(ZonaTuristicaTXT.Text) || string.IsNullOrEmpty(NumeroPisosTXT.Text) || string.IsNullOrEmpty(NumeroHabitacionesTXT.Text) || string.IsNullOrEmpty(NumeroPiscinasTXT.Text) || string.IsNullOrEmpty(DomicilioTXT.Text) || string.IsNullOrEmpty(PaisCB.Text) || string.IsNullOrEmpty(EstadoCB.Text) || string.IsNullOrEmpty(CiudadCB.Text) || string.IsNullOrEmpty(FechaOperacionDTP.Text) || string.IsNullOrEmpty(FechaRegistroDTP.Text))
            {
                MessageBox.Show("Por favor, complete todos los campos.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!NumeroHabitacionesTXT.Text.All(char.IsDigit) || !NumeroPisosTXT.Text.All(char.IsDigit) || !NumeroPiscinasTXT.Text.All(char.IsDigit))
            {
                MessageBox.Show("El número de pisos, piscinas o habitaciones deben contener solo números.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (PlayaCH.Checked && NoPlayaCH.Checked)
            {
                MessageBox.Show("Seleccione solo una opción para el frente de playa.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (siSalon.Checked && noSalon.Checked)
            {
                MessageBox.Show("Seleccione solo una opción para el salón de eventos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (Convert.ToInt32(NumeroHabitacionesTXT.Text) < 0 || Convert.ToInt32(NumeroPisosTXT.Text) < 0 || Convert.ToInt32(NumeroPiscinasTXT.Text) < 0)
            {
                MessageBox.Show("El número de pisos, habitaciones o piscinas no puede ser menor a 0", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var nombreHotel = NombreHotelTXT.Text;
            var zonaTuristica = ZonaTuristicaTXT.Text;
            var numeroPisos = Convert.ToInt32(NumeroPisosTXT.Text);
            var fechaOperacion = DateTime.Parse(FechaOperacionDTP.Text);
            var fechaRegistro = DateTime.Parse(FechaRegistroDTP.Text);
            var numeroPiscinas = Convert.ToInt32(NumeroPiscinasTXT.Text);
            var numeroHabitaciones = Convert.ToInt32(NumeroHabitacionesTXT.Text);
            var pais = PaisCB.Text;
            var estado = EstadoCB.Text;
            var ciudad = CiudadCB.Text;
            var domicilio = DomicilioTXT.Text;
            var Playa = 0;
            var SalonEventos = 0;
            if (PlayaCH.Checked) Playa = 1;
            if (NoPlayaCH.Checked) Playa = 0;
            if (siSalon.Checked) SalonEventos = 1;
            if (noSalon.Checked) SalonEventos = 0;

            if (Login.baseDatos == 2)
            {
                List<string> serviciosSeleccionados = new List<string>();

                foreach (var item in ServicioAdicionalCB.SelectedItems)
                {
                    serviciosSeleccionados.Add(item.ToString());
                }

                Guid idHotelNuevo = Guid.NewGuid();
                Hotel1 nuevoHotel = new Hotel1
                {
                    id_hotel = idHotelNuevo,
                    id_usuario = idUsuario,
                    nombre_hotel = nombreHotel,
                    pais = pais,
                    estado = estado,
                    ciudad = ciudad,
                    domicilio = domicilio,
                    zona_turistica = zonaTuristica,

                    num_pisos = numeroPisos,
                    fecha_operacion = fechaOperacion,
                    frente_playa = Playa,
                    num_piscinas = numeroPiscinas,
                    salon_eventos = SalonEventos,
                    num_habitaciones = numeroHabitaciones,
                    fecha_registro = fechaRegistro,
                    fecha_modificacion = fechaRegistro
                };

                ResumenHoteles resumen = new ResumenHoteles
                {
                    Id_Hotel = idHotelNuevo,
                    Nombre_Hotel = nombreHotel,
                    Pais = pais,
                    Estado = estado,
                    Ciudad = ciudad
                };

                EnlaceCassandra enlace = new EnlaceCassandra();
                enlace.insertaHotel(nuevoHotel, servicios);
                enlace.insertaResumenHotel(resumen);

                MessageBox.Show("Se registró correctamente el hotel: " + nuevoHotel.nombre_hotel, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarTablasTipoHabCass();
            }
        }

        private void EliminarAmenidadBTN_Click(object sender, EventArgs e)
        {
            if (Login.baseDatos == 2)
            {
                if (AmenidadesCB.SelectedItem != null)
                {
                    string seleccion = AmenidadesCB.SelectedItem.ToString();
                    AmenidadesCB.Items.Remove(seleccion);

                    var seleccionados = AmenidadesCB.CheckedItems.Cast<string>().ToList();
                    AmenidadesTXT.Text = string.Join(", ", seleccionados);
                }
                else
                {
                    MessageBox.Show("Selecciona una amenidad para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void AmenidadesCB_ItemCheck(object sender, EventArgs e)
        {
            BeginInvoke((Action)(() =>
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in AmenidadesCB.CheckedItems)
                {
                    if (item is Amenidad amenidad)
                        sb.Append(amenidad.nombreAmenidad + ", ");
                }

                AmenidadesTXT.Text = sb.ToString().TrimEnd(' ', ',');
            }));
        }

        private void CaracteristicasCB_ItemCheck(object sender, EventArgs e)
        {
            BeginInvoke((Action)(() =>
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in CaracteristicasCB.CheckedItems)
                {
                    if (item is Caracteristica caracteristica)
                        sb.Append(caracteristica.nombreCaracteristica + ", ");
                }
                CaracteristicasTXT.Text = sb.ToString().TrimEnd(' ', ',');
            }));
        }

        private void ServicioAdicionalCB_ItemCheck(object sender, EventArgs e)
        {
            BeginInvoke((Action)(() =>
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in ServicioAdicionalCB.CheckedItems)
                {
                    if (item is ServicioAdicional servicioAdicional)
                        sb.Append(servicioAdicional.nombreServicio + ", ");
                }
                ServiciosTXT.Text = sb.ToString().TrimEnd(' ', ',');
            }));
        }

        private void listaHabitaciones_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                for (int i = 0; i < listaHabitaciones.Items.Count; i++)
                {
                    if (i != e.Index && listaHabitaciones.GetItemChecked(i))
                    {
                        listaHabitaciones.SetItemChecked(i, false);
                    }
                }
            }
            BeginInvoke((Action)(() =>
            {
                var item = listaHabitaciones.Items[e.Index];
                if (item is TipoHab tipoHab && listaHabitaciones.GetItemChecked(e.Index))
                {
                    NivelHabitacionHotelTXT.Text = tipoHab.nivelHabitacion;
                }
                else
                {
                    NivelHabitacionHotelTXT.Clear();
                }
            }));
            if (Login.baseDatos == 2)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    var item = listaHabitaciones.SelectedItem as TipoHabitacion;

                    if (item != null)
                    {
                        NivelHabitacionHotelTXT.Text = item.NivelHabitacion;
                    }
                });
            }
        }


        private void HotelCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Login.baseDatos == 2)
            {
                if (HotelCB.SelectedItem is Hotel1 hotelSeleccionado)
                {
                    int totalHabitaciones = hotelSeleccionado.num_habitaciones;
                    HabitacionesTotalesTXT.Text = totalHabitaciones.ToString();

                    EnlaceCassandra enlace = new EnlaceCassandra();
                    int asignadas = enlace.ObtenerCantidadAsignadaTotalPorHotel(hotelSeleccionado.id_hotel);

                    int restantes = totalHabitaciones - asignadas;
                    HabitacionesRestantesTXT.Text = restantes.ToString();

                    CargarDistribucionHabitaciones();
                }
            }
        }

        private void HabitacionesAsignadasTXT_TextChanged(object sender, EventArgs e)
        {
            int total = 0;
            int asignadas = 0;

            int.TryParse(HabitacionesTotalesTXT.Text, out total);
            int.TryParse(HabitacionesAsignadasTXT.Text, out asignadas);

            int restantes = total - asignadas;

            if (restantes < 0)
            {
                HabitacionesRestantesTXT.Text = "0";
            }
            else
            {
                HabitacionesRestantesTXT.Text = restantes.ToString();
            }
        }

        private void BTN_AsignarHabitacion_Click(object sender, EventArgs e)
        {
            if (!HabitacionesAsignadasTXT.Text.All(char.IsDigit) || string.IsNullOrEmpty(HabitacionesAsignadasTXT.Text) || Convert.ToInt32(HabitacionesAsignadasTXT.Text) < 0)
            {
                MessageBox.Show("El número de habitaciones asignadas debe contener solo números mayores a 0 y no debe estar vacio el campo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!(listaHabitaciones.CheckedItems.Count == 1))
            {
                MessageBox.Show("Debe seleccionar solo un tipo de habitación.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var NumeroHabitacion = Convert.ToInt32(HabitacionesAsignadasTXT.Text);
            var tipoSeleccionado = listaHabitaciones.CheckedItems[0] as TipoHab;

            if (Login.baseDatos == 2)
            {
                int totalHabitaciones;

                if (!int.TryParse(HabitacionesTotalesTXT.Text, out totalHabitaciones))
                {
                    MessageBox.Show("El número total de habitaciones no es válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var idHotel = (Guid)HotelCB.SelectedValue;
                var tipoSeleccionado1 = listaHabitaciones.CheckedItems[0] as TipoHabitacion;
                var idTipoHab = tipoSeleccionado1.IdTipoHab;

                EnlaceCassandra enlace = new EnlaceCassandra();
                int asignadas = enlace.ObtenerCantidadAsignada(idHotel, idTipoHab);
                int restantes = totalHabitaciones - asignadas;

                if (NumeroHabitacion > restantes)
                {
                    MessageBox.Show($"Solo quedan {restantes} habitaciones disponibles.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
                HabitacionesRestantesTXT.Text = restantes.ToString();

                DistribucionHabitacion RegistrarAsignacion = new DistribucionHabitacion
                {
                    Id_Hotel = idHotel,
                    Id_Tipo_Hab = idTipoHab,
                    Nombre_Tipo = tipoSeleccionado1.NivelHabitacion,
                    Cantidad = NumeroHabitacion
                };
                EnlaceCassandra enlace1 = new EnlaceCassandra();
                if (enlace1.ExisteAsignacion(idHotel, idTipoHab))
                {
                    enlace1.ActualizaCantidadAsignada(idHotel, idTipoHab, asignadas + NumeroHabitacion);
                }
                else
                {
                    enlace1.insertaAsignacion(RegistrarAsignacion);
                }

                MessageBox.Show("Habitacion registrada correctamente.", "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                CargarDistribucionHabitaciones();
            }
        }


        private void CargarTablasTipoHabCass()
        {
            EnlaceCassandra enlace = new EnlaceCassandra();
            var listaHoteles = enlace.GetAllHoteles();

            HotelCB.DataSource = listaHoteles;
            HotelCB.DisplayMember = "nombre_hotel";
            HotelCB.ValueMember = "id_hotel";

            HotelTHCB.DataSource = listaHoteles;
            HotelTHCB.DisplayMember = "nombre_hotel";
            HotelTHCB.ValueMember = "id_hotel";

            HotelesRegistradosDTG.DataSource = listaHoteles;

            HotelesRegistradosDTG.Columns["id_hotel"].Visible = false;
            HotelesRegistradosDTG.Columns["id_usuario"].Visible = false;
            HotelesRegistradosDTG.Columns["fecha_modificacion"].Visible = false;
            HotelesRegistradosDTG.Columns["salon_eventos"].Visible = false;
            HotelesRegistradosDTG.Columns["frente_playa"].Visible = false;

            HotelesRegistradosDTG.Columns["nombre_hotel"].HeaderText = "Hotel";
            HotelesRegistradosDTG.Columns["zona_turistica"].HeaderText = "Zona turistica";
            HotelesRegistradosDTG.Columns["num_pisos"].HeaderText = "# de pisos";
            HotelesRegistradosDTG.Columns["fecha_operacion"].HeaderText = "Fecha de operacion";
            HotelesRegistradosDTG.Columns["num_piscinas"].HeaderText = "# de piscinas";
            HotelesRegistradosDTG.Columns["salon_eventos"].HeaderText = "Salon de eventos";
            HotelesRegistradosDTG.Columns["num_habitaciones"].HeaderText = "# de habitaciones";
            HotelesRegistradosDTG.Columns["fecha_registro"].HeaderText = "Fecha de registro";
            HotelesRegistradosDTG.Columns["pais"].HeaderText = "Fecha de registro";
            HotelesRegistradosDTG.Columns["estado"].HeaderText = "Estado";
            HotelesRegistradosDTG.Columns["ciudad"].HeaderText = "Ciudad";
            HotelesRegistradosDTG.Columns["domicilio"].HeaderText = "Domicilio";

            HotelesRegistradosDTG.Columns["nombre_hotel"].DisplayIndex = 1;
            HotelesRegistradosDTG.Columns["zona_turistica"].DisplayIndex = 2;
            HotelesRegistradosDTG.Columns["num_pisos"].DisplayIndex = 3;
            HotelesRegistradosDTG.Columns["fecha_operacion"].DisplayIndex = 4;
            HotelesRegistradosDTG.Columns["num_piscinas"].DisplayIndex = 5;
            HotelesRegistradosDTG.Columns["salon_eventos"].DisplayIndex = 6;
            HotelesRegistradosDTG.Columns["frente_playa"].DisplayIndex = 7;
            HotelesRegistradosDTG.Columns["num_habitaciones"].DisplayIndex = 8;
            HotelesRegistradosDTG.Columns["fecha_registro"].DisplayIndex = 9;
            HotelesRegistradosDTG.Columns["estado"].DisplayIndex = 10;
            HotelesRegistradosDTG.Columns["ciudad"].DisplayIndex = 11;
            HotelesRegistradosDTG.Columns["domicilio"].DisplayIndex = 12;

            TablaHotelDTG.DataSource = listaHoteles;

            TablaHotelDTG.Columns["id_hotel"].Visible = false;
            TablaHotelDTG.Columns["id_usuario"].Visible = false;
            TablaHotelDTG.Columns["fecha_modificacion"].Visible = false;

            TablaHotelDTG.Columns["nombre_hotel"].HeaderText = "Nombre Hotel";
            TablaHotelDTG.Columns["zona_turistica"].HeaderText = "Zona Turística";
            TablaHotelDTG.Columns["num_pisos"].HeaderText = "# de Pisos";
            TablaHotelDTG.Columns["fecha_operacion"].HeaderText = "Fecha Operación";
            TablaHotelDTG.Columns["num_piscinas"].HeaderText = "# de Piscinas";
            TablaHotelDTG.Columns["salon_eventos"].HeaderText = "Salón Eventos";
            TablaHotelDTG.Columns["frente_playa"].HeaderText = "Frente a la Playa";
            TablaHotelDTG.Columns["num_habitaciones"].HeaderText = "# de Habitaciones";
            TablaHotelDTG.Columns["fecha_registro"].HeaderText = "Fecha de Registro";

            TablaHotelDTG.Columns["nombre_hotel"].DisplayIndex = 1;
            TablaHotelDTG.Columns["zona_turistica"].DisplayIndex = 2;
            TablaHotelDTG.Columns["num_pisos"].DisplayIndex = 3;
            TablaHotelDTG.Columns["fecha_operacion"].DisplayIndex = 4;
            TablaHotelDTG.Columns["num_piscinas"].DisplayIndex = 5;
            TablaHotelDTG.Columns["salon_eventos"].DisplayIndex = 6;
            TablaHotelDTG.Columns["frente_playa"].DisplayIndex = 7;
            TablaHotelDTG.Columns["num_habitaciones"].DisplayIndex = 8;
            TablaHotelDTG.Columns["fecha_registro"].DisplayIndex = 9;

            EnlaceCassandra enlace1 = new EnlaceCassandra();
            var listaTipoHab = enlace1.GetAllTipoHabitaciones();

            listaHabitaciones.DataSource = listaTipoHab;
            listaHabitaciones.DisplayMember = "NivelHabitacion";
            listaHabitaciones.ValueMember = "IdTipoHab";

            TablaTipoHabitacion.DataSource = listaTipoHab;
            TablaTipoHabitacion.Columns["IdTipoHab"].Visible = false;
            TablaTipoHabitacion.Columns["IdHotel"].Visible = false;
            TablaTipoHabitacion.Columns["FrenteA"].Visible = false;
            TablaTipoHabitacion.Columns["Estatus"].Visible = false;
            TablaTipoHabitacion.Columns["NivelHabitacion"].HeaderText = "Nivel";
            TablaTipoHabitacion.Columns["NumeroCamas"].HeaderText = "# de camas";
            TablaTipoHabitacion.Columns["TipoCama"].HeaderText = "Tipo de cama";
            TablaTipoHabitacion.Columns["Precio"].HeaderText = "Precio";
            TablaTipoHabitacion.Columns["NumeroPersonas"].HeaderText = "# de personas";
            TablaTipoHabitacion.Columns["FrenteA"].HeaderText = "Frente a";
            TablaTipoHabitacion.Columns["NombreHotel"].HeaderText = "Hotel";

            TablaTipoHabitacion.Columns["IdTipoHab"].DisplayIndex = 0;
            TablaTipoHabitacion.Columns["NivelHabitacion"].DisplayIndex = 1;
            TablaTipoHabitacion.Columns["NumeroCamas"].DisplayIndex = 2;
            TablaTipoHabitacion.Columns["TipoCama"].DisplayIndex = 3;
            TablaTipoHabitacion.Columns["Precio"].DisplayIndex = 4;
            TablaTipoHabitacion.Columns["NumeroPersonas"].DisplayIndex = 5;
            TablaTipoHabitacion.Columns["FrenteA"].DisplayIndex = 6;
            TablaTipoHabitacion.Columns["Precio"].DefaultCellStyle.Format = "C2";

            TablaTipoHabDTG.DataSource = listaTipoHab;
            TablaTipoHabDTG.Columns["IdTipoHab"].Visible = false;
            TablaTipoHabDTG.Columns["IdHotel"].Visible = false;
            TablaTipoHabDTG.Columns["FrenteA"].Visible = false;
            TablaTipoHabDTG.Columns["Estatus"].Visible = false;
            TablaTipoHabDTG.Columns["NivelHabitacion"].HeaderText = "Nivel";
            TablaTipoHabDTG.Columns["NumeroCamas"].HeaderText = "# de camas";
            TablaTipoHabDTG.Columns["TipoCama"].HeaderText = "Tipo de cama";
            TablaTipoHabDTG.Columns["Precio"].HeaderText = "Precio";
            TablaTipoHabDTG.Columns["NumeroPersonas"].HeaderText = "# de personas";
            TablaTipoHabDTG.Columns["NombreHotel"].HeaderText = "Hotel";

            TablaTipoHabDTG.Columns["NombreHotel"].DisplayIndex = 0;
            TablaTipoHabDTG.Columns["NivelHabitacion"].DisplayIndex = 1;
            TablaTipoHabDTG.Columns["NumeroCamas"].DisplayIndex = 2;
            TablaTipoHabDTG.Columns["TipoCama"].DisplayIndex = 3;
            TablaTipoHabDTG.Columns["Precio"].DisplayIndex = 4;
            TablaTipoHabDTG.Columns["NumeroPersonas"].DisplayIndex = 5;
            TablaTipoHabDTG.Columns["FrenteA"].DisplayIndex = 6;
            TablaTipoHabDTG.Columns["Precio"].DefaultCellStyle.Format = "C2";
        }

        private void CargarDistribucionHabitaciones()
        {
            EnlaceCassandra enlace = new EnlaceCassandra();

            var listaHoteles = enlace.GetAllHoteles();
            var distribuciones = enlace.GetAllDistribucionHab();
            if (distribuciones != null || listaHoteles != null)
            {
            }

            var datosGrid = (from d in distribuciones
                             join h in listaHoteles on d.Id_Hotel equals h.id_hotel
                             select new
                             {
                                 Hotel = h.nombre_hotel,
                                 Cantidad = d.Cantidad,
                                 NivelHabitacion = d.Nombre_Tipo
                             }).ToList();

            TablaTHAsignadasDTG.DataSource = datosGrid;
            TablaTHAsignadasDTG.Columns["NivelHabitacion"].HeaderText = "Nivel";
        }

        private void EliminarBTN_Click(object sender, EventArgs e)
        {
            if (Login.baseDatos == 2)
            {
                if (ServicioAdicionalCB.SelectedItem is Servicio servicioSeleccionado)
                {
                    servicios.Remove(servicioSeleccionado);

                    ServicioAdicionalCB.Items.Remove(servicioSeleccionado);

                    MessageBox.Show("Servicio eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Selecciona un servicio para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void AmenidadesCB_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                if (Login.baseDatos == 2)
                {
                    var seleccionados = AmenidadesCB.CheckedItems.Cast<string>().ToList();

                    string itemActual = AmenidadesCB.Items[e.Index].ToString();
                    if (e.NewValue == CheckState.Checked && !seleccionados.Contains(itemActual))
                    {
                        seleccionados.Add(itemActual);
                    }
                    else if (e.NewValue == CheckState.Unchecked && seleccionados.Contains(itemActual))
                    {
                        seleccionados.Remove(itemActual);
                    }

                    AmenidadesTXT.Text = string.Join(", ", seleccionados);
                }
            });
        }

        private void EliminarCaracteristicaBTN_Click(object sender, EventArgs e)
        {
            if (Login.baseDatos == 2)
            {
                string seleccion = CaracteristicasCB.SelectedItem.ToString();
                CaracteristicasCB.Items.Remove(seleccion);

                var seleccionadas = CaracteristicasCB.CheckedItems.Cast<string>().ToList();
                CaracteristicasTXT.Text = string.Join(", ", seleccionadas);
            }
            else
            {
                MessageBox.Show("Selecciona una característica para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

    }
}