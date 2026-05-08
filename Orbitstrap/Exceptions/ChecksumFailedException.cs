using System;

namespace Orbitstrap.Exceptions;

internal class ChecksumFailedException : Exception
{
	public ChecksumFailedException(string message)
		: base(message)
	{
	}
}
