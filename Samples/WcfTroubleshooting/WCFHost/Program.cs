
namespace WCFHost
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    [ServiceContract(Namespace = "http://WCFHost/")]
    public interface ICalculator
    {
        [OperationContract]
        double Add(double number1, double number2);
        
        [OperationContract]
        double Subtract(double number1, double number2);

        [OperationContract]
        double Multiply(double number1, double number2);
        
        [OperationContract]
        float Divide(int number1, int number2);

        [OperationContract]
        double PowerOf(double number1, double number2);
    }

    class Calculator : ICalculator
    {
        public double Add(double number1, double number2)
        {
            var result = number1 + number2;
            Console.WriteLine("Add({0},{1})", number1, number2);
            Console.WriteLine("Result: {0}", result);
            return result;
        }

        public double Subtract(double number1, double number2)
        {
            var result = number1 - number2;
            Console.WriteLine("Subtract({0},{1})", number1, number2);
            Console.WriteLine("Result: {0}", result);
            return result;
        }

        public double Multiply(double number1, double number2)
        {
            throw new NotImplementedException("Multiplication operation not implemented!");
        }

        public float Divide(int number1, int number2)
        {
            var result = number1 / number2;
            Console.WriteLine("Divide({0},{1})", number1, number2);
            Console.WriteLine("Answer: {0}", result);
            return result;
        }

        public double PowerOf(double number1, double number2)
        {
            var result = Math.Pow(number1, number2);
            Console.WriteLine("PowerOf({0},{1})", number1, number2);
            Console.WriteLine("Answer: {0}", result);
            return result;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var baseAddress = new Uri("http://localhost:8080/Calculator");

            using (var host = new ServiceHost(typeof(Calculator), baseAddress))
            {
                host.AddServiceEndpoint(typeof(ICalculator), new BasicHttpBinding(), "Calculator");
                var serviceMetadataBehavior = new ServiceMetadataBehavior
                                                    {
                                                        HttpGetEnabled = true,
                                                        MetadataExporter =
                                                            {
                                                                PolicyVersion =
                                                                    PolicyVersion
                                                                    .Policy15
                                                            }
                                                    };
                host.Description.Behaviors.Add(serviceMetadataBehavior);

                host.Open();

                Console.WriteLine("The host is ready at: {0}", baseAddress);
                Console.WriteLine("Press <Enter> to stop the host");
                Console.ReadKey();

                host.Close();
            }
        }
    }
}
