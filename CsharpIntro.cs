using System.Reflection;

namespace CsharpIntro
{
    public class App
    {
        public static void Main(String[] args)
        {
            if (args.Length == 1 && args[0] == "test")
            {
                TestHarness.RunAll("Tests");
            }
            else if (args.Length == 4 && args[0] == "fizzbuzz")
            {
                FizzBuzz(
                    new ConsoleWriter(),
                    int.Parse(args[1]),
                    int.Parse(args[2]),
                    int.Parse(args[3])
                );
            }
            else if (args.Length == 2 && args[0] == "factor")
            {
                PrimeFactorization(
                    new ConsoleWriter(),
                    int.Parse(args[1])
                );
            }
            else
            {
                Console.WriteLine("usage: COMMAND [arg [arg [...]]]");
                Console.WriteLine("");
                Console.WriteLine("available COMMANDs:");
                Console.WriteLine("       test");
                Console.WriteLine("       fizzbuzz int X int Y int N");
                Console.WriteLine("       factor int X");
            }
        }

        public static void FizzBuzz(IWriter writer, int x, int y, int n)
        {
            for (int i = 1; i <= n; ++i)
            {
                bool isFizz = i % x == 0;
                bool isBuzz = i % y == 0;

                if (isFizz || isBuzz)
                {
                    if (isFizz)
                    {
                        writer.Write("Fizz");
                    }

                    if (isBuzz)
                    {
                        writer.Write("Buzz");
                    }
                }
                else
                {
                    writer.Write(i.ToString());
                }

                writer.Write("\n");
            }
        }

        public static void PrimeFactorization(IWriter writer, int x)
        {
            if (x < 0)
            {
                PrimeFactorization(writer, -x);
                return;
            }

            int maxRelevantFactor = x / 2;
            bool[] excludedFactors = new bool[maxRelevantFactor + 1];
            List<String> output = [];

            for (int factor = 2; factor <= maxRelevantFactor; ++factor)
            {
                if (true == excludedFactors[factor])
                {
                    continue;
                }

                if (x % factor == 0)
                {
                    int multiplicity = 0;
                    int rest = x;

                    while (rest >= factor && rest % factor == 0)
                    {
                        multiplicity += 1;
                        rest /= factor;
                    }

                    output.Add(String.Format("{0}^{1}", factor, multiplicity));

                    int toExclude = factor;

                    while (toExclude <= maxRelevantFactor)
                    {
                        excludedFactors[toExclude] = true;
                        toExclude += factor;
                    }
                }
            }

            if (output.Count == 0)
            {
                output.Add(String.Format("{0}^1", x));
            }

            writer.Write(String.Format("[{0}]\n", String.Join(", ", output)));
        }
    }

    class Tests
    {
        public static void TestFizzBuzz()
        {
            var writer = new TestWriter();
            App.FizzBuzz(writer, 2, 3, 7);

            TestHarness.AssertEqual(
                String.Join("", writer.GetWrittenLines()),
                "1\nFizz\nBuzz\nFizz\n5\nFizzBuzz\n7\n"
            );

            writer = new TestWriter();
            App.FizzBuzz(writer, 2, 4, 8);

            TestHarness.AssertEqual(
                String.Join("", writer.GetWrittenLines()),
                "1\nFizz\n3\nFizzBuzz\n5\nFizz\n7\nFizzBuzz\n"
            );
        }

        public static void TestPrimeFactorization()
        {
            static void test(int x, String expect)
            {
                var writer = new TestWriter();
                App.PrimeFactorization(writer, x);
                TestHarness.AssertEqual(
                    String.Join("", writer.GetWrittenLines()),
                    expect
                );
            }

            test(0, "[0^1]\n");
            test(1, "[1^1]\n");
            test(2, "[2^1]\n");
            test(3, "[3^1]\n");
            test(4, "[2^2]\n");
            test(2 * 3 * 5 * 7 * 11 * 13, "[2^1, 3^1, 5^1, 7^1, 11^1, 13^1]\n");
            test(2 * 2 * 3 * 3 * 3 * 5 * 5 * 5 * 5 * 5, "[2^2, 3^3, 5^5]\n");
            test(1234, "[2^1, 617^1]\n");
        }
    }

    public interface IWriter
    {
        public void Write(String s);
    }

    class ConsoleWriter : IWriter
    {
        public ConsoleWriter() { }
        public void Write(String s)
        {
            Console.Write(s);
        }
    }

    class TestWriter : IWriter
    {
        private List<String> lines = new List<String>();
        public TestWriter() { }

        public void Write(String s)
        {
            lines.Add(s);
        }

        public List<String> GetWrittenLines()
        {
            return lines;
        }
    }

    class TestHarness
    {
        public static void RunAll(String suite)
        {
            Type clazz = Type.GetType(String.Format("CsharpIntro.{0}", suite))
                ?? throw new Exception("Reflection lookup error - test suite not found");

            MethodInfo[] allInfo = clazz.GetMethods();

            int num_tests = 0;
            int num_passed = 0;
            int num_failed = 0;

            List<String> failures = new List<String>();

            foreach (MethodInfo info in allInfo)
            {
                if (info.Name.StartsWith("Test"))
                {
                    num_tests += 1;

                    var error = Run(suite, info.Name);

                    if (error == null)
                    {
                        num_passed += 1;
                    }
                    else
                    {
                        num_failed += 1;

                        failures.Add(String.Format("{0}: {1}", info.Name, error));
                    }
                }
            }

            Console.WriteLine(
                String.Format(
                    "Ran {0} test(s), {1} passed, {2} failed.",
                    num_tests,
                    num_passed,
                    num_failed
                )
            );

            if (failures.Count > 0)
            {
                Console.WriteLine("");
                Console.WriteLine("Failures:");
                foreach (String failure in failures)
                {
                    Console.WriteLine(failure);
                }
            }
        }

        public static Exception? Run(String suite, String name)
        {
            Type clazz = Type.GetType(String.Format("CsharpIntro.{0}", suite))
                ?? throw new Exception("Reflection lookup error - test suite not found");

            MethodInfo test = clazz.GetMethod(name)
                ?? throw new Exception("Reflection lookup error - test method not found");

            try
            {
                Console.Write(name);
                Console.Write(": ");

                test.Invoke(null, null);

                Console.WriteLine("OK");
            }
            catch (Exception e)
            {
                Console.WriteLine("FAIL!");
                return e;
            }

            return null;
        }

        public static void AssertEqual<T>(T left, T right)
        {
            if (!EqualityComparer<T>.Default.Equals(left, right))
            {
                throw new Exception("Assertion failed");
            }
        }
    }
}
