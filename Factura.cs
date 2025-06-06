using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace AAVD
{
    public partial class Factura : Form
    {
        public Factura()
        {
            InitializeComponent();
        }

        private List<CheckOutLigero> checkOuts;
        private Guid idUsuario = SesionUsuario.IdUsuarioActual;
        private List<ClienteLigero> clientes;

        public class Filtro
        {
            public string Apellidos { get; set; }
            public string RFC { get; set; }
            public string Correo { get; set; }
        }

        private void AbrirControlEnPanel(System.Windows.Forms.UserControl control)
        {
            MenuContenedor.Controls.Clear();
            control.Dock = DockStyle.Fill;
            MenuContenedor.Controls.Add(control);
            control.BringToFront();
        }

        private void Factura_Load(object sender, EventArgs e)
        {
            AbrirControlEnPanel(new Menu());
            var NuevoForm = new Login();
            if (Login.baseDatos == 2)
            {
                LlenarCombosCheckOuts();
            }
            this.FindForm().Size = NuevoForm.Size;
            this.FindForm().StartPosition = FormStartPosition.Manual;
            this.FindForm().Location = NuevoForm.Location;
        }

        private void LlenarCombosCheckOuts()
        {
            var enlace = new EnlaceCassandra();
            checkOuts = enlace.Get_CheckOuts_Ligeros();
            if (checkOuts == null || !checkOuts.Any())
            {
                return;
            }

            ApellidosCB.DataSource = checkOuts;
            ApellidosCB.DisplayMember = "apellidos";
            ApellidosCB.ValueMember = "codigo_reserva";

            RFCCB.DataSource = checkOuts;
            RFCCB.DisplayMember = "rfc";
            RFCCB.ValueMember = "id_cliente";

            CorreoCB.DataSource = checkOuts;
            CorreoCB.DisplayMember = "correo";
            CorreoCB.ValueMember = "id_cliente";
        }

        private void CargarDetallesHotelPorCliente()
        {
            if (RFCCB.SelectedValue is Guid idCliente)
            {
                var enlace = new EnlaceCassandra();

                var reservacion = enlace.ObtenerReservacionPorIdCliente1(idCliente);

                if (reservacion == null)
                {
                    MessageBox.Show("No se encontró la reservación.");
                    return;
                }

                var enlace1 = new EnlaceCassandra();
                var servicios = enlace1.ObtenerServiciosPorHotel(reservacion.id_hotel);

                if (servicios != null && servicios.Any())
                {
                    ServiciosDTG.DataSource = servicios;
                    if (ServiciosDTG.Columns["Costo"] != null)
                    {
                        ServiciosDTG.Columns["Costo"].DefaultCellStyle.Format = "C2";
                        ServiciosDTG.Columns["Costo"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }

                    ServiciosDTG.Columns["Nombre_Servicio"].HeaderText = "Servicio";
                    ServiciosDTG.Columns["Id_Servicio"].HeaderText = "Código";

                    if (ServiciosDTG.Columns["Id_Servicio"] != null)
                        ServiciosDTG.Columns["Id_Servicio"].Visible = false;
                }
                else
                {
                    ServiciosDTG.DataSource = null;
                    MessageBox.Show("Este hotel no tiene servicios adicionales registrados.");
                }
                var habitacion = enlace.ObtenerTipoHabitacionPorId1(reservacion.id_tipohab);

                if (habitacion != null)
                {
                    NombreHotel.Text = habitacion.nombreHotel;
                    NivelHabitacion.Text = habitacion.nivelHabitacion;
                }
                else
                {
                    NombreHotel.Text = "(No encontrado)";
                    NivelHabitacion.Text = "(No encontrado)";
                }

                var hotelResumen = enlace.ObtenerResumenHotelPorId(reservacion.id_hotel);
                var resumen = enlace.ObtenerResumenReservacionPorCliente(idCliente);

                if (hotelResumen != null)
                {
                    DomicilioHotel.Text = hotelResumen.Ciudad;
                }
                else
                {
                    DomicilioHotel.Text = "(No disponible)";
                }

                if (habitacion != null && reservacion != null)
                {
                    var hospedajeInfo = new[]
                    {
                    new {
                        nivelHabitacion = habitacion.nivelHabitacion,
                        anticipo = resumen.Anticipo,
                        restante = resumen.Pendiente,
                        totalHospedaje = resumen.Total
                        }
                    };

                    HospedajeDTG.DataSource = hospedajeInfo;

                    HospedajeDTG.Columns["anticipo"].DefaultCellStyle.Format = "C2";
                    HospedajeDTG.Columns["restante"].DefaultCellStyle.Format = "C2";
                    HospedajeDTG.Columns["totalHospedaje"].DefaultCellStyle.Format = "C2";

                    HospedajeDTG.Columns["nivelHabitacion"].HeaderText = "Nivel Habitación";
                    HospedajeDTG.Columns["anticipo"].HeaderText = "Anticipo";
                    HospedajeDTG.Columns["restante"].HeaderText = "Restante";
                    HospedajeDTG.Columns["totalHospedaje"].HeaderText = "Total";
                }
            }
            else
            {
                MessageBox.Show("ID de cliente inválido.");
            }
        }

        private void BuscarBTN_Click(object sender, EventArgs e)
        {
            Filtro filtro = new Filtro
            {
                Apellidos = ApellidosCB.Text,
                RFC = RFCCB.Text,
                Correo = CorreoCB.Text
            };

            if (string.IsNullOrWhiteSpace(filtro.Apellidos) && string.IsNullOrWhiteSpace(filtro.RFC) && string.IsNullOrWhiteSpace(filtro.Correo))
            {
                MessageBox.Show("Seleccione al menos un criterio de búsqueda (apellidos, RFC o correo).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                NombreCompleto.Text = "";
                RFCTXT.Text = "";
                TelefonoCasa.Text = "";
                TelefonoCelular.Text = "";
                CiudadTXT.Text = "";
                CodigoPostal.Text = "";
                NombreHotel.Text = "";
                NivelHabitacion.Text = "";
                DomicilioHotel.Text = "";
                MetodoPago.Text = "";
                Anticipo.Text = "";
                TotalTXT.Text = "";
                return;
            }

            if (Login.baseDatos == 2)
            {
                if (RFCCB.SelectedValue is Guid idCliente)
                {
                    EnlaceCassandra enlace1 = new EnlaceCassandra();
                    var cliente1 = enlace1.ObtenerClientePorId(idCliente);

                    if (cliente1 != null)
                    {
                        NombreCompleto.Text = $"{cliente1.Nombre} {cliente1.PrimerApellido} {cliente1.SegundoApellido}";
                        RFCTXT.Text = cliente1.RFC;

                        TelefonoCasa.Text = cliente1.TelefonoCasa.FirstOrDefault().Value ?? "";
                        TelefonoCelular.Text = cliente1.TelefonoCelular.FirstOrDefault().Value ?? "";
                        CiudadTXT.Text = cliente1.Ciudad;
                        CodigoPostal.Text = cliente1.CodigoPostal.ToString();
                    }
                    else
                    {
                        MessageBox.Show("Cliente no encontrado.");
                    }
                    CargarResumenReservacion();
                    NumeroSerie.Text = Guid.NewGuid().ToString();
                    FolioFiscal.Text = Guid.NewGuid().ToString();
                    FolioInterno.Text = Guid.NewGuid().ToString("N").Substring(0, 8);
                    CargarDetallesHotelPorCliente();
                }
            }
        }

        private void ApellidosCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ApellidosCB.SelectedValue is Guid codigo_reserva)
            {
                var selected = checkOuts.FirstOrDefault(c => c.codigo_reserva == codigo_reserva);
                if (selected != null)
                {
                    RFCCB.Text = selected.rfc;
                    CorreoCB.Text = selected.correo;
                }
                else
                {
                    MessageBox.Show("No se encontró información de CheckOut para esta reservación.");
                }
            }
            else
            {
            }
        }

        private void CargarResumenReservacion()
        {
            if (RFCCB.SelectedValue is Guid idCliente)
            {
                var enlace = new EnlaceCassandra();
                var resumen = enlace.ObtenerResumenReservacionPorCliente(idCliente);

                if (resumen != null)
                {
                    MetodoPago.Text = resumen.Metodo_pago ?? "(Sin método)";
                    Servicios.Text = resumen.Total_servicios.ToString("C");
                    Anticipo.Text = resumen.Anticipo.ToString("C");

                    Subtotal.Text = resumen.Subtotal.ToString("C");
                    TotalImpuesto.Text = resumen.IVA.ToString("C");
                    TotalTXT.Text = resumen.Total.ToString("C");
                    RestanteTXT.Text = resumen.Pendiente.ToString("C");
                    string totalLetra = NumeroALetras.Convertir(resumen.Total);
                    TotalLetra.Text = totalLetra.ToUpper() + " M.N.";

                }
                else
                {
                    MessageBox.Show("No se encontró el resumen de reservación para este cliente.");
                }
            }
            else
            {
                MessageBox.Show("ID de cliente inválido.");
            }
        }
    }

    public static class NumeroALetras
    {
        public static string Convertir(decimal numero)
        {
            long entero = (long)Math.Floor(numero);
            int centavos = (int)((numero - entero) * 100);

            string letras = NumeroEnLetras(entero) + " pesos";

            if (centavos > 0)
            {
                letras += " con " + NumeroEnLetras(centavos) + " centavos";
            }

            return letras;
        }

        private static string NumeroEnLetras(long numero)
        {
            if (numero == 0) return "cero";
            if (numero < 0) return "menos " + NumeroEnLetras(Math.Abs(numero));

            string[] unidades = { "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve",
                              "diez", "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete",
                              "dieciocho", "diecinueve" };

            string[] decenas = { "", "", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta",
                             "ochenta", "noventa" };

            string[] centenas = { "", "ciento", "doscientos", "trescientos", "cuatrocientos", "quinientos",
                              "seiscientos", "setecientos", "ochocientos", "novecientos" };

            string resultado = "";

            if (numero == 100) return "cien";

            if (numero < 20)
            {
                resultado = unidades[numero];
            }
            else if (numero < 100)
            {
                resultado = decenas[numero / 10];
                if (numero % 10 > 0)
                    resultado += " y " + unidades[numero % 10];
            }
            else if (numero < 1000)
            {
                resultado = centenas[numero / 100];
                if (numero % 100 > 0)
                    resultado += " " + NumeroEnLetras(numero % 100);
            }
            else if (numero < 1000000)
            {
                long miles = numero / 1000;
                long resto = numero % 1000;

                if (miles == 1)
                    resultado = "mil";
                else
                    resultado = NumeroEnLetras(miles) + " mil";

                if (resto > 0)
                    resultado += " " + NumeroEnLetras(resto);
            }
            else if (numero < 1000000000000)
            {
                long millones = numero / 1000000;
                long resto = numero % 1000000;

                if (millones == 1)
                    resultado = "un millón";
                else
                    resultado = NumeroEnLetras(millones) + " millones";

                if (resto > 0)
                    resultado += " " + NumeroEnLetras(resto);
            }

            return resultado.Trim();
        }
    }
}