using CotizacionMVC.Models.Valor;
using Xunit;
using FluentAssertions;
namespace CotizacionMVC.Tests.Pruebas_Unitarias.ValueObjects
{
    public class ContactoTests
    {
        [Fact]
        public void Constructor_ConSoloTelefono_CrearInstanciaCorrectamente()
        {
            // Act 
            var contacto = new Contacto("1234567890", null, null, null);

            // Assert
            contacto.Telefono.Should().Be("1234567890");
            contacto.TelefonoMovil.Should().BeNull();
        }

        public void Constructor_ConSoloCorreo_CrearInstanciaCorrectamente()
        {
            // Act 
            var contacto = new Contacto(null, null, "correo@ejemplo.com", null);

            // Assert
            contacto.Correo.Should().Be("cliente@gmail.com");
            contacto.TieneMedioDeContacto().Should().BeTrue();

        }

        [Fact]
        public void Constructor_SinNingunMedio_DeContacto_LanzarExcepcion()
        {
            // Act
            Assert.Throws<ArgumentException>(() =>
            new Contacto(null, null, "emailInvalido", null));

        }


        [Fact]
        public void Constructor_EmailInvalido_LanzarExcepcion()
        {
            // Act
            Assert.Throws<ArgumentException>(() =>
            new Contacto(null, null, "emailInvalido", null));
        }

        [Fact]

        public void Constructor_TelefonoCorto_LanzarExcepcion()
        {
            // Act
            Assert.Throws<ArgumentException>(() =>
            new Contacto("12345", null, null, null));
        }

        [Fact]
        public void Constructor_TelefonoMuyCorto_LanzaExcepcion()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new Contacto("123", null, null, null));
        }

        [Fact]
        public void ObtenerMedioPrincipal_CuandoHayTelefono_RetornaTelefono()
        {
            // Arrange
            var contacto = new Contacto("5512345678", "5523456789", "cliente@ejemplo.com", null);

            // Act
            var medio = contacto.ObtenerMedioPrincipal();

            // Assert
            medio.Should().Be("5512345678");
        }

        [Fact]
        public void ObtenerMedioPrincipal_CuandoNoHayTelefono_RetornaMovil()
        {
            // Arrange
            var contacto = new Contacto(null, "5523456789", "cliente@ejemplo.com", null);

            // Act
            var medio = contacto.ObtenerMedioPrincipal();

            // Assert
            medio.Should().Be("5523456789");
        }

        [Fact]
        public void ObtenerMedioPrincipal_CuandoSoloEmail_RetornaEmail()
        {
            // Arrange
            var contacto = new Contacto(null, null, "cliente@ejemplo.com", null);

            // Act
            var medio = contacto.ObtenerMedioPrincipal();

            // Assert
            medio.Should().Be("cliente@ejemplo.com");
        }

        [Fact]
        public void ConNombreContacto_AsignaNombreCorrectamente()
        {
            // Arrange
            var contacto = new Contacto("5512345678", null, null, null);

            // Act
            var contactoConNombre = contacto.ConNombreContacto("Juan Pérez");

            // Assert
            contactoConNombre.NombreContacto.Should().Be("Juan Pérez");
            contactoConNombre.Telefono.Should().Be("5512345678");
        }

        [Fact]
        public void ToString_CuandoHayNombreContacto_RetornaNombre()
        {
            // Arrange
            var contacto = new Contacto("5512345678", null, null, "Juan Pérez");

            // Act
            var resultado = contacto.ToString();

            // Assert
            resultado.Should().Be("Juan Pérez");
        }

        [Fact]
        public void ToString_CuandoNoHayNombre_RetornaEmail()
        {
            // Arrange
            var contacto = new Contacto(null, null, "cliente@ejemplo.com", null);

            // Act
            var resultado = contacto.ToString();

            // Assert
            resultado.Should().Be("cliente@ejemplo.com");
        }
    }
}