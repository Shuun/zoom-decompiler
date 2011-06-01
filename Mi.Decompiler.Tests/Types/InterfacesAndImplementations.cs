using System;

namespace InterfacesAndImplementations
{
	public interface IA
	{
	}
	
    public interface IA2 : IA
	{
	}
	
    public interface IB
	{
	}

	public class C : IA2, IB
	{
	}
}
