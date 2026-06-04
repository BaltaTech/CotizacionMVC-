using Xunit;
using FluentAssertions;
using CotizacionMVC.Models.Valor;

namespace CotizacionMVC.Tests.Pruebas_Unitarias.ValueObjects
{
    public class DineroTests
    {
        [Fact]
        public void Constructor_ConCantidadNegativa_LanzaExcepcion()
        {
            Assert.Throws<ArgumentException>(() => new Dinero(-100m, "MXN"));
        }

        [Fact]

        public void Constructor_ConMonedaInvalida_LanzaExcepcion()
        {
            Assert.Throws<ArgumentException>(() => new Dinero(100m, "EUR"));
        }

        [Fact]
        public void Sumar_DosCantidadesConMismaMoneda_RetornaSumaCorrecta()
        {
            var dinero1 = new Dinero(100m, "MXN");
            var dinero2 = new Dinero(50m, "MXN");
            var resultado = dinero1.Sumar(dinero2);
            resultado.Monto.Should().Be(150m);
            resultado.Moneda.Should().Be("MXN");
        }

        [Fact]

        public void Sumar_DiferentesMonedas_LanzaExcepcion()
        {
            var dinero1 = new Dinero(100m, "MXN");
            var dinero2 = new Dinero(50m, "USD");
            Assert.Throws<InvalidOperationException>(() => dinero1.Sumar(dinero2));
        }

        [Fact]

        public void Constructor_ConCantidadValida_CrearInstanciaCorrectamente()
        {
            var dinero = new Dinero(100.50m, "MXN");

            //Assert

            dinero.Monto.Should().Be(100.50m);
            dinero.Moneda.Should().Be("MXN");
        }

        [Fact]
        public void Constructor_ConCero_DeberiaEjecutarse()
        {
            var dinero = new Dinero(0, "MXN");

            //Assert

            dinero.Monto.Should().Be(0);
            dinero.Moneda.Should().Be("MXN");
        }

        [Fact]
        public void Constructor_RedondeaDecimales()
        {
            var dinero = new Dinero(100.5678m, "MXN");
            //Assert
            dinero.Monto.Should().Be(100.57m);
            dinero.Moneda.Should().Be("MXN");
        }

        [Fact]
        public void Convertir_MismaMoneda_RetornaMismoMonto()
        {
            // Act 
            var dinero = new Dinero(100m, "MXN");
            var resultado = dinero.ConvertirA("MXN", 20.50m);

            // Asset 
            resultado.Monto.Should().Be(100);
            resultado.Moneda.Should().Be("MXN");
        }

        [Fact]
        
        public void Convertir_MXN_A_USD_RetornaMontoConvertido()
        {
            // Act 
            var dinero = new Dinero(1000, "MXN");
            var tipoCambio = 20.50m;

            var resultado = dinero.ConvertirA("USD", tipoCambio);

            //Assert

            resultado.Monto.Should().BeApproximately(48.78m, 0.01m);
            resultado.Moneda.Should().Be("USD");
        }

        [Fact]
        public void ConvertirA_DeUSD_A_MXN_RetornaConversionCorrecta()
        {
            // Arrange
            var dinero = new Dinero(100, "USD");
            var tipoCambio = 20.50m;

            // Act
            var resultado = dinero.ConvertirA("MXN", tipoCambio);

            // Assert
            resultado.Monto.Should().Be(2050);
            resultado.Moneda.Should().Be("MXN");
        }

        [Fact]
        public void ConvertirA_ConTipoCambioCero_LanzaExcepcion()
        {
            // Arrange
            var dinero = new Dinero(100, "USD");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => dinero.ConvertirA("MXN", 0));
        }

        [Fact]
        public void Restar_CantidadValida_RetornaDiferencia()
        {
            // Arrange
            var dinero1 = new Dinero(500, "MXN");
            var dinero2 = new Dinero(200, "MXN");

            // Act
            var resultado = dinero1.Restar(dinero2);

            // Assert
            resultado.Monto.Should().Be(300);
            resultado.Moneda.Should().Be("MXN");
        }

        [Fact]
        public void Restar_ResultadoNegativo_LanzaExcepcion()
        {
            // Arrange
            var dinero1 = new Dinero(100, "MXN");
            var dinero2 = new Dinero(200, "MXN");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => dinero1.Restar(dinero2));
        }

        [Fact]
        public void Multiplicar_CantidadPositiva_RetornaProducto()
        {
            // Arrange
            var dinero = new Dinero(100.50m, "MXN");

            // Act
            var resultado = dinero.Multiplicar(3);

            // Assert
            resultado.Monto.Should().Be(301.50m);
            resultado.Moneda.Should().Be("MXN");
        }

        [Fact]
        public void Multiplicar_PorCero_RetornaCero()
        {
            // Arrange
            var dinero = new Dinero(100, "MXN");

            // Act
            var resultado = dinero.Multiplicar(0);

            // Assert
            resultado.Monto.Should().Be(0);
        }

        [Fact]
        public void Equals_MismoMontoYMoneda_RetornaTrue()
        {
            // Arrange
            var dinero1 = new Dinero(100.50m, "MXN");
            var dinero2 = new Dinero(100.50m, "MXN");

            // Assert
            dinero1.Should().Be(dinero2);
        }

        [Fact]
        public void Equals_DiferenteMonto_RetornaFalse()
        {
            // Arrange
            var dinero1 = new Dinero(100, "MXN");
            var dinero2 = new Dinero(200, "MXN");

            // Assert
            dinero1.Should().NotBe(dinero2);
        }

        [Fact]
        public void OperatorPlus_SumaCorrecta()
        {
            // Arrange
            var dinero1 = new Dinero(100, "MXN");
            var dinero2 = new Dinero(50, "MXN");

            // Act
            var resultado = dinero1 + dinero2;

            // Assert
            resultado.Monto.Should().Be(150);
        }

        [Fact]
        public void ToString_FormatoCorrecto()
        {
            // Arrange
            var dinero = new Dinero(1234.56m, "MXN");

            // Act
            var resultado = dinero.ToString();

            // Assert
            resultado.Should().Be("MXN 1,234.56");
        }


    }
}

