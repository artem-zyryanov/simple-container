using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using SimpleContainer.Configuration;
using SimpleContainer.Helpers;
using SimpleContainer.Interface;

namespace SimpleContainer.Tests.Helpers
{
	public abstract class SimpleContainerTestBase : UnitTestBase
	{
		protected List<IDisposable> disposables;

		protected override void SetUp()
		{
			base.SetUp();
			disposables = new List<IDisposable>();
			LogBuilder = new StringBuilder();
		}

		public static StringBuilder LogBuilder { get; private set; }

		protected override void TearDown()
		{
			if (disposables != null)
				foreach (var disposable in disposables)
					disposable.Dispose();
			base.TearDown();
		}

		protected ContainerFactory Factory()
		{
			var targetTypes = GetType().GetNestedTypesRecursive(BindingFlags.NonPublic | BindingFlags.Public);
			var factory = new ContainerFactory()
				.WithAssembliesFilter(x => x.Name.StartsWith("SimpleContainer"))
				.WithTypes(GetType().GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public));
		}

		protected IContainer Container(Action<ContainerConfigurationBuilder> configure = null)
		{
			var result = Factory()
				.WithConfigurator(configure)
				.Build();
			disposables.Add(result);
			return result;
		}
	}
}