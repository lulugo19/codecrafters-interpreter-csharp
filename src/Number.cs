using System.Globalization;

public class Number
{
    public decimal Value {private get; init;}

    public Number(decimal value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString("0.0###########################", CultureInfo.InvariantCulture.NumberFormat);
    }
}