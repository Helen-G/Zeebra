using System;
using System.Collections.Generic;
using System.Linq;
using Moq;

namespace AFT.RegoV2.GameApi.Tests.Core
{
    public abstract class MoqUnitTestBase
    {
        protected readonly ITestConfig Config = new TestConfig();

        /// <summary>
        /// We need this method in order to achieve:
        /// 1. No relying on the order of dependencies
        /// 2. Auto mocked dependencies (with MockBehavior.Loose) 
        /// </summary>
        /// <typeparam name="TConcrete">The type of the object being tested</typeparam>
        /// <typeparam name="TInterface">The type of the interface object implement (or same type as the object)</typeparam>
        /// <param name="dependencies">A list of dependencies that you don't want to auto-mock</param>
        /// <returns>The object that is being tested - TInterface implementing TConcrete</returns>

        protected static TInterface Create<TConcrete, TInterface>(params object[] dependencies)
            where TConcrete : TInterface
        {
            return Create<TConcrete, TInterface>(MockBehavior.Loose, dependencies);
        }

        /// <summary> 
        /// We need this method in order to achieve:
        /// 1. No relying on the order of dependencies
        /// 2. Auto mocked dependencies   
        /// </summary>
        /// <typeparam name="TConcrete">The type of the object being tested</typeparam>
        /// <typeparam name="TInterface">The type of the interface object implement (or same type as the object)</typeparam>
        /// <param name="mockBehavior">Loose or Strict Mock Behavior for those dependencies that are auto mocked</param>
        /// <param name="dependencies">A list of dependencies that you don't want to auto-mock</param>
        /// <returns>The object that is being tested - TInterface implementing TConcrete</returns>
        protected static TInterface Create<TConcrete, TInterface>(MockBehavior mockBehavior, params object[] dependencies)
            where TConcrete : TInterface
        {
            var dependenciesList =  
                dependencies != null && dependencies.Length > 0
                ? new List<object>(dependencies)
                : new List<object>();
            var constructor = typeof(TConcrete).GetConstructors().FirstOrDefault();
            if (constructor == null) throw new InvalidOperationException("Object " + typeof(TConcrete) + " does not have public constructor");
            var list = new List<object>();
            foreach (var param in constructor.GetParameters())
            {
                var paramType = param.ParameterType; 
                var found = false;
                if (dependenciesList.Count > 0)
                {
                    for(var i = dependenciesList.Count - 1; i >= 0; --i)
                    {
                        var currentDependency = dependenciesList[i];
                        var currentDependencyType = currentDependency.GetType();
                        if (paramType.IsAssignableFrom(currentDependencyType))
                        {
                            found = true;
                            dependenciesList.RemoveAt(i);
                            list.Add(currentDependency);
                            break;
                        }
                    }
                }
                if (!found)
                {
                    var mockOfParamType = typeof(Mock<>).MakeGenericType(param.ParameterType);
                    var mock = (Mock)Activator.CreateInstance(mockOfParamType, mockBehavior);
                    list.Add(mock.Object);
                }
            }
            return (TInterface)Activator.CreateInstance(typeof(TConcrete), list.ToArray());
        }
    }
}