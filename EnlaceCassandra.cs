using Cassandra;
using Cassandra.Mapping;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace AAVD
{
    internal class EnlaceCassandra
    {
        private static string _dbServer { set; get; }
        private static string _dbKeySpace { set; get; }
        private static Cluster _cluster;
        private static ISession _session;

        private static void conectar()
        {
            _dbServer = ConfigurationManager.AppSettings["node"].ToString();
            _dbKeySpace = ConfigurationManager.AppSettings["database"].ToString();

            _cluster = Cluster.Builder()
                .AddContactPoint(_dbServer)
                .Build();

            _session = _cluster.Connect(_dbKeySpace);
        }

        private static void desconectar()
        {
            _cluster.Dispose();
        }


        private string ConvertMapToCql(Dictionary<int, string> map)
        {
            if (map == null || map.Count == 0) return "{}";
            var items = map.Select(kvp => $"{kvp.Key}: '{kvp.Value}'");
            return "{" + string.Join(", ", items) + "}";
        }

        private string ConvertSetToCql(HashSet<string> set)
        {
            if (set == null || set.Count == 0) return "{}";
            var items = set.Select(item => $"'{item}'");
            return "{" + string.Join(", ", items) + "}";
        }

        public void insertaCliente(ClienteDatos1 param)
        {
            try
            {
                conectar();

                string query = "INSERT INTO cliente (" +
                               "Id_Cliente, Nombre, PrimerApellido, SegundoApellido, FechaNacimientoCliente, " +
                               "TelefonoCasa, TelefonoCelular, EstadoCivil, RFC, Correo, " +
                               "FechaRegistro, FechaModificacion, Id_Ubicacion, Ciudad, Estado, Pais, " +
                               "CodigoPostal) " +
                               "VALUES ({0}, '{1}', '{2}', '{3}', '{4}', {5}, {6}, '{7}', '{8}', {9}, '{10}', '{11}', {12}, '{13}', '{14}', '{15}', {16});";

                string telefonosCasa = ConvertMapToCql(param.TelefonoCasa);
                string telefonosCel = ConvertMapToCql(param.TelefonoCelular);
                string correos = ConvertSetToCql(param.Correo);

                string qry = string.Format(query,
                    param.Id_Cliente,
                    param.Nombre,
                    param.PrimerApellido,
                    param.SegundoApellido,
                    param.FechaNacimientoCliente.ToString("yyyy-MM-dd"),
                    telefonosCasa,
                    telefonosCel,
                    param.EstadoCivil,
                    param.RFC,
                    correos,
                    param.FechaRegistro.ToString("yyyy-MM-dd"),
                    param.FechaModificacion.ToString("yyyy-MM-dd"),
                    param.Id_Ubicacion,
                    param.Ciudad,
                    param.Estado,
                    param.Pais,
                    param.CodigoPostal
                );

                _session.Execute(qry);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error: " + e.Message, "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                desconectar();
            }
        }

        public void insertaUsuario(UsuarioDatos1 param)
        {
            try
            {
                conectar();

                string query = "INSERT INTO usuarios (" +
                               "Id_Usuario, Id_Admin, NumeroNomina, NombreUsuario, PrimerApellido, SegundoApellido, " +
                               "TelefonoCelular, TelefonoCasa, CorreoUsuario, ContrasenaUsuario, " +
                               "FechaRegistroUsuario, FechaModificacionUsuario) " +
                               "VALUES ({0}, {1}, {2}, '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}');";

                string qry = string.Format(query,
                    param.Id_Usuario,
                    param.Id_Admin,
                    param.NumeroNomina,
                    param.NombreUsuario,
                    param.PrimerApellido,
                    param.SegundoApellido,
                    param.TelefonoCelular,
                    param.TelefonoCasa,
                    param.CorreoUsuario,
                    param.ContrasenaUsuario,
                    param.FechaRegistroUsuario.ToString("yyyy-MM-dd"),
                    param.FechaModificacionUsuario.ToString("yyyy-MM-dd")
                );

                _session.Execute(qry);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error: " + e.Message, "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                desconectar();
            }
        }

        public void insertaInicioSesion(InicioSesion param)
        {
            try
            {
                conectar();

                string query = "INSERT INTO login_usuarios (correoUsuario, contraseniaaUsuario, tipoUsuario) " +
                               "VALUES ('{0}', '{1}', {2});";

                string qry = string.Format(query,
                    param.correoUsuario,
                    param.contraseniaaUsuario,
                    param.tipoUsuario
                );

                _session.Execute(qry);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error: " + e.Message, "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                desconectar();
            }
        }

        public void insertaHotel(Hotel1 hotel, List<Servicio> servicios)
        {
            try
            {
                conectar();

                string queryHotel = string.Format(
                    "INSERT INTO hoteles (id_hotel, id_usuario, nombre_hotel, pais, estado, ciudad, domicilio, zona_turistica, num_pisos, fecha_operacion, frente_playa, num_piscinas, salon_eventos, num_habitaciones, fecha_registro, fecha_modificacion) " +
                    "VALUES ({0}, {1}, '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', {8}, '{9}', {10}, {11}, {12}, {13}, '{14}', '{15}');",
                    hotel.id_hotel, hotel.id_usuario, hotel.nombre_hotel, hotel.pais, hotel.estado, hotel.ciudad, hotel.domicilio, hotel.zona_turistica, hotel.num_pisos, hotel.fecha_operacion.ToString("yyyy-MM-dd"), hotel.frente_playa, hotel.num_piscinas, hotel.salon_eventos, hotel.num_habitaciones, hotel.fecha_registro.ToString("yyyy-MM-dd"), hotel.fecha_modificacion.ToString("yyyy-MM-dd"));

                _session.Execute(queryHotel);

                foreach (var servicio in servicios)
                {
                    Guid idServicio = Guid.NewGuid();

                    string queryServicio = string.Format(
                        "INSERT INTO servicios_adicionales (id_hotel, id_servicio, nombre_servicio, costo) " +
                        "VALUES ({0}, {1}, '{2}', {3});",
                        hotel.id_hotel,
                        idServicio,
                        servicio.Nombre.Replace("'", "''"),
                        servicio.Costo.ToString().Replace(",", ".")
                    );

                    _session.Execute(queryServicio);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al insertar hotel: " + ex.Message);
            }
            finally
            {
                desconectar();
            }
        }

        public void insertaResumenHotel(ResumenHoteles resumen)
        {
            try
            {
                conectar();
                string query = string.Format(
                    "INSERT INTO hoteles_resumen (Id_Hotel, Nombre_Hotel, Pais, Estado, Ciudad) " +
                    "VALUES ({0}, '{1}', '{2}', '{3}', '{4}');",
                    resumen.Id_Hotel, resumen.Nombre_Hotel, resumen.Pais, resumen.Estado, resumen.Ciudad
                );

                _session.Execute(query);
            }
            finally
            {
                desconectar();
            }
        }

        public void insertaTipoHabitacion(TipoHabitacion habitacion)
        {
            try
            {
                conectar();

                string query = "INSERT INTO tipoHabitacion (" +
                               "IdTipoHab, IdHotel, nombreHotel, nivelHabitacion, numeroCamas, tipoCama, precio, numeroPersonas, frenteA, caracteristicas, amenidades, estatus" +
                               ") VALUES ({0}, {1}, '{2}', '{3}', {4}, '{5}', {6}, {7}, {8}, '{9}', '{10}', {11});";

                string cql = string.Format(query,
                    habitacion.IdTipoHab,
                    habitacion.IdHotel,
                    habitacion.NombreHotel.Replace("'", "''"),
                    habitacion.NivelHabitacion.Replace("'", "''"),
                    habitacion.NumeroCamas,
                    habitacion.TipoCama.Replace("'", "''"),
                    habitacion.Precio.ToString().Replace(",", "."),
                    habitacion.NumeroPersonas,
                    habitacion.FrenteA,
                    habitacion.Caracteristicas.Replace("'", "''"),
                    habitacion.Amenidades.Replace("'", "''"),
                    habitacion.Estatus
                );

                _session.Execute(cql);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error al insertar habitación: " + e.Message);
            }
            finally
            {
                desconectar();
            }
        }

        public void insertaAsignacion(DistribucionHabitacion habitacion)
        {
            try
            {
                conectar();
                string query = "INSERT INTO distribucion_habitaciones (id_hotel, id_tipo_hab, nombre_tipo, cantidad) " +
                            "VALUES ({0}, {1}, '{2}',{3});";

                string qry = string.Format(query,
                    habitacion.Id_Hotel,
                    habitacion.Id_Tipo_Hab,
                    habitacion.Nombre_Tipo,
                    habitacion.Cantidad
                );

                _session.Execute(qry);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al insertar asignación en Cassandra: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void insertaReservacion(Reservacion1 param)
        {
            try
            {
                conectar();

                string query = "INSERT INTO reservaciones (" +
                               "id_reservacion, id_usuario, id_hotel, id_tipohab, nombre_hotel, id_cliente, apellidos, " +
                               "rfc, correo, codigo_reserva, fecha_llegada, fecha_salida, total, total_servicios, anticipo, restante, " +
                               "metodo_pago, personas_hospedadas, estatus, fecha_checkin, fecha_checkout, " +
                               "checkin, checkout, fecha_registro) " +
                               "VALUES ({0}, {1}, {2}, {3}, '{4}', {5}, '{6}', '{7}', '{8}', {9}, '{10}', '{11}', {12}, {13}, {14}, {15}, " +
                               "'{16}', {17}, '{18}', '{19}', '{20}', {21}, {22}, '{23}');";

                string qry = string.Format(query,
                    param.Id_Reservacion,
                    param.Id_Usuario,
                    param.Id_Hotel,
                    param.Id_TipoHab,
                    param.NombreHotel?.Replace("'", "''") ?? "",
                    param.Id_Cliente,
                    param.Apellidos?.Replace("'", "''") ?? "",
                    param.Rfc?.Replace("'", "''") ?? "",
                    param.Correo?.Replace("'", "''") ?? "",
                    param.CodigoReserva,
                    param.FechaLlegada.ToString("yyyy-MM-dd HH:mm:ss"),
                    param.FechaSalida.ToString("yyyy-MM-dd HH:mm:ss"),
                    param.Total.ToString(CultureInfo.InvariantCulture),
                    param.Total_servicios.ToString(CultureInfo.InvariantCulture),
                    param.Anticipo.ToString(CultureInfo.InvariantCulture),
                    param.Restante.ToString(CultureInfo.InvariantCulture),
                    param.MetodoPago?.Replace("'", "''") ?? "",
                    param.PersonasHospedadas,
                    param.Estatus?.Replace("'", "''") ?? "",
                    param.FechaCheckIn.ToString("yyyy-MM-dd HH:mm:ss"),
                    param.FechaCheckOut.ToString("yyyy-MM-dd HH:mm:ss"),
                    param.CheckIn ? "true" : "false",
                    param.CheckOut ? "true" : "false",
                    param.FechaRegistro.ToString("yyyy-MM-dd HH:mm:ss")
                );

                _session.Execute(qry);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error al insertar reservación: " + e.Message, "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                desconectar();
            }
        }

        public void insertaResumenRes(ResumenReservacion param)
        {
            try
            {
                conectar();

                string query = "INSERT INTO resumen_reservacion (" +
                               "id_cliente, rfc, metodo_pago, total_servicios, subtotal, iva, total, anticipo, pendiente" +
                               ") VALUES ({0}, '{1}', '{2}', {3}, {4}, {5}, {6}, {7}, {8});";

                string qry = string.Format(query,
                    param.Id_Cliente,
                    param.RFC.Replace("'", "''"),
                    param.MetodoPago.Replace("'", "''"),
                    param.TotalServicios.ToString(CultureInfo.InvariantCulture),
                    param.Subtotal.ToString(CultureInfo.InvariantCulture),
                    param.IVA.ToString(CultureInfo.InvariantCulture),
                    param.Total.ToString(CultureInfo.InvariantCulture),
                    param.Anticipo.ToString(CultureInfo.InvariantCulture),
                    param.Pendiente.ToString(CultureInfo.InvariantCulture)
                );

                _session.Execute(qry);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error al insertar resumen de reservación: " + e.Message, "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                desconectar();
            }
        }
        public void ActualizarCheckIn(ReservacionCheckInDatos datos)
        {
            try
            {
                conectar();

                string query = "UPDATE reservaciones SET checkin = true WHERE id_reservacion = {0};";

                string cql = string.Format(query,
                    datos.IdReservacion
                );

                _session.Execute(cql);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error al actualizar check-In en reservaciones: " + e.Message, "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                desconectar();
            }
        } 
        public void ActualizarCheckOut(ReservacionCheckOutDatos datos)
        {
            try
            {
                conectar();

                string query = "UPDATE reservaciones SET checkout = true WHERE id_reservacion = {0};";

                string cql = string.Format(query,
                    datos.IdReservacion
                );

                _session.Execute(cql);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error al actualizar check-Out en reservaciones: " + e.Message, "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                desconectar();
            }
        }

        public void InsertarReservacionCheckIn(ReservacionCheckInDatos datos)
        {
            try
            {
                conectar();

                string query = "INSERT INTO reservacionCheckIn (" +
                               "id_reservacion, id_cliente, apellidos, rfc, correo, " +
                               "codigo_reserva, estatus, fecha_checkin, fecha_registro) " +
                               "VALUES ({0}, {1}, '{2}', '{3}', '{4}', {5}, '{6}', '{7}', '{8}');";

                string cql = string.Format(query,
                    datos.IdReservacion,
                    datos.IdCliente,
                    datos.Apellidos.Replace("'", "''"),
                    datos.RFC.Replace("'", "''"),
                    datos.Correo.Replace("'", "''"),
                    datos.CodigoReserva,
                    datos.Estatus,
                    datos.FechaCheckIn.ToString("yyyy-MM-dd HH:mm:ss"),
                    datos.FechaRegistro.ToString("yyyy-MM-dd HH:mm:ss")
                );

                _session.Execute(cql);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error al insertar en reservacionCheckIn: " + e.Message, "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                desconectar();
            }
        }

        public void InsertarReservacionCheckOut(ReservacionCheckOutDatos datos)
        {
            try
            {
                conectar();

                string query = "INSERT INTO reservacionCheckOut (" +
                               "id_reservacion, id_cliente, apellidos, rfc, correo, " +
                               "codigo_reserva, estatus, fecha_checkout, fecha_registro) " +
                               "VALUES ({0}, {1}, '{2}', '{3}', '{4}', {5}, '{6}', '{7}', '{8}');";

                string cql = string.Format(query,
                    datos.IdReservacion,
                    datos.IdCliente,
                    datos.Apellidos.Replace("'", "''"),
                    datos.RFC.Replace("'", "''"),
                    datos.Correo.Replace("'", "''"),
                    datos.CodigoReserva,
                    datos.Estatus,
                    datos.FechaCheckOut.ToString("yyyy-MM-dd HH:mm:ss"),
                    datos.FechaRegistro.ToString("yyyy-MM-dd HH:mm:ss")
                );

                _session.Execute(cql);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error al insertar en reservacionCheckOut: " + e.Message, "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                desconectar();
            }
        }

        public DateTime? ObtenerFechaCheckInPorCodigoReserva(Guid codigoReserva)
        {
            try
            {
                conectar();

                string query = "SELECT fecha_checkin FROM reservacionCheckIn WHERE codigo_reserva = ?;";
                var prepared = _session.Prepare(query);
                var bound = prepared.Bind(codigoReserva);

                var row = _session.Execute(bound).FirstOrDefault();

                if (row != null && !row.IsNull("fecha_checkin"))
                {
                    return row.GetValue<DateTime>("fecha_checkin");
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener la fecha de check-in: " + ex.Message);
                return null;
            }
            finally
            {
                desconectar();
            }
        }

        public DateTime? ObtenerFechaCheckOutPorCodigoReserva(Guid codigoReserva)
        {
            try
            {
                conectar();

                string query = $"SELECT fecha_checkout FROM reservacionCheckOut WHERE codigo_reserva = {codigoReserva};";
                var rs = _session.Execute(query);

                var row = rs.FirstOrDefault();
                if (row != null && !row.IsNull("fecha_checkout"))
                {
                    return row.GetValue<DateTime>("fecha_checkout");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener fecha de check-out: " + ex.Message);
            }
            finally
            {
                desconectar();
            }

            return null;
        }

        public void InsertarCancelacion(CancelacionDatos datos)
        {
            try
            {
                conectar();

                string query = "INSERT INTO cancelaciones (" +
                               "codigo_reserva, id_reservacion, id_cliente, apellidos, rfc, correo, fecha_cancelacion) " +
                               "VALUES ({0}, {1}, {2}, '{3}', '{4}', '{5}', '{6}');";

                string cql = string.Format(query,
                    datos.CodigoReserva,
                    datos.IdReservacion,
                    datos.IdCliente,
                    datos.Apellidos.Replace("'", "''"),
                    datos.RFC.Replace("'", "''"),
                    datos.Correo.Replace("'", "''"),
                    datos.FechaCancelacion.ToString("yyyy-MM-dd HH:mm:ss")
                );

                _session.Execute(cql);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar cancelación: " + ex.Message);
            }
            finally
            {
                desconectar();
            }
        }

        public IEnumerable<InicioSesion> Get_One_Login(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
            {
                throw new ArgumentException("El correo no puede estar vacío.");
            }

            string query = "SELECT correoUsuario, contraseniaaUsuario, tipoUsuario FROM login_usuarios WHERE correoUsuario = ?;";
            conectar();
            IMapper mapper = new Mapper(_session);

            IEnumerable<InicioSesion> users = mapper.Fetch<InicioSesion>(query, correo);

            desconectar();
            return users.ToList();
        }

        public List<ClienteDatos1> Get_All_Clientes()
        {
            string query = "SELECT Id_Cliente, Nombre, PrimerApellido, SegundoApellido, FechaNacimientoCliente, " +
                           "TelefonoCasa, TelefonoCelular, EstadoCivil, RFC, Correo, " +
                           "FechaRegistro, FechaModificacion, Id_Ubicacion, Ciudad, Estado, Pais, CodigoPostal " +
                           "FROM cliente;";

            conectar();

            IMapper mapper = new Mapper(_session);
            IEnumerable<ClienteDatos1> clientes = mapper.Fetch<ClienteDatos1>(query);

            desconectar();

            return clientes.ToList();
        }

        public List<ClienteLigero> Get_Clientes_Ligeros()
        {
            string query = "SELECT Id_Cliente, PrimerApellido, SegundoApellido, RFC, Correo FROM cliente;";

            conectar();

            IMapper mapper = new Mapper(_session);
            IEnumerable<ClienteLigero> clientes = mapper.Fetch<ClienteLigero>(query);

            desconectar();

            return clientes.ToList();
        }

        public ClienteDatos1 ObtenerClientePorId(Guid idCliente)
        {
            string query = "SELECT Id_Cliente, Nombre, PrimerApellido, SegundoApellido, TelefonoCasa, TelefonoCelular, RFC, Ciudad, CodigoPostal " +
                "FROM cliente WHERE Id_Cliente = ?;";

            conectar();

            IMapper mapper = new Mapper(_session);
            ClienteDatos1 cliente = mapper.SingleOrDefault<ClienteDatos1>(
                query, idCliente
            );

            desconectar();

            return cliente;
        }

        public ResumenReservacionDatos ObtenerResumenReservacionPorCliente(Guid idCliente)
        {
            try
            {
                conectar();

                string query = "SELECT id_cliente, rfc, metodo_pago, total_servicios, subtotal, iva, total, anticipo, pendiente " +
                               "FROM resumen_reservacion WHERE id_cliente = ?;";

                IMapper mapper = new Mapper(_session);
                ResumenReservacionDatos resumen = mapper.SingleOrDefault<ResumenReservacionDatos>(query, idCliente);

                return resumen;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener el resumen de reservación: " + ex.Message);
                return null;
            }
            finally
            {
                desconectar();
            }
        }

        public List<UsuarioDatos1> GetAllUsuarios()
        {
            List<UsuarioDatos1> usuarios = new List<UsuarioDatos1>();

            try
            {
                conectar();

                string query = "SELECT Id_Usuario, Id_Admin, NumeroNomina, NombreUsuario, PrimerApellido, SegundoApellido, " +
                               "TelefonoCelular, TelefonoCasa, CorreoUsuario, ContrasenaUsuario, FechaRegistroUsuario, FechaModificacionUsuario " +
                               "FROM usuarios;";

                IMapper mapper = new Mapper(_session);
                IEnumerable<UsuarioDatos1> results = mapper.Fetch<UsuarioDatos1>(query);

                usuarios = results.ToList();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error al obtener usuarios: " + e.Message);
            }
            finally
            {
                desconectar();
            }

            return usuarios;
        }

        public UsuarioDatos1 GetUsuarioPorCorreo(string correo)
        {
            UsuarioDatos1 usuario = null;

            try
            {
                conectar();

                string query = "SELECT Id_Usuario, Id_Admin, NumeroNomina, NombreUsuario, PrimerApellido, SegundoApellido, " +
                               "TelefonoCelular, TelefonoCasa, CorreoUsuario, ContrasenaUsuario, FechaRegistroUsuario, FechaModificacionUsuario " +
                               "FROM usuarios WHERE CorreoUsuario = ?;";

                IMapper mapper = new Mapper(_session);
                var resultado = mapper.Fetch<UsuarioDatos1>(query, correo).FirstOrDefault();

                usuario = resultado;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error al obtener el usuario por correo: " + e.Message);
            }
            finally
            {
                desconectar();
            }

            return usuario;
        }

        public List<Hotel1> GetAllHoteles()
        {
            List<Hotel1> hoteles = new List<Hotel1>();

            try
            {
                conectar();

                string query = "SELECT id_hotel, id_usuario, nombre_hotel, pais, estado, ciudad, domicilio, zona_turistica, num_pisos, fecha_operacion, frente_playa, num_piscinas, salon_eventos, num_habitaciones, fecha_registro, fecha_modificacion FROM hoteles;";

                IMapper mapper = new Mapper(_session);
                IEnumerable<Hotel1> results = mapper.Fetch<Hotel1>(query);

                hoteles = results.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener hoteles: " + ex.Message);
            }
            finally
            {
                desconectar();
            }

            return hoteles;
        }

        public List<TipoHabitacion> GetAllTipoHabitaciones()
        {
            List<TipoHabitacion> habitaciones = new List<TipoHabitacion>();

            try
            {
                conectar();

                string query = "SELECT IdTipoHab, IdHotel, nombreHotel, nivelHabitacion, numeroCamas, tipoCama, precio, " +
                               "numeroPersonas, frenteA, caracteristicas, amenidades, estatus FROM tipoHabitacion;";

                IMapper mapper = new Mapper(_session);
                IEnumerable<TipoHabitacion> results = mapper.Fetch<TipoHabitacion>(query);

                habitaciones = results.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener las habitaciones: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                desconectar();
            }

            return habitaciones;
        }

        public TipoHabitacion ObtenerTipoHabitacionPorId(Guid idTipoHab)
        {
            try
            {
                conectar();

                string query = $"SELECT * FROM tipoHabitacion WHERE IdTipoHab = {idTipoHab};";

                IMapper mapper = new Mapper(_session);
                var habitacion = mapper.Single<TipoHabitacion>(query);

                return habitacion;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener detalles de la habitación: " + ex.Message);
                return null;
            }
            finally
            {
                desconectar();
            }
        }

        public ReservacionDatos ObtenerReservacionPorIdCliente(Guid idCliente)
        {
            try
            {
                conectar();

                string query = $"SELECT * FROM reservaciones WHERE id_cliente = {idCliente};";
                IMapper mapper = new Mapper(_session);

                var reservacion = mapper.SingleOrDefault<ReservacionDatos>(query);
                return reservacion;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener la reservación: " + ex.Message);
                return null;
            }
            finally
            {
                desconectar();
            }
        }

        public DataTable ObtenerDatosReporte()
        {
            DataTable tabla = new DataTable();

            try
            {
                conectar();

                tabla.Columns.Add("Hotel", typeof(string));
                tabla.Columns.Add("Ciudad", typeof(string));
                tabla.Columns.Add("Pais", typeof(string));
                tabla.Columns.Add("Año", typeof(int));
                tabla.Columns.Add("Mes", typeof(string));
                tabla.Columns.Add("Ingresos por hospedaje", typeof(decimal));
                tabla.Columns.Add("Ingresos por servicios adicionales", typeof(decimal));
                tabla.Columns.Add("Ingresos totales", typeof(decimal));

                var reservacionesQuery = "SELECT id_hotel, fecha_registro, anticipo, restante, total_servicios, total FROM reservaciones;";
                var reservaciones = _session.Execute(reservacionesQuery);

                foreach (var fila in reservaciones)
                {
                    if (fila.IsNull("id_hotel") || fila.IsNull("fecha_registro")) continue;

                    Guid idHotel = fila.GetValue<Guid>("id_hotel");
                    DateTime fechaRegistro = fila.GetValue<DateTime>("fecha_registro");

                    decimal anticipo = fila.GetValue<decimal?>("anticipo") ?? 0;
                    decimal restante = fila.GetValue<decimal?>("restante") ?? 0;
                    decimal totalServicios = fila.GetValue<decimal?>("total_servicios") ?? 0;
                    decimal total = fila.GetValue<decimal?>("total") ?? 0;

                    decimal ingresoHospedaje = anticipo + restante - totalServicios;

                    var hotelQuery = $"SELECT nombre_hotel, pais, ciudad FROM hoteles_resumen WHERE id_hotel = {idHotel};";
                    var resultadoHotel = _session.Execute(hotelQuery).FirstOrDefault();

                    string nombreHotel = resultadoHotel?.GetValue<string>("nombre_hotel") ?? "Desconocido";
                    string ciudad = resultadoHotel?.GetValue<string>("ciudad") ?? "Desconocido";
                    string pais = resultadoHotel?.GetValue<string>("pais") ?? "Desconocido";

                    tabla.Rows.Add(
                        nombreHotel,
                        ciudad,
                        pais,
                        fechaRegistro.Year,
                        fechaRegistro.ToString("MMM", CultureInfo.InvariantCulture),
                        ingresoHospedaje,
                        totalServicios,
                        total
                    );
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error al obtener datos del reporte: " + e.Message, "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                desconectar();
            }

            return tabla;
        }

        public ReservacionDatos1 ObtenerReservacionPorIdCliente1(Guid idCliente)
        {
            conectar();
            string query = $"SELECT id_tipohab, id_hotel FROM reservaciones WHERE id_cliente = {idCliente};";
            IMapper mapper = new Mapper(_session);
            var reservacion = mapper.SingleOrDefault<ReservacionDatos1>(query);
            desconectar();
            return reservacion;
        }

        public List<ServicioAdicional> ObtenerServiciosPorHotel(Guid idHotel)
        {
            try
            {
                conectar();

                string query = $"SELECT id_servicio, nombre_servicio, costo FROM servicios_adicionales WHERE id_hotel = {idHotel};";
                IMapper mapper = new Mapper(_session);
                var servicios = mapper.Fetch<ServicioAdicional>(query).ToList();

                return servicios;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener servicios adicionales: " + ex.Message);
                return new List<ServicioAdicional>();
            }
            finally
            {
                desconectar();
            }
        }

        public TipoHabitacion1 ObtenerTipoHabitacionPorId1(Guid idTipoHab)
        {
            conectar();
            string query = $"SELECT * FROM tipoHabitacion WHERE IdTipoHab = {idTipoHab};";
            IMapper mapper = new Mapper(_session);
            var habitacion = mapper.SingleOrDefault<TipoHabitacion1>(query);
            desconectar();
            return habitacion;
        }

        public ResumenHoteles1 ObtenerResumenHotelPorId(Guid idHotel)
        {
            conectar();
            string query = $"SELECT ciudad FROM hoteles_resumen WHERE id_hotel = {idHotel};";
            IMapper mapper = new Mapper(_session);
            var resumen = mapper.SingleOrDefault<ResumenHoteles1>(query);
            desconectar();
            return resumen;
        }

        public List<ResumenHoteles> GetAllResumenHoteles()
        {
            List<ResumenHoteles> lista = new List<ResumenHoteles>();

            try
            {
                conectar();
                string query = "SELECT id_hotel, nombre_hotel, pais, estado, ciudad FROM hoteles_resumen;";
                IMapper mapper = new Mapper(_session);
                lista = mapper.Fetch<ResumenHoteles>(query).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener resumen de hoteles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return lista;
        }

        public List<Reservacion1> GetAllReservacion()
        {
            string query = "SELECT id_reservacion, id_usuario, id_hotel, id_tipohab, nombre_hotel, id_cliente, apellidos,rfc, correo, codigo_reserva, fecha_llegada, fecha_salida, total, anticipo, restante, metodo_pago, personas_hospedadas, estatus, fecha_checkin, fecha_checkout, checkin, checkout, fecha_registro FROM reservaciones";

            conectar();
            IMapper mapper = new Mapper(_session);
            IEnumerable<Reservacion1> result = mapper.Fetch<Reservacion1>(query);
            desconectar();

            return result.ToList();
        }

        public Reservacion1 ObtenerInfoReservacionPorId(Guid idReservacion)
        {
            try
            {
                conectar();

                var prepared = _session.Prepare("SELECT codigo_reserva, fecha_registro, apellidos, rfc, correo, metodo_pago, restante, fecha_llegada, fecha_salida, total FROM reservaciones WHERE id_reservacion = ?");
                var bound = prepared.Bind(idReservacion);

                var rowSet = _session.Execute(bound);
                var row = rowSet.FirstOrDefault();

                if (row == null)
                {
                    MessageBox.Show("No se encontró ninguna reservación con ese ID.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return null;
                }

                var reservacion = new Reservacion1
                {
                    CodigoReserva = row.GetValue<Guid>("codigo_reserva"),
                    FechaRegistro = row.GetValue<DateTime>("fecha_registro"),
                    Apellidos = row.GetValue<string>("apellidos"),
                    Rfc = row.GetValue<string>("rfc"),
                    Correo = row.GetValue<string>("correo"),
                    MetodoPago = row.GetValue<string>("metodo_pago"),
                    Restante = row.GetValue<decimal>("restante"),
                    Total = row.GetValue<decimal>("total"),
                    FechaLlegada = row.GetValue<DateTime>("fecha_llegada"),
                    FechaSalida = row.GetValue<DateTime>("fecha_salida")
                };

                return reservacion;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener datos de la reservación: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                desconectar();
            }
        }

        public bool ExisteCheckInPorCodigoReserva(Guid codigoReserva)
        {
            try
            {
                conectar();

                string query = "SELECT codigo_reserva FROM reservacionCheckIn WHERE codigo_reserva = ?;";
                var statement = _session.Prepare(query);
                var bound = statement.Bind(codigoReserva);

                var result = _session.Execute(bound);

                return result.Any();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al verificar código de reserva: " + ex.Message);
                return true;
            }
            finally
            {
                desconectar();
            }
        }

        public int ObtenerCantidadAsignada(Guid idHotel, Guid idTipoHab)
        {
            int totalAsignadas = 0;

            try
            {
                conectar();
                string query = "SELECT cantidad FROM distribucion_habitaciones WHERE id_hotel = ? AND id_tipo_hab = ?;";

                IMapper mapper = new Mapper(_session);
                var resultado = mapper.Fetch<int>(query, idHotel, idTipoHab).FirstOrDefault();

                totalAsignadas = resultado;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener cantidad asignada: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return totalAsignadas;
        }

        public bool ExisteAsignacion(Guid idHotel, Guid idTipoHab)
        {
            try
            {
                conectar();
                string query = "SELECT COUNT(*) FROM distribucion_habitaciones WHERE id_hotel = ? AND id_tipo_hab = ?;";
                var mapper = new Mapper(_session);
                var resultado = mapper.Fetch<int>(query, idHotel, idTipoHab).FirstOrDefault();

                return resultado > 0;
            }
            catch
            {
                return false;
            }
        }

        public int ObtenerCantidadAsignadaTotalPorHotel(Guid idHotel)
        {
            int totalAsignadas = 0;

            try
            {
                conectar();
                string query = "SELECT cantidad FROM distribucion_habitaciones WHERE id_hotel = ?;";

                IMapper mapper = new Mapper(_session);
                var resultados = mapper.Fetch<DistribucionHabitacion>(query, idHotel);

                foreach (var item in resultados)
                {
                    totalAsignadas += item.Cantidad;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener cantidad asignada: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return totalAsignadas;
        }

        public List<DistribucionHabitacion> GetAllDistribucionHab()
        {
            List<DistribucionHabitacion> lista = new List<DistribucionHabitacion>();

            try
            {
                conectar();
                string query = "SELECT id_hotel, cantidad, nombre_tipo FROM distribucion_habitaciones;";
                IMapper mapper = new Mapper(_session);
                lista = mapper.Fetch<DistribucionHabitacion>(query).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener la distribución de habitaciones: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return lista;
        }

        public List<DistribucionHabitacion> GetDistribucionPorHotel(Guid idHotel)
        {
            List<DistribucionHabitacion> lista = new List<DistribucionHabitacion>();

            try
            {
                conectar();
                string query = "SELECT id_tipo_hab, nombre_tipo, cantidad FROM distribucion_habitaciones WHERE id_hotel = ?;";
                IMapper mapper = new Mapper(_session);
                lista = mapper.Fetch<DistribucionHabitacion>(query, idHotel).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener tipos de habitación: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return lista;
        }

        public List<ServicioAdicionalView> GetServiciosAdicionales(Guid idHotel)
        {
            conectar();

            string query = $"SELECT id_servicio, nombre_servicio, costo FROM servicios_adicionales WHERE id_hotel = {idHotel};";
            var servicios = new List<ServicioAdicionalView>();

            var rs = _session.Execute(query);

            foreach (var row in rs)
            {
                servicios.Add(new ServicioAdicionalView
                {
                    IdServicio = row.GetValue<Guid>("id_servicio"),
                    NombreServicio = row.GetValue<string>("nombre_servicio"),
                    Costo = row.GetValue<decimal>("costo")
                });
            }

            return servicios;
        }

        public int ObtenerCantidadHabitaciones(Guid idHotel, Guid idTipoHab)
        {
            try
            {
                conectar();

                string query = "SELECT cantidad FROM distribucion_habitaciones " +
                               "WHERE id_hotel = {0} AND id_tipo_hab = {1};";

                string qry = string.Format(query, idHotel, idTipoHab);

                var result = _session.Execute(qry);
                var row = result.FirstOrDefault();

                if (row != null && row["cantidad"] != null)
                {
                    return Convert.ToInt32(row["cantidad"]);
                }

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error al consultar la cantidad de habitaciones: " + e.Message);
                return -1;
            }
            finally
            {
                desconectar();
            }
        }

        public List<CheckOutLigero> Get_CheckOuts_Ligeros()
        {
            string query = "SELECT id_reservacion, id_cliente, apellidos, rfc, correo, codigo_reserva FROM reservacionCheckOut;";

            conectar();

            IMapper mapper = new Mapper(_session);
            IEnumerable<CheckOutLigero> checkOuts = mapper.Fetch<CheckOutLigero>(query);

            desconectar();

            return checkOuts.ToList();
        }

        public int ObtenerReservacionesTraslapadas(Guid idHotel, Guid idTipoHab, DateTime fechaLlegada, DateTime fechaSalida)
        {
            try
            {
                conectar();

                string query = "SELECT COUNT(*) FROM reservaciones " +
                               "WHERE id_hotel = {0} AND id_tipohab = {1} " +
                               "AND fecha_llegada < '{2}' AND fecha_salida > '{3}' ALLOW FILTERING;";

                string qry = string.Format(query,
                    idHotel,
                    idTipoHab,
                    fechaSalida.ToString("yyyy-MM-dd HH:mm:ss"),
                    fechaLlegada.ToString("yyyy-MM-dd HH:mm:ss")
                );

                var result = _session.Execute(qry);
                var row = result.FirstOrDefault();

                if (row != null)
                {
                    return Convert.ToInt32(row[0]);
                }

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error al consultar reservaciones traslapadas: " + e.Message);
                return -1;
            }
            finally
            {
                desconectar();
            }
        }

        public List<ReservacionCheckInDatos> ObtenerTodosCheckIns()
        {
            List<ReservacionCheckInDatos> lista = new List<ReservacionCheckInDatos>();

            try
            {
                conectar();

                string query = "SELECT id_reservacion, id_cliente, apellidos, rfc, correo, " +
                               "codigo_reserva, estatus, fecha_checkin, fecha_registro " +
                               "FROM reservacionCheckIn;";

                var rs = _session.Execute(query);

                foreach (var row in rs)
                {
                    var dato = new ReservacionCheckInDatos
                    {
                        IdReservacion = row.GetValue<Guid>("id_reservacion"),
                        IdCliente = row.GetValue<Guid>("id_cliente"),
                        Apellidos = row.GetValue<string>("apellidos"),
                        RFC = row.GetValue<string>("rfc"),
                        Correo = row.GetValue<string>("correo"),
                        CodigoReserva = row.GetValue<Guid>("codigo_reserva"),
                        Estatus = row.GetValue<string>("estatus"),
                        FechaCheckIn = row.GetValue<DateTime>("fecha_checkin"),
                        FechaRegistro = row.GetValue<DateTime>("fecha_registro")
                    };

                    lista.Add(dato);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener datos de check-ins: " + ex.Message);
            }
            finally
            {
                desconectar();
            }

            return lista;
        }

        public List<ReservacionCheckOutDatos> ObtenerTodosCheckOuts()
        {
            List<ReservacionCheckOutDatos> lista = new List<ReservacionCheckOutDatos>();

            try
            {
                conectar();

                string query = "SELECT id_reservacion, id_cliente, apellidos, rfc, correo, " +
                               "codigo_reserva, estatus, fecha_checkout, fecha_registro " +
                               "FROM reservacionCheckOut;";

                var rs = _session.Execute(query);

                foreach (var row in rs)
                {
                    var dato = new ReservacionCheckOutDatos
                    {
                        IdReservacion = row.GetValue<Guid>("id_reservacion"),
                        IdCliente = row.GetValue<Guid>("id_cliente"),
                        Apellidos = row.GetValue<string>("apellidos"),
                        RFC = row.GetValue<string>("rfc"),
                        Correo = row.GetValue<string>("correo"),
                        CodigoReserva = row.GetValue<Guid>("codigo_reserva"),
                        Estatus = row.GetValue<string>("estatus"),
                        FechaCheckOut = row.GetValue<DateTime>("fecha_checkout"),
                        FechaRegistro = row.GetValue<DateTime>("fecha_registro")
                    };

                    lista.Add(dato);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener datos de check-ins: " + ex.Message);
            }
            finally
            {
                desconectar();
            }

            return lista;
        }

        public DateTime? ObtenerFechaLlegadaPorCodigoReserva(Guid idReservacion)
        {
            try
            {
                conectar();

                string query = $"SELECT fecha_llegada FROM reservaciones WHERE id_reservacion =  {idReservacion};";
                var rs = _session.Execute(query);
                var row = rs.FirstOrDefault();

                if (row != null && !row.IsNull("fecha_llegada"))
                {
                    return row.GetValue<DateTime>("fecha_llegada");
                }

                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener fecha de llegada: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                desconectar();
            }
        }

        public ReservacionCheckOutDatos ObtenerInfoReservacionCheckOutPorCodigo(Guid codigoReserva)
        {
            try
            {
                conectar();

                string query = "SELECT id_reservacion, id_cliente, apellidos, rfc, correo, codigo_reserva, estatus, fecha_checkout, fecha_registro " +
                               "FROM reservacionCheckOut WHERE codigo_reserva = ?;";

                var prepared = _session.Prepare(query);
                var statement = prepared.Bind(codigoReserva);
                var result = _session.Execute(statement).FirstOrDefault();

                if (result != null)
                {
                    return new ReservacionCheckOutDatos
                    {
                        IdReservacion = result.GetValue<Guid>("id_reservacion"),
                        IdCliente = result.GetValue<Guid>("id_cliente"),
                        Apellidos = result.GetValue<string>("apellidos"),
                        RFC = result.GetValue<string>("rfc"),
                        Correo = result.GetValue<string>("correo"),
                        CodigoReserva = result.GetValue<Guid>("codigo_reserva"),
                        Estatus = result.GetValue<string>("estatus"),
                        FechaCheckOut = result.GetValue<DateTime>("fecha_checkout"),
                        FechaRegistro = result.GetValue<DateTime>("fecha_registro")
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener datos de CheckOut: " + ex.Message);
            }
            finally
            {
                desconectar();
            }

            return null;
        }

        public List<Row> ObtenerReservacionesDesdeCassandra()
        {
            try
            {
                conectar();

                string query = "SELECT * FROM reservaciones WHERE checkout = true;";
                var rs = _session.Execute(query);

                return rs.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener reservaciones: " + ex.Message);
                return new List<Row>();
            }
            finally
            {
                desconectar();
            }
        }

        public (DateTime? fecha, string estatus) ObtenerCheckIn(Guid codigoReserva)
        {
            conectar();
            try
            {
                var rs = _session.Execute(
                    new SimpleStatement("SELECT fecha_checkin, estatus FROM reservacionCheckIn WHERE codigo_reserva = ?", codigoReserva));
                var row = rs.FirstOrDefault();
                return row != null
                    ? (row.GetValue<DateTime?>("fecha_checkin"), row.GetValue<string>("estatus"))
                    : (null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener Check-In: " + ex.Message);
                return (null, null);
            }
            finally
            {
                desconectar();
            }
        }

        public (DateTime? fecha, string estatus) ObtenerCheckOut(Guid codigoReserva)
        {
            conectar();
            try
            {
                var rs = _session.Execute(
                    new SimpleStatement("SELECT fecha_checkout, estatus FROM reservacionCheckOut WHERE codigo_reserva = ?", codigoReserva));
                var row = rs.FirstOrDefault();
                return row != null
                    ? (row.GetValue<DateTime?>("fecha_checkout"), row.GetValue<string>("estatus"))
                    : (null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener Check-Out: " + ex.Message);
                return (null, null);
            }
            finally
            {
                desconectar();
            }
        }

        public List<ReporteOcupacion> ObtenerListaOcupacion()
        {
            var cultura = new CultureInfo("es-MX");
            var lista = new List<ReporteOcupacion>();

            try
            {
                conectar();

                var hoteles = _session.Execute("SELECT id_hotel, nombre_hotel, ciudad FROM hoteles;").ToList();

                var distribucion = _session.Execute("SELECT id_hotel, id_tipo_hab, nombre_tipo, cantidad FROM distribucion_habitaciones;").ToList();

                var reservaciones = _session.Execute("SELECT id_hotel, id_tipohab, fecha_llegada, checkin FROM reservaciones;");

                var grupos = reservaciones
                    .GroupBy(r => new
                    {
                        IdHotel = r.GetValue<Guid>("id_hotel"),
                        IdTipoHab = r.GetValue<Guid>("id_tipohab"),
                        Anio = r.GetValue<DateTime>("fecha_llegada").Year,
                        Mes = cultura.DateTimeFormat.GetMonthName(r.GetValue<DateTime>("fecha_llegada").Month)
                    });

                foreach (var grupo in grupos)
                {
                    int ocupadas = grupo.Count();

                    var dist = distribucion.FirstOrDefault(d =>
                        d.GetValue<Guid>("id_hotel") == grupo.Key.IdHotel &&
                        d.GetValue<Guid>("id_tipo_hab") == grupo.Key.IdTipoHab);

                    var hotel = hoteles.FirstOrDefault(h =>
                        h.GetValue<Guid>("id_hotel") == grupo.Key.IdHotel);

                    if (dist == null || hotel == null)
                        continue;

                    int total = dist.GetValue<int>("cantidad");
                    double porcentaje = total > 0 ? (ocupadas / (double)total) * 100 : 0;

                    lista.Add(new ReporteOcupacion
                    {
                        Ciudad = hotel.GetValue<string>("ciudad"),
                        Hotel = hotel.GetValue<string>("nombre_hotel"),
                        Anio = grupo.Key.Anio,
                        Mes = grupo.Key.Mes,
                        TipoHabitacion = dist.GetValue<string>("nombre_tipo"),
                        HabitacionesOcupadas = ocupadas,
                        TotalHabitaciones = total,
                        PorcentajeOcupacion = Math.Round(porcentaje, 2)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar el reporte de ocupación: " + ex.Message);
            }
            finally
            {
                desconectar();
            }

            return lista;
        }

        public List<ReporteOcupacionHotel> ObtenerListaOcupacionPorHotel()
        {
            var cultura = new CultureInfo("es-MX");
            var lista = new List<ReporteOcupacionHotel>();

            try
            {
                conectar();

                var hoteles = _session.Execute("SELECT id_hotel, nombre_hotel, ciudad FROM hoteles;").ToList();
                var distribucion = _session.Execute("SELECT id_hotel, cantidad FROM distribucion_habitaciones;").ToList();
                var reservaciones = _session.Execute("SELECT id_hotel, fecha_llegada, checkin FROM reservaciones;");

                var grupos = reservaciones
                    .GroupBy(r => new
                    {
                        IdHotel = r.GetValue<Guid>("id_hotel"),
                        Anio = r.GetValue<DateTime>("fecha_llegada").Year,
                        Mes = cultura.DateTimeFormat.GetMonthName(r.GetValue<DateTime>("fecha_llegada").Month)
                    });

                foreach (var grupo in grupos)
                {
                    int ocupadas = grupo.Count();

                    var hotel = hoteles.FirstOrDefault(h => h.GetValue<Guid>("id_hotel") == grupo.Key.IdHotel);
                    if (hotel == null)
                        continue;

                    int total = distribucion
                        .Where(d => d.GetValue<Guid>("id_hotel") == grupo.Key.IdHotel)
                        .Sum(d => d.GetValue<int>("cantidad"));

                    double porcentaje = total > 0 ? (ocupadas / (double)total) * 100 : 0;

                    lista.Add(new ReporteOcupacionHotel
                    {
                        Ciudad = hotel.GetValue<string>("ciudad"),
                        Hotel = hotel.GetValue<string>("nombre_hotel"),
                        Anio = grupo.Key.Anio,
                        Mes = grupo.Key.Mes,
                        HabitacionesOcupadas = ocupadas,
                        HabitacionesTotales = total,
                        PorcentajeOcupacion = Math.Round(porcentaje, 2)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar el resumen de ocupación por hotel: " + ex.Message);
            }
            finally
            {
                desconectar();
            }

            return lista;
        }

        public void actualizarCliente(ClienteDatos1 param)
        {
            try
            {
                conectar();

                string query = "UPDATE cliente SET " +
                               "Nombre = '{0}', PrimerApellido = '{1}', SegundoApellido = '{2}', " +
                               "FechaNacimientoCliente = '{3}', TelefonoCasa = {4}, TelefonoCelular = {5}, " +
                               "EstadoCivil = '{6}', RFC = '{7}', Correo = {8}, FechaModificacion = '{9}', " +
                               "Ciudad = '{10}', Estado = '{11}', Pais = '{12}', CodigoPostal = {13} " +
                               "WHERE Id_Cliente = {14};";

                string telefonosCasa = ConvertMapToCql(param.TelefonoCasa);
                string telefonosCel = ConvertMapToCql(param.TelefonoCelular);
                string correos = ConvertSetToCql(param.Correo);

                string qry = string.Format(query,
                    param.Nombre,
                    param.PrimerApellido,
                    param.SegundoApellido,
                    param.FechaNacimientoCliente.ToString("yyyy-MM-dd"),
                    telefonosCasa,
                    telefonosCel,
                    param.EstadoCivil,
                    param.RFC,
                    correos,
                    param.FechaModificacion.ToString("yyyy-MM-dd"),
                    param.Ciudad,
                    param.Estado,
                    param.Pais,
                    param.CodigoPostal,
                    param.Id_Cliente
                );

                _session.Execute(qry);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar: " + ex.Message);
            }
            finally
            {
                desconectar();
            }
        }

        public void actualizarUsuario(UsuarioDatos1 usuario)
        {
            try
            {
                conectar();

                string query = "UPDATE usuarios SET " +
                               "NombreUsuario = '{0}', PrimerApellido = '{1}', SegundoApellido = '{2}', " +
                               "TelefonoCelular = '{3}', TelefonoCasa = '{4}', " +
                               "CorreoUsuario = '{5}', ContrasenaUsuario = '{6}', " +
                               "FechaModificacionUsuario = '{7}' " +
                               "WHERE NumeroNomina = {8};";

                string qry = string.Format(query,
                    usuario.NombreUsuario,
                    usuario.PrimerApellido,
                    usuario.SegundoApellido,
                    usuario.TelefonoCelular,
                    usuario.TelefonoCasa,
                    usuario.CorreoUsuario,
                    usuario.ContrasenaUsuario,
                    usuario.FechaModificacionUsuario.ToString("yyyy-MM-dd"),
                    usuario.NumeroNomina
                );

                _session.Execute(qry);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar usuario: " + ex.Message);
            }
            finally
            {
                desconectar();
            }
        }

        public void ActualizaCantidadAsignada(Guid idHotel, Guid idTipoHab, int nuevaCantidad)
        {
            try
            {
                conectar();
                string query = "UPDATE distribucion_habitaciones SET cantidad = ? WHERE id_hotel = ? AND id_tipo_hab = ?;";
                var mapper = new Mapper(_session);
                mapper.Execute(query, nuevaCantidad, idHotel, idTipoHab);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar la asignación: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public bool ActualizarFechaCheckIn(Guid idReservacion, DateTime nuevaFechaCheckIn)
        {
            try
            {
                conectar();

                string query = "UPDATE reservaciones SET fecha_checkin = ? WHERE id_reservacion = ?";

                var statement = _session.Prepare(query);
                var bound = statement.Bind(nuevaFechaCheckIn, idReservacion);

                _session.Execute(bound);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error actualizando fecha de check-in: " + ex.Message);
                return false;
            }
            finally
            {
                desconectar();
            }
        }

        public void eliminarCliente(Guid idCliente)
        {
            try
            {
                conectar();

                string query = string.Format("DELETE FROM cliente WHERE Id_Cliente = {0};", idCliente);

                _session.Execute(query);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el cliente: " + ex.Message);
            }
            finally
            {
                desconectar();
            }
        }

        public void eliminarUsuario(int numeroNomina)
        {
            try
            {
                conectar();

                string query = $"DELETE FROM usuarios WHERE NumeroNomina = {numeroNomina};";

                _session.Execute(query);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el usuario: " + ex.Message);
            }
            finally
            {
                desconectar();
            }
        }

        public void EliminarReservacion(Guid idReservacion)
        {
            try
            {
                conectar();

                string query = $"DELETE FROM reservaciones WHERE id_reservacion = {idReservacion};";
                _session.Execute(query);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar la reservación: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                desconectar();
            }
        }
    }
}