
namespace WCFClient
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            var calculatorClient = new CalculatorClient();

            var operation = args[0];

            var number1 = int.Parse(args[1]);
            var number2 = int.Parse(args[2]);

            if (string.Equals(operation, "-Add", StringComparison.OrdinalIgnoreCase))
            {
                var resultAdd = calculatorClient.Add(number1, number2);
                Console.WriteLine("resultAdd: {0}", resultAdd);
            }
            else if (string.Equals(operation, "-Subtract", StringComparison.OrdinalIgnoreCase))
            {
                var resultSubtract = calculatorClient.Subtract(number1, number2);
                Console.WriteLine("resultSubtract: {0}", resultSubtract);
            }
            else if (string.Equals(operation, "-Multiply", StringComparison.OrdinalIgnoreCase))
            {
                var resultMultiply = calculatorClient.Multiply(number1, number2);
                Console.WriteLine("resultMultiply: {0}", resultMultiply);
            }
            else if (string.Equals(operation, "-Divide", StringComparison.OrdinalIgnoreCase))
            {
                var resultDivide = calculatorClient.Divide(number1, number2);
                Console.WriteLine("resultDivide: {0}", resultDivide);
            }
            else if (string.Equals(operation, "-PowerOf", StringComparison.OrdinalIgnoreCase))
            {
                var resultPowerOf = calculatorClient.PowerOf(number1, number2);
                Console.WriteLine("resultPowerOf: {0}", resultPowerOf);
            }
            else
            {
                Console.WriteLine("Invalid operation entered");
                throw new ArgumentException("Invalid operation entered");
            }

            calculatorClient.Close();

            // Console.WriteLine("Press <ENTER> to stop client");
            // Console.ReadLine();
        }
    }
}
