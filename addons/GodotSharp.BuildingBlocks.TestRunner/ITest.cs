using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions.Execution;
using Godot;

namespace GodotSharp.BuildingBlocks.TestRunner
{
    public interface ITest
    {
        Node Node => (Node)this;
        int RequiredFrames => 0;

        /// <summary>
        /// Implement to test initial state of scene before being added to tree
        /// </summary>
        protected void InitTests()
            => throw new NotImplementedException();

        /// <summary>
        /// Implement to test state of scene after being added to tree
        /// </summary>
        protected void EnterTests()
            => throw new NotImplementedException();

        /// <summary>
        /// Implement to test state of scene after OnProcess has been called x times (where x = RequiredFrames)
        /// </summary>
        protected void ProcessTests()
            => throw new NotImplementedException();

        bool? RunInitTests(out IEnumerable<string> errors)
            => TestScope(InitTests, out errors);

        bool? RunEnterTests(out IEnumerable<string> errors)
            => TestScope(EnterTests, out errors);

        bool? EvaluateProcessTests(out IEnumerable<string> errors)
            => TestScope(ProcessTests, out errors);

        protected bool? TestScope(Action test, out IEnumerable<string> errors)
        {
            try
            {
                using (new AssertionScope()) test();

                errors = null;
                return true;
            }
            catch (NotImplementedException)
            {
                errors = null;
                return null;
            }
            catch (Exception e)
            {
                errors = e.Message.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                return false;
            }
        }

        public static T GetTest<T>() where T : Node, ITest
        {
            var tscn = typeof(T).GetCustomAttribute<SceneTreeAttribute>().SceneFile;
            var test = (T)ResourceLoader.Load<PackedScene>(tscn).Instance();
            test.Name = typeof(T).Name;
            return test;
        }
    }
}
