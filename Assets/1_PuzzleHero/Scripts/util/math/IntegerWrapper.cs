using System.Collections;
using System;

[Serializable]
public class IntegerWrapper 
{
	private static readonly byte STEP = 8;
	public byte[] bytes = new byte[4];
	public byte rd = (byte)UnityEngine.Random.Range(0, byte.MaxValue);

	public static implicit operator IntegerWrapper(int x) 
	{
		IntegerWrapper ret = new IntegerWrapper();
		ret.SetValue(x);
		return ret;
	}

	public static explicit operator int(IntegerWrapper x) 
	{
		if (x == null) throw new InvalidOperationException();
		return x.GetValue();
	}

	public static bool operator <(IntegerWrapper c1, IntegerWrapper c2)
	{
		return c1.GetValue() < c2.GetValue();
	}

	public static bool operator >(IntegerWrapper c1, IntegerWrapper c2)
	{
		return c1.GetValue() > c2.GetValue();
	}

	public static bool operator >=(IntegerWrapper c1, IntegerWrapper c2)
	{
		return c1.GetValue() >= c2.GetValue();
	}

	public static bool operator <=(IntegerWrapper c1, IntegerWrapper c2)
	{
		return c1.GetValue() <= c2.GetValue();
	}

	public static bool operator ==(IntegerWrapper c1, IntegerWrapper c2)
	{
		return c1.GetValue() == c2.GetValue();
	}

	public static bool operator !=(IntegerWrapper c1, IntegerWrapper c2)
	{
		return c1.GetValue() != c2.GetValue();
	}

	public static IntegerWrapper operator +(IntegerWrapper c1, IntegerWrapper c2)
	{
		IntegerWrapper ret = new IntegerWrapper();
		ret.SetValue(c1.GetValue() + c2.GetValue());
		return ret;
	}

	public static IntegerWrapper operator -(IntegerWrapper c1, IntegerWrapper c2)
	{
		IntegerWrapper ret = new IntegerWrapper();
		ret.SetValue(c1.GetValue() - c2.GetValue());
		return ret;
	}

	public static IntegerWrapper operator --(IntegerWrapper c1)
	{
		IntegerWrapper ret = new IntegerWrapper();
		ret.SetValue(c1.GetValue() - 1);
		return ret;
	}

	public static IntegerWrapper operator ++(IntegerWrapper c1)
	{
		IntegerWrapper ret = new IntegerWrapper();
		ret.SetValue(c1.GetValue() + 1);
		return ret;
	}

	public static implicit operator string(IntegerWrapper x) 
	{
		return x.GetValue().ToString();
	}

	public IntegerWrapper()
	{
		SetValue(0);
	}

	public override string ToString ()
	{
		return GetValue().ToString();
	}
	
	public int GetValue()
	{
		int ret = 0;

		for(int i =0 ; i < bytes.Length; ++i)
		{
			ret |= (bytes[i]^ rd) << ((bytes.Length - i - 1) * STEP) ;
		}
		return ret;
	}

	public void SetValue(int x)
	{
		for(int i =0 ; i < bytes.Length; ++i)
		{
			bytes[i] =(byte) ((byte)( x >> ((bytes.Length - i - 1) * STEP) & 0xFF) ^ rd);
		}

	}
}
