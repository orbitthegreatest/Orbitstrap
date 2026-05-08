using System;
using System.Net;

namespace Orbitstrap.Exceptions;

public class InvalidChannelException : Exception
{
	public HttpStatusCode? StatusCode;

	public InvalidChannelException(HttpStatusCode? statusCode)
	{
		StatusCode = statusCode;
	}
}
