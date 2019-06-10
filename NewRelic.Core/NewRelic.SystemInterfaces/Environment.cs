﻿using System;

namespace NewRelic.SystemInterfaces
{
	public class Environment : IEnvironment
	{
		public string GetEnvironmentVariable(string variable)
		{
			return System.Environment.GetEnvironmentVariable(variable);
		}

		public string GetEnvironmentVariable(string variable, EnvironmentVariableTarget environmentVariableTarget)
		{
			return System.Environment.GetEnvironmentVariable(variable, environmentVariableTarget);
		}
	}
}