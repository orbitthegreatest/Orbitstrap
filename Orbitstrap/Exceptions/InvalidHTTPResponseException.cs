using System;

namespace Orbitstrap.Exceptions;

internal class InvalidHTTPResponseException : Exception
{
	public InvalidHTTPResponseException(string message)
		: base(message)
	{
	}
}
