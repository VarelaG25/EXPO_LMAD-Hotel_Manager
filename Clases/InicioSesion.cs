using System;
using System.Collections.Generic;

namespace AAVD
{
    public static class SesionUsuario
    {
        public static Guid IdUsuarioActual { get; set; }
        public static string NombreUsuario { get; set; }
    }

    public class InicioSesion
    {
        public string correoUsuario { get; set; }
        public string contraseniaaUsuario { get; set; }
        public int tipoUsuario { get; set; }
    }

    public class ClienteDatos1
    {
        public Guid Id_Cliente { get; set; }
        public string Nombre { get; set; }
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
        public DateTime FechaNacimientoCliente { get; set; }

        public Dictionary<int, string> TelefonoCasa { get; set; }
        public Dictionary<int, string> TelefonoCelular { get; set; }

        public string EstadoCivil { get; set; }
        public string RFC { get; set; }

        public HashSet<string> Correo { get; set; }

        public DateTime FechaRegistro { get; set; }
        public DateTime FechaModificacion { get; set; }

        public Guid Id_Ubicacion { get; set; }
        public string Ciudad { get; set; }
        public string Estado { get; set; }
        public string Pais { get; set; }
        public int CodigoPostal { get; set; }

        public string NombreCompleto
        {
            get
            {
                return $"{Nombre} {PrimerApellido} {SegundoApellido}";
            }
        }
        public string CorreoMostrar
        {
            get
            {
                return Correo != null && Correo.Count > 0 ? string.Join(", ", Correo) : "Sin correo";
            }
        }
        public string TelefonoCasaMostrar
        {
            get
            {
                return TelefonoCasa != null && TelefonoCasa.Count > 0
                    ? string.Join(", ", TelefonoCasa.Values)
                    : "Sin teléfono de casa";
            }
        }
        public string TelefonoCelularMostrar
        {
            get
            {
                return TelefonoCelular != null && TelefonoCelular.Count > 0
                    ? string.Join(", ", TelefonoCelular.Values)
                    : "Sin celular";
            }
        }
    }

    public class UsuarioDatos1
    {
        public Guid Id_Usuario { get; set; }
        public bool Id_Admin { get; set; }
        public int NumeroNomina { get; set; }
        public string NombreUsuario { get; set; }
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
        public string TelefonoCelular { get; set; }
        public string TelefonoCasa { get; set; }

        public string CorreoUsuario { get; set; }
        public string ContrasenaUsuario { get; set; }
        public DateTime FechaRegistroUsuario { get; set; }
        public DateTime FechaModificacionUsuario { get; set; }

        public string NombreCompleto
        {
            get
            {
                return $"{NombreUsuario} {PrimerApellido} {SegundoApellido}";
            }
        }
    }

    public class Servicio
    {
        public string Nombre { get; set; }
        public decimal Costo { get; set; }

        public override string ToString()
        {
            return $"{Nombre} (${Costo})";
        }
    }

    public class Hotel1
    {
        public Guid id_hotel { get; set; }
        public Guid id_usuario { get; set; }
        public string nombre_hotel { get; set; }
        public string pais { get; set; }
        public string estado { get; set; }
        public string ciudad { get; set; }
        public string domicilio { get; set; }
        public string zona_turistica { get; set; }

        public int num_pisos { get; set; }
        public DateTime fecha_operacion { get; set; }
        public int frente_playa { get; set; }
        public int num_piscinas { get; set; }
        public int salon_eventos { get; set; }
        public int num_habitaciones { get; set; }
        public DateTime fecha_registro { get; set; }
        public DateTime fecha_modificacion { get; set; }
    }

    public class ResumenHoteles
    {
        public Guid Id_Hotel { get; set; }
        public string Nombre_Hotel { get; set; }
        public string Pais { get; set; }
        public string Estado { get; set; }
        public string Ciudad { get; set; }
    }

    public class TipoHabitacion
    {
        public Guid IdTipoHab { get; set; }
        public Guid IdHotel { get; set; }
        public string NombreHotel { get; set; }
        public string NivelHabitacion { get; set; }
        public int NumeroCamas { get; set; }
        public string TipoCama { get; set; }
        public decimal Precio { get; set; }
        public int NumeroPersonas { get; set; }
        public bool FrenteA { get; set; }
        public string Caracteristicas { get; set; }
        public string Amenidades { get; set; }
        public int Estatus { get; set; }
    }

    public class DistribucionHabitacion
    {
        public Guid Id_Hotel { get; set; }
        public Guid Id_Tipo_Hab { get; set; }
        public string Nombre_Tipo { get; set; }
        public int Cantidad { get; set; }
    }

    public class Reservacion1
    {
        public Guid Id_Reservacion { get; set; }
        public Guid Id_Usuario { get; set; }
        public Guid Id_Hotel { get; set; }
        public Guid Id_TipoHab { get; set; }
        public string NombreHotel { get; set; }
        public Guid Id_Cliente { get; set; }
        public string Apellidos { get; set; }
        public string Rfc { get; set; }
        public string Correo { get; set; }
        public Guid CodigoReserva { get; set; }
        public DateTime FechaLlegada { get; set; }
        public DateTime FechaSalida { get; set; }
        public decimal Total { get; set; }
        public decimal Total_servicios { get; set; }
        public decimal Anticipo { get; set; }
        public decimal Restante { get; set; }
        public string MetodoPago { get; set; }
        public int PersonasHospedadas { get; set; }
        public string Estatus { get; set; }
        public DateTime FechaCheckIn { get; set; }
        public DateTime FechaCheckOut { get; set; }
        public bool CheckIn { get; set; }
        public bool CheckOut { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public class ReservacionDatos
    {
        public Guid IdReservacion { get; set; }
        public Guid IdCliente { get; set; }
        public Guid IdTipoHab { get; set; }
        public string NombreHotel { get; set; }
    }

    public class ReservacionDatos1
    {
        public Guid id_tipohab { get; set; }
        public Guid id_hotel { get; set; }
    }

    public class TipoHabitacion1
    {
        public Guid IdTipoHab { get; set; }
        public Guid IdHotel { get; set; }
        public string nombreHotel { get; set; }
        public string nivelHabitacion { get; set; }
    }

    public class ResumenHoteles1
    {
        public Guid Id_Hotel { get; set; }
        public string Ciudad { get; set; }
    }

    public class ServicioAdicional
    {
        public Guid Id_Servicio { get; set; }
        public string Nombre_Servicio { get; set; }
        public decimal Costo { get; set; }
    }

    //auxiliar -------------------------------------------

    public class ReservacionCheckInDatos
    {
        public Guid IdReservacion { get; set; }
        public Guid IdCliente { get; set; }
        public string Apellidos { get; set; }
        public string RFC { get; set; }
        public string Correo { get; set; }
        public Guid CodigoReserva { get; set; }
        public string Estatus { get; set; }
        public DateTime FechaCheckIn { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public class ReservacionCheckOutDatos
    {
        public Guid IdReservacion { get; set; }
        public Guid IdCliente { get; set; }
        public string Apellidos { get; set; }
        public string RFC { get; set; }
        public string Correo { get; set; }
        public Guid CodigoReserva { get; set; }
        public string Estatus { get; set; }
        public DateTime FechaCheckOut { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public class CancelacionDatos
    {
        public Guid CodigoReserva { get; set; }
        public Guid IdReservacion { get; set; }
        public Guid IdCliente { get; set; }
        public string Apellidos { get; set; }
        public string RFC { get; set; }
        public string Correo { get; set; }
        public DateTime FechaCancelacion { get; set; }
    }

    public class DistribucionGrid
    {
        public string Hotel { get; set; }
        public int Cantidad { get; set; }
        public string NivelHabitacion { get; set; }
    }

    public class ServicioAdicionalView
    {
        public Guid IdServicio { get; set; }
        public string NombreServicio { get; set; }
        public decimal Costo { get; set; }

        public override string ToString()
        {
            return $"{NombreServicio} - ${Costo:F2}";
        }
    }

    public class ClienteLigero
    {
        public Guid Id_Cliente { get; set; }
        public string PrimerApellido { get; set; } // sin ?
        public string SegundoApellido { get; set; }
        public string RFC { get; set; }
        public HashSet<string> Correo { get; set; }
    }

    public class CheckOutLigero
    {
        public Guid id_reservacion { get; set; }
        public Guid id_cliente { get; set; }
        public Guid codigo_reserva { get; set; }
        public string apellidos { get; set; }
        public string rfc { get; set; }
        public string correo { get; set; }
    }

    public class ResumenReservacion
    {
        public Guid Id_Cliente { get; set; }
        public string RFC { get; set; }
        public string MetodoPago { get; set; }
        public decimal TotalServicios { get; set; }
        public decimal Subtotal { get; set; }
        public decimal IVA { get; set; }
        public decimal Total { get; set; }
        public decimal Anticipo { get; set; }
        public decimal Pendiente { get; set; }
    }

    public class ResumenReservacionDatos
    {
        public Guid IdCliente { get; set; }
        public string RFC { get; set; }
        public string Metodo_pago { get; set; }
        public decimal Total_servicios { get; set; }
        public decimal Subtotal { get; set; }
        public decimal IVA { get; set; }
        public decimal Total { get; set; }
        public decimal Anticipo { get; set; }
        public decimal Pendiente { get; set; }
    }

    public class CheckInResumen
    {
        public Guid CodigoReserva { get; set; }
        public string Apellidos { get; set; }
        public string Correo { get; set; }
        public DateTime FechaCheckIn { get; set; }
    }

    public class HistorialReservacion
    {
        public string Hotel { get; set; }
        public string NombreCompleto { get; set; }
        public int PersonasHospedadas { get; set; }
        public Guid CodigoReserva { get; set; }
        public DateTime FechaReservacion { get; set; }
        public decimal Anticipo { get; set; }
        public decimal Total { get; set; }
        public decimal MontoServicios { get; set; }
        public decimal MontoHospedaje { get; set; }
        public DateTime? FechaCheckIn { get; set; }
        public string EstatusCheckIn { get; set; }
        public DateTime? FechaCheckOut { get; set; }
        public string EstatusCheckOut { get; set; }
    }

    public class ReporteOcupacion
    {
        public string Ciudad { get; set; }
        public string Hotel { get; set; }
        public int Anio { get; set; }
        public string Mes { get; set; }
        public string TipoHabitacion { get; set; }
        public int HabitacionesOcupadas { get; set; }
        public int TotalHabitaciones { get; set; }
        public double PorcentajeOcupacion { get; set; }
    }

    public class ReporteOcupacionHotel
    {
        public string Ciudad { get; set; }
        public string Hotel { get; set; }
        public int Anio { get; set; }
        public string Mes { get; set; }
        public int HabitacionesOcupadas { get; set; }
        public int HabitacionesTotales { get; set; }
        public double PorcentajeOcupacion { get; set; }
    }
}