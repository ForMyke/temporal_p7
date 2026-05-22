rem Definir y descomentar la siguiente variable de entorno:
rem set CATALINA_HOME=<ruta-absoluta-del-directorio-de-Tomcat>
javac -cp "WEB-INF/lib/*;." servicio/Servicio.java
del /Q WEB-INF\classes\servicio\*
copy servicio\*.class WEB-INF\classes\servicio\.
jar cvf Servicio.war WEB-INF META-INF
del %CATALINA_HOME%\webapps\Servicio.war
rmdir /S /Q %CATALINA_HOME%\webapps\Servicio
copy Servicio.war %CATALINA_HOME%\webapps\.
