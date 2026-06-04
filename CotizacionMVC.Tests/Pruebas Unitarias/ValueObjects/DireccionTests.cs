using CotizacionMVC.Models.Valor;
using Xunit;
using FluentAssertions;

namespace CotizacionMVC.Tests.Pruebas_Unitarias.ValueObjects
{
    public class DireccionTests
    {
        [Fact]
        public void Constructor_ConDatosValidos_CreaInstanciaCorrectamente()
        {
            // Act
            var direccion = new Direccion(
                "Insurgentes", "123", "Centro", "CDMX", "CDMX", "06100");

            // Assert
            direccion.Calle.Should().Be("Insurgentes");
            direccion.Colonia.Should().Be("Centro");
            direccion.Ciudad.Should().Be("CDMX");
            direccion.CodigoPostal.Should().Be("06100");
        }

        [Fact]
        public void Constructor_SinCalle_LanzaExcepcion()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new Direccion("", "123", "Centro", "CDMX", "CDMX", "06100"));
        }

        [Fact]
        public void Constructor_CodigoPostalInvalido_LanzaExcepcion()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new Direccion("Insurgentes", "123", "Centro", "CDMX", "CDMX", "123"));
        }

        [Fact]
        public void DireccionCompleta_ConNumeroExterior_FormatoCorrecto()
        {
            // Arrange
            var direccion = new Direccion(
                "Insurgentes", "123", "Centro", "CDMX", "CDMX", "06100");

            // Act
            var resultado = direccion.DireccionCompleta();

            // Assert
            resultado.Should().Be("Insurgentes 123, Centro, CDMX, CDMX, CP 06100");
        }

        [Fact]
        public void DireccionCompleta_SinNumeroExterior_ColocaSIN()
        {
            // Arrange
            var direccion = new Direccion(
                "Insurgentes", null, "Centro", "CDMX", "CDMX", "06100");

            // Act
            var resultado = direccion.DireccionCompleta();

            // Assert
            resultado.Should().Be("Insurgentes S/N, Centro, CDMX, CDMX, CP 06100");
        }

        [Fact]
        public void DireccionCompleta_ConNumeroInterior_FormatoCorrecto()
        {
            // Arrange
            var direccion = new Direccion(
                "Insurgentes", "123", "Centro", "CDMX", "CDMX", "06100", "2A");

            // Act
            var resultado = direccion.DireccionCompleta();

            // Assert
            resultado.Should().Be("Insurgentes 123 Int. 2A, Centro, CDMX, CDMX, CP 06100");
        }

        [Fact]
        public void EsCompleta_CuandoTodosCamposPresentes_RetornaTrue()
        {
            // Arrange
            var direccion = new Direccion(
                "Insurgentes", "123", "Centro", "CDMX", "CDMX", "06100");

            // Act
            var resultado = direccion.EsCompleta();

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public void EsCompleta_CuandoFaltaCalle_RetornaFalse()
        {
            // Esta prueba no se puede crear porque el constructor valida
            // Mejor probar con un objeto que tenga calle vacía (no es posible)
        }
    }
}
