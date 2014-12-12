using SimpleContainer.Infection;

namespace SimpleContainer.Configuration
{
	public class ServiceConfigurationBuilder<T> :
		AbstractServiceConfigurationBuilder<ServiceConfigurationBuilder<T>, ContainerConfigurationBuilder, T>
	{
		public ServiceConfigurationBuilder(ContainerConfigurationBuilder builder)
			: base(builder)
		{
		}

		public ServiceContractConfigurationBuilder<T> Contract<TContract>() where TContract : RequireContractAttribute, new()
		{
			return new ServiceContractConfigurationBuilder<T>(builder.Contract<TContract>());
		}

		public ServiceContractConfigurationBuilder<T> Contract(string contractName)
		{
			return new ServiceContractConfigurationBuilder<T>(builder.Contract(contractName));
		}

		public ServiceConfigurationBuilder<T> MakeStatic()
		{
			builder.MakeStatic(typeof (T));
			return this;
		}
	}
}