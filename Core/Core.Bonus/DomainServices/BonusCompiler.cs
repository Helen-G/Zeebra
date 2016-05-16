using System;
using System.CodeDom.Compiler;
using System.Linq;
using AFT.RegoV2.Core.Domain.Bonus.Data;
using ServiceStack.Common.Extensions;

namespace AFT.RegoV2.Core.Domain.Bonus.DomainServices
{
    public class BonusCompiler
    {
        public BonusBuildResult CompileBonus(string script)
        {
            var result = new BonusBuildResult();
            var buildResult = BuildAssembly(script);

            result.IsValid = !buildResult.Errors.HasErrors;

            if (buildResult.Errors.HasErrors)
            {
                foreach (CompilerError error in buildResult.Errors)
                {
                    var errorMessage = string.Format("Line {0}: {1}.", error.Line, error.ErrorText);
                    result.Errors.Add(new ValidationError { ErrorMessage = errorMessage });                  
                }
            }
            else
            {
                var assembly = buildResult.CompiledAssembly;
                var type = assembly.GetTypes().Single();
                var method = type.GetMethods().First();

                var instance = assembly.CreateInstance(type.FullName);
                var bonus = method.Invoke(instance, null);
                result.Bonus = (Data.Bonus)bonus;
            }

            return result;
        }

        CompilerResults BuildAssembly(string code)
        {
            var provider = CodeDomProvider.CreateProvider("CSharp");
            var compilerParams = new CompilerParameters
            {
                GenerateInMemory = true,
                GenerateExecutable = false
            };
            var referencedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic).Select(x => x.Location).ToArray();
            referencedAssemblies.ForEach(x => compilerParams.ReferencedAssemblies.Add(x));

            return provider.CompileAssemblyFromSource(compilerParams, code);
        }
    }
}