// Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.

namespace Crazor.Validation
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class SkipRecursiveValidation : Attribute
	{
	}
}
