using System.Globalization;

namespace AST;

public abstract class Literal
{
    public class Boolean : Literal
    {
        public bool Value {get; init;}

        public Boolean(bool value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class Nil : Literal
    {
        public override string ToString()
        {
            return "nil";
        }

        private static Nil _instance = new Nil();

        public static Nil Instance => _instance;
    }

    public class Number : Literal
    {
        public decimal Value {get; init;}

        public Number(decimal value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString("0.0###########################", CultureInfo.InvariantCulture.NumberFormat);
        }
    }

    public class String : Literal
    {
        public string Value {get; init;}

        public String(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"\"{Value}\"";
        }
    }
}