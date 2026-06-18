// (c) Carlos Pineda Guerrero. 2026

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;

namespace servicio;
    public class borra_usuario
    {
        class Respuesta
        {
            public string mensaje;
            public Respuesta(string mensaje)
            {
                this.mensaje = mensaje;
            }
        }
        [Function("borra_usuario")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete")] HttpRequest req)
        {
            try
            {
                int? id_usuario = int.Parse(req.Query["id_usuario"]);
                string? token = req.Query["token"];
                string? Server = Environment.GetEnvironmentVariable("Server");
                string? UserID = Environment.GetEnvironmentVariable("UserID");
                string? Password = Environment.GetEnvironmentVariable("Password");
                string? Database = Environment.GetEnvironmentVariable("Database");
                string cs = "Server=" + Server + ";UserID=" + UserID + ";Password=" + Password + ";" + "Database=" + Database + ";SslMode=Preferred;";
                var conexion = new MySqlConnection(cs);
                conexion.Open();
                MySqlTransaction transaccion = conexion.BeginTransaction();
                try
                {
                    if (!login.verifica_acceso(conexion,id_usuario,token)) throw new Exception("Acceso denegado");
                    var cmd_1 = new MySqlCommand();
                    cmd_1.Connection = conexion;
                    cmd_1.Transaction = transaccion;
                    cmd_1.CommandText = "DELETE FROM fotos_usuarios WHERE id_usuario=@id_usuario";
                    cmd_1.Parameters.AddWithValue("@id_usuario", id_usuario);
                    cmd_1.ExecuteNonQuery();
                    var cmd_2 = new MySqlCommand();
                    cmd_2.Connection = conexion;
                    cmd_2.Transaction = transaccion;
                    cmd_2.CommandText = "DELETE FROM usuarios WHERE id_usuario=@id_usuario";
                    cmd_2.Parameters.AddWithValue("@id_usuario", id_usuario);
                    cmd_2.ExecuteNonQuery();
                    transaccion.Commit();
                    return new OkObjectResult(JsonConvert.SerializeObject(new Respuesta("Se borró el usuario")));
                }
                catch (Exception e)
                {
                    transaccion.Rollback();
                    throw new Exception(e.Message);
                }
                finally
                {
                    conexion.Close();
                }
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(JsonConvert.SerializeObject(new Respuesta(e.Message)));
            }
        }
    }
