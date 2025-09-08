using CalculatorLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculatorTests
{
    [TestClass]
    public class CalculatorTests
    {
        // Create calculator instance for all tests
        private Calculator _calculator = new Calculator();

        [TestMethod]
        public void Add_TwoNumbers_ReturnsSum()
        {
            // Test 1: 5 + 3 = 8
            double result = _calculator.Add(5, 3);
            Assert.AreEqual(8, result);

            // Test 2: -1 + 1 = 0
            result = _calculator.Add(-1, 1);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void Subtract_TwoNumbers_ReturnsDifference()
        {
            // Test 1: 10 - 4 = 6
            double result = _calculator.Subtract(10, 4);
            Assert.AreEqual(6, result);

            // Test 2: 5 - 10 = -5
            result = _calculator.Subtract(5, 10);
            Assert.AreEqual(-5, result);
        }

        [TestMethod]
        public void Multiply_TwoNumbers_ReturnsProduct()
        {
            // Test 1: 3 * 4 = 12
            double result = _calculator.Multiply(3, 4);
            Assert.AreEqual(12, result);

            // Test 2: 5 * 0 = 0
            result = _calculator.Multiply(5, 0);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void Divide_TwoNumbers_ReturnsQuotient()
        {
            // Test 1: 10 / 2 = 5
            double result = _calculator.Divide(10, 2);
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void Divide_ByZero_ThrowsException()
        {
            // Test division by zero
            Assert.ThrowsException<DivideByZeroException>(() =>
            {
                _calculator.Divide(5, 0);
            });
        }
    }
}