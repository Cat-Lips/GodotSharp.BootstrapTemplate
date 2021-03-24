using System;
using System.Collections.Generic;
using Godot;
using GodotSharp.BuildingBlocks.TestRunner;

namespace MyGame.Tests
{
    [SceneTree]
    public abstract partial class Run : Control
    {
        private static IEnumerable<Func<ITest>> Tests
        {
            get
            {
                // TODO: Add tests here...
                yield return () => ITest.GetTest<ExampleTestScenario>();
            }
        }

        [GodotOverride]
        public void OnEnterTree()
            => _.TestRunner.Initialise(Tests);
    }
}
