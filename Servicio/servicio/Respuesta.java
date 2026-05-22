/*
  Respuesta.java
  Permite regresar al cliente REST un mensaje
  Carlos Pineda Guerrero, septiembre 2024
*/

package servicio;

public class Respuesta
{
	public String mensaje;

	Respuesta(String mensaje)
	{
		this.mensaje = mensaje;
	}
}
