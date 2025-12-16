using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using CasoEstudio2.Models;
using System.Data;

namespace CasoEstudio2.Controllers
{
    public class CasasController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public CasasController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("CasoEstudioConnection");
        }

        // GET: Casas/ConsultaCasas
        public IActionResult ConsultaCasas()
        {
            List<CasasModel> casas = new List<CasasModel>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand command = new SqlCommand("SP_ConsultarCasas", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                casas.Add(new CasasModel
                                {
                                    IdCasa = reader.GetInt64(0),
                                    DescripcionCasa = reader.GetString(1),
                                    PrecioCasa = reader.GetDecimal(2),
                                    UsuarioAlquiler = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    FechaAlquiler = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log del error (en producción usar un logger apropiado)
                ViewBag.Error = "Error al consultar las casas: " + ex.Message;
            }

            return View(casas);
        }

        // GET: Casas/AlquilerCasas
        public IActionResult AlquilerCasas()
        {
            try
            {
                // Obtener lista de casas disponibles para el dropdown
                List<CasasModel> casasDisponibles = ObtenerCasasDisponibles();

                // Crear lista de SelectListItem para el dropdown
                ViewBag.CasasDisponibles = casasDisponibles.Select(c => new SelectListItem
                {
                    Value = c.IdCasa.ToString(),
                    Text = c.DescripcionCasa,
                    // Agregar el precio como data attribute
                }).ToList();

                // También pasar la lista completa para poder obtener los precios con JavaScript
                ViewBag.CasasData = casasDisponibles.Select(c => new
                {
                    id = c.IdCasa,
                    precio = c.PrecioCasa
                }).ToList();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar las casas disponibles: " + ex.Message;
            }

            return View(new CasasModel());
        }

        // POST: Casas/AlquilarCasa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AlquilarCasa(long IdCasa, string UsuarioAlquiler)
        {
            // Validar datos recibidos
            if (IdCasa <= 0 || string.IsNullOrWhiteSpace(UsuarioAlquiler))
            {
                TempData["ErrorMessage"] = "Debe seleccionar una casa e ingresar su nombre.";
                return RedirectToAction(nameof(AlquilerCasas));
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand command = new SqlCommand("SP_AlquilarCasa", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Agregar parámetros
                        command.Parameters.AddWithValue("@IdCasa", IdCasa);
                        command.Parameters.AddWithValue("@UsuarioAlquiler", UsuarioAlquiler.Trim());
                        command.Parameters.AddWithValue("@FechaAlquiler", DateTime.Now);

                        connection.Open();

                        // Ejecutar el procedimiento
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int filasActualizadas = reader.GetInt32(0);

                                if (filasActualizadas > 0)
                                {
                                    TempData["SuccessMessage"] = "¡Casa alquilada exitosamente!";
                                }
                                else
                                {
                                    TempData["ErrorMessage"] = "La casa seleccionada ya no está disponible.";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al alquilar la casa: " + ex.Message;
                return RedirectToAction(nameof(AlquilerCasas));
            }

            // Redireccionar a la vista de consulta
            return RedirectToAction(nameof(ConsultaCasas));
        }

        // Método auxiliar para obtener casas disponibles
        private List<CasasModel> ObtenerCasasDisponibles()
        {
            List<CasasModel> casas = new List<CasasModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SP_ObtenerCasasDisponibles", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            casas.Add(new CasasModel
                            {
                                IdCasa = reader.GetInt64(0),
                                DescripcionCasa = reader.GetString(1),
                                PrecioCasa = reader.GetDecimal(2)
                            });
                        }
                    }
                }
            }

            return casas;
        }
    }
}