using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SimpleContainer.Helpers;

namespace SimpleContainer.Implementation
{
	//todo ��������� ��� �������
	//todo ��������� ���������� ���������� ��� ������ ���������� + usedContracts
	internal class ContainerService
	{
		private static readonly TimeSpan waitTimeout = TimeSpan.FromSeconds(5);

		private List<int> usedContractIndexes;
		private readonly List<object> instances = new List<object>();
		private IEnumerable<object> typedArray;
		private readonly object lockObject = new object();
		private bool instantiated;

		public int TopSortIndex { get; private set; }
		public IObjectAccessor arguments;
		public bool createNew;
		public Type type;
		public ResolutionContext context;
		public string[] FinalUsedContracts { get; private set; }
		public bool Failed { get; private set; }

		public IEnumerable<object> AsEnumerable()
		{
			return typedArray ?? (typedArray = instances.CastToObjectArrayOf(type));
		}

		public void AddInstance(object instance)
		{
			instances.Add(instance);
		}

		public IReadOnlyList<object> Instances
		{
			get { return instances; }
		}

		public void FilterInstances(Func<object, bool> filter)
		{
			instances.RemoveAll(o => !filter(o));
		}

		public void UseAllContracts(int contractsCount)
		{
			usedContractIndexes = Enumerable.Range(0, contractsCount).Select((i, x) => i).ToList();
		}

		public void UnionUsedContracts(ContainerService dependency)
		{
			if (dependency.usedContractIndexes == null)
				return;
			if (usedContractIndexes == null)
				usedContractIndexes = new List<int>();
			foreach (var otherIndex in dependency.usedContractIndexes)
				if (otherIndex < context.requiredContracts.Count && !usedContractIndexes.Contains(otherIndex))
					usedContractIndexes.Add(otherIndex);
		}

		public void Union(ContainerService other)
		{
			foreach (var instance in other.instances)
				if (!instances.Contains(instance))
					instances.Add(instance);
			UnionUsedContracts(other);
		}

		public void UseContractWithIndex(int index)
		{
			if (usedContractIndexes == null)
				usedContractIndexes = new List<int>();
			if (!usedContractIndexes.Contains(index))
				usedContractIndexes.Add(index);
		}

		public void EndResolveDependencies()
		{
			FinalUsedContracts = GetUsedContractNamesFromContext();
		}

		public string[] GetUsedContractNames()
		{
			return FinalUsedContracts ?? GetUsedContractNamesFromContext();
		}

		private string[] GetUsedContractNamesFromContext()
		{
			return usedContractIndexes == null
				? new string[0]
				: usedContractIndexes.Select(i => context.requiredContracts[i].name).ToArray();
		}

		public object SingleInstance()
		{
			if (instances.Count == 1)
				return instances[0];
			var prefix = instances.Count == 0
				? "no implementations for " + type.Name
				: string.Format("many implementations for {0}\r\n{1}", type.Name,
					instances.Select(x => "\t" + x.GetType().FormatName()).JoinStrings("\r\n"));
			throw new SimpleContainerException(string.Format("{0}\r\n{1}", prefix, context.Format(type)));
		}

		public void WaitForResolve()
		{
			if (!instantiated && !Failed)
				lock (lockObject)
					while (!instantiated && !Failed)
						if (!Monitor.Wait(lockObject, waitTimeout))
							throw new SimpleContainerException(string.Format("service [{0}] wait for resolve timed out after [{1}] millis",
								type.FormatName(), waitTimeout.TotalMilliseconds));
		}

		public bool AcquireInstantiateLock()
		{
			if (instantiated)
				return false;
			Monitor.Enter(lockObject);
			if (!instantiated)
				return true;
			Monitor.Exit(lockObject);
			return false;
		}

		public void InstantiatedSuccessfully(int topSortIndex)
		{
			TopSortIndex = topSortIndex;
			Failed = false;
			instantiated = true;
		}

		public void InstantiatedUnsuccessfully()
		{
			Failed = true;
		}

		public void ReleaseInstantiateLock()
		{
			Monitor.PulseAll(lockObject);
			Monitor.Exit(lockObject);
		}

		public void Throw(string format, params object[] args)
		{
			context.Report("<---------------");
			context.Throw(format, args);
		}
	}
}